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
    public class DuckWeed : BlockPlant
    {
        public string orientation = "empty";
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            
        }

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
            return block.IsLiquid() && block.LiquidLevel == 7 && block.LiquidCode == "water" && upblock.Id==0;
        }

        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            // Don't spawn in 4 deep water
            if (blockAccessor.GetBlock(pos.X, pos.Y - 5, pos.Z, BlockLayersAccess.Fluid).Id != 0) return false;

            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand);
        }
    }
}
 
 
 