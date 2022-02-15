using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Util;


namespace wildcraft
{
    public class BlockEntityHerb : BlockEntity
    {
        static Random rand = new Random();

        double lastCheckAtTotalDays = 0;
        double transitionHoursLeft = -1;

        RoomRegistry roomreg;
        public int roomness;

        public BlockEntityHerb(){}

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

                RegisterGameTickListener(CheckGrow, 8000);
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();
            }
        }


        private void CheckGrow(float dt)
        {
            if (Block.Attributes == null)
                {
#if DEBUG
                Api.World.Logger.Notification("Ghost herb block entity at {0}. Block.Attributes is null, will remove game tick listener", Pos);
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

            bool changed = false;

            while (daysToCheck > 1f / Api.World.Calendar.HoursPerDay)
            {
                if (!changed)
                {
                    if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos) > Pos.Y) // Fast pre-check
                    {
                        Room room = roomreg?.GetRoomForPosition(Pos);
                        roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
                    }
                    else
                    {
                        roomness = 0;
                    }
                }

                changed = true;

                daysToCheck -= 1f / Api.World.Calendar.HoursPerDay;

                lastCheckAtTotalDays += 1f / Api.World.Calendar.HoursPerDay;
                transitionHoursLeft -= 1f;

                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDateValues, lastCheckAtTotalDays);
                if (conds == null) return;
                if (roomness > 0)
                {
                    conds.Temperature += 5;
                }

                bool reset = 
                    conds.Temperature < Block.Attributes["resetBelowTemperature"].AsFloat(-999) ||
                    conds.Temperature > Block.Attributes["resetAboveTemperature"].AsFloat(999)
                ;

                bool stop = 
                    conds.Temperature < Block.Attributes["stopBelowTemperature"].AsFloat(-999) ||
                    conds.Temperature > Block.Attributes["stopAboveTemperature"].AsFloat(999)
                ;

                bool revert = 
                    conds.Temperature < Block.Attributes["revertBlockBelowTemperature"].AsFloat(-999) ||
                    conds.Temperature > Block.Attributes["revertBlockAboveTemperature"].AsFloat(999)
                ;

                if (stop || reset)
                {
                    transitionHoursLeft += 1f;
                    
                    if (reset)
                    {
                        transitionHoursLeft = GetHoursForNextStage();
                        if (Block.Variant["state"] != "harvested" && revert)
                        {
                            Block nextBlock = Api.World.GetBlock(Block.CodeWithVariant("state", "harvested"));
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

        public double GetHoursForNextStage()
        {
            if (IsRipe()) return (4 * (5 + rand.NextDouble()) * 0.8) * Api.World.Calendar.HoursPerDay;

            return ((5 + rand.NextDouble()) * 0.8) * Api.World.Calendar.HoursPerDay;
        }

        public bool IsRipe()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            return block.LastCodePart() == "normal";
        }

        bool DoGrow()
        { 
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string nowCodePart = block.LastCodePart();
            string nextCodePart = (nowCodePart == "harvested") ? "normal" : "harvested";



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


        

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            transitionHoursLeft = tree.GetDouble("transitionHoursLeft");


            lastCheckAtTotalDays = tree.GetDouble("lastCheckAtTotalDays");

            roomness = tree.GetInt("roomness");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("transitionHoursLeft", transitionHoursLeft);
            tree.SetDouble("lastCheckAtTotalDays", lastCheckAtTotalDays);

            tree.SetInt("roomness", roomness);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            base.GetBlockInfo(forPlayer, sb);
            double daysleft = transitionHoursLeft / Api.World.Calendar.HoursPerDay;

            if (block.LastCodePart() == "normal")
            {
                return;
            }

            if (daysleft < 1)
            {
                sb.AppendLine(Lang.Get("Will grow in less than a day, weather permitting"));
            }
            else
            {
                sb.AppendLine(Lang.Get("Will grow in {0} days, weather permitting", (int)daysleft));
            }

            if (roomness > 0)
            {
                sb.AppendLine(Lang.Get("greenhousetempbonus"));
            }
        }
    }
}