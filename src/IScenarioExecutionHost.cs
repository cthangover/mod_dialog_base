namespace Cthangover.Mods.Dialog
{
    using Cthangover.Mods.Dialog.Action.Impls;

    /// <summary>Per-execution presentation and wait services. A host never owns another runtime's mutable actions.</summary>
    public interface IScenarioExecutionHost
    {
        /// <summary>Displays timed speech without occupying the modal DialogBox.</summary>
        void ShowText(ActionText action, DialogRuntime runtime);
        /// <summary>Starts a host-clock delay; completion advances only this execution.</summary>
        void Delay(ActionDelay action, DialogRuntime runtime);
        /// <summary>Waits for a live condition or its deadline.</summary>
        void WaitCondition(ActionWaitCondition action, DialogRuntime runtime);
        /// <summary>Evaluates against this execution's roles and scope.</summary>
        bool EvaluateCondition(string condition);
        /// <summary>Releases waits and presentation. Aborts do not count as completed scenarios.</summary>
        void Finished(bool aborted);
    }
}
