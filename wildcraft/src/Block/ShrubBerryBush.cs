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
    public class ShrubBerryBush : WildcraftBerryBush
    {
        MeshData[] prunedmeshes;

        public MeshData GetPrunedShrubMesh(BlockPos pos)
        {
            if (api == null) return null;
            if (prunedmeshes == null) genPrunedShrubMeshes();

            int rnd = RandomizeAxes == EnumRandomizeAxes.XYZ ? GameMath.MurmurHash3Mod(pos.X, pos.Y, pos.Z, prunedmeshes.Length) : GameMath.MurmurHash3Mod(pos.X, 0, pos.Z, prunedmeshes.Length);

            return prunedmeshes[rnd];
        }

        private void genPrunedShrubMeshes()
        {
            var capi = api as ICoreClientAPI;

            prunedmeshes = new MeshData[Shape.BakedAlternates.Length];

            var selems = new string[] { "Berries13/*","Berries15/*", "Berries17/*", "Berries19/*", "Stems3/*", "Stems5/*", "Stems7/*", "Stems9/*"};
            for(var i = 13; i <= 20; i = i + 2){
                if (State == "empty"){
                    selems = selems.Remove("Berries13/*");
                    selems = selems.Remove("Berries15/*");
                    selems = selems.Remove("Berries17/*");
                    selems = selems.Remove("Berries19/*");
                } 
            }

            for (int i = 0; i < Shape.BakedAlternates.Length; i++)
            {
                var cshape = Shape.BakedAlternates[i];
                var shape = capi.TesselatorManager.GetCachedShape(cshape.Base);
                capi.Tesselator.TesselateShape(this, shape, out prunedmeshes[i], this.Shape.RotateXYZCopy, null, selems);
            }
        }
    }
}