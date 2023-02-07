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
    public class AquaticPlant : WildcraftPlant
    {
        public ICoreAPI Api => api;
        int depth;


        public override void OnLoaded(ICoreAPI api)
        {
             base.OnLoaded(api);

            if (Variant["state"] == "harvested")
                return;

            if(Attributes["isPoisonous"].ToString() == "true") isPoisonous = true;

            depth = 15;

        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            return (block.Fertility > 0);
        }

        // Worldgen placement, tests to see how many blocks below water the plant is being placed, and if that's allowed for the plant

       public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            Block block = blockAccessor.GetBlock(pos);

            if (!block.IsReplacableBy(this))
            {
                return false;
            }

            Block belowBlock = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);

/*
            if (belowBlock.Fertility > 0)
            {
                Block placingBlock = blockAccessor.GetBlock(Code);
                if (placingBlock == null) return false;
                
                blockAccessor.SetBlock(placingBlock.BlockId, pos);
                return true;
            }
*/
            if (belowBlock.LiquidCode == "water")
            {
                for(var currentDepth = 0; currentDepth <= depth + 1; currentDepth ++)
                {
                    belowBlock = blockAccessor.GetBlock(pos.X, pos.Y - currentDepth, pos.Z);
                    if (belowBlock.Fertility > 0)
                    {
                        Block placingBlock = blockAccessor.GetBlock(Code);
                        if (placingBlock == null) return false;

                        blockAccessor.SetBlock(placingBlock.BlockId, pos.DownCopy(currentDepth - 1));
                        return true;
                    }
                }
            }

            return false;
        } 
    }
}
 
 
 