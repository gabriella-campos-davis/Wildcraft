using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace wildcraft 
{
    public class BEShrubBerryBush : BEWildcraftBerryBush
    {

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Pruned)
            {
                mesher.AddMeshData((Block as ShrubBerryBush).GetPrunedShrubMesh(Pos));
                return true;
            }

            return base.OnTesselation(mesher, tessThreadTesselator);
        }
    }
}