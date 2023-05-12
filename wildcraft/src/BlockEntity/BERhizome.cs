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
        public Block plantBlock;
        public string rootType = "";
        long growListenerId;

        public ItemStack root {get; set;}

        RoomRegistry roomreg;
        public int roomness;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.Block.EntityClass = "BERhizome";

            if (api is ICoreServerAPI)
            {
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();
                growListenerId = RegisterGameTickListener(CheckGrow, 2000);
            }
        }

        public override void OnBlockPlaced(ItemStack rootStack)
        {
            rootType = rootStack.Item.Variant["type"].ToString();
            root = rootStack;
            //this.Block.Attributes[].ToAttribute.SetString("timer", growListenerId);
        }

        

    public override void OnBlockBroken(IPlayer byPlayer = null)
    {
        //SpawnItemEntity(ItemStack itemstack, Vec3d position, Vec3d velocity = null)
    }



        private void DoGrow()
        {

        }

        private void CheckGrow(float dt)
        {
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

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)        
        {
            this.Api.World.Logger.Chat(rootType);
            sb.AppendLine(Lang.Get("Contains {0} roots", rootType));
        }
    }
}