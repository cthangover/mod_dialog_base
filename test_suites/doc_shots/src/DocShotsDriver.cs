using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cthangover.Core.Bootstrap;
using Cthangover.Core.Interactive;
using Cthangover.Core.Scenes;
using Cthangover.Core.Settings;
using Cthangover.Core.UI;
using Cthangover.Core.UI.Tool;
using Cthangover.Core.Utils;
using Cthangover.Mods.Interface.Tool;
using Cthangover.Mods.Interface.View;
using Godot;

namespace Cthangover.Mods.Dialog.DocShots
{
    /// <summary>
    /// Plays a plain-text steps file in a running game and saves screenshots for
    /// mod documentation, so a page's pictures come from the real application
    /// without one driver per mod. One step per line, <c>#</c> starts a comment:
    /// <list type="bullet">
    /// <item><c>scene &lt;name&gt;</c> — aborts the running dialog (as "Go to scene" does) and switches; waits for the scene, then settles.</item>
    /// <item><c>time HH:MM</c> — sets the clock of the current day.</item>
    /// <item><c>wait &lt;ticks&gt;</c> — 0.1 s ticks.</item>
    /// <item><c>abort</c>, <c>next [n]</c>, <c>choose &lt;i&gt;</c>, <c>waitanswer</c> — the dialog box.</item>
    /// <item><c>panel &lt;id&gt;</c> — toggles a toolbar panel like its button.</item>
    /// <item><c>key &lt;Key&gt; [ticks]</c> — presses a key (Godot name, e.g. <c>E</c>, <c>W</c>, <c>Escape</c>) and releases it after the ticks.</item>
    /// <item><c>tap &lt;node&gt; &lt;i&gt;</c> — left-clicks child <c>i</c> of the first node with that name (an inventory cell); <c>press &lt;text&gt;</c> — presses the visible button with that text or with a label of that text inside.</item>
    /// <item><c>give &lt;item&gt; [count]</c>, <c>party &lt;character&gt;</c> — seeds the inventory and the party.</item>
    /// <item><c>signal &lt;node&gt; &lt;signal&gt; [arg]</c> — emits a signal of the first node with that name, as its own input handling would (a world map zone click).</item>
    /// <item><c>hover &lt;x&gt; &lt;y&gt;</c>, <c>click &lt;x&gt; &lt;y&gt;</c> — moves the mouse to (and left-clicks at) a point given as fractions of the window, through the normal input routing (scene interactives).</item>
    /// <item><c>drag &lt;x1&gt; &lt;y1&gt; &lt;x2&gt; &lt;y2&gt;</c> — presses the left button at the first point, moves to the second with it held and releases there (dragging a battle card onto a target).</item>
    /// <item><c>hurt &lt;character&gt; &lt;hp&gt;</c> — sets a party member's current health (to show healing).</item>
    /// <item><c>tool &lt;id&gt;</c> — opens a registered tool window (<c>schema_palette</c>, <c>items_editor</c>…) the way the toolbar's tool list does.</item>
    /// <item><c>type &lt;text&gt;</c> — replaces the text of the focused line edit (click it first).</item>
    /// <item><c>call &lt;tool&gt; &lt;method&gt; [args…]</c> — calls a public method with string arguments on a window opened by <c>tool</c>.</item>
    /// <item><c>run &lt;scenario line&gt;</c> — runs one scenario line through the dialog runner, as an interactive object's inline commands do (<c>run action rpg.battle enemies=rpg_wolf</c>).</item>
    /// <item><c>shot &lt;name&gt;</c> — saves <c>&lt;name&gt;.png</c> (windowed runs only).</item>
    /// </list>
    /// Waits are tick counts, not conditions on the picture; only reaching each
    /// scene and the dialog states is checked. Result line:
    /// <c>[TEST_RESULT] name=doc_shots passed=N failed=M</c>, exit 0/1, 300 s watchdog.
    /// </summary>
    public partial class DocShotsDriver : Node
    {
        private const float TickSeconds = 0.1f;
        private const float TimeoutSeconds = 300f;
        private const int SettleTicks = 30;
        private const int NextGapTicks = 5;
        private const long MinutesPerDay = 24 * 60;

