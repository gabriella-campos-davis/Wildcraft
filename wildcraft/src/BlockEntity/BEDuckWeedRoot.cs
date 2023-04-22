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
using Vintagestory.GameContent;

namespace wildcraft
{
    public class BEDuckWeedRoot : BlockEntity
    {
        double totalHoursTillGrowth;
        long growListenerId;
        
        Block duckweedBlock;
        float swampyPoint = 18;
        float permaDuckweed = 32;
        Block block;

        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            
            block = api.World.BlockAccessor.GetBlock(Pos);

            if (api is ICoreServerAPI)
            {
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            ICoreServerAPI sapi = Api as ICoreServerAPI;
        }


        private void CheckGrow(float dt)
        {
            if (Api.World.Calendar.TotalHours < totalHoursTillGrowth)
                return;

            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            if (conds == null)
            {
                return;
            }

            BlockPos nPos = Pos.UpCopy(1);
            if (conds.Temperature > swampyPoint)
            {
                DoGrow();
                return;
            }


        }
        private void DoGrow()
        {
            Random rand = new Random();
            if (rand.Next(0, 10) > 8)
            {
                bool lakeTop = false;
                int currentDepth = 1;

                Block aboveBlock;

                while(lakeTop == false)
                {
                    aboveBlock = this.Api.World.BlockAccessor.GetBlock(Pos.UpCopy(currentDepth));

                    if (aboveBlock.LiquidCode != "water")
                    {
                        //if the block ontop of the lake is already duckweed we dont need to be here
                        if(aboveBlock.FirstCodePart() == "duckweed") this.Api.World.BlockAccessor.SetBlock(0, Pos);

                        Block placingBlock = this.Api.World.BlockAccessor.GetBlock(new AssetLocation("wildcraft:duckweed-duckweed-nesw"));
                        if (placingBlock == null)
                        {
                            this.Api.World.Logger.Chat("duckwwed root tried place block and it's null. returns");
                            return;
                        } 

                        this.Api.World.BlockAccessor.SetBlock(placingBlock.BlockId, Pos.UpCopy(currentDepth));
                        lakeTop = true;
                    }

                    currentDepth += 1;
                }
                this.Api.World.BlockAccessor.SetBlock(0, Pos);
            }
            
        }
    }
}
