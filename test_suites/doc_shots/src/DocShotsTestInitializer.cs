using System.Linq;
using Cthangover.Core.Mods;
using Cthangover.Core.Utils;
using Godot;

namespace Cthangover.Mods.Dialog.DocShots
{
    /// <summary>
    /// Entry point of the doc_shots suite: adds the driver when the exact flag
    /// <c>--itest=doc_shots</c> is given. <c>--doc-steps=&lt;file&gt;</c> is the
    /// steps file, <c>--shots=&lt;dir&gt;</c> where the pictures go.
    /// </summary>
    public class DocShotsTestInitializer : IModInitializer
    {
        private static bool driverQueued;

        /// <summary>Nothing per mod.</summary>
        public void OnModLoaded(string modId)
        {
        }

        /// <summary>Queues the driver once.</summary>
        public void OnModResourcesReady()
        {
            var args = OS.GetCmdlineUserArgs();
            if (driverQueued || !args.Any(a => a == "--itest=doc_shots"))
                return;

            driverQueued = true;
            var steps = ArgValue(args, "--doc-steps=");
            var shots = ArgValue(args, "--shots=");
            GameLogger.Log("TEST", $"DocShotsTestInitializer: steps={steps ?? "none"}, shots={shots ?? "off"}");
            ModRoot.AddDeferred(new DocShotsDriver(steps, shots));
        }

        private static string ArgValue(string[] args, string prefix) =>
            args.Where(a => a.StartsWith(prefix, System.StringComparison.Ordinal))
                .Select(a => a.Substring(prefix.Length))
                .FirstOrDefault();
    }
}
