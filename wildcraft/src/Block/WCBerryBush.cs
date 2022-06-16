using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.Linq;
using Vintagestory.API.Common.Entities;
using wildcraft.config;

namespace wildcraft
{
    public class WCBerryBush : BlockBerryBush
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
                BlockEntityWCBerryBush bebush = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityWCBerryBush;
                bebush.Prune();

                if (!byPlayer.InventoryManager.TryGiveItemstack(clipping))
                {
                    world.SpawnItemEntity(clipping, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                world.PlaySoundAt(harvestingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                return true;
            }
            
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
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
