namespace Cthangover.Mods.Dialog
{
    using System.Collections.Generic;
    /// <summary>A detached UI snapshot; no tool retains mutable actions or a gameplay session.</summary>
    public sealed class BackgroundRunInfo
    {
        public string RunId { get; init; }
        public string ScenarioId { get; init; }
        public int Line { get; init; }
        public string Label { get; init; }
        public bool Paused { get; init; }
        public string Wait { get; init; }
        public double Remaining { get; init; }
        public string Text { get; init; }
        public string Speaker { get; init; }
        public string Body { get; init; }
        public string Details { get; init; }
        public IReadOnlyDictionary<string, string> Variables { get; init; }
        public IReadOnlyDictionary<string, string> Roles { get; init; }
    }
}
