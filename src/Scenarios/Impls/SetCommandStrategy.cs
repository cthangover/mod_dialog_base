namespace Cthangover.Mods.Dialog.Scenarios
{
    /// <summary>Legacy set name value remains literal. The previously unsupported name=expression form calculates a number.</summary>
    public class SetCommandStrategy : NumericAssignmentCommandStrategy
    {
        public override string Command => "set";
        protected override bool CalculatePositional => false;
    }
}
