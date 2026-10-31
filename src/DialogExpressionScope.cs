namespace Cthangover.Mods.Dialog
{
    using System;
    using System.Globalization;
    using Cthangover.Mods.Expressions;
    using Cthangover.Mods.Stats.Formulas;
    /// <summary>Numeric reads from the current dialog only; strings never become executable expression fragments.</summary>
    public class DialogExpressionScope : IFormulaScope
    {
        private readonly DialogRuntime runtime;
        public DialogExpressionScope(DialogRuntime runtime){this.runtime=runtime;}
        public Random Random=>runtime.ExpressionRandom;
        public bool TryGet(string name,out double value)
        {
            var text=runtime.GetVariable(name);value=0;
            if(text==null)return false;
            if(!double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out value))
                throw new ExpressionEvaluationException("Variable '"+name+"' is not numeric.");
            return true;
        }
    }
}
