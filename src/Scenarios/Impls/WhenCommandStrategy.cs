namespace Cthangover.Mods.Dialog.Scenarios
{
    using System.Collections.Generic;
    using Cthangover.Core.Localization;
    using Cthangover.Core.Scenes;
    using Cthangover.Mods.Dialog;

    /// <summary>
    /// A conditional jump in the middle of a dialog:
    /// <code>when "stat(cha) &gt;= 5 &amp;&amp; door == 'open'" -> :persuaded [else=refused]</code>
    /// evaluates the shared condition language (flags, quests, stats, equipment, GDScript
    /// conditions — everything mods registered) at the moment the line runs, and jumps to
    /// the arrow label when it holds, to <c>else</c> when it does not (or goes on).
    /// <para/>
    /// It is the RPG Maker "Conditional Branch" without a new syntax: the condition is the
    /// same text a scenario header's <c>condition:</c> takes. Several words without quotes
    /// are joined back with spaces, so short conditions need no quotes.
    /// </summary>
    public class WhenCommandStrategy : IScenarioCommandStrategy, IScenarioCommandSpec
    {
        /// <inheritdoc />
        public string Command => "when";

        /// <inheritdoc />
        public int MinPositional => 1;

        /// <inheritdoc />
        public IReadOnlyList<string> AllowedNamed => new[] { "else", "wait", "timeout" };

        /// <inheritdoc />
        public string JumpTargetNamedKey => "else";

        /// <inheritdoc />
        public void Execute(DialogQueue dlg, List<string> positional, Dictionary<string, string> named, string arrowTarget, ILocalizationProvider locale)
        {
            var condition = string.Join(" ", positional);
            named.TryGetValue("else", out var otherwise);
            if (named.TryGetValue("wait", out var wait) && wait == "true")
            {
                if (!named.TryGetValue("timeout", out var timeout) || !double.TryParse(timeout,
                    System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds)
                    || !double.IsFinite(seconds) || seconds <= 0)
                    throw new System.FormatException("when wait=true requires a positive finite timeout");
                dlg.Queue.Add(new Cthangover.Mods.Dialog.Action.Impls.ActionWaitCondition
                {
                    ConditionText = condition, Timeout = seconds,
                    TrueTarget = Label(arrowTarget), TimeoutTarget = Label(otherwise),
                });
                return;
            }
            var branch = dlg.IfGoTo(null, Label(arrowTarget), Label(otherwise));
            branch.ConditionText = condition;
        }

        private static string Label(string target) => string.IsNullOrWhiteSpace(target) ? null : target.Trim().TrimStart(':');
    }
}
