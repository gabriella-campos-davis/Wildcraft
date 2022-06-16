using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace wildcraft
{
    public class BlockEntitySeedling : BlockEntity
    {
        double totalHoursTillGrowth;
        long growListenerId;
        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        NatFloat nextStageDaysRnd
        {
            get
            {
                NatFloat matureDays = NatFloat.create(EnumDistribution.UNIFORM, 7f, 2f);
                if (Block?.Attributes != null)
                {
                    return Block.Attributes["matureDays"].AsObject(matureDays);
                }
                return matureDays;
            }
        }

        float GrowthRateMod => Api.World.Config.GetString("saplingGrowthRate").ToFloat(1);

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            totalHoursTillGrowth = Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, Api.World.Rand) * 24 * GrowthRateMod;
        }


        private void CheckGrow(float dt)
        {
            if (Api.World.Calendar.TotalHours < totalHoursTillGrowth)
                return;

            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            if (conds == null || conds.Temperature < 5)
            {
                return;
            }

            if (conds.Temperature < 0)
            {
                totalHoursTillGrowth = Api.World.Calendar.TotalHours + (float)Api.World.Rand.NextDouble() * 72 * GrowthRateMod;
                return;
            }
            ICoreServerAPI sapi = Api as ICoreServerAPI;

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block == null){
                return;
            }

            string herbtype = this.Block.Variant["wildflora"].ToString();
            if(herbtype == null){
                return;
            }

            Block herbBlock;
            if(herbtype == "waterchestnut"){
                herbBlock = Api.World.GetBlock(AssetLocation.Create("wildcraft:waterplant-" + herbtype + "-land-harvested-free", "wildcraft"));
            } else {
                herbBlock= Api.World.GetBlock(AssetLocation.Create("wildcraft:leafygroundvegetable-" + herbtype + "-harvested", "wildcraft"));
            }

            Api.World.BlockAccessor.SetBlock(herbBlock.BlockId, Pos);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("totalHoursTillGrowth", totalHoursTillGrowth);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            totalHoursTillGrowth = tree.GetDouble("totalHoursTillGrowth", 0);
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            double hoursleft = totalHoursTillGrowth - Api.World.Calendar.TotalHours;
            double daysleft = hoursleft / Api.World.Calendar.HoursPerDay;

            if (daysleft <= 1)
            {
                dsc.AppendLine(Lang.Get("Will grow in less than a day"));
            }
            else
            {
                dsc.AppendLine(Lang.Get("Will grow in about {0} days", (int)daysleft));
            }
        }
    }
}
