using postsandbeams.block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace postsandbeams
{
    class PostsAndBeamsCore : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.Logger.Notification("Loaded Posts And Beams!");

            api.RegisterBlockClass("BlockPost", typeof(BlockPost));
        }
    }
}
