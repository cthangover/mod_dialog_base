namespace Cthangover.Mods.Dialog
{
    /// <summary>Catalog source plus a current, human-inspectable admission decision.</summary>
    public sealed class BackgroundScenarioInfo
    {
        public string Id { get; init; }
        public string ModId { get; init; }
        public string Path { get; init; }
        public string Map { get; init; }
        public string Source { get; init; }
        public string Body { get; init; }
        public string Status { get; init; }
        public string Details { get; init; }
    }
}
