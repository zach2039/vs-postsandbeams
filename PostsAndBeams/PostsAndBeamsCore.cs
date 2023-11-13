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
        private IServerNetworkChannel serverChannel;
        private ICoreAPI api;

        public override void StartPre(ICoreAPI api)
        {
            Instance = this;

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
            catch 
            {
                api.StoreModConfig(PostsAndBeamsConfig.Loaded, cfgFileName);
            }

            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            this.Api = api;
            base.Start(api);

            api.RegisterBlockClass("BlockPost", typeof(BlockPost));
            api.RegisterBlockBehaviorClass("BreakIfNotConnectedPost", typeof(BlockBehaviorBreakIfNotConnectedPost));

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
            sapi.Event.PlayerJoin += this.OnPlayerJoin; 
            
            // Create server channel for config data sync
            this.serverChannel = sapi.Network.RegisterChannel("postsandbeams")
                .RegisterMessageType<SyncConfigClientPacket>()
                .SetMessageHandler<SyncConfigClientPacket>((player, packet) => {});
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
