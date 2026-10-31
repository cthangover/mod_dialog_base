namespace Cthangover.Mods.Dialog.Action.Impls
{
    using System;
    using System.Globalization;
    using Cthangover.Mods.Stats.Formulas;
    /// <summary>Evaluate once at the queue position; dependent actions are aborted on invalid input instead of seeing a stale result.</summary>
    public class ActionCalculate : ActionCommand
    {
        public string Name {get;set;}
        public string Expression {get;set;}
        public string ArgumentError {get;set;}
        public override WaitType WaitType {get;set;}=WaitType.NoWait;
        public override void DoRun(DialogRuntime runtime)
        {
            try
            {
                if(ArgumentError!=null)throw new ArgumentException(ArgumentError);
                var value=Formula.Parse(Expression).EvaluateStrict(new DialogExpressionScope(runtime));
                runtime.SetVariable(Name,value.ToString("G17",CultureInfo.InvariantCulture));
            }
            catch(Exception ex){runtime.FailExpression($"Assignment '{Name}': {ex.Message}",SourceLine);}
        }
    }
}
