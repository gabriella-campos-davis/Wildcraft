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

        new public MeshData GetPrunedMesh(BlockPos pos)
        {
            if (api == null) return null;
            if (prunedmeshes == null) genPrunedMeshes();

            int rnd = RandomizeAxes == EnumRandomizeAxes.XYZ ? GameMath.MurmurHash3Mod(pos.X, pos.Y, pos.Z, prunedmeshes.Length) : GameMath.MurmurHash3Mod(pos.X, 0, pos.Z, prunedmeshes.Length);

            return prunedmeshes[rnd];
        }

        private void genPrunedMeshes()
        {
            var capi = api as ICoreClientAPI;

            prunedmeshes = new MeshData[Shape.BakedAlternates.Length];

            var selems = new string[] { "Berries", "Berries2", "Berries3", "Berries4", "Berries5", "Berries6", "Berries7", "Berries8", "Berries9", "Berries10",
                                        "middleStem", "Stem2", "Stem3", "Stem4", "Stem5", "Stem6", "Stem7", "Stem8"};
            if (State == "empty")
            {
                selems = selems.Remove("Berries");
                selems = selems.Remove("Berries2");
                selems = selems.Remove("Berries3");
                selems = selems.Remove("Berries4");
                selems = selems.Remove("Berries5");
                selems = selems.Remove("Berries6");
                selems = selems.Remove("Berries7");
                selems = selems.Remove("Berries8");
                selems = selems.Remove("Berries9");
                selems = selems.Remove("Berries10");
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