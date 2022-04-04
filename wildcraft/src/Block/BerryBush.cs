    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Util;
    using Vintagestory.GameContent;
    using System.Linq;
    using Vintagestory.API.Common.Entities;

namespace wildcraft
{
    public class BerryBush : BlockPlant
    {
        public static float dmg = 0.5f;
        public string[] immuneCreatures;

        List<ItemStack> shearStacklist = new List<ItemStack>();
        public bool prickly;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            prickly = Attributes["prickly"].AsBool();
            immuneCreatures = Attributes["immuneCreatures"].AsArray<string>();

            foreach (Item item in api.World.Items)
            {
                if (item.Code == null)
                    continue;

                if (item.Tool == EnumTool.Shears)
                {
                    shearStacklist.Add(new ItemStack(item));
                }
            }
        }
        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block belowBlock = blockAccessor.GetBlock(pos.DownCopy());
            if (belowBlock.Fertility > 0)
                return true;
            if (!(belowBlock is BerryBush))
                return false;

            Block belowbelowBlock = blockAccessor.GetBlock(pos.DownCopy(2));
            return belowbelowBlock.Fertility > 0 && this.Attributes?.IsTrue("stackable") == true && belowBlock.Attributes?.IsTrue("stackable") == true;
        }


        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            if (Textures == null || Textures.Count == 0) return 0;
            BakedCompositeTexture tex = Textures?.First().Value?.Baked;
            if (tex == null) return 0;

            int color = capi.BlockTextureAtlas.GetRandomColor(tex.TextureSubId, rndIndex);
            color = capi.World.ApplyColorMapOnRgba("climatePlantTint", SeasonColorMap, color, pos.X, pos.Y, pos.Z);
            return color;
        }


        public override int GetColor(ICoreClientAPI capi, BlockPos pos)
        {
            int color = base.GetColorWithoutTint(capi, pos);

            return capi.World.ApplyColorMapOnRgba("climatePlantTint", SeasonColorMap, color, pos.X, pos.Y, pos.Z, false);
        }
        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if(prickly != true || entity == null || immuneCreatures == null){
                return;
            }
            for(int i = 0; i < immuneCreatures.Length; i++){
                if(entity.Code.ToString().Contains(immuneCreatures[i])){
                    return;
                }
            }
            if (world.Side == EnumAppSide.Server && entity is EntityAgent && !(entity as EntityAgent).ServerControls.Sneak)
            {
                if (world.Rand.NextDouble() > 0.7)
                {
                    entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = this, Type = EnumDamageType.PiercingAttack, SourcePos = pos.ToVec3d() }, dmg);
                }
            }
            base.OnEntityInside(world, entity, pos);
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
