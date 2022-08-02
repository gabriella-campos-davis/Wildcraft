using System;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using wildcraft.config;

namespace wildcraft
{
    public class WildcraftPoultice : ItemPoultice
    {
        WildcraftConfig config = new();
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (secondsUsed > 0.7f && byEntity.World.Side == EnumAppSide.Server)
            {
                JsonObject attr = slot.Itemstack.Collectible.Attributes;
                float health = attr["health"].AsFloat();
                bool isHealingOverTimeEnabled = WildcraftConfig.Current.poulticeHealOverTime;

                if(!isHealingOverTimeEnabled){
                    byEntity.ReceiveDamage(new DamageSource()
                        {
                        Source = EnumDamageSource.Internal,
                        Type = health > 0 ? EnumDamageType.Heal : EnumDamageType.Poison
                        }, Math.Abs(health));
                } 
                else 
                {
                    var isPoulticeActive = BuffStuff.BuffManager.IsBuffActive(byEntity, "PoulticeBuff");
                    var isRashDebuff = BuffStuff.BuffManager.IsBuffActive(byEntity, "RashDebuff");
                    // only consume item and apply buff if there isn't a buff already active
                    if (!isPoulticeActive) {
                        var buff = new PoulticeBuff();
                        buff.init(health);
                        buff.Apply(entitySel?.Entity != null ? entitySel.Entity : byEntity);
                        if(isRashDebuff && this.Code.GetName().Contains("yarrow")){
                            BuffStuff.BuffManager.GetActiveBuff(byEntity, "RashDebuff").Remove();
                        }
                    }
                }
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
    }
}