using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

using postsandbeams.block;
using postsandbeams.blockbehavior;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace postsandbeams
{
    class PostsAndBeamsCore : ModSystem
    {
        public static PostsAndBeamsCore Instance { get; private set; }

        public ICoreClientAPI CApi { get; private set; }
        public ICoreServerAPI SApi { get; private set; }
        public ICoreAPI Api { get; private set; }

        public override void StartPre(ICoreAPI api)
        {
            Instance = this;
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            CApi = capi;
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            base.StartServerSide(sapi);

            SApi = sapi;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Api = api;

            PostsAndBeamsCore.Instance.Api.RegisterBlockClass("BlockPost", typeof(BlockPost));
            PostsAndBeamsCore.Instance.Api.RegisterBlockBehaviorClass("BreakIfNotConnectedPost", typeof(BlockBehaviorBreakIfNotConnectedPost));

            try
            {
                var Config = PostsAndBeamsCore.Instance.Api.LoadModConfig<PostsAndBeamsConfig>("postsandbeams.json");
                if (Config != null)
                {
                    PostsAndBeamsCore.Instance.Api.Logger.Notification("Mod Config successfully loaded.");
                    PostsAndBeamsConfig.Current = Config;
                }
                else
                {
                    PostsAndBeamsCore.Instance.Api.Logger.Warning("No Mod Config specified. Falling back to default settings!");
                    PostsAndBeamsConfig.Current = PostsAndBeamsConfig.GetDefault();
                }
            }
            catch
            {
                PostsAndBeamsConfig.Current = PostsAndBeamsConfig.GetDefault();
                PostsAndBeamsCore.Instance.Api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                PostsAndBeamsCore.Instance.Api.StoreModConfig(PostsAndBeamsConfig.Current, "postsandbeams.json");
            }

            PostsAndBeamsCore.Instance.Api.Logger.Notification("Loaded Posts And Beams!");
        }

        public override void Dispose()
        {
            if (Api == null) return;

            Instance = null;
        }
    }
}
