namespace Cthangover.Mods.Dialog.Action.Impls
{
    /// <summary>An explicit asynchronous condition wait; ordinary when remains a one-shot branch.</summary>
    public class ActionWaitCondition : ActionCommand
    {
        /// <summary>Evaluated against the execution's live context.</summary>
        public string ConditionText { get; set; }
        /// <summary>Maximum host-clock wait.</summary>
        public double Timeout { get; set; }
        /// <summary>Label selected when the condition becomes true.</summary>
        public string TrueTarget { get; set; }
        /// <summary>Label selected on timeout, or fall-through when absent.</summary>
        public string TimeoutTarget { get; set; }
        public override WaitType WaitType { get; set; } = WaitType.WaitEvent;
        public override void DoRun(DialogRuntime runtime)
        {
            if (runtime.Host == null)
                runtime.FailExpression("when wait=true requires an execution host", SourceLine);
            else runtime.Host.WaitCondition(this, runtime);
        }
    }
}
