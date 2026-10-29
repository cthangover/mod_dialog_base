namespace Cthangover.Mods.Dialog.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Mods.Dialog.Action.Impls;
    using Godot;

    /// <summary>
    /// <see cref="ActionAnimatedForeground"/> really animates: frames are cut on the
    /// ViewBox foreground while the dialog goes on, a one-shot animation stops on its
    /// last frame, and a looping one stops when the dialog ends.
    /// </summary>
    public class ActionAnimatedForegroundTests
    {
        [ModTest("animation/one_shot_reaches_last_frame", TimeoutSeconds = 10)]
        static async Task OneShotReachesLastFrame(ModTestContext ctx)
        {
            var stage = await DialogStage.Create();
            var view = stage.AddViewBox();
            await DialogStage.Frames(1);
            var frames = Frames(3);
            try
            {
                var anim = new ActionAnimatedForeground { Textures = frames, Speed = 20f, NextFrameSpeed = 20f, IsLoop = false };
                var dlg = new DialogQueue();
                dlg.Queue.Add(anim);
                dlg.Queue.Add(new ProbeAction());

                stage.Box.SetDialogQueueAndRun(dlg, null, 0, null);
                ModAssert.True(view.Foreground.Texture == frames[0], "first frame is shown at once");
                ModAssert.True(anim.IsAnimating, "animation outlives its NoWait action");
                await DialogStage.Seconds(0.6);

                ModAssert.True(view.Foreground.Texture == frames[2], "animation reached the last frame");
                ModAssert.False(anim.IsAnimating, "one-shot animation stopped");
            }
            finally
            {
                stage.Free();
                foreach (var texture in frames) texture.Dispose();
            }
        }

        [ModTest("animation/loop_stops_with_dialog_end", TimeoutSeconds = 10)]
        static async Task LoopStopsWithDialogEnd(ModTestContext ctx)
        {
            var stage = await DialogStage.Create();
            var view = stage.AddViewBox();
            await DialogStage.Frames(1);
            var frames = Frames(2);
            try
            {
                var anim = new ActionAnimatedForeground { Textures = frames, Speed = 20f, NextFrameSpeed = 20f, IsLoop = true };
                var dlg = new DialogQueue();
                dlg.Queue.Add(anim);
                dlg.Queue.Add(new ProbeAction());

                stage.Box.SetDialogQueueAndRun(dlg, null, 0, null);
                await DialogStage.Seconds(0.3);
                ModAssert.True(anim.IsAnimating, "looping animation keeps playing while the dialog waits");

                stage.Box.NextAction();
                ModAssert.False(anim.IsAnimating, "dialog end stops the animation");
                var shown = view.Foreground.Texture;
                await DialogStage.Seconds(0.3);
                ModAssert.True(view.Foreground.Texture == shown, "no frame is cut after the dialog ended");
            }
            finally
            {
                stage.Free();
                foreach (var texture in frames) texture.Dispose();
            }
        }

        private static List<Texture2D> Frames(int count)
        {
            var list = new List<Texture2D>();
            for (var i = 0; i < count; i++)
            {
                using var image = Image.CreateEmpty(2, 2, false, Image.Format.Rgba8);
                list.Add(ImageTexture.CreateFromImage(image));
            }
            return list;
        }
    }
}
