

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

		/// <summary>
		/// Gets the distance of a post in a given direction
		/// </summary>
		/// <param name="world"></param>
		/// <param name="posBeam"></param>
		/// <param name="searchDirection"></param>
		/// <param name="maxDistance"></param>
		/// <returns>distance from post, starting at 1 if directly adjacent; -1 if not found</returns>
		private int FindConnectedPostWithinDistanceInDirection(IWorldAccessor world, BlockPos posBeam, BlockFacing searchDirection, int maxDistance)
		{
			int distance = -1;
			// Search in the direction until we find a post
			for (int i = 1; i <= maxDistance; i++)
			{
				Block blockFound = world.BlockAccessor.GetBlock(posBeam.Copy().Add(searchDirection, i));

				if (blockFound == null)
				{
					// Exit early if no block found
					distance = -1;
					break;
				}

				if (blockFound is BlockPost)
				{
					distance = i; 
					break;
				}
				else if (blockFound.GetBehavior<BlockBehaviorBreakIfNotConnectedPost>() == null)
				{
					// Exit early if non-beam block separates post and beam
					distance = -1;
					break;
				}
			}

			// -1 means a post was not in the max allowable distance
			return distance;
		}

        public bool IsConnectedAndFacingPost(IWorldAccessor world, BlockPos pos)
		{
			// TODO: Make configurable
			int maxPostDistance = 3;

			// Search for valid post connection in either end directions of beam
			BlockFacing searchDirA = BlockFacing.FromFirstLetter(this.block.Variant["orientation"][0]);
			BlockFacing searchDirB = BlockFacing.FromFirstLetter(this.block.Variant["orientation"][1]);

			int distanceA = FindConnectedPostWithinDistanceInDirection(world, pos, searchDirA, maxPostDistance);
			int distanceB = -1;

			// Try to search in other direction if we didn't find any posts
			if (distanceA == -1)
			{
				distanceB = FindConnectedPostWithinDistanceInDirection(world, pos, searchDirB, maxPostDistance);
			}

			if (distanceA == -1 && distanceB == -1)
			{
				// No post found inline with beam
				return false;
			}
			
			return true;
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
		{
			handling = EnumHandling.PassThrough;
		}
    }
}