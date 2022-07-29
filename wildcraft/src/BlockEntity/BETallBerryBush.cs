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
        public bool DoGrow()
        {
            if (Api.World.BlockAccessor.GetBlock(this.Pos.X,this.Pos.Y + 1, this.Pos.Z).BlockId == 0 && Api.World.BlockAccessor.GetBlock(this.Pos.X,this.Pos.Y - 1, this.Pos.Z).Code != this.Block.Code){
                Block clipping = Api.World.BlockAccessor.GetBlock(AssetLocation.Create("wildcraft:clipping-" + this.Block.Variant + "-alive"));
                Api.World.BlockAccessor.SetBlock(clipping.BlockId, Pos);
            }

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