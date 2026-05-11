using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using PostsAndBeams.ModBlock;

namespace PostsAndBeams.ModBlockBehavior
{
    // Added to existing wooden post blocks. Lets the player right-click a post while
    // holding any vanilla lantern to swap it for the corresponding woodenpost-lantern
    // hybrid block, transferring the lantern's runtime attributes (material/lining/glass)
    // onto the new BE.
    public class BlockBehaviorLanternAttachable : BlockBehavior
    {
        public BlockBehaviorLanternAttachable(Block block) : base(block)
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

            // Match torchholder convention (BlockPost.OnBlockInteractStart): only swap
            // on empty posts. Connected posts (type != "empty") have visible connection
            // arms our hybrid block doesn't preserve. We CONSUME the interaction here
            // (rather than falling through) to block vanilla lantern OmniAttachable
            // from placing a "floating" lantern with a 4 px gap to the post face —
            // matches how vanilla HorizontalAttachable refuses torchholders at junctions.
            if (block.Variant["type"] != "empty")
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }

            // If a beam-hybrid sits directly above this post, its rendered lantern body
            // hangs into this cell and would visually collide with the post-hybrid we'd
            // create here. Refuse so the player isn't surprised by overlapping lanterns.
            Block aboveBlock = world.BlockAccessor.GetBlock(blockSel.Position.UpCopy());
            if (aboveBlock is BlockWoodenBeamLantern)
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }

            string size = heldLantern.Variant["size"];
            if (size == null) size = "small";

            string wood = block.Variant["wood"];
            string bark = block.Variant["bark"];

            // Variant name = post face the lantern is attached to. Our shapebytype rotateY
            // values in the hybrid blocktype JSON are tuned so this naming matches the
            // visible bracket position.
            BlockFacing side = blockSel.Face;
            if (side == null || !side.IsHorizontal)
            {
                BlockFacing[] suggestions = Block.SuggestedHVOrientation(byPlayer, blockSel);
                side = (suggestions != null && suggestions.Length > 0) ? suggestions[0] : BlockFacing.NORTH;
            }

            AssetLocation hybridCode = new AssetLocation(
                "postsandbeams",
                "woodenpost-lantern-" + wood + "-" + bark + "-" + size + "-" + side.Code);
            Block hybridBlock = world.GetBlock(hybridCode);
            if (hybridBlock == null)
            {
                // Consume the interaction rather than falling through. Vanilla lantern's
                // OmniAttachable would otherwise search neighbouring cells for any valid
                // attachment surface (e.g. a beam underside above), placing the lantern
                // somewhere unexpected. A missing hybrid variant (compat-mod wood without
                // a corresponding hybrid blocktype patch) should silently refuse, not
                // redirect.
                handling = EnumHandling.PreventDefault;
                return true;
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
