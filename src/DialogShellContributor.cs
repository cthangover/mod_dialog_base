namespace Cthangover.Mods.Dialog
{
    using Cthangover.Core.Scenes;
    using Cthangover.Core.UI.Executable;
    using Cthangover.Mods.Dialog.Executable;
    using Cthangover.Mods.Interface.Shell;
    using Godot;
    /// <summary>Dialogs are optional content of a ready shell, rather than the owner of shell readiness.</summary>
    public sealed class DialogShellContributor : IShellContributor
    {
        public void Attach(Node root)
        {
            if (root is IExecutableEventChain || root.FindChild("DialogEvents", true, false) != null) return;
            var canvas = TscnScenes.LoadAndBuild("scenes/ui/dialog_canvas.tscn");
            if (canvas == null) return;
            (root.GetNodeOrNull<Node>("Scene") ?? root).AddChild(canvas);
            var chain = new ExecutableMainEventChain { Name = "DialogEvents" };
            chain.AddToGroup("main_event_chain"); root.AddChild(chain);
        }
    }
}
