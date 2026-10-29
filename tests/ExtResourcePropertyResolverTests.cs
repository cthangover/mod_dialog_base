namespace Cthangover.Mods.Dialog.Tests
{
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Core.Scenes.TscnBuild;
    using Cthangover.Core.Scenes.TscnBuild.PropertyResolvers;
    using Cthangover.Core.Scenes.TscnParsing;
    using Cthangover.Mods.Interface.View;
    using Godot;

    /// <summary>Packing a nested prefab must release the temporary tree while preserving its children in the packed result.</summary>
    public class ExtResourcePropertyResolverTests
    {
        private static Node temporary;

        [ModTest("prefabs/packing_releases_the_temporary_tree")]
        static void PackingReleasesTemporaryTree(ModTestContext ctx)
        {
            var target = new DialogBox();
            try
            {
                var scene = new ParsedScene();
                scene.ExtResources.Add(new ExtResourceRef { Id = "nested", Type = "PackedScene", Path = "test_nested.tscn" });
                var context = new PropertyContext(scene, CreateTemporary, null);
                new ExtResourcePropertyResolver().Apply(target, "answerBoxScene", TscnPropertyValue.FromExtResource("nested"), context);
                ModAssert.False(GodotObject.IsInstanceValid(temporary), "packing frees its input instance immediately");
                using var packed = target.Get("answerBoxScene").AsGodotObject() as PackedScene;
                ModAssert.NotNull(packed, "the target retains the packed prefab");
                var instance = packed.Instantiate();
                try
                {
                    ModAssert.Equal("PackedRoot", instance.Name.ToString(), "packed root survives freeing the temporary tree");
                    ModAssert.Equal(1, instance.GetChildCount(), "packed child is retained");
                    ModAssert.Equal("PackedChild", instance.GetChild(0).Name.ToString(), "child owners were assigned before packing");
                }
                finally { instance.Free(); }
            }
            finally
            {
                target.Free();
                if (GodotObject.IsInstanceValid(temporary)) temporary.Free();
                temporary = null;
            }
        }

        private static Node CreateTemporary(string path)
        {
            temporary = new Control { Name = "PackedRoot" };
            temporary.AddChild(new Label { Name = "PackedChild", Text = "nested prefab" });
            return temporary;
        }
    }
}