        private readonly string shotsDir;
        private readonly List<string[]> steps = new();
        private readonly List<string> failures = new();
        private int passed;
        private int index;
        private int waitTicks;
        private int repeat;
        private double elapsed;
        private bool started;
        private string pendingScene;
        private Key heldKey = Key.None;
        private readonly Dictionary<string, Window> tools = new();
        private Window lastTool;

        /// <summary>Reads the steps; a missing file fails the run at the first tick.</summary>
        public DocShotsDriver(string stepsFile, string shotsDir)
        {
            this.shotsDir = shotsDir;
            Name = "DocShotsDriver";
            if (stepsFile != null && File.Exists(stepsFile))
            {
                foreach (var line in File.ReadAllLines(stepsFile))
                {
                    var text = line.Split('#')[0].Trim();
                    if (text.Length > 0)
                        steps.Add(text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
            }
            else
            {
                failures.Add($"steps file '{stepsFile}' not found");
            }
            var timer = new Timer { WaitTime = TickSeconds, Autostart = true };
            AddChild(timer);
            timer.Timeout += Tick;
        }

        private SceneManager Scenes => GetTree().Root.GetNodeOrNull<SceneManager>("SceneManager");

        private static DialogBox Box => SceneContextNode.FindNode<DialogBox>("DialogBox");

        private void Tick()
        {
            if (index < 0)
                return;
            elapsed += TickSeconds;
            if (failures.Count > 0 && !started)
            {
                Finish();
                return;
            }
            if (elapsed > TimeoutSeconds)
            {
                Fail($"timeout at step {index + 1} (scene='{SceneManager.GetCurrentSceneName()}')");
                Finish();
                return;
            }
            if (waitTicks > 0)
            {
                waitTicks--;
                return;
            }
            if (heldKey != Key.None)
            {
                Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = heldKey, Keycode = heldKey, Pressed = false });
                heldKey = Key.None;
                waitTicks = NextGapTicks;
                return;
            }
            if (!started)
            {
                if (!Boot.IsReady || string.IsNullOrEmpty(SceneManager.GetCurrentSceneName()))
                    return;
                started = true;
                GameLogger.Log("TEST", $"DocShotsDriver: started in '{SceneManager.GetCurrentSceneName()}', {steps.Count} steps");
                waitTicks = SettleTicks;
                return;
            }
            if (index >= steps.Count)
            {
                Finish();
                return;
            }

            try
            {
                if (RunStep(steps[index]))
                {
                    index++;
                    repeat = 0;
                }
            }
            catch (Exception ex)
            {
                Fail($"step {index + 1} '{string.Join(' ', steps[index])}': {ex.GetType().Name}: {ex.Message}");
                Finish();
            }
        }

