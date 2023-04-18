using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using System.Linq;
using System;
using Vintagestory.API.Common.Entities;
using wildcraft.config;
using wildcraft;

namespace wildcraft
{
    public class WildcraftWater : BlockWater
    {
        bool swampy;
        Block duckweedBlock;
        float swampyPoint = 18;
        float freezingPoint = -4;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            swampy = Flow == "still" && Height == 7;
            if (Attributes != null)
            {
                swampy &= Attributes["swampy"].AsBool(true);

                duckweedBlock = api.World.GetBlock(AssetLocation.Create("wildcraft:duckweed-duckweed-nesw"));
                swampyPoint = Attributes["swampyPoint"].AsFloat(18);
                freezingPoint = Attributes["freezingPoint"].AsFloat(-4);
            }
            else
            {
                duckweedBlock = api.World.GetBlock(AssetLocation.Create("wildcraft:duckweed-duckweed-nesw"));
            }
        }
        public override bool ShouldReceiveServerGameTicks(IWorldAccessor world, BlockPos pos, Random offThreadRandom, out object extra)
        {
            extra = null;
            BlockPos nPos = pos.UpCopy(1);

            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds.Temperature >= swampyPoint && world.BlockAccessor.GetBlock(nPos) is not DuckWeed)
            {
                if (!GlobalConstants.MeltingFreezingEnabled) return false;

                if (swampy && offThreadRandom.NextDouble() < 0.6)
                {
                    int rainY = world.BlockAccessor.GetRainMapHeightAt(pos);
                    if (rainY <= pos.Y)
                    {
                        for (int i = 0; i < BlockFacing.HORIZONTALS.Length; i++)
                        {
                            BlockFacing.HORIZONTALS[i].IterateThruFacingOffsets(nPos);

                            if (world.BlockAccessor.GetBlock(nPos) is not DuckWeed || world.BlockAccessor.GetBlock(nPos).Replaceable < 6000)
                            {
                                if (conds != null && conds.Temperature >= swampyPoint && conds.Rainfall >= 0.6)
                                {
                                    if (world.BlockAccessor.GetBlock(pos.X, pos.Y - 5, pos.Z, BlockLayersAccess.Fluid).Id != 0 || world.BlockAccessor.GetBlock(pos).LiquidCode != "water") return false;
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            } 
            else if (conds.Temperature <= freezingPoint)
            {
                return base.ShouldReceiveServerGameTicks(world, pos, offThreadRandom, out extra);
            }
            else return false;
        }

        public override void OnServerGameTick(IWorldAccessor world, BlockPos pos, object extra = null)
        {
            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds.Temperature >= swampyPoint){
                world.BlockAccessor.SetBlock(duckweedBlock.Id, pos.UpCopy(1));
            }

            else if (conds.Temperature <= freezingPoint){
                base.OnServerGameTick(world, pos, extra = null);
            }
        }
    }
}
