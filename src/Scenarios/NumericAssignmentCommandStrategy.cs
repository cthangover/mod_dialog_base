namespace Cthangover.Mods.Dialog.Scenarios
{
    using System.Collections.Generic;
    using Cthangover.Core.Localization;
    using Cthangover.Mods.Stats.Formulas;

    /// <summary>Shared assignment syntax and validation; each command strategy chooses literal or calculated positional values.</summary>
    public abstract class NumericAssignmentCommandStrategy : IScenarioCommandStrategy, IScenarioCommandSpec
    {
        public abstract string Command { get; }
        protected virtual bool CalculatePositional => true;
        public int MinArguments => 1;
        public int MaxPositional => 2;
        public bool PreserveNamedArgumentCase => true;
        public string ValidateArguments(List<string> positional, Dictionary<string,string> named, string arrowTarget)
        {
            if(!string.IsNullOrEmpty(arrowTarget))return "Assignments do not accept a jump target.";
            if(!TryAssignment(positional,named,out var name,out var value))return "Use name value or name=expression, with one assignment per line.";
            if(string.IsNullOrWhiteSpace(name))return "Assignment name is empty.";
            if((CalculatePositional||named.Count>0)&&!Formula.TryParse(value,out _,out var error))return error;
            return null;
        }
        public void Execute(DialogQueue dialog,List<string> positional,Dictionary<string,string> named,string arrowTarget,ILocalizationProvider locale)
        {
            var error=ValidateArguments(positional,named,arrowTarget);
            TryAssignment(positional,named,out var name,out var value);
            if(error!=null||CalculatePositional||named.Count>0)dialog.Calculate(name,value,error);
            else dialog.Set(name,value);
        }
        public void NormalizeArguments(List<string> positional, Dictionary<string, string> named)
        {
            if (positional.Count == 0 && named.Count == 1 && TryAssignment(positional, named, out var name, out var value))
            {
                positional.Add(name);
                positional.Add(value);
                named.Clear();
            }
        }
        private static bool TryAssignment(List<string> positional,Dictionary<string,string> named,out string name,out string value)
        {
            name=value=null;
            if(positional.Count==2&&named.Count==0){name=positional[0];value=positional[1];return true;}
            if(positional.Count==0&&named.Count==1)
                foreach(var pair in named){name=pair.Key;value=pair.Value;return true;}
            return false;
        }
    }
}
