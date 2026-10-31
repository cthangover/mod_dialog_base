namespace Cthangover.Mods.Dialog.Tests
{
    using Cthangover.Core.Actions;
    internal class NumericCaptureAction : IScenarioAction
    {
        public string Name=>"dsl_test.capture";
        public string Amount {get;private set;}
        public string Item {get;private set;}
        public void Run(IActionContext context){Amount=context.GetParam("count");Item=context.GetParam("id");}
    }
}
