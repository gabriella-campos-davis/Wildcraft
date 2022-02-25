   using System;
    using System.Collections.Generic;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Util;

namespace wildcraft
{
    public class ItemHerbSeed : Item
    {
        WorldInteraction[] interactions;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client)
                return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "herbseedInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null || block.EntityClass == null)
                        continue;
                    if (block.Fertility > 0)
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-plant",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "sneak",
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null || !byEntity.Controls.Sneak)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            string herbtype = Variant["herbs"];
            Block herbBlock;
            if(this.Code.GetName().Contains("waterchestnut")){
                herbBlock = byEntity.World.GetBlock(AssetLocation.Create("wildcraft:waterplant-" + herbtype + "-land-harvested-free", Code.Domain));
            } else {
                herbBlock= byEntity.World.GetBlock(AssetLocation.Create("wildcraft:leafygroundvegetable-" + herbtype + "-harvested", Code.Domain));
            }

            if (herbBlock != null)
            {
                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer)
                    byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                blockSel = blockSel.Clone();
                blockSel.Position.Up();

                string failureCode = "";
                if (!herbBlock.TryPlaceBlock(api.World, byPlayer, itemslot.Itemstack, blockSel, ref failureCode))
                {
                    if (api is ICoreClientAPI capi && failureCode != null && failureCode != "__ignore__")
                    {
                        capi.TriggerIngameError(this, failureCode, Lang.Get("placefailure-" + failureCode));
                    }
                }
                else
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), blockSel.Position.X + 0.5f, blockSel.Position.Y, blockSel.Position.Z + 0.5f, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }
                }

                handHandling = EnumHandHandling.PreventDefault;
            }
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}