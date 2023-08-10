using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using Vintagestory.API.Common.Entities;
using wildcraft.config;
using wildcraft;

namespace wildcraft
{
    public class WildcraftMoss : BlockPlant
    {
        Block darkMoss;
        Block lightMoss;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            
            darkMoss = api.World.BlockAccessor.GetBlock(new AssetLocation("wildcraft:moss-d-dark-sporangia-free"));
            lightMoss = api.World.BlockAccessor.GetBlock(new AssetLocation("wildcraft:moss-d-bright-sporangia-free"));
        }
        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            ClimateCondition baseClimate = api.World.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.WorldGenValues);

            if (baseClimate.Rainfall > 0.5)
            {
                if (baseClimate.Rainfall > 0.8)
                {
                    api.World.BlockAccessor.SetBlock(darkMoss.Id, pos);
                    return true;
                }

                api.World.BlockAccessor.SetBlock(lightMoss.Id, pos);
                return true;
            }

            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand);
        }
    }
}
 
 
 