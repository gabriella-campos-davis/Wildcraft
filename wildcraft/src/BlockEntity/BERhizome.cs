using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace wildcraft
{
    public class BERhizome : BlockEntity
    {
        Block plantBlock;
        string plantType;
        long growListenerId;


        RoomRegistry roomreg;
        public int roomness;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            api.World.Logger.Chat("BERhizome initialized!!");
            api.World.Logger.Chat(api.World.BlockAccessor.GetBlock(Pos).Code.ToString());

            if (api is ICoreServerAPI)
            {
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        public override void OnBlockPlaced(ItemStack rootStack)
        {
            plantType = rootStack.Item.Variant["type"].ToString();
            this.Api.World.Logger.Chat(plantType);
        }

        private void CheckGrow(float dt)
        {
            this.Api.World.Logger.Chat("CheckGrow");
            return;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            tree.SetFloat("timer", growListenerId);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            growListenerId = tree.GetLong("timer");
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            
            dsc.AppendLine("abcdefg");
        }
    }
}