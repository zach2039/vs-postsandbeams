

using postsandbeams.block;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace postsandbeams.blockbehavior
{
    public class BlockBehaviorBreakIfNotConnectedPost : BlockBehavior
    {
        public BlockBehaviorBreakIfNotConnectedPost(Block block) : base(block)
        {
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handled)
		{
			handled = EnumHandling.PassThrough;
			if (!this.IsConnectedAndFacingPost(world, pos))
			{
				world.BlockAccessor.BreakBlock(pos, null, 1f);
			}
			base.OnNeighbourBlockChange(world, pos, neibpos, ref handled);
		}

		public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
		{
			if (!this.IsConnectedAndFacingPost(world, blockSel.Position))
			{
				handling = EnumHandling.PreventDefault;
				failureCode = "nopostpresent";
				return false;
			}
			return base.CanPlaceBlock(world, byPlayer, blockSel, ref handling, ref failureCode);
		}

        public bool IsConnectedAndFacingPost(IWorldAccessor world, BlockPos pos)
		{
			foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
			{
				if (world.BlockAccessor.GetBlock(pos.Copy().Offset(facing)) is BlockPost)
				{
					if (this.block.Variant["orientation"][0] == facing.Code[0] || this.block.Variant["orientation"][1] == facing.Code[0])
					{
						// Only consider connections if beam is facing post
						return true;
					}
				}

				if (world.BlockAccessor.GetBlock(pos.Copy().Offset(facing)).GetBehavior<BlockBehaviorBreakIfNotConnectedPost>() != null)
				{
					if (this.block.Variant["orientation"][0] == facing.Code[0] || this.block.Variant["orientation"][1] == facing.Code[0])
					{
						// Allow other beams as well, since they cannot persist if missing a post 
						return true;
					}
				}
			}
			return false;
		}

		public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropQuantityMultiplier, ref EnumHandling handled)
		{
			handled = EnumHandling.PreventSubsequent;
            return new ItemStack[]
            {
                new ItemStack(this.block, 1)
            };
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
		{
			handling = EnumHandling.PassThrough;
		}

       
    }
}