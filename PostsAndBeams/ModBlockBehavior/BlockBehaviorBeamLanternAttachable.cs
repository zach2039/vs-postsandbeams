using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using PostsAndBeams.ModBlock;

namespace PostsAndBeams.ModBlockBehavior
{
    // Added to existing wooden beam blocks. Right-click the BEAM'S UNDERSIDE while holding
    // a vanilla lantern to swap it for the bottom-hanging hybrid block. Other faces fall
    // through to vanilla OmniAttachable (lantern hangs in adjacent cell).
    public class BlockBehaviorBeamLanternAttachable : BlockBehavior
    {
        public BlockBehaviorBeamLanternAttachable(Block block) : base(block)
        {
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            ItemSlot active = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemStack heldStack = active?.Itemstack;
            BlockLantern heldLantern = heldStack?.Collectible as BlockLantern;
            if (heldLantern == null)
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            }

            // Only intercept the underside-of-beam click. Top + sides fall through to
            // vanilla lantern OmniAttachable.
            if (blockSel.Face != BlockFacing.DOWN)
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            }

            // The beam-hybrid renders its lantern body in the cell directly below the
            // beam. If that cell has another block whose geometry occupies the upper
            // portion (post-hybrid lantern, torchholder bracket+torch), the body would
            // visually overlap with that geometry. Refuse the swap in those cases.
            Block belowBlock = world.BlockAccessor.GetBlock(blockSel.Position.DownCopy());
            if (belowBlock is BlockWoodenPostLantern || belowBlock is BlockTorchHolderOrientable)
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }

            string size = heldLantern.Variant["size"];
            if (size == null) size = "small";

            string wood = block.Variant["wood"];
            string bark = block.Variant["bark"];
            string orientation = block.Variant["orientation"]; // "ns" or "we"

            AssetLocation hybridCode = new AssetLocation(
                "postsandbeams",
                "woodenbeam-lantern-" + wood + "-" + bark + "-" + orientation + "-" + size);
            Block hybridBlock = world.GetBlock(hybridCode);
            if (hybridBlock == null)
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            }

            string material = heldStack.Attributes.GetString("material", "copper");
            string lining = heldStack.Attributes.GetString("lining", "plain");
            string glass = heldStack.Attributes.GetString("glass", "quartz");

            // Block placement + BE attributes: both sides. Client predicts visually so
            // the lantern doesn't briefly render with default (copper) attributes while
            // waiting for the server to sync DidPlace.
            world.BlockAccessor.SetBlock(hybridBlock.BlockId, blockSel.Position);
            BELantern be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELantern;
            if (be != null)
            {
                be.DidPlace(material, lining, glass);
                be.MarkDirty(true);
            }

            // Inventory consume + sound: server-authoritative.
            if (world.Side == EnumAppSide.Server)
            {
                if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    active.TakeOut(1);
                    active.MarkDirty();
                }

                BlockSounds sounds = heldLantern.Sounds ?? block.Sounds;
                if (sounds?.Place != null)
                {
                    world.PlaySoundAt(sounds.Place, blockSel.Position, 0.1, byPlayer, 1f);
                }
            }

            handling = EnumHandling.PreventDefault;
            return true;
        }
    }
}
