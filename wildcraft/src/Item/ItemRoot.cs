using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace wildcraft
{
    public class ItemRoot : Item
    {
        public AssetLocation plantingSound;
        public AssetLocation plantedSound;
        bool plantableInWater;

        float plantingTime;

        string dirtType;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            plantableInWater = this.Attributes["plantableInWater"].AsBool();

            plantingTime = 0.6F;
            string soundCode = "game:sounds/block/leafy-picking";
            string plantedSoundCode = "game:sounds/block/dirt1";

            if (soundCode != null && plantedSoundCode != null) {
                plantingSound = AssetLocation.Create(soundCode);
                plantedSound = AssetLocation.Create(plantedSoundCode);

            }
        }

        
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.Handled;

             if (blockSel == null || byEntity?.World == null || !byEntity.Controls.ShiftKey)
            {
                return;
            }

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            if (!byEntity.Api.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return;
            }

            if(!byEntity.Api.World.BlockAccessor.GetBlock(blockSel.Position).HasBehavior<BehaviorMyceliumHost>() || byEntity.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is not null)
            {
                return;
            }
            
            byEntity.Api.World.PlaySoundAt(plantingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

            Vec3d particleVec3D = new Vec3d(blockSel.Position.X + 0.5F, blockSel.Position.Y + 1, blockSel.Position.Z + 0.5F);
            byEntity.Api.World.SpawnCubeParticles(blockSel.Position, particleVec3D, 0.25F, 50, 1F, byPlayer);

            return;
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
             if (blockSel == null || byEntity?.World == null || !byEntity.Controls.ShiftKey)
            {
                return false;
            }

            if(!byEntity.Api.World.BlockAccessor.GetBlock(blockSel.Position).HasBehavior<BehaviorMyceliumHost>() || byEntity.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is not null)
            {
                return false;
            }

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemAttack);

            if (byEntity.Api.World.Rand.NextDouble() < 0.05)
            {
                byEntity.Api.World.PlaySoundAt(plantingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                Vec3d particleVec3D = new Vec3d(blockSel.Position.X + 0.5F, blockSel.Position.Y + 1, blockSel.Position.Z + 0.5F);
                byEntity.Api.World.SpawnCubeParticles(blockSel.Position, particleVec3D, 0.25F, 50, 1F, byPlayer);
            }

            return byEntity.Api.World.Side == EnumAppSide.Client || secondsUsed < plantingTime;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            if (blockSel == null || byEntity?.World == null || !byEntity.Controls.ShiftKey)
            {
                return;
            }

            bool waterBlock = byEntity.World.BlockAccessor.GetBlock(blockSel.Position.AddCopy(blockSel.Face), BlockLayersAccess.Fluid).LiquidCode == "water";

                
            /*
            if(waterBlock && !plantableInWater)
            {
                return;
            }*/


            if (secondsUsed > plantingTime - 0.05f && byEntity.Api.World.Side == EnumAppSide.Server)
            {
                BlockPos rootPos = blockSel.Position.Copy();
                Block playerSelection = byEntity.Api.World.BlockAccessor.GetBlock(rootPos);

                if (playerSelection == null)
                {
                    return;
                } 

                if(playerSelection.HasBehavior<BehaviorMyceliumHost>() && byEntity.Api.World.BlockAccessor.GetBlockEntity(rootPos) is null)
                {
                    //this "lastCodePart" check is to check the soils grasscoverage. If it has grass, putting a root in the dirt block will result in the block losing it's grass coverage.
                    if(playerSelection.LastCodePart() != "none" )
                    {
                        dirtType = playerSelection.LastCodePart(1);
                        Block dirtBlock = byEntity.Api.World.BlockAccessor.GetBlock(new AssetLocation("game:soil-"+ dirtType + "-none"));

                        if (dirtBlock == null)
                        {
                            return;
                        }
                        byEntity.Api.World.BlockAccessor.ExchangeBlock(dirtBlock.BlockId, rootPos);
                    }

                    ItemStack rootStack = new ItemStack(this, 1);

                    byEntity.Api.World.BlockAccessor.SpawnBlockEntity("BERhizome", rootPos, rootStack);

                    byEntity.Api.World.PlaySoundAt(plantedSound, rootPos.X, rootPos.Y, rootPos.Z, byPlayer);

                    Vec3d particleVec3D = new Vec3d(rootPos.X + 0.5F, rootPos.Y + 1, rootPos.Z + 0.5F);
                    byEntity.Api.World.SpawnCubeParticles(blockSel.Position, particleVec3D, 0.25F, 50, 1F, byPlayer);

                    slot.TakeOut(1);
                    slot.MarkDirty();
                }   
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    HotKeyCode = "shift",
                    ActionLangCode = "wildcraft:heldhelp-plantroot",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}