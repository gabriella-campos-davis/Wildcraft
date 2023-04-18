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
    public class BETallBerryBush : BEWildcraftBerryBush
    {

        double lastCheckAtTotalDays = 0;
        double transitionHoursLeft = -1;

        double? totalDaysForNextStageOld = null; // old v1.13 data format, here for backwards compatibility

        RoomRegistry roomreg;
        new public int roomness;
        
        public BETallBerryBush() : base()
        {

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                if (transitionHoursLeft <= 0)
                {
                    transitionHoursLeft = GetHoursForNextStage();
                    lastCheckAtTotalDays = api.World.Calendar.TotalDays;
                }

                if (Api.World.Config.GetBool("processCrops", true))
                {
                    RegisterGameTickListener(CheckGrow, 8000);
                }

                api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();

                if (totalDaysForNextStageOld != null)
                {
                    transitionHoursLeft = ((double)totalDaysForNextStageOld - Api.World.Calendar.TotalDays) * Api.World.Calendar.HoursPerDay;
                }
            }
        }

        public void CheckGrow(float dt)
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

            // In case this block was imported from another older world. In that case lastCheckAtTotalDays would be a future date.
            lastCheckAtTotalDays = Math.Min(lastCheckAtTotalDays, Api.World.Calendar.TotalDays);


            // We don't need to check more than one year because it just begins to loop then
            double daysToCheck = GameMath.Mod(Api.World.Calendar.TotalDays - lastCheckAtTotalDays, Api.World.Calendar.DaysPerYear);

            ClimateCondition baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            if (baseClimate == null) return;
            float baseTemperature = baseClimate.Temperature;

            bool changed = false;
            float oneHour = 1f / Api.World.Calendar.HoursPerDay;
            float resetBelowTemperature = 0, resetAboveTemperature = 0, stopBelowTemperature = 0, stopAboveTemperature = 0, revertBlockBelowTemperature = 0, revertBlockAboveTemperature = 0;
            if (daysToCheck > oneHour)
            {
                resetBelowTemperature = Block.Attributes["resetBelowTemperature"].AsFloat(-999);
                resetAboveTemperature = Block.Attributes["resetAboveTemperature"].AsFloat(999);
                stopBelowTemperature = Block.Attributes["stopBelowTemperature"].AsFloat(-999);
                stopAboveTemperature = Block.Attributes["stopAboveTemperature"].AsFloat(999);
                revertBlockBelowTemperature = Block.Attributes["revertBlockBelowTemperature"].AsFloat(-999);
                revertBlockAboveTemperature = Block.Attributes["revertBlockAboveTemperature"].AsFloat(999);

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
                lastCheckAtTotalDays += oneHour;
                transitionHoursLeft -= 1f;

                baseClimate.Temperature = baseTemperature;
                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, lastCheckAtTotalDays);
                if (roomness > 0)
                {
                    conds.Temperature += 5;
                }

                bool reset =
                    conds.Temperature < resetBelowTemperature ||
                    conds.Temperature > resetAboveTemperature;

                bool stop =
                    conds.Temperature < stopBelowTemperature ||
                    conds.Temperature > stopAboveTemperature;
                
                bool revert = 
                    conds.Temperature < revertBlockBelowTemperature ||
                    conds.Temperature > revertBlockAboveTemperature;

                if (stop || reset)
                {
                    transitionHoursLeft += 1f;
                    
                    if (reset)
                    {
                        transitionHoursLeft = GetHoursForNextStage();
                        if (revert && Block.Variant["state"] != "empty")
                        {
                            Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "empty"));
                            Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, Pos);
                        }
                        

                    }

                    continue;
                }

                if (transitionHoursLeft <= 0)
                {
                    if (!DoGrow()) return;
                    transitionHoursLeft = GetHoursForNextStage();
                }
            }

            if (changed) MarkDirty(false);
        }

        public bool DoGrow()
        {
            if (Api.World.BlockAccessor.GetBlock(Pos.X, Pos.Y + 1, Pos.Z).BlockMaterial == 0 && 
                !(Api.World.BlockAccessor.GetBlock(Pos.X, Pos.Y - 1, Pos.Z) is WildcraftBerryBush))
            {
                Block clipping = Api.World.BlockAccessor.GetBlock(AssetLocation.Create("wildcraft:growth-" + this.Block.Variant["type"] + "-alive"));
                if(clipping is null) {
                    throw new ArgumentNullException(nameof(clipping), "BETallBerryBush growth is Null. Exiting");
                }
                Api.World.BlockAccessor.SetBlock(clipping.BlockId, Pos.AddCopy(0, 1, 0));
            }

            if (Api.World.Calendar.TotalDays - LastPrunedTotalDays > Api.World.Calendar.DaysPerYear)
            {
                Pruned = false;
            }

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string nowCodePart = block.LastCodePart();
            string nextCodePart = (nowCodePart == "empty") ? "flowering" : ((nowCodePart == "flowering") ? "ripe" : "empty");

            if(block is null) return false;
            AssetLocation loc = block.CodeWithPart(nextCodePart, 1);

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