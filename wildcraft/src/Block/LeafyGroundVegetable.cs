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

        
    public class LeafyGroundVegetable : BlockPlant
    {
        WorldInteraction[] interactions = null;

        public string[] immuneCreatures;
        public static readonly string normalCodePart = "normal";
        
        public static readonly string harvestedCodePart = "harvested";
        public float dmg = 1f / 8f;
        public bool prickly;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            prickly = Attributes["prickly"].AsBool();
            immuneCreatures = Attributes["immuneCreatures"].AsArray<string>();

            if (Variant["state"] == "harvested")
                return;

            interactions = ObjectCacheUtil.GetOrCreate(api, "mushromBlockInteractions", () =>
            {
                List<ItemStack> knifeStacklist = new List<ItemStack>();

                foreach (Item item in api.World.Items)
                {
                    if (item.Code == null)
                        continue;

                    if (item.Tool == EnumTool.Knife)
                    {
                        knifeStacklist.Add(new ItemStack(item));
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-mushroom-harvest",
                        MouseButton = EnumMouseButton.Left,
                        Itemstacks = knifeStacklist.ToArray()
                    }
                };
            });
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {

            /*
            if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                failureCode = "__ignore__";
                return false;
            }
            */

            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            if (byPlayer != null)
            {
                EnumTool? tool = byPlayer.InventoryManager.ActiveTool;
                if (IsGrown() && tool == EnumTool.Knife)
                {
                    Block harvestedBlock = GetHarvestedBlock(world);
                    world.BlockAccessor.SetBlock(harvestedBlock.BlockId, pos);
                }
            }
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[] { new BlockDropItemStack(handbookStack) };
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {

            if (IsGrown())
            {
                if (Attributes?.IsTrue("forageStatAffected") == true)
                {
                    dropQuantityMultiplier *= byPlayer?.Entity?.Stats.GetBlended("forageDropRate") ?? 1;
                }

                return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
            }

            if(IsRoot() && !IsGrown()){
                return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);     
            }
            else
            {
               return null;
            }
        }


        public bool IsGrown()
        {
            return Code.Path.Contains(normalCodePart);
        }

        public bool IsRoot()
        {
            if(Variant["herbs"] == "bearleek" ||Variant["herbs"] == "burdock" || Variant["herbs"] == "marshmallow" || Variant["herbs"] == "chicory" || Variant["herbs"] == "liquorice"){
                return true;
            } else {
                return false;
            }
        }

        public Block GetNormalBlock(IWorldAccessor world)
        {
            AssetLocation newBlockCode = Code.CopyWithPath(Code.Path.Replace(harvestedCodePart, normalCodePart));
            return world.GetBlock(newBlockCode);
        }

        public Block GetHarvestedBlock(IWorldAccessor world)
        {
            AssetLocation newBlockCode = Code.CopyWithPath(Code.Path.Replace(normalCodePart, harvestedCodePart));
            return world.GetBlock(newBlockCode);
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
                        if(Variant["herbs"] == "stingingnettle"){
                            var sting = new StingingNettle();
                            sting.Apply(entity);
                        } else if(Variant["herbs"] == "poisonoak"){
                            var poison = new PoisonOak();
                            poison.Apply(entity);
                        } else{
                            entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = this, Type = EnumDamageType.PiercingAttack, SourcePos = pos.ToVec3d() }, dmg);
                        }
                    }
            }
            base.OnEntityInside(world, entity, pos);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

    }
}
