using System.Collections.Generic;
using System.Linq;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using wildcraft.config;

namespace wildcraft
{
    public class PricklyBerryBush : WildcraftBerryBush
    {
        public bool canDamage = WildcraftConfig.Current.berryBushCanDamage;
        public string[] willDamage = WildcraftConfig.Current.berryBushWillDamage;
        public float dmg = WildcraftConfig.Current.berryBushDamage;
        public float dmgTick = WildcraftConfig.Current.berryBushDamageTick;
        
        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if (!canDamage || entity == null || willDamage == null)
            {
                return;
            }

            foreach(string creature in willDamage) 
            {
                if(entity.Code.ToString().Contains(creature))
                {
                    goto damagecreature;
                }
            }
            return;
            damagecreature:
            /* ChatGPT alternative without goto:
            bool shouldDamage = false;
            foreach (string creature in willDamage)
            {
                if (entity.Code.ToString().Contains(creature))
                {
                    shouldDamage = true;
                    break;
                }
            }

            if (!shouldDamage)
            {
                return;
            }
            */

            if (world.Side == EnumAppSide.Server && entity is EntityAgent)   //if the creature ins't sneaking, deal damage.
            {
                EntityAgent agent = (EntityAgent)entity;
                if (agent.ServerControls.TriesToMove && !agent.ServerControls.Sneak)
                {
                    if (world.Rand.NextDouble() > dmgTick) //while standing in the bush, how often will it hurt you
                    {
                        entity.ReceiveDamage(new DamageSource() 
                        { 
                            Source = EnumDamageSource.Block, 
                            SourceBlock = this, 
                            Type = EnumDamageType.PiercingAttack, 
                            SourcePos = pos.ToVec3d() 
                        }
                        , dmg); //Deal damage
                    }
                }
            }
            base.OnEntityInside(world, entity, pos);
        }
    }
}
