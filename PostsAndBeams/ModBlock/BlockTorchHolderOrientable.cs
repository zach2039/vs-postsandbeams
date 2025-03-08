using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlock
{
    // Allows particles to be properly offset on wooden post torch holder
    public class BlockTorchHolderOrientable : BlockTorchHolder
    {
        public override AssetLocation GetRotatedBlockCode(int angle)
        {
            BlockFacing oldFacing = BlockFacing.FromCode(this.Variant["horizontalorientation"]);
            BlockFacing newFacing = BlockFacing.HORIZONTALS_ANGLEORDER[((360 - angle) / 90 + oldFacing.HorizontalAngleIndex) % 4];
            return base.CodeWithParts(newFacing.Code);
        }
    }
}
