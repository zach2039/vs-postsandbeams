using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlock
{
    public class BlockPost : BlockFence
    {
		public new string GetOrientations(IWorldAccessor world, BlockPos pos)
		{
			string orientations = this.GetPostCode(world, pos, BlockFacing.NORTH) + this.GetPostCode(world, pos, BlockFacing.EAST) + this.GetPostCode(world, pos, BlockFacing.SOUTH) + this.GetPostCode(world, pos, BlockFacing.WEST);
			if (orientations.Length == 0)
			{
				orientations = "empty";
			}
			return orientations;
		}

		private string GetPostCode(IWorldAccessor world, BlockPos pos, BlockFacing facing)
		{
			if (this.ShouldConnectAt(world, pos, facing))
			{
				return facing.Code[0].ToString() ?? "";
			}
			return "";
		}

		public new bool ShouldConnectAt(IWorldAccessor world, BlockPos ownPos, BlockFacing side)
		{
			Block block = world.BlockAccessor.GetBlock(ownPos.AddCopy(side));
			JsonObject attributes = block.Attributes;

			if (attributes != null)
			{
				if (block.Variant != null)
				{
					if ((side == BlockFacing.NORTH || side == BlockFacing.SOUTH) && attributes["postConnect"]["ns"].Exists)
					{
						return block.Variant["orientation"] == "ns";
					}
					else if ((side == BlockFacing.EAST || side == BlockFacing.WEST) && attributes["postConnect"]["we"].Exists)
					{
						return block.Variant["orientation"] == "we";
					}
				}
				else if (attributes["postConnect"][side.Code].Exists)
				{
					return block.Attributes["postConnect"][side.Code].AsBool(false);
				}
			}

			return false;
		}

		public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
			string orientations = this.GetOrientations(world, blockSel.Position);
			Block block = world.BlockAccessor.GetBlock(base.CodeWithVariant("type", orientations));
			if (block == null)
			{
				block = this;
			}
			if (block.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
			{
				world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
				return true;
			}
			return false;
		}

		public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
		{
			string orientations = this.GetOrientations(world, pos);
			AssetLocation newBlockCode = base.CodeWithVariant("type", orientations);
			if (this.Code.Equals(newBlockCode))
			{
				EnumHandling handled = EnumHandling.PassThrough;
				BlockBehavior[] blockBehaviors = this.BlockBehaviors;
				for (int i = 0; i < blockBehaviors.Length; i++)
				{
					blockBehaviors[i].OnNeighbourBlockChange(world, pos, neibpos, ref handled);
					if (handled == EnumHandling.PreventSubsequent)
					{
						return;
					}
				}
				if (handled == EnumHandling.PassThrough && (this == this.snowCovered1 || this == this.snowCovered2 || this == this.snowCovered3) && pos.X == neibpos.X && pos.Z == neibpos.Z && pos.Y + 1 == neibpos.Y && world.BlockAccessor.GetBlock(neibpos).Id != 0)
				{
					world.BlockAccessor.SetBlock(this.notSnowCovered.Id, pos);
				}
				return;
			}
			Block block = world.BlockAccessor.GetBlock(newBlockCode);
			if (block == null)
			{
				return;
			}
			world.BlockAccessor.SetBlock(block.BlockId, pos);
			world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
		}		
    }
}
