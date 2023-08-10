    using System;
    using System.Collections.Generic;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Util;

namespace wildcraft
{
    public class ItemHerb : Item
    {
        public EnumTransitionType state = EnumTransitionType.Perish;
        WorldInteraction[] interactions;

        float transitionedHours = 12;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client)
                return;
            ICoreClientAPI capi = api as ICoreClientAPI;
        }

        public override void InGuiIdle(IWorldAccessor world, ItemStack stack)
        {
            int frameCounter = 0;
            if(frameCounter < 3) 
            {
                frameCounter += 1;
                return;
            }
            if(state == EnumTransitionType.Perish) return;
            if(this.Attributes["transitionableProps"].GetType() == EnumTransitionType.Dry.GetType())
            {
                this.SetTransitionState(stack, EnumTransitionType.Perish, this.transitionedHours);
            }
        }
        public override void OnGroundIdle(EntityItem entityItem)
        {
            if(this.api.World.BlockAccessor.GetDistanceToRainFall(entityItem.Pos.AsBlockPos) == 99)
            {
                //this.SetTransitionState(stack, EnumTransitionType.Dry, this.transitionedHours);
            }
            else
            {
                //this.SetTransitionState(stack, EnumTransitionType.Perish, this.transitionedHours);
            }
        }
    }
}
