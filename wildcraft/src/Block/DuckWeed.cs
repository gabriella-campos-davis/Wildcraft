using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using System.Collections.Generic;
using System.Linq;
using System;
using Vintagestory.API.Common.Entities;
using wildcraft.config;
using wildcraft;

namespace wildcraft
{
    public class DuckWeed : BlockPlant
    {
        public string orientation = "empty";

        public float dieOrGrowTemp = 8f;
        float permaDuckweed = 25;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }

        /*
        public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
        {
            if(api.World.BlockAccessor.GetBlock(pos.X, pos.Y, pos.AddCopy(0, 0, -1).Z ).FirstCodePart(1) == "duckweed")
            {
                if(orientation == "empty") orientation = "";
                orientation = orientation + "n"; 
            }
            if(api.World.BlockAccessor.GetBlock(pos.AddCopy(1, 0, 0).X, pos.Y, pos.Z).FirstCodePart(1) == "duckweed" )
            {
                if(orientation == "empty") orientation = "";
                orientation = orientation + "e";
            }
            if(api.World.BlockAccessor.GetBlock(pos.X, pos.Y, pos.AddCopy(0, 0, 1).Z ).FirstCodePart(1) == "duckweed")
            {
                if(orientation == "empty") orientation = "";
                orientation = orientation + "s"; 
            }
            if(api.World.BlockAccessor.GetBlock(pos.AddCopy(-1, 0, 0).X, pos.Y, pos.Z).FirstCodePart() == "duckweed")
            {
                if(orientation == "empty") orientation = "";
                orientation = orientation + "w";
            }

            if(orientation != "empty")
            {
                if(orientation.Length > 2 && orientation != "empty"){
                    orientation = "nesw";
                } 
                api.World.Logger.Chat(orientation);
                Block newDuckweed = this.api.World.BlockAccessor.GetBlock(new AssetLocation("wildcraft:duckweed-duckweed-" + orientation));
                if(newDuckweed is null) return;
                api.World.BlockAccessor.ExchangeBlock(newDuckweed.Id, pos);
            }

        }
        */
        
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (CanPlantStay(world.BlockAccessor, blockSel.Position.UpCopy()))
            {
                blockSel = blockSel.Clone();
                blockSel.Position = blockSel.Position.Up();
                return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }
            
            failureCode = "requirefullwater";

            return false;
        }

        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.DownCopy(), BlockLayersAccess.Fluid);
            Block upblock = blockAccessor.GetBlock(pos, BlockLayersAccess.Fluid);
            if (block.LiquidCode != "water") return false;
            if (blockAccessor.GetBlock(pos.X, pos.Y - 5, pos.Z, BlockLayersAccess.Fluid).Id != 0) return false;
            return block.IsLiquid() && block.LiquidLevel == 7 && block.LiquidCode == "water" && upblock.Id==0;
        }

        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            // Don't spawn in 4 deep water
            if (blockAccessor.GetBlock(pos.X, pos.Y - 5, pos.Z, BlockLayersAccess.Fluid).Id != 0) return false;

            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand);
        }

        public override bool ShouldReceiveServerGameTicks(IWorldAccessor world, BlockPos pos, Random offThreadRandom, out object extra)
        {
            extra = null;
            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if(conds.WorldGenTemperature >= permaDuckweed) return false;
            float chance = conds == null ? 0 : GameMath.Clamp(Math.Abs(-(conds.Temperature - dieOrGrowTemp) / 20f), 0, 1);
            return offThreadRandom.NextDouble() < chance;
        }

        public override void OnServerGameTick(IWorldAccessor world, BlockPos pos, object extra = null)
        {
            ClimateCondition conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds.Temperature <= dieOrGrowTemp) // did we get a sever tick to die or beacuse we need to grow?
            {
                Random rand = new Random();
                //random chance to spawn a duckweed root when it dies.
                if (rand.Next(0, 10) > 8)
                {
                    bool lakeBed = false;
                    int currentDepth = 1;

                    Block belowBlock;

                    //find the lakebed, place the root, then die
                    while(lakeBed == false)
                    {
                        belowBlock = world.BlockAccessor.GetBlock(pos.X, pos.DownCopy(currentDepth).Y, pos.Z);

                        if (belowBlock.LiquidCode != "water" && belowBlock.BlockMaterial != EnumBlockMaterial.Plant)
                        {
                            Block placingBlock = world.BlockAccessor.GetBlock(new AssetLocation("wildcraft:duckweedroot"));
                            if (placingBlock == null) return;

                            if(world.BlockAccessor.GetBlock(pos.X, pos.DownCopy(currentDepth-1).Y, pos.Z).BlockMaterial ==  EnumBlockMaterial.Plant) return;
                            world.BlockAccessor.SetBlock(placingBlock.BlockId, pos.DownCopy(currentDepth - 1));
                            lakeBed = true;
                        }
                        currentDepth += 1;
                    }
                    world.BlockAccessor.SetBlock(0, pos);
                }
                world.BlockAccessor.SetBlock(0, pos);
            }

            if (conds.Temperature > dieOrGrowTemp)
            {
                Random rand = new Random();
                if(rand.Next(0,10) > 7) // give less chance to grow
                {
                    int randomDirection = rand.Next(0,4);
                    BlockPos newDuckweedPos = new BlockPos(pos.X, pos.Y, pos.Z);

                    if(randomDirection == 0) newDuckweedPos.Add(1,0,0);
                    else if(randomDirection == 1) newDuckweedPos.Add(-1,0,0);
                    else if(randomDirection == 2) newDuckweedPos.Add(0,0,1);
                    else if(randomDirection == 3) newDuckweedPos.Add(0,0,-1);

                    if(world.BlockAccessor.GetBlock(newDuckweedPos).BlockMaterial != EnumBlockMaterial.Air) return;
                    if(world.BlockAccessor.GetBlock(newDuckweedPos.DownCopy(), BlockLayersAccess.Fluid).LiquidCode != "water") return;

                    Block placingBlock = world.BlockAccessor.GetBlock(new AssetLocation("wildcraft:duckweed-duckweed-nesw"));
                    world.BlockAccessor.SetBlock(placingBlock.BlockId, newDuckweedPos);
                }
            }     
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return null;
        }
    }
}