using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using PostsAndBeams.ModBlock;
using PostsAndBeams.ModBlockBehavior;
using PostsAndBeams.ModNetwork;

namespace PostsAndBeams
{
    class PostsAndBeamsCore : ModSystem
    {
        private IServerNetworkChannel serverChannel;
        private ICoreAPI api;

        public override void StartPre(ICoreAPI api)
        {
            string cfgFileName = "PostsAndBeams.json";

            try
            {
                PostsAndBeamsConfig cfgFromDisk;
                if ((cfgFromDisk = api.LoadModConfig<PostsAndBeamsConfig>(cfgFileName)) == null)
                {
                    api.StoreModConfig(PostsAndBeamsConfig.Loaded, cfgFileName);
                }
                else
                {
                    PostsAndBeamsConfig.Loaded = cfgFromDisk;
                }
            }
            catch (Exception e)
            {
                api.Logger.Warning("[PostsAndBeams] Failed to load {0}, reverting to defaults: {1}", cfgFileName, e);
                api.StoreModConfig(PostsAndBeamsConfig.Loaded, cfgFileName);
            }

            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            this.api = api;
            base.Start(api);

            api.RegisterBlockClass("BlockPost", typeof(BlockPost));
            api.RegisterBlockClass("BlockTorchHolderOrientable", typeof(BlockTorchHolderOrientable));
            
            api.RegisterBlockBehaviorClass("BreakIfNotConnectedPost", typeof(BlockBehaviorBreakIfNotConnectedPost));
            api.RegisterBlockBehaviorClass("NWOrientableCustomDrops", typeof(BlockBehaviorNWOrientableCustomDrops));
            api.RegisterBlockBehaviorClass("HorizontalOrientableHandleDrops", typeof(BlockBehaviorHorizontalOrientableHandleDrops));

            api.Logger.Notification("Loaded Posts And Beams!");
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Send connecting players config settings
            this.serverChannel.SendPacket(
                new SyncConfigClientPacket {
                    MaxDistanceBeamFromPostBlocks = PostsAndBeamsConfig.Loaded.MaxDistanceBeamFromPostBlocks
                }, player);
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            // Channel must exist before PlayerJoin can fire and call OnPlayerJoin
            this.serverChannel = sapi.Network.RegisterChannel("postsandbeams")
                .RegisterMessageType<SyncConfigClientPacket>()
                .SetMessageHandler<SyncConfigClientPacket>((player, packet) => {});

            sapi.Event.PlayerJoin += this.OnPlayerJoin;
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            // Sync config settings with clients
            capi.Network.RegisterChannel("postsandbeams")
                .RegisterMessageType<SyncConfigClientPacket>()
                .SetMessageHandler<SyncConfigClientPacket>(p => {
                    this.Mod.Logger.Event("Received config settings from server");
                    PostsAndBeamsConfig.Loaded.MaxDistanceBeamFromPostBlocks = p.MaxDistanceBeamFromPostBlocks;
                });
        }
        
        public override void Dispose()
        {
            if (this.api is ICoreServerAPI sapi)
            {
                sapi.Event.PlayerJoin -= this.OnPlayerJoin;
            }
        }
    }
}
