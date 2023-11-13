using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace postsandbeams.block
{
    public class BlockPost : Block
    {
		public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
		{
		}

		public bool ShouldConnectAt(IWorldAccessor world, BlockPos ownPos, BlockFacing side)
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

		private string GetPostCode(IWorldAccessor world, BlockPos pos, BlockFacing facing)
		{
			if (this.ShouldConnectAt(world, pos, facing))
			{
				return facing.Code[0].ToString() ?? "";
			}
			return "";
		}

		private string GetOrientations(IWorldAccessor world, BlockPos pos)
		{
			string orientations = this.GetPostCode(world, pos, BlockFacing.NORTH) + this.GetPostCode(world, pos, BlockFacing.EAST) + this.GetPostCode(world, pos, BlockFacing.SOUTH) + this.GetPostCode(world, pos, BlockFacing.WEST);
			if (orientations.Length == 0)
			{
				orientations = "empty";
			}
			return orientations;
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
				base.OnNeighbourBlockChange(world, pos, neibpos);
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

		public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
		{
			Block block = world.BlockAccessor.GetBlock(base.CodeWithVariants(new string[]
			{
				"type",
				"cover"
			}, new string[]
			{
				"ew",
				"free"
			}));
			return new ItemStack[]
			{
				new ItemStack(block, 1)
			};
		}
    }
}
