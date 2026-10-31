namespace Cthangover.Mods.Dialog.Tests
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Cthangover.Core.Factories.Impls;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Mods.Dialog.Scenarios;
    using Godot;

    /// <summary>Old string scripts, calculated assignments and action arguments share the existing DSL strategy pipeline.</summary>
    public class NumericAssignmentTests
    {
        [ModTest("numeric_assignment/literals_case_and_assignment_syntax")]
        static void Assignments(ModTestContext ctx)
        {
            var runtime=Run("set greeting Hello\nset literal 1+2\nset Value 7\ncset total \"1 + 2 + ${Value}\"\nset Money=rnd(3,3)\ncset quoted=\"${total} * 2\"\nend\n");
            ModAssert.Equal("Hello",runtime.GetVariable("greeting"),"old strings stay strings");
            ModAssert.Equal("1+2",runtime.GetVariable("literal"),"set does not guess expressions");
            ModAssert.Equal("10",runtime.GetVariable("total"),"earlier numeric string is read at runtime");
            ModAssert.Equal("3",runtime.GetVariable("Money"),"assigned variable keeps its case");
            ModAssert.Equal("20",runtime.GetVariable("quoted"),"quoted assignment expression retains spaces");
            ModAssert.Null(runtime.LastExpressionError,"valid assignments have no errors");
        }
        [ModTest("numeric_assignment/errors_abort_dependents_without_overwriting")]
        static void Errors(ModTestContext ctx)
        {
            foreach(var expression in new[]{"1/0","${missing}+1","rnd(5,3)","max()"})
            {
                var runtime=Run("set amount 9\ncset amount \""+expression+"\"\nset after wrong\nend\n");
                ModAssert.Equal("9",runtime.GetVariable("amount"),"bad assignment preserves the previous value for inspection");
                ModAssert.Null(runtime.GetVariable("after"),"dependent actions never run after a bad calculation");
                ModAssert.True(runtime.LastExpressionError?.Contains(":2:")==true,"error reports the source line");
            }
            var text=Run("set value abc\ncset result ${value}+1\nset after wrong\nend\n");
            ModAssert.True(text.LastExpressionError?.Contains("not numeric")==true,"non-numeric variables are diagnosed");
        }
        [ModTest("numeric_assignment/invariant_format_and_json_tokens")]
        static void Formatting(ModTestContext ctx)
        {
            var culture=CultureInfo.CurrentCulture;
            try
            {
                // A hand-built comma culture: ru-RU/es-ES cannot be created where ICU is missing
                // (invariant globalization), and the rule must hold for any culture anyway.
                var comma=(CultureInfo)CultureInfo.InvariantCulture.Clone();
                comma.NumberFormat.NumberDecimalSeparator=",";
                comma.NumberFormat.NumberGroupSeparator=" ";
                CultureInfo.CurrentCulture=comma;
                ModAssert.Equal("2.5",Run("cset amount 5/2\nend\n").GetVariable("amount"),"stored numeric syntax does not depend on UI culture");
            }
            finally{CultureInfo.CurrentCulture=culture;}
            var positional=new List<string>();var named=new Dictionary<string,string>();
            ScenarioParser.SplitLine("light_set [{\"x\":0.2,\"color\":\"#fff\"}]",positional,named,out _);
            ModAssert.Equal("[{\"x\":0.2,\"color\":\"#fff\"}]",positional[0],"JSON quotes are not stripped by assignment quoting");
            ModAssert.Equal(0,named.Count,"JSON never becomes a named argument");
        }
        [ModTest("numeric_assignment/command_owned_validation")]
        static void Validation(ModTestContext ctx)
        {
            IScenarioCommandSpec spec=new CsetCommandStrategy();
            ModAssert.NotNull(spec.ValidateArguments(new(){"amount"},new(),""),"missing operand diagnosed");
            ModAssert.NotNull(spec.ValidateArguments(new(){"amount","rnd(1)"},new(),""),"function arity checked without rolling");
            ModAssert.Null(spec.ValidateArguments(new(),new(){["Money"]="rnd(3,5)"},""),"assignment form recognized by strategy contract");
            var positional=new List<string>();var named=new Dictionary<string,string>{["Money"]="rnd(3,5)"};
            spec.NormalizeArguments(positional,named);
            ModAssert.Equal("Money",positional[0],"schema normalization preserves case");
            ModAssert.Equal("rnd(3,5)",positional[1],"schema receives the unevaluated expression");
            ModAssert.Equal(0,named.Count,"normalized arguments are canonical");
        }
        [ModTest("numeric_assignment/localized_text_and_action_share_one_value",TimeoutSeconds=10)]
        static async Task TextAndAction(ModTestContext ctx)
        {
            var stage=await DialogStage.Create();var capture=new NumericCaptureAction();
            ScenarioActionFactory.Instance.Register(capture);
            try
            {
                var dialog=ScenarioParser.Parse("cset money=rnd(3,3)\naction dsl_test.capture id=money/coin count=${money}\nptext \"Fallback\" key=mod_dialog_base_name\nend\n",new NumericTextLocale());
                stage.Box.SetDialogQueueAndRun(dialog,null,0,null);
                ModAssert.Equal("3",capture.Amount,"action receives a number, never the placeholder");
                ModAssert.Equal("money/coin",capture.Item,"ordinary string parameters still work");
                ModAssert.Equal("Received 3 coins.",stage.Box.GetNode<Label>("DialogBody/TextField").Text,"localized ptext displays the same stored value");
                stage.Box.NextAction();
                ModAssert.Equal("3",stage.Box.Runtime.GetVariable("money"),"continuing the text does not reroll");
            }
            finally{ScenarioActionFactory.Instance.UnregisterAssembly(typeof(NumericCaptureAction).Assembly);stage.Free();}
        }
        [ModTest("numeric_assignment/undefined_action_parameter_stops_action")]
        static void MissingParameter(ModTestContext ctx)
        {
            var capture=new NumericCaptureAction();ScenarioActionFactory.Instance.Register(capture);
            try
            {
                var runtime=Run("set marker kept\naction dsl_test.capture count=${missing}\nset marker wrong\nend\n");
                ModAssert.Null(capture.Amount,"invalid parameter is never delivered to the action");
                ModAssert.Equal("kept",runtime.GetVariable("marker"),"later state changes are skipped");
                ModAssert.True(runtime.LastExpressionError?.Contains("missing")==true,"undefined parameter is reported");
            }
            finally{ScenarioActionFactory.Instance.UnregisterAssembly(typeof(NumericCaptureAction).Assembly);}
        }
        private static DialogRuntime Run(string script)
        {
            var dialog=ScenarioParser.Parse(script);var runtime=new DialogRuntime();
            runtime.SetDialogQueueAndRun(null,dialog.Queue,null);runtime.Run();return runtime;
        }
    }
}
