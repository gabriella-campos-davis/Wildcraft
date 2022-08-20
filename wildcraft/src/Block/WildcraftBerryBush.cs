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
    public class WildcraftBerryBush : BlockBerryBush
    {
        ItemStack clipping = new();
        
        public AssetLocation harvestingSound;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            clipping = new ItemStack(api.World.GetItem(AssetLocation.Create("wildcraft:clipping-" + this.Variant["type"] + "-green")), 1);

            string code = "game:sounds/block/leafy-picking";
            if (code != null) {
                harvestingSound = AssetLocation.Create(code, this.Code.Domain);
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Knife && WildcraftConfig.Current.useKnifeForClipping) ||
                (byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool == EnumTool.Shears && WildcraftConfig.Current.useShearsForClipping))
            {

                if(clipping == null){
                    return false;
                }

                if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWildcraftBerryBush bewcbush) bewcbush.Prune();

                clipping = new ItemStack(api.World.GetItem(AssetLocation.Create("wildcraft:clipping-" + this.Variant["type"] + "-green")), 1);
                if (byPlayer?.InventoryManager.TryGiveItemstack(clipping) == false)
                {
                    world.SpawnItemEntity(clipping, byPlayer.Entity.SidedPos.XYZ);
                }

                world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel); 
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            if (belowBlock.Fertility > 0) return true;
            if (!(belowBlock is WildcraftBerryBush)) return false;

            Block belowbelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
            return belowbelowBlock.Fertility > 0 && this.Attributes?.IsTrue("stackable") == true && belowBlock.Attributes?.IsTrue("stackable") == true;
        }


        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (this.Variant["state"] == "ripe")
            {
                ItemStack[] drops = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
                return drops;
            }
            else
            {
                return null;
            }
        }
    }
}
