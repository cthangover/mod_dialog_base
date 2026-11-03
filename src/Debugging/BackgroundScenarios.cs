namespace Cthangover.Mods.Dialog
{
    /// <summary>Optional background-host capability shared by authoring tools without a dependency on a particular gameplay mod.</summary>
    public static class BackgroundScenarios
    {
        public static IBackgroundScenarioTools Provider { get; set; }
    }
}
