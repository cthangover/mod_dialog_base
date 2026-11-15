using System;
using System.Collections.Generic;
using Cthangover.Core.Mods.Consumers;

namespace Cthangover.Mods.Dialog
{
    /// <summary>
    /// Tracks optional presentation factories with their supplying assembly. The ordinary prefab remains the
    /// fallback, so installing a 3D presentation never changes a 2D canvas.
    /// </summary>
    public sealed class DialogBoxFactoryConsumer : ModTypeConsumer<IDialogBoxFactory>
    {
        internal static readonly List<IDialogBoxFactory> Factories = new();

        /// <summary>
        /// Stable consumer identity for symmetric provider registration and removal during mod-set changes.
        /// </summary>
        public override string Id
        {
            get
            {
                return "dialog_box_factories";
            }
        }

        protected override void Add(IDialogBoxFactory instance, string modId)
        {
            Factories.Add(instance);
        }

        protected override void Remove(IDialogBoxFactory instance, string modId)
        {
            Factories.Remove(instance);
            if (instance is IDisposable owner)
            {
                owner.Dispose();
            }
        }
    }
}
