using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlock
{
    // A wooden beam fused with a bottom-hanging lantern. The block's own shape is the
    // beam (referenced from existing beam shapes). The custom BE renders a chain + the
    // vanilla lantern body suspended below the beam at runtime.
    //
    // Unlike BlockWoodenPostLantern, this class does NOT subclass BlockLantern — the
    // block's static shape (a beam) doesn't need the BlockLantern texture indexer, and
    // BELantern's vanilla rendering path is fully overridden by BEWoodenBeamLantern.
    public class BlockWoodenBeamLantern : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            BELantern belantern = be as BELantern;
            if (belantern == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            ItemSlot active = byPlayer.InventoryManager.ActiveHotbarSlot;
            BlockLantern heldLantern = active?.Itemstack?.Collectible as BlockLantern;

            // Holding a lantern: refuse outright. Falling through to vanilla OmniAttachable
            // would let it search neighbouring cells for any valid attachment area, so the
            // click on a beam-hybrid could silently place a second lantern in a different
            // spot. Consume so the player has to detach first.
            if (heldLantern != null)
            {
                return true;
            }

            // Empty hand (or non-lantern item): detach.
            DetachLantern(world, byPlayer, blockSel.Position, belantern);
            return true;
        }

        private void DetachLantern(IWorldAccessor world, IPlayer byPlayer, BlockPos pos, BELantern be)
        {
            // Server is authoritative for the swap and item return.
            if (world.Side != EnumAppSide.Server) return;

            ItemStack lanternStack = MakeLanternStack(world, be.material, be.lining, be.glass);

            string wood = this.Variant["wood"];
            string bark = this.Variant["bark"];
            string orientation = this.Variant["orientation"];
            AssetLocation beamCode = new AssetLocation("postsandbeams", "woodenbeam-" + wood + "-" + bark + "-" + orientation);
            Block beamBlock = world.GetBlock(beamCode);
            if (beamBlock == null) return;

            // SetBlock destroys the old BE, places the new block, and triggers neighbour
            // updates in one call. Per VS docs this is the correct API for swapping
            // between blocks with different entity classes.
            world.BlockAccessor.SetBlock(beamBlock.BlockId, pos);

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

            // Beams in this mod aren't directly placeable — they're created by clicking
            // with a `woodenpost-{wood}-{bark}-ew` item, which BlockPost.OnHeldInteractStart
            // converts into a beam. Regular beams' NWOrientableCustomDrops therefore drops
            // that post-ew item, not the beam block itself. Match that convention so cascade
            // and direct breaks return an item the player can actually re-place.
            string wood = this.Variant["wood"];
            string bark = this.Variant["bark"];
            Block postEwBlock = world.GetBlock(new AssetLocation("postsandbeams", "woodenpost-" + wood + "-" + bark + "-ew"));
            if (postEwBlock != null) drops.Add(new ItemStack(postEwBlock));

            return drops.ToArray();
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            BELantern be = world.BlockAccessor.GetBlockEntity(pos) as BELantern;
            string mat = be?.material ?? "copper";
            string lin = be?.lining ?? "plain";
            string glass = be?.glass ?? "quartz";
            ItemStack stack = MakeLanternStack(world, mat, lin, glass);
            if (stack != null) return stack;
            return base.OnPickBlock(world, pos);
        }

        private ItemStack MakeLanternStack(IWorldAccessor world, string material, string lining, string glass)
            => LanternHybridHelpers.MakeLanternStack(world, this.Variant["size"], material, lining, glass);

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "postsandbeams:blockhelp-woodenbeamlantern-detach",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }
    }
}
