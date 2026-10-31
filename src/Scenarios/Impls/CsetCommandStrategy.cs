namespace Cthangover.Mods.Dialog.Scenarios
{
    /// <summary>Explicit numeric assignment; calculation happens when the queue reaches the action, never while parsing.</summary>
    public class CsetCommandStrategy : NumericAssignmentCommandStrategy
    {
        public override string Command => "cset";
    }
}
