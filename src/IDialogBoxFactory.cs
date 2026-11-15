using Godot;

namespace Cthangover.Mods.Dialog
{
    /// <summary>
    /// Optional shell presentation creates the existing dialog controller with another prefab. Returning null
    /// keeps the normal 2D presentation; runtime, choices and save ownership always remain in dialog_base.
    /// </summary>
    public interface IDialogBoxFactory
    {
        /// <summary>
        /// Returns a detached owned box only when this factory supports the canvas's actual shell. The canvas
        /// takes tree ownership; implementations must release failed detached construction themselves. A factory
        /// implementing IDisposable releases live supplied UI when its consumer registration is removed.
        /// </summary>
        DialogBox Create(DialogCanvas canvas);
    }
}
