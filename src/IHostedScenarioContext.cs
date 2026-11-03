namespace Cthangover.Mods.Dialog
{
    /// <summary>Optional action-context capability exposing the execution owner without a global current-scenario singleton.</summary>
    public interface IHostedScenarioContext
    {
        IScenarioExecutionHost Host { get; }
    }
}
