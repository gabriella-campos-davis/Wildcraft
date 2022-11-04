 using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using Vintagestory.API.Common.Entities;

namespace wildcraft
{
    public class WaterPlant : BlockPlant
    {
        public ICoreAPI Api => api;

     // Worldgen placement, attempt to place a rhizome in the soil beneath the plant
     public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            Block block = blockAccessor.GetBlock(pos);

            if (!block.IsReplacableBy(this))
            {
                return false;
            }

            Block belowBlock = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            if (belowBlock.Fertility > 0)
            {
                Block placingBlock = blockAccessor.GetBlock(CodeWithVariant("habitat", "land"));
                if (placingBlock == null) return false;
                blockAccessor.SetBlock(placingBlock.BlockId, pos);
                return true;
            }

            if (belowBlock.LiquidCode == "water")
            {
                belowBlock = blockAccessor.GetBlock(pos.X, pos.Y - 2, pos.Z);
                if (belowBlock.Fertility > 0)
                {
                    Block placingBlock = blockAccessor.GetBlock(CodeWithVariant("habitat", "land"));
                    if (placingBlock == null) return false;
                    blockAccessor.SetBlock(placingBlock.BlockId, pos.DownCopy());
                    return true;
                }
            }


            return false;
        }   
    }
}
 
 
 
 