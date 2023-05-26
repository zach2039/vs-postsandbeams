using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlockBehavior
{
    public class BlockBehaviorUnstableFallingSupportable : BlockBehavior
    {
        public BlockBehaviorUnstableFallingSupportable(Block block) : base(block)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            attachableFaces = null;

            if (properties["attachableFaces"].Exists)
            {
                string[] faces = properties["attachableFaces"].AsArray<string>();
                attachableFaces = new BlockFacing[faces.Length];

                for (int i = 0; i < faces.Length; i++)
                {
                    attachableFaces[i] = BlockFacing.FromCode(faces[i]);
                }
            }
            
            var areas = properties["attachmentAreas"].AsObject<Dictionary<string, RotatableCube>>(null);
            attachmentAreas = new Cuboidi[6];
            if (areas != null)
            {
                foreach (var val in areas)
                {
                    val.Value.Origin.Set(8, 8, 8);
                    BlockFacing face = BlockFacing.FromFirstLetter(val.Key[0]);
                    attachmentAreas[face.Index] = val.Value.RotatedCopy().ConvertToCuboidi();
                }
            } else
            {
                attachmentAreas[4] = properties["attachmentArea"].AsObject<Cuboidi>(null);
            }

            ignorePlaceTest = properties["ignorePlaceTest"].AsBool(false);
            exceptions = properties["exceptions"].AsObject(new AssetLocation[0], block.Code.Domain);
            fallSideways = properties["fallSideways"].AsBool(false);
            dustIntensity = properties["dustIntensity"].AsFloat(0);

            fallSidewaysChance = properties["fallSidewaysChance"].AsFloat(0.3f);
            string sound = properties["fallSound"].AsString(null);
            if (sound != null)
            {
                fallSound = AssetLocation.Create(sound, block.Code.Domain);
            }

            impactDamageMul = properties["impactDamageMul"].AsFloat(1f);
            this.fallDownwardChance = properties["fallDownwardChance"].AsFloat(PostsAndBeamsConfig.Current.unstableFallingSupportableDropChance);
            this.cascadeFall = properties["cascadeFall"].AsBool(PostsAndBeamsConfig.Current.canUnstableFallingSupportableCascade);
		}

        private bool IsReplacableBeneathAndSideways(IWorldAccessor world, BlockPos pos)
		{
			for (int i = 0; i < 4; i++)
			{
				BlockFacing facing = BlockFacing.HORIZONTALS[i];
				Block nBlock = world.BlockAccessor.GetBlockOrNull(pos.X + facing.Normali.X, pos.Y + facing.Normali.Y, pos.Z + facing.Normali.Z, 4);
				if (nBlock != null && nBlock.Replaceable >= 6000)
				{
					nBlock = world.BlockAccessor.GetBlockOrNull(pos.X + facing.Normali.X, pos.Y + facing.Normali.Y - 1, pos.Z + facing.Normali.Z, 4);
					if (nBlock != null && nBlock.Replaceable >= 6000)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsReplacableBeneath(IWorldAccessor world, BlockPos pos)
		{
			return world.BlockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z).Replaceable > 6000;
		}

        private bool tryFalling(IWorldAccessor world, BlockPos pos, ref EnumHandling handling, ref string failureCode)
		{
			if (world.Side != EnumAppSide.Server)
			{
				return false;
			}

            //world.Api.Logger.Notification("tryFalling");

            // Check for beams 3 blocks away and above, mark as supported and stop falling
            bool supported = false;
            world.BlockAccessor.SearchBlocks(pos.AddCopy(-3, -1, -3), pos.AddCopy(3, 3, 3), (blk, blkpos) => {
                //world.Api.Logger.Notification("checking" + blk.Code);
                
                if (blk.FirstCodePart().Contains("woodenbeam"))
                {
                    supported = true;
                    return false;
                }

                return true;
            });

            if (supported)
			{
                //world.Api.Logger.Notification("supported");
				return false;
			}

            //world.Api.Logger.Notification("unsupported");

            if (world.Rand.NextDouble() >= (double)this.fallDownwardChance)
			{
                //world.Api.Logger.Notification(this.IsReplacableBeneath(world, pos).ToString());
				handling = EnumHandling.PassThrough;
				return false;
			}

            return tryFallingIgnoreDownwardChanceAndSupports(world, pos, ref handling, ref failureCode);
        }

        private bool tryFallingIgnoreDownwardChanceAndSupports(IWorldAccessor world, BlockPos pos, ref EnumHandling handling, ref string failureCode)
		{
			if (world.Side != EnumAppSide.Server)
			{
				return false;
			}

            //world.Api.Logger.Notification("tryFallingIgnoreDownwardChanceAndSupports");

			if (!this.fallSideways && this.IsAttached(world.BlockAccessor, pos))
			{
				return false;
			}
			if (!((world as IServerWorldAccessor).Api as ICoreServerAPI).Server.Config.AllowFallingBlocks)
			{
				return false;
			}
			if (!this.IsReplacableBeneath(world, pos) && (!this.fallSideways || world.Rand.NextDouble() >= (double)this.fallSidewaysChance || !this.IsReplacableBeneathAndSideways(world, pos)))
			{
				handling = EnumHandling.PassThrough;
				return false;
			}
			if (world.GetNearestEntity(pos.ToVec3d().Add(0.5, 0.5, 0.5), 1f, 1.5f, delegate(Entity e)
			{
				EntityBlockFalling ebf = e as EntityBlockFalling;
				return ebf != null && ebf.initialPos.Equals(pos);
			}) == null)
			{
				EntityBlockFalling entityblock = new EntityBlockFalling(this.block, world.BlockAccessor.GetBlockEntity(pos), pos, this.fallSound, this.impactDamageMul, true, this.dustIntensity);
				world.SpawnEntity(entityblock);
				handling = EnumHandling.PreventSubsequent;
				return true;
			}
			handling = EnumHandling.PreventDefault;
			failureCode = "entityintersecting";
			return false;
		}

        public virtual bool IsAttached(IBlockAccessor blockAccessor, BlockPos pos)
		{
			BlockPos tmpPos;
			if (this.attachableFaces == null)
			{
				tmpPos = pos.DownCopy(1);
				return blockAccessor.GetBlock(tmpPos).CanAttachBlockAt(blockAccessor, this.block, tmpPos, BlockFacing.UP, this.attachmentAreas[5]);
			}
			tmpPos = new BlockPos();
			for (int i = 0; i < this.attachableFaces.Length; i++)
			{
				BlockFacing face = this.attachableFaces[i];
				tmpPos.Set(pos).Add(face, 1);
				if (blockAccessor.GetBlock(tmpPos).CanAttachBlockAt(blockAccessor, this.block, tmpPos, face.Opposite, this.attachmentAreas[face.Index]))
				{
					return true;
				}
			}
			return false;
		}

        public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
		{
			handling = EnumHandling.PassThrough;
			if (this.ignorePlaceTest)
			{
				return true;
			}
			Cuboidi attachmentArea = this.attachmentAreas[4];
			BlockPos pos = blockSel.Position.DownCopy(1);
			Block onBlock = world.BlockAccessor.GetBlock(pos);
			if (blockSel != null && !this.IsAttached(world.BlockAccessor, blockSel.Position) && !onBlock.CanAttachBlockAt(world.BlockAccessor, this.block, pos, BlockFacing.UP, attachmentArea))
			{
				JsonObject attributes = this.block.Attributes;
				if ((attributes == null || !attributes["allowUnstablePlacement"].AsBool(false)) && !this.exceptions.Contains(onBlock.Code))
				{
					handling = EnumHandling.PreventSubsequent;
					failureCode = "requiresolidground";
					return false;
				}
			}

			return this.tryFalling(world, blockSel.Position, ref handling, ref failureCode);
		}

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handling)
		{
			base.OnNeighbourBlockChange(world, pos, neibpos, ref handling);
			if (world.Side == EnumAppSide.Client)
			{
				return;
			}
			EnumHandling bla = EnumHandling.PassThrough;
			string bla2 = "";
			bool didFall = this.tryFalling(world, pos, ref bla, ref bla2);

            if (didFall && this.cascadeFall)
            {
                // Be sinister and make other blocks of similar behavior fall above and around
                BlockPos[] positionsToPoke = new BlockPos[] {
                    pos.NorthCopy(),
                    pos.SouthCopy(),
                    pos.EastCopy(),
                    pos.WestCopy(),
                    pos.UpCopy(),
                };

                foreach (BlockPos posToPoke in positionsToPoke)
                {
                    Block block = world.BlockAccessor.GetBlock(posToPoke);
                    if (block != null)
                    {   
                        BlockBehaviorUnstableFallingSupportable blockBehavior = block.GetBehavior<BlockBehaviorUnstableFallingSupportable>() as BlockBehaviorUnstableFallingSupportable;
                        if (blockBehavior != null)
                        {
                            blockBehavior.tryFallingIgnoreDownwardChanceAndSupports(world, posToPoke, ref bla, ref bla2);
                        }
                    }
                }                                
            }
		}

        private float fallDownwardChance = 0.1f;

        private bool cascadeFall = true;

        private bool ignorePlaceTest;

		private AssetLocation[] exceptions;

		public bool fallSideways;

		private float dustIntensity;

		private float fallSidewaysChance = 0.3f;

		private AssetLocation fallSound;

		private float impactDamageMul;

		private Cuboidi[] attachmentAreas;

		private BlockFacing[] attachableFaces;
    }
}