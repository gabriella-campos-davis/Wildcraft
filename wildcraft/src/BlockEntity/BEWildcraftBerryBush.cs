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
    public class BEWildcraftBerryBush : BlockEntity, IAnimalFoodSource
    {
        public string[] states = {"barren", "empty", "flowering", "ripe"};
        public string[] stages = {"child", "teen", "adult", "mature", "ancient"};

        public string state = "empty";
        public string stage = 0;

        public bool isPruned = false;

        public int barrenBelowTemperature; //if below this temp, revert the berry bush to a 'barren' state.
        public int barrenAboveTemperature;

        public int emptyBelowTemperature; //if below this temperature, revet the berry bush to an 'empty' state
        public int emptyAboveTemperature;

        public int stopGrowingBelowTemperature; //if below this temperature, pause growth timers
        public int stopGrowingAboveTemperature;

        public int sproutAboveTemperature; //if above this tem

        public double sproutGrowthHoursLeft = -1; //sprout for states.
        public double checkSproutGrowthDays;

        public double bushGrowthHoursLeft = -1; //bush for stages, and for prune growth.
        public double checkBushGrowthDays;

        RoomRegistry roomreg;
        public int roomness;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                if (sproutGrowthHoursLeft <= 0)
                {
                    sproutGrowthHoursLeft = GetHoursForNextState();
                    checkSproutGrowthDays = api.World.Calendar.TotalDays;
                }

                if (bushGrowthHoursLeft <= 0)
                {
                    bushGrowthHoursLeft = GetHoursForNextStage();
                    checkBushGrowthDays = api.World.Calendar.TotalDays;
                }

                if (Api.World.Config.GetBool("processCrops", true))
                {
                    RegisterGameTickListener(CheckSproutGrowth, 8000); //these two could be configurable (?)
                    RegisterGameTickListener(CheckBushGrowth, 16000); //this is a random number and needs to be changed

                }

                api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();
            }
        }

        public double GetHoursForNextState()
        {
            if (IsRipe()) return (4 * (5 + rand.NextDouble()) * 0.8) * Api.World.Calendar.HoursPerDay;

            return ((5 + rand.NextDouble()) * 0.8) * Api.World.Calendar.HoursPerDay;
        }

        public bool IsRipe()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            return block.state == "ripe";
        }

        public double GetHoursForNextStage()
        {
            return Api.World.Calendar.DaysPerYear * Api.World.Calendar.HoursPerDay;
        }



        private void CheckSproutGrowth(float dt)
        {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

            if (Block.Attributes == null)
                {
#if DEBUG
                Api.World.Logger.Notification("Ghost berry bush block entity at {0}. Block.Attributes is null, will remove game tick listener", Pos);
                foreach (long handlerId in TickHandlers)
                {
                    Api.Event.UnregisterGameTickListener(handlerId);
                }
#endif
                return;
            }

            // In case this block was imported from another older world. In that case checkSproutGrowthDays would be a future date.
            checkSproutGrowthDays = Math.Min(checkSproutGrowthDays, Api.World.Calendar.TotalDays);


            // We don't need to check more than one year because it just begins to loop then
            double daysToCheck = GameMath.Mod(Api.World.Calendar.TotalDays - checkSproutGrowthDays, Api.World.Calendar.DaysPerYear);

            ClimateCondition baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            if (baseClimate == null) return;
            float baseTemperature = baseClimate.Temperature;

            bool changed = false;
            float oneHour = 1f / Api.World.Calendar.HoursPerDay; 
            float resetBelowTemperature = 0, resetAboveTemperature = 0, stopBelowTemperature = 0, stopAboveTemperature = 0, revertBlockBelowTemperature = 0, revertBlockAboveTemperature = 0;
            if (daysToCheck > oneHour)
            {
                stopGrowingBelowTemperature = Block.Attributes["stopGrowingBelowTemperature".AsFloat(-999)];
                stopGrowingAboveTemperature = block.Attributes["stopGrowingAboveTemperature"].AsFloat(999);

                emptyBelowTemperature = block.Attributes["emptyBelowTemperature"].AsFloat(-999);
                emptyAboveTemperature = block.Attributes["emptyAboveTemperature"].AsFloat(999);

                barrenBelowTemperature = block.Attributes["barrenBelowTemperature"].AsFloat(-999);
                barrenAboveTemperature = block.Attributes["barrenAboveTemperature"].AsFloat(999);

                sproutAboveTemperature = Block.Attributes["sproutAboveTemperature"].AsFloat(-999);

                if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos) > Pos.Y) // Fast pre-check
                {
                    Room room = roomreg?.GetRoomForPosition(Pos);
                    roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
                }
                else
                {
                    roomness = 0;
                }

                changed = true;
            }

            while (daysToCheck > oneHour)
            {
                daysToCheck -= oneHour;
                sproutGrowthHoursLeft += oneHour;
                checkSproutGrowthDays -= 1f;

                baseClimate.Temperature = baseTemperature;
                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, checkSproutGrowthDays);
                if (roomness > 0)
                {
                    conds.Temperature += 5;
                }

                bool stop =
                    conds.Temperature < stopGrowingBelowTemperature ||
                    conds.Temperature > stopGrowingAboveTemperature;

                bool empty =
                    conds.Temperature < emptyBelowTemperature ||
                    conds.Temperature > emptyAboveTemperature;
                
                bool barren = 
                    conds.Temperature < barrenBelowTemperature ||
                    conds.Temperature > barrenAboveTemperature;

                if (stop || empty)
                {
                    sproutGrowthHoursLeft += 1f;
                    
                    if (empty)
                    {
                        sproutGrowthHoursLeft = GetHoursForNextState();
                        if (Block.state != "empty")
                        {
                            Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "empty"));//needs updating
                            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
                        }
                    }

                    if (barren)
                    {
                        sproutGrowthHoursLeft = GetHoursForNextState();
                        if (Block.state == "empty")
                        {
                            Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "barren"));//needs updating
                            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
                        }
                    }

                    continue;
                }

                if (sproutGrowthHoursLeft <= 0)
                {
                    if (!DoSproutGrow()) return;
                    sproutGrowthHoursLeft = GetHoursForNextState();
                }
            }

            if (changed) MarkDirty(false);
        }

        
        private void CheckBushGrowth(float dt)
        {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

            if (Block.Attributes == null)
                {
#if DEBUG
                Api.World.Logger.Notification("Ghost berry bush block entity at {0}. Block.Attributes is null, will remove game tick listener", Pos);
                foreach (long handlerId in TickHandlers)
                {
                    Api.Event.UnregisterGameTickListener(handlerId);
                }
#endif
                return;
            }

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string nowCodePart = block.LastCodePart();

            if(nowCodePart == "ancient")
            {
                 Api.Event.UnregisterGameTickListener("CheckBushGrowth");
                 return;
            }

            // In case this block was imported from another older world. In that case checkBushGrowthDays would be a future date.
            checkBushGrowthDays = Math.Min(checkBushGrowthDays, Api.World.Calendar.TotalDays);


            // We don't need to check more than one year because it just begins to loop then
            double daysToCheck = GameMath.Mod(Api.World.Calendar.TotalDays - checkBushGrowthDays, Api.World.Calendar.DaysPerYear);

            ClimateCondition baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            if (baseClimate == null) return;
            float baseTemperature = baseClimate.Temperature;

            bool changed = false;
            float oneHour = 1f / Api.World.Calendar.HoursPerDay; 
            float resetBelowTemperature = 0, resetAboveTemperature = 0, stopBelowTemperature = 0, stopAboveTemperature = 0, revertBlockBelowTemperature = 0, revertBlockAboveTemperature = 0;
            if (daysToCheck > oneHour)
            {
                sproutAboveTemperature = Block.Attributes["sproutAboveTemperature"].AsFloat(-999);

                if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos) > Pos.Y) // Fast pre-check
                {
                    Room room = roomreg?.GetRoomForPosition(Pos);
                    roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
                }
                else
                {
                    roomness = 0;
                }

                changed = true;
            }

            while (daysToCheck > oneHour)
            {
                daysToCheck -= oneHour;
                bushGrowthHoursLeft += oneHour;
                checkBushGrowthDays -= 1f;

                baseClimate.Temperature = baseTemperature;
                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, checkBushGrowthDays);
                if (roomness > 0)
                {
                    conds.Temperature += 5;
                }

                bool growBush = conds.Temperature > sproutAboveTemperature;

                if (growBush)
                {
                    bushGrowthHoursLeft += 1f;
                    bushGrowthHoursLeft = GetHoursForNextState();

                    if (Block.stage < 4)
                    {
                        Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("stage", stages[stage]));//needs updating
                        Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
                    }
                }
                
                continue;
            }

            if (bushGrowthHoursLeft <= 0)
            {
                 if (!DoBushGrow()) return;
                bushGrowthHoursLeft = GetHoursForNextState();
            }

            if (changed) MarkDirty(false);
        }


        bool DoBushGrow()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string nowCodePart = block.LastCodePart();
            string nextCodePart;
            switch (nowCodePart) {
                case "child": nextCodePart = "teen"; break;
                case "teen": nextCodePart = "adult"; break;
                case "adult": nextCodePart = "mature"; break;
                case "mature": nextCodePart = "ancient"; break;
                default: Api.World.Logger.Notification("No berry bush age detcted.", Pos); return; break;
            }

            AssetLocation loc = block.CodeWithParts(nextCodePart);
            if (!loc.Valid)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return false;
            }

            Block nextBlock = Api.World.GetBlock(loc);
            if (nextBlock?.Code == null) return false;

            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);

            MarkDirty(true);
            return true;
        }

        bool DoSproutGrow()
        {
            if (Api.World.Calendar.TotalDays - LastPrunedTotalDays > Api.World.Calendar.DaysPerYear)
            {
                Pruned = false;
            }

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string nowCodePart = block.LastCodePart();
            string nextCodePart = (nowCodePart == "empty") ? "flowering" : ((nowCodePart == "flowering") ? "ripe" : "empty");


            AssetLocation loc = block.CodeWithParts(nextCodePart);
            if (!loc.Valid)
            {
                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                return false;
            }

            Block nextBlock = Api.World.GetBlock(loc);
            if (nextBlock?.Code == null) return false;

            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);

            MarkDirty(true);
            return true;
        }

    }
}