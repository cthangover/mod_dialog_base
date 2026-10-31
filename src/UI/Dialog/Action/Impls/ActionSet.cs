using System;
using Cthangover.Core.UI;
using Cthangover.Mods.Interface.Base.Lists;
using Cthangover.Core.UI.Dialog;
using Cthangover.Core.UI.Event;
using Cthangover.Core.UI.Executable;
using Cthangover.Core.Scenarios;
using Cthangover.Core.Scenes;
using Cthangover.Core.Interactive;

namespace Cthangover.Mods.Dialog.Action.Impls
{
    /// <summary>
    /// Sets a dialog runtime variable (Name=Value) for use by subsequent actions
    /// via the ${var} substitution syntax. The optional Callback Func&lt;string&gt;
    /// allows lazy evaluation — if set, Value is ignored and Callback is invoked
    /// at runtime. This enables dynamic values (e.g. quest state, character names)
    /// that can't be known when the dialog script is authored.
    /// </summary>
    public class ActionSet : ActionCommand
    {
        /// <summary>Variable name. Referenced by subsequent actions via ${name} syntax.</summary>
        public string Name { get; set; }
        /// <summary>Literal value to store. Ignored if <see cref="Callback"/> is set.</summary>
        public string Value { get; set; }
        /// <summary>Lazy evaluation delegate. When non-null, invoked at runtime to compute the value dynamically.</summary>
        public Func<string> Callback { get; set; }
        /// <summary>Only action parameter templates expand references; ordinary set literals retain their historical behavior.</summary>
        public bool UseProcessText { get; set; }

        /// <summary>Variable assignment is instant — the dialog continues without pause.</summary>
        public override WaitType WaitType { get; set; } = WaitType.NoWait;

        /// <summary>Stores <see cref="Value"/> — or the result of <see cref="Callback"/> when set — as the runtime variable <see cref="Name"/>.</summary>
        public override void DoRun(DialogRuntime runtime)
        {
            var val = Callback != null ? Callback() : Value;
            if (UseProcessText)
            {
                try { val = runtime.ProcessParameter(val); }
                catch (Cthangover.Mods.Expressions.ExpressionEvaluationException ex)
                { runtime.FailExpression(ex.Message, SourceLine); return; }
            }
            runtime.SetVariable(Name, val ?? string.Empty);
        }
    }
}