        /// <summary>Runs one step; false keeps it for the next tick.</summary>
        private bool RunStep(string[] step)
        {
            var arg = step.Length > 1 ? step[1] : null;
            switch (step[0])
            {
                case "scene":
                    if (pendingScene == null)
                    {
                        pendingScene = arg;
                        Box?.Abort();
                        Scenes.SwitchScene(arg);
                        return false;
                    }
                    if (SceneManager.GetCurrentSceneName() != pendingScene)
                        return false;
                    Check(true, $"entered '{pendingScene}'");
                    pendingScene = null;
                    waitTicks = SettleTicks;
                    return true;
                case "time":
                    var parts = arg.Split(':');
                    var time = GameData.Instance.Runtime.Time;
                    time.SetTime(time.Tick - time.Tick % MinutesPerDay + int.Parse(parts[0]) * 60 + int.Parse(parts[1]));
                    waitTicks = NextGapTicks;
                    return true;
                case "wait":
                    waitTicks = int.Parse(arg);
                    return true;
                case "abort":
                    Box?.Abort();
                    waitTicks = NextGapTicks;
                    return true;
                case "next":
                    Box?.NextAction();
                    waitTicks = NextGapTicks;
                    repeat++;
                    return repeat >= (arg == null ? 1 : int.Parse(arg));
                case "waitanswer":
                    if (Box?.IsWaitingAnswer != true)
                    {
                        if (Box != null && Box.IsRunning)
                            Box.NextAction();
                        waitTicks = NextGapTicks;
                        return false;
                    }
                    Check(true, "dialog waits for an answer");
                    return true;
                case "choose":
                    Check(Box != null && Box.TrySelectVariant(int.Parse(arg)), $"option {arg} taken");
                    waitTicks = SettleTicks;
                    return true;
                case "panel":
                    UiRegistry.Toggle(arg);
                    waitTicks = SettleTicks / 2;
                    return true;
                case "key":
                    heldKey = Enum.Parse<Key>(arg);
                    Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = heldKey, Keycode = heldKey, Pressed = true });
                    waitTicks = step.Length > 2 ? int.Parse(step[2]) : 1;
                    return true;
                case "tap":
                    Check(Tap(arg, int.Parse(step[2])), $"tap {arg} {step[2]}");
                    waitTicks = NextGapTicks;
                    return true;
                case "press":
                    var label = string.Join(" ", step.Skip(1));
                    Check(Press(GetTree().Root, label), $"press '{label}'");
                    waitTicks = NextGapTicks;
                    return true;
                case "give":
                    GameData.Instance.Runtime.Inventory.Add(arg, step.Length > 2 ? int.Parse(step[2]) : 1);
                    return true;
                case "party":
                    GameData.Instance.Runtime.CharacterData.AddCharacterToParty(arg);
                    return true;
                case "signal":
                    var source = GetTree().Root.FindChild(arg, true, false);
                    Check(source != null && source.HasSignal(step[2]), $"signal {arg} {step[2]}");
                    if (source != null && source.HasSignal(step[2]))
                    {
                        if (step.Length > 3)
                            source.EmitSignal(step[2], step[3]);
                        else
                            source.EmitSignal(step[2]);
                    }
                    waitTicks = NextGapTicks;
                    return true;
                case "hover":
                case "click":
                    Mouse(ParseFraction(arg), ParseFraction(step[2]), step[0] == "click");
                    waitTicks = NextGapTicks;
                    return true;
                case "drag":
                    Drag(ParseFraction(arg), ParseFraction(step[2]), ParseFraction(step[3]), ParseFraction(step[4]));
                    waitTicks = NextGapTicks;
                    return true;
                case "hurt":
                    Check(GameData.Instance.Runtime.CharacterData.Characters.TryGetValue(arg, out var hurt) && hurt.Attributes?.Health != null, $"hurt {arg}");
                    if (hurt?.Attributes?.Health != null)
                        hurt.Attributes.Health.Value = int.Parse(step[2]);
                    return true;
                case "tool":
                    var provider = ToolFactory.Instance.GetAll().FirstOrDefault(t => t.Id == arg);
                    Check(provider != null, $"tool {arg}");
                    if (provider != null)
                        tools[arg] = lastTool = ToolWindow.ShowWindow(provider.CreateWindow());
                    waitTicks = SettleTicks;
                    return true;
                case "type":
                    TypeText(string.Join(' ', step.Skip(1)));
                    waitTicks = SettleTicks / 3;
                    return true;
                case "call":
                    CallTool(step);
                    waitTicks = SettleTicks / 2;
                    return true;
                case "run":
                    var runner = GetTree().Root.GetNodeOrNull<Node>("/root/DialogRunner") as IInteractiveDialogRunner;
                    Check(runner != null, $"run {string.Join(' ', step.Skip(1))}");
                    runner?.ExecuteInlineCommands(string.Join(' ', step.Skip(1)));
                    waitTicks = NextGapTicks;
                    return true;
                case "shot":
                    Shot(arg);
                    waitTicks = NextGapTicks;
                    return true;
                default:
                    throw new InvalidOperationException("unknown step");
            }
        }

        private void Check(bool condition, string what)
        {
            if (condition)
            {
                passed++;
                GameLogger.Log("TEST", $"[PASS] {what}");
            }
            else
            {
                Fail(what);
            }
        }

        private void Fail(string what)
        {
            failures.Add(what);
            GameLogger.Log("TEST", $"[FAIL] {what}", LogLevel.Error);
        }

        private bool Tap(string nodeName, int child)
        {
            var parent = GetTree().Root.FindChild(nodeName, true, false);
            if (parent == null || child >= parent.GetChildCount() || parent.GetChild(child) is not Control target)
                return false;
            target.EmitSignal(Control.SignalName.GuiInput, new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true });
            return true;
        }

        private static bool Press(Node node, string label)
        {
            if (node is BaseButton button && button.IsVisibleInTree() && !button.Disabled
                && (((button as Button)?.Text ?? string.Empty).Trim() == label || HasLabel(button, label)))
            {
                button.EmitSignal(BaseButton.SignalName.Pressed);
                return true;
            }
            foreach (var child in node.GetChildren())
                if (Press(child, label))
                    return true;
            return false;
        }

        private static bool HasLabel(Node node, string label)
        {
            foreach (var child in node.GetChildren())
                if ((child is Label text && text.Text.Trim() == label) || HasLabel(child, label))
                    return true;
            return false;
        }

        private void Mouse(float x, float y, bool click)
        {
            var viewport = GetViewport();
            var position = new Vector2(x, y) * viewport.GetVisibleRect().Size;
            viewport.WarpMouse(position);
            viewport.PushInput(new InputEventMouseMotion { Position = position, GlobalPosition = position });
            if (!click)
                return;
            viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = position, GlobalPosition = position });
            viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = position, GlobalPosition = position });
        }

        /// <summary>
        /// Replaces the text of the focused line edit (in the last opened tool window, else in the game) and emits its
        /// change signal, as a player's typing would; focus it first with <c>click</c>.
        /// </summary>
        private void TypeText(string text)
        {
            var viewport = GodotObject.IsInstanceValid(lastTool) ? (Viewport)lastTool : GetViewport();
            var edit = viewport.GuiGetFocusOwner() as LineEdit;
            Check(edit != null, $"type {text}");
            if (edit == null)
                return;
            edit.Text = text;
            edit.EmitSignal(LineEdit.SignalName.TextChanged, text);
        }

        /// <summary>
        /// Calls a public method of a tool window opened by an earlier <c>tool</c> step, by name and with string
        /// arguments: <c>call schema_palette SetValue stat str</c>. The tool types live in mods this suite does not
        /// depend on, so the method is found by reflection; a missing window or method is a failure.
        /// </summary>
        private void CallTool(string[] step)
        {
            var window = step.Length > 2 && tools.TryGetValue(step[1], out var w) ? w : null;
            var args = step.Skip(3).Cast<object>().ToArray();
            var method = window?.GetType().GetMethods().FirstOrDefault(m => m.Name == step[2]
                && m.GetParameters().Length == args.Length && m.GetParameters().All(p => p.ParameterType == typeof(string)));
            Check(method != null, $"call {string.Join(' ', step.Skip(1))}");
            method?.Invoke(window, args);
        }

        private void Drag(float x1, float y1, float x2, float y2)
        {
            var viewport = GetViewport();
            var size = viewport.GetVisibleRect().Size;
            var from = new Vector2(x1, y1) * size;
            var to = new Vector2(x2, y2) * size;
            viewport.WarpMouse(from);
            viewport.PushInput(new InputEventMouseMotion { Position = from, GlobalPosition = from });
            viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = from, GlobalPosition = from, ButtonMask = MouseButtonMask.Left });
            for (var i = 1; i <= 8; i++)
            {
                var point = from.Lerp(to, i / 8f);
                viewport.PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point, ButtonMask = MouseButtonMask.Left, Relative = (to - from) / 8f });
            }
            viewport.WarpMouse(to);
            viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = to, GlobalPosition = to });
        }

        private static float ParseFraction(string text)
        {
            return float.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void Shot(string name)
        {
            if (string.IsNullOrEmpty(shotsDir) || DisplayServer.GetName() == "headless")
                return;
            DirAccess.MakeDirRecursiveAbsolute(shotsDir);
            ViewportCapture.SavePng(GetViewport(), shotsDir.TrimEnd('/', '\\') + "/" + name + ".png");
            GameLogger.Log("TEST", $"DocShotsDriver: saved {name}.png");
        }

        private void Finish()
        {
            index = -1;
            GameLogger.Log("TEST", $"[TEST_RESULT] name=doc_shots passed={passed} failed={failures.Count}");
            GetTree().Quit(failures.Count == 0 ? 0 : 1);
        }
    }
}
