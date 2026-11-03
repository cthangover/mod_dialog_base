namespace Cthangover.Mods.Dialog
{
    using System.Collections.Generic;
    using Godot;

    /// <summary>Live authoring seam. Runtime IDs select one execution, scenario IDs select source and breakpoints.</summary>
    public interface IBackgroundScenarioTools
    {
        IReadOnlyList<BackgroundScenarioInfo> Catalog();
        IReadOnlyList<BackgroundRunInfo> Executions();
        IReadOnlyList<string> Events { get; }
        bool WorldPaused { get; set; }
        void Observe(Node owner, bool enabled);
        bool Validate(string modId, string path, string text, out string error);
        bool Reload(out string error);
        bool Play(string modId, string path, string text, bool company, out string error);
        bool Command(string runId, string command, out string error);
        bool HasBreakpoint(string scenarioId, int line);
        void ToggleBreakpoint(string scenarioId, int line);
    }
}
