using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using PostsAndBeams.ModBlockBehavior;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Util;

namespace PostsAndBeams.ModBlock
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

			bool postBelow = world.BlockAccessor.GetBlock(blockSel.Position.DownCopy()) is BlockPost;
			if (blockSel.Face.IsHorizontal && !postBelow)
			{
				// Try to place a beam instead first, if no post below
				string beamOrientation = blockSel.Face.IsAxisNS ? "ns" : "we";

				AssetLocation blockBeamAsset = new AssetLocation(base.Code.Domain, String.Concat("woodenbeam", "-", this.Variant["wood"],
					"-", this.Variant["bark"], "-", beamOrientation));
				Block blockBeam = world.BlockAccessor.GetBlock(blockBeamAsset);

				bool canPlaceBeam = blockBeam != null && blockBeam.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode);
				if (canPlaceBeam)
				{
					bool successfulPostPlacement = blockBeam.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

					BlockBehaviorBreakIfNotConnectedPost breakIfNotConnectedPost = blockBeam.GetBehavior<BlockBehaviorBreakIfNotConnectedPost>();
					if (successfulPostPlacement) 
					{
						if (byPlayer.Entity.Controls.ShiftKey) // only allow autoplace if shift is not held
							return true;

						if (breakIfNotConnectedPost == null)
							return true;

						// Try to stack beams auto-magically up to max distance away if there is a post within distance
						BlockSelection blockSelectionTowards = blockSel.Clone();
						blockSelectionTowards.Position = blockSelectionTowards.Position.Offset(blockSel.Face);

						int distanceAutoplace = breakIfNotConnectedPost.FindConnectedPostWithinDistanceInDirection(world, blockSel.Position, blockSel.Face, PostsAndBeamsConfig.Loaded.MaxDistanceBeamFromPostBlocks, true) - 1;

						if (!(distanceAutoplace <= 0)) // Distance less than or equal to 0 means no placin'
						{
							for (int i = 0; i < distanceAutoplace; i++)
							{
								if (itemstack.StackSize <= 1 && byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative) // Check for less eq 1, since after this method an item is consumed
									break;

								string failureCodeTowards = "";
								if (blockBeam.CanPlaceBlock(world, byPlayer, blockSelectionTowards, ref failureCodeTowards))
								{
									blockBeam.DoPlaceBlock(world, byPlayer, blockSelectionTowards, itemstack);
									if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
										itemstack.StackSize--;

									// Force an update to get adjacents connecting, if near
									world.BlockAccessor.TriggerNeighbourBlockUpdate(blockSelectionTowards.Position);
								}
								else
								{
									break;
								}

								blockSelectionTowards.Position = blockSelectionTowards.Position.Offset(blockSel.Face);
							}
						}

						return true;
					}
				}
			}
			
			// Try placing the post next, if beam was unplaceable
			if (block.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
			{
				bool successfulPostPlacement = block.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

				if (successfulPostPlacement) 
				{
					if (byPlayer.Entity.Controls.ShiftKey) // only allow autoplace if shift is not held
						return true;

					// Try to stack posts auto-magically up to three tall if we succesfully placed the first of the three
					BlockSelection blockSelectionUp = blockSel.Clone();
					for (int i = 0; i < 2; i++)
					{
						if (itemstack.StackSize <= 1 && byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative) // Check for less eq 1, since after this method an item is consumed
							break;

						blockSelectionUp.Position = blockSelectionUp.Position.Add(0,1,0);

						string failureCodeUp = "";
						if (block.CanPlaceBlock(world, byPlayer, blockSelectionUp, ref failureCodeUp))
						{
							block.DoPlaceBlock(world, byPlayer, blockSelectionUp, itemstack);
							if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
								itemstack.StackSize--;	

							// Force an update to get adjacents connecting, if near
							world.BlockAccessor.TriggerNeighbourBlockUpdate(blockSelectionUp.Position);
						}
						else if (world.BlockAccessor.GetBlock(blockSelectionUp.Position).Code.BeginsWith(this.Code.Domain, "woodenbeam"))
						{
							// Replace beams in path of auto-pillar with posts; dont consume the item and cancel placing afterwards
							world.BlockAccessor.SetBlock(block.BlockId, blockSelectionUp.Position);
							
							world.BlockAccessor.TriggerNeighbourBlockUpdate(blockSelectionUp.Position);
							break;
						}
						else
						{
							break;
						}
					}

					return true;
				}
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

		public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
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
			return new ItemStack(block, 1);
		}

		public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
		{
			return new WorldInteraction[]
			{
				new WorldInteraction
				{
					ActionLangCode = "heldhelp-placeauto",
					MouseButton = EnumMouseButton.Right
				},
				new WorldInteraction
				{
					ActionLangCode = "heldhelp-placesingle",
					HotKeyCode = "shift",
					MouseButton = EnumMouseButton.Right
				}
			};
		}
    }
}
