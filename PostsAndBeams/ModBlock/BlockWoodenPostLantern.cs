using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlock
{
    // A wooden post fused with a lantern. Subclasses vanilla BlockLantern so that the
    // lantern half is textured via the inherited [textureCode] indexer using the BE's
    // material/lining/glass attributes. Wood/bark/size/orientation are block-level variants;
    // material/lining/glass live on the BELantern subclass at runtime.
    public class BlockWoodenPostLantern : BlockLantern
    {
        // Mesh-cache namespace per wood/bark/size so distinct combinations don't share
        // the lantern-attribute-keyed inner cache.
        public override string baseCacheKey
            => "blockWoodenPostLantern-"
                + this.Variant["wood"] + "-"
                + this.Variant["bark"] + "-"
                + this.Variant["size"];

        public override AssetLocation GetRotatedBlockCode(int angle)
        {
            BlockFacing oldFacing = BlockFacing.FromCode(this.Variant["horizontalorientation"]);
            BlockFacing newFacing = BlockFacing.HORIZONTALS_ANGLEORDER[((360 - angle) / 90 + oldFacing.HorizontalAngleIndex) % 4];
            return base.CodeWithParts(newFacing.Code);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            BELantern belantern = be as BELantern;
            if (belantern == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            ItemSlot active = byPlayer.InventoryManager.ActiveHotbarSlot;
            BlockLantern heldLantern = active?.Itemstack?.Collectible as BlockLantern;

            // Holding a lantern: refuse outright. Falling through to vanilla OmniAttachable
            // would let it search neighbouring cells for any valid attachment area (e.g., a
            // beam underside above), so the click on a post-hybrid would silently place a
            // second hanging lantern in a different spot. Consume the interaction so the
            // player has to detach (or click somewhere else) before placing another lantern.
            if (heldLantern != null)
            {
                return true;
            }

            // Empty hand (or non-lantern item): detach and return the lantern.
            DetachLantern(world, byPlayer, blockSel.Position, belantern);
            return true;
        }

        private void DetachLantern(IWorldAccessor world, IPlayer byPlayer, BlockPos pos, BELantern be)
        {
            // Server is authoritative for the swap and item return. Running on the
            // client would double up the inventory grant and the block exchange.
            if (world.Side != EnumAppSide.Server) return;

            ItemStack lanternStack = MakeLanternStack(world, be.material, be.lining, be.glass);

            string wood = this.Variant["wood"];
            string bark = this.Variant["bark"];
            AssetLocation postCode = new AssetLocation("postsandbeams", "woodenpost-" + wood + "-" + bark + "-empty");
            Block postBlock = world.GetBlock(postCode);
            if (postBlock == null) return;

            // SetBlock destroys the old BE, places the new block, and triggers neighbour
            // updates in one call. Per VS docs this is the correct API for swapping
            // between blocks with different entity classes.
            world.BlockAccessor.SetBlock(postBlock.BlockId, pos);

            if (lanternStack != null && !byPlayer.InventoryManager.TryGiveItemstack(lanternStack, true))
            {
                world.SpawnItemEntity(lanternStack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            if (Sounds?.Place != null)
            {
                world.PlaySoundAt(Sounds.Place, pos, 0.1, byPlayer, 1f);
            }
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            List<ItemStack> drops = new List<ItemStack>();

            BELantern be = world.BlockAccessor.GetBlockEntity(pos) as BELantern;
            string mat = be?.material ?? "copper";
            string lin = be?.lining ?? "plain";
            string glass = be?.glass ?? "quartz";

            ItemStack lantern = MakeLanternStack(world, mat, lin, glass);
            if (lantern != null) drops.Add(lantern);

            // Drop the post-ew variant (the placement item — that's what's in creative
            // inventory and what the player holds). Same convention as the regular beam
            // and the beam-hybrid.
            string wood = this.Variant["wood"];
            string bark = this.Variant["bark"];
            Block postBlock = world.GetBlock(new AssetLocation("postsandbeams", "woodenpost-" + wood + "-" + bark + "-ew"));
            if (postBlock != null) drops.Add(new ItemStack(postBlock));

            return drops.ToArray();
        }

        private ItemStack MakeLanternStack(IWorldAccessor world, string material, string lining, string glass)
            => LanternHybridHelpers.MakeLanternStack(world, this.Variant["size"], material, lining, glass);

        // Vanilla BlockLantern.OnBlockBroken spawns OnPickBlock (a single lantern stack) instead
        // of consulting GetDrops, so without this override the post never drops back. Reimplement
        // the standard break flow but spawn the GetDrops result (post + lantern).
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            bool preventDefault = false;
            foreach (BlockBehavior bh in BlockBehaviors)
            {
                EnumHandling handled = EnumHandling.PassThrough;
                bh.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier, ref handled);
                if (handled == EnumHandling.PreventDefault) preventDefault = true;
                if (handled == EnumHandling.PreventSubsequent) return;
            }
            if (preventDefault) return;

            if (world.Side == EnumAppSide.Server
                && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] drops = GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
                if (drops != null)
                {
                    foreach (ItemStack drop in drops)
                    {
                        world.SpawnItemEntity(drop, pos, null);
                    }
                }
                if (Sounds != null)
                {
                    world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos, -0.5, byPlayer, 1f);
                }
            }

            if (EntityClass != null)
            {
                BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
                if (be != null) be.OnBlockBroken(byPlayer);
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        // Vanilla BlockLantern.OnPickBlock does CodeWithParts("up") to produce the ground form
        // (e.g. lantern-small-north -> lantern-small-up). For our hybrid the last variant is
        // horizontalorientation, so that lookup yields a code that doesn't exist and the resulting
        // null block crashes the ItemStack ctor. Return the corresponding vanilla lantern stack
        // with the current BE attributes instead.
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            BELantern be = world.BlockAccessor.GetBlockEntity(pos) as BELantern;
            string mat = be?.material ?? "copper";
            string lin = be?.lining ?? "plain";
            string glass = be?.glass ?? "quartz";
            ItemStack stack = MakeLanternStack(world, mat, lin, glass);
            // Don't fall back to base.OnPickBlock — vanilla BlockLantern.OnPickBlock
            // does CodeWithParts("up") which assumes the last variant is a position
            // value (up/down/north/etc), and crashes for our horizontalorientation
            // variant. Return a self-stack as a safe non-crashing fallback instead.
            return stack ?? new ItemStack(this);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "postsandbeams:blockhelp-woodenpostlantern-detach",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }
    }
}
