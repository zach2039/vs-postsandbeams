using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

using PostsAndBeams.ModBlock;
using PostsAndBeams.ModBlockBehavior;

namespace PostsAndBeams
{
    class PostsAndBeamsCore : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Logger.Notification("Loaded Posts And Beams!");

            api.RegisterBlockClass("BlockPost", typeof(BlockPost));
            api.RegisterBlockBehaviorClass("BreakIfNotConnectedPost", typeof(BlockBehaviorBreakIfNotConnectedPost));
            api.RegisterBlockBehaviorClass("UnstableFallingSupportable", typeof(BlockBehaviorUnstableFallingSupportable));

            try
            {
                var Config = api.LoadModConfig<PostsAndBeamsConfig>("postsandbeams.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    PostsAndBeamsConfig.Current = Config;
                }
                else
                {
                    api.Logger.Warning("No Mod Config specified. Falling back to default settings!");
                    PostsAndBeamsConfig.Current = PostsAndBeamsConfig.GetDefault();
                }
            }
            catch
            {
                PostsAndBeamsConfig.Current = PostsAndBeamsConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                api.StoreModConfig(PostsAndBeamsConfig.Current, "postsandbeams.json");
            }
        }
    }
}
