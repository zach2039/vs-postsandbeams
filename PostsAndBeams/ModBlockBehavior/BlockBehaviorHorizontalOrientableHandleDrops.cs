using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlockBehavior
{
    /// <summary>
	/// A version of vanilla's BlockBehaviorHorizontalOrientable that allows skipping drops like BlockBehaviorHorizontalAttachable
	/// </summary>
    public class BlockBehaviorHorizontalOrientableHandleDrops : BlockBehaviorHorizontalOrientable
    {
        public BlockBehaviorHorizontalOrientableHandleDrops(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            this.handleDrops = properties["handleDrops"].AsBool(true);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropQuantityMultiplier, ref EnumHandling handled)
        {
            if (!this.handleDrops)
            {
                handled = EnumHandling.PassThrough;
                return null;
            }
            return base.GetDrops(world, pos, byPlayer, ref dropQuantityMultiplier, ref handled);
        }

        private bool handleDrops = true;
    }
}
