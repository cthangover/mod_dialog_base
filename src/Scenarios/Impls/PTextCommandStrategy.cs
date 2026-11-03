using System.Collections.Generic;
using Cthangover.Core.Localization;
using Cthangover.Mods.Dialog;
using Cthangover.Mods.Dialog.Action;
using Cthangover.Core.UI;
using Cthangover.Mods.Interface.Base.Lists;
using Cthangover.Core.UI.Dialog;
using Cthangover.Core.UI.Event;
using Cthangover.Core.UI.Executable;
using Cthangover.Core.Scenarios;
using Cthangover.Core.Scenes;
using Cthangover.Core.Interactive;

namespace Cthangover.Mods.Dialog.Scenarios
{
    /// <summary>
    /// Handles the <c>ptext</c> DSL command. A "plain text" variant of
    /// <see cref="TextCommandStrategy"/> that skips speaker attribution
    /// but still supports <c>first</c>/<c>second</c> avatar faces and
    /// <c>hide_color</c> and <c>wait</c> behavior flags. Text content
    /// can be provided directly or resolved via a <c>key=</c> localization key.
    /// </summary>
    public class PTextCommandStrategy : IScenarioCommandStrategy, IScenarioCommandSpec
    {
        /// <inheritdoc/>
        public string Command => "ptext";
        /// <inheritdoc/>
        public int MinArguments => 1;
        /// <inheritdoc/>
        public int MaxPositional => 1;
        /// <inheritdoc/>
        public IReadOnlyList<string> AllowedNamed => new[] { "key", "first", "second", "hide_color", "wait", "who", "duration", "cue" };

        /// <inheritdoc/>
        public void Execute(DialogQueue dlg, List<string> positional, Dictionary<string, string> named, string arrowTarget, ILocalizationProvider locale)
        {
            var textVal = positional.Count > 0 ? positional[0] : "";
            if (named.TryGetValue("key", out var key) && locale != null)
            {
                var loc = locale.Get(key);
                if (loc != null) textVal = loc;
            }

            var first = named.TryGetValue("first", out var f) ? f : null;
            var second = named.TryGetValue("second", out var s) ? s : null;
            var hideColor = named.TryGetValue("hide_color", out var hc) && hc == "true";

            var action = dlg.PText(textVal, first, second, hideColor);
            action.FallbackText = positional.Count > 0 ? positional[0] : "";
            action.LocaleKey = key;
            action.Speaker = named.TryGetValue("who", out var who) ? who : null;
            action.Cue = named.TryGetValue("cue", out var cue) ? cue : null;
            if (named.TryGetValue("duration", out var duration))
            {
                if (!double.TryParse(duration, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds)
                    || !double.IsFinite(seconds) || seconds <= 0)
                    throw new System.FormatException("duration must be a positive finite number");
                action.Duration = seconds;
            }

            if (named.TryGetValue("wait", out var wait) && wait == "false")
                action.WaitType = WaitType.NoWait;
        }
    }
}
