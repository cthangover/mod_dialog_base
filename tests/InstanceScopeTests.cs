namespace Cthangover.Mods.Dialog.Tests
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Core.Scenes;
    using Cthangover.Core.Settings;
    using Cthangover.Core.Utils;
    using Cthangover.Mods.Dialog.Scenarios;

    /// <summary>
    /// <c>self.</c> in a dialog (contract <c>rpg-model-v2.md</c> 5.5): the queue's
    /// <see cref="DialogQueue.Scope"/> — the placement that started it — turns <c>self.open</c> into the
    /// story flag <c>&lt;placement&gt;:open</c> in <c>flag</c>, <c>when</c> and option conditions. Two
    /// instances of one object keep their own state, which is an ordinary saved flag.
    /// </summary>
    public class InstanceScopeTests
    {
        private const string First = "test_scope_map/barrel_1";
        private const string Second = "test_scope_map/barrel_2";

        /// <summary>The shared scenario of both barrels: open once, then say so.</summary>
        private const string Barrel = "when \"self.open == 1\" -> :opened\nflag self.open 1\nset outcome opened_now\nend\n:opened\nset outcome already_open\nend\n";

        [ModTest("scope/two_instances_are_independent_and_survive_a_save", TimeoutSeconds = 10)]
        static async Task TwoInstancesAreIndependentAndSurviveASave(ModTestContext ctx)
        {
            var stage = await DialogStage.Create();
            var runtime = GameData.Instance.Runtime;
            var savedFlags = runtime.Flags;
            runtime.Flags = new Dictionary<string, string>();
            try
            {
                ModAssert.Equal("opened_now", Play(stage, Barrel, First), "the first barrel opens");
                ModAssert.Equal("already_open", Play(stage, Barrel, First), "the first barrel remembers it");
                ModAssert.Equal("opened_now", Play(stage, Barrel, Second), "the second barrel is still closed");
                ModAssert.Equal("1", runtime.Flags.TryGetValue(First + ":open", out var value) ? value : null, "the flag has the placement's full name");
                ModAssert.Equal(2, runtime.Flags.Count, "no global 'open' flag is written");
                ModAssert.True(ScenarioCondition.Evaluate(First + ":open == 1"), "other scenarios read it by its full name");

                var slot = RuntimeSerializer.BuildCoreSlot(runtime, "1.0.0", "test", new HashSet<string>());
                var json = JsonSerializer.Serialize(slot, JsonOptionsProvider.WriteCompactOptions);
                runtime.Flags = new Dictionary<string, string>();
                ModAssert.False(ScenarioCondition.Evaluate("self.open == 1", First), "state cleared before the load");
                RuntimeSerializer.RestoreCoreState(JsonSerializer.Deserialize<ModSlot>(json, JsonOptionsProvider.ReadOptions), runtime);

                ModAssert.True(ScenarioCondition.Evaluate("self.open == 1", First), "the first barrel is open after the load");
                ModAssert.True(ScenarioCondition.Evaluate("self.open == 1", Second), "the second barrel is open after the load");
                ModAssert.Equal("already_open", Play(stage, Barrel, Second), "the loaded state drives the dialog");
            }
            finally
            {
                runtime.Flags = savedFlags;
                stage.Free();
            }
        }

        [ModTest("scope/option_conditions_read_the_instance", TimeoutSeconds = 10)]
        static async Task OptionConditionsReadTheInstance(ModTestContext ctx)
        {
            var stage = await DialogStage.Create();
            var runtime = GameData.Instance.Runtime;
            var savedFlags = runtime.Flags;
            runtime.Flags = new Dictionary<string, string> { [First + ":open"] = "1" };
            try
            {
                var dlg = ScenarioParser.Parse("select \"?\"\noption \"Close\" \"if=self.open == 1\" -> :a\noption \"Open\" \"if=self.open != 1\" -> :b\noption \"Leave\" -> :c\n:a\nend\n:b\nend\n:c\nend\n");
                dlg.Scope = First;
                ModAssert.True(stage.Box.SetDialogQueueAndRun(dlg, null, 0, null), "dialog starts");
                ModAssert.Equal(2, stage.Box.AnswerVariantCount, "only the option of the open instance and the plain one");
            }
            finally
            {
                runtime.Flags = savedFlags;
                stage.Free();
            }
        }

        [ModTest("scope/self_without_a_scope_writes_nothing", TimeoutSeconds = 10)]
        static async Task SelfWithoutAScopeWritesNothing(ModTestContext ctx)
        {
            var stage = await DialogStage.Create();
            var runtime = GameData.Instance.Runtime;
            var savedFlags = runtime.Flags;
            runtime.Flags = new Dictionary<string, string>();
            try
            {
                ModAssert.Equal("opened_now", Play(stage, Barrel, null), "the dialog still runs to its end");
                ModAssert.Equal(0, runtime.Flags.Count, "self. outside a scope is never a global flag");
            }
            finally
            {
                runtime.Flags = savedFlags;
                stage.Free();
            }
        }

        private static string Play(DialogStage stage, string scenario, string scope)
        {
            var dlg = new ScenarioConverter().Convert(scenario);
            dlg.Scope = scope;
            ModAssert.True(stage.Box.SetDialogQueueAndRun(dlg, null, 0, null), "dialog starts");
            return stage.Box.Runtime.GetVariable("outcome");
        }
    }
}
