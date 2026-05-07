using Vintagestory.API.Common;

namespace PostsAndBeams.ModBlock
{
    // Shared helpers for lantern hybrid blocks (BlockWoodenPostLantern,
    // BlockWoodenBeamLantern). Extracted to avoid copy-pasting MakeLanternStack
    // between the two block classes.
    internal static class LanternHybridHelpers
    {
        // Construct a vanilla lantern itemstack of the given size carrying the supplied
        // material/lining/glass attributes. Mirrors how vanilla recipes produce lantern
        // outputs. Returns null if the vanilla block can't be resolved.
        public static ItemStack MakeLanternStack(IWorldAccessor world, string size, string material, string lining, string glass)
        {
            // Match attribute defaults applied below — without this, a null size silently
            // returns null and the caller's drop is lost.
            size = size ?? "small";

            Block lanternBlock = world.GetBlock(new AssetLocation("game", "lantern-" + size + "-up"));
            if (lanternBlock == null) return null;

            ItemStack stack = new ItemStack(lanternBlock);
            stack.Attributes.SetString("material", material ?? "copper");
            stack.Attributes.SetString("lining", lining ?? "plain");
            stack.Attributes.SetString("glass", glass ?? "quartz");
            return stack;
        }
    }
}
