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
    public class RhizomatusPlant : WildcraftPlant
    {
        public ICoreAPI Api => api;

     // Worldgen placement, attempt to place a rhizome in the soil beneath the plant
        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            // Grab the opposite face of the block
            //BlockPos rootPos = onBlockFace.IsHorizontal ? pos.AddCopy(onBlockFace) : pos.AddCopy(onBlockFace.Opposite);
            BlockPos rootPos = pos.AddCopy(onBlockFace.Opposite);

            var block = blockAccessor.GetBlock(rootPos);

            // If the block the plant is attached to isn't a mycelium host check the one below it
            if (!block.HasBehavior<BehaviorMyceliumHost>())
            {
                rootPos.Down();
                block = blockAccessor.GetBlock(rootPos);
                if (!block.HasBehavior<BehaviorMyceliumHost>()) return false;
            }
            
            // If all goes well attempt to spawn a rhizome in the soil
            blockAccessor.SpawnBlockEntity("BEWildcraftRhizome", rootPos);
            (blockAccessor.GetBlockEntity(rootPos) as BEWildcraftRhizome).OnGenerated(blockAccessor, worldGenRand, this);
            return true;
        }
    }
}