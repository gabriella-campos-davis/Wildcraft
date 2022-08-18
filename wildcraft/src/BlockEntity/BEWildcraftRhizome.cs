using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wildcraft
{
    public class WildcraftRhizomeProps
    {
        /// <summary>
        /// Temperature below which all plants associated with the rhizome die
        /// </summary>
        public float dieWhenTempBelow = 0;
        /// <summary>
        /// Temperature above which all plants associated with the rhizome die
        /// </summary>
        public float dieWhenTempAbove = 45;
        /// <summary>
        /// Temperature below which the growth is stunted and the growing days counter is not incremented
        /// </summary>
        public float comfortableTemperatureMin = 5;
        /// <summary>
        /// Temperature above which the growth is stunted and the growing days counter is not incremented
        /// </summary>
        public float comfortableTemperatureMax = 35;
        /// <summary>
        /// Whether the plants produced by this rhizome are destroyed when the growth days exceeds the fruiting days or not
        /// </summary>
        public bool dieAfterFruiting = true;
        /// <summary>
        /// The minimum number of days that the rhizome is considered to be growing for before beginning fruiting and potentially deleting it's plants,
        /// the actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public double fruitingDaysMin = 20;
        /// <summary>
        /// The maximum number of days that the rhizome is considered to be fruiting for before ending fruiting and potentially deleting it's plants,
        /// the actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public double fruitingDaysMax = 40;
        /// <summary>
        /// The minimum number of days that the rhizome is considered to be growing for before beginning fruiting and growing plants
        /// The actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public double growingDaysMin = 10;
        /// <summary>
        /// The maximum number of days that the rhizome is considered to be growing for before beginning fruiting and growing plants
        /// The actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public double growingDaysMax = 20;
        /// <summary>
        /// The maximum distance far plants will be grown from the central position of the rhizome on each axis
        /// </summary>
        public int growRange = 7;
        /// <summary>
        /// The minimum number of days that the rhizome is considered to be growing for before beginning fruiting and growing plants
        /// The actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public int grownPlantNumberMin = 2;
        /// <summary>
        /// The maximum number of days that the rhizome is considered to be growing for before beginning fruiting and growing plants
        /// The actual figure is a number randomly chosen between the minimum and maximum values
        /// </summary>
        public int grownPlantNumberMax = 12;
    }

    public class BEWildcraftRhizome : BlockEntity
    {
        Vec3i[] grownRhizomeOffsets = new Vec3i[0];
        
        double plantsGrownTotalDays = 0;
        double plantsDiedTotalDays = -999999;
        double plantsGrowingDays = 0;
        double lastUpdateTotalDays = 0;

        AssetLocation plantBlockCode;

        WildcraftRhizomeProps props;
        Block wcPlantBlock;

        double fruitingDays = 20;
        double growingDays = 20;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.Side == EnumAppSide.Server)
            {
                int interval = 10000;
                RegisterGameTickListener(onServerTick, interval, -api.World.Rand.Next(interval));

                plantBlockCode = this.Block.Code;
                
                // If the rhizome's plant block is invalid nuke the rhizome
                if (plantBlockCode != null && !setwcPlantBlock(Api.World.GetBlock(plantBlockCode)))
                {
                    Api.Logger.Error("Invalid wildcraft rhizome plant type '{0}' at {1}. Will delete block entity.", plantBlockCode, Pos);
                    Api.Event.EnqueueMainThreadTask(() => Api.World.BlockAccessor.RemoveBlockEntity(Pos), "deleterhizomeBE");
                }
                Api.Logger.Notification("Wildcraft Rhizome blockentity initialized with '{0}' as it's plant code at position {1}.", plantBlockCode, Pos);
            }
        }

        private void onServerTick(float dt)
        {
            bool isFruiting = grownRhizomeOffsets.Length > 0;
            if (isFruiting && props.dieWhenTempBelow > -99 && props.dieWhenTempAbove < 99)
            {
                var conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, Api.World.Calendar.TotalDays);
                if (conds == null) return;
                if (props.dieWhenTempBelow > conds.Temperature || props.dieWhenTempAbove < conds.Temperature)
                {
                    Api.Logger.Notification("{0} rhizome at {1} killed {2} fruiting plants due to temperature shock! Temperature is '{3}' and plant tolerance is {4}C-{5}C.", plantBlockCode, Pos, grownRhizomeOffsets.Length, conds.Temperature, props.dieWhenTempBelow, props.dieWhenTempAbove);
                    DestroyGrownPlants();
                    return;
                }
            }

            // Our fruiting time is up - end fruiting and kill the plants
            if (props.dieAfterFruiting && isFruiting && plantsGrownTotalDays + fruitingDays < Api.World.Calendar.TotalDays)
            {

                Api.Logger.Notification("{0} rhizome at {1} done fruiting and set to die after fruiting, killing {2} fruiting plants! Growingtotaldays {3} + fruitingDays {4} > total calendar days {5}",  plantBlockCode, Pos, grownRhizomeOffsets.Length, plantsGrownTotalDays, fruitingDays, Api.World.Calendar.TotalDays);
                DestroyGrownPlants();
                return;
            }

            if (!isFruiting)
            {
                // Get the date that the rhizome was last updated
                lastUpdateTotalDays = Math.Max(lastUpdateTotalDays, Api.World.Calendar.TotalDays - 50); // Don't check more than 50 days into the past

                // While the last update date is less than the total days the rhizome has been extant this year
                while (Api.World.Calendar.TotalDays - lastUpdateTotalDays > 1)
                {
                    // Don't bother attempting to catch up if the climate data is bad (I think)
                    var conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, lastUpdateTotalDays + 0.5);
                    if (conds == null) return;

                    // Only add growth days if the temperature is within tolerable levels for growth
                    if (conds.Temperature > props.comfortableTemperatureMin && conds.Temperature < props.comfortableTemperatureMax)
                    {
                        // Increment one growing day
                        plantsGrowingDays += Api.World.Calendar.TotalDays - lastUpdateTotalDays;
                    }

                    // Bring the last update date up a day
                    lastUpdateTotalDays++;
                }
                // If the rhizome has been growing long enough spawn plants and reset growing days to zero
                if (plantsGrowingDays > growingDays)
                {
                    Api.Logger.Notification("{0} rhizome at {1} done growing- {2}/{3} growing days, spawning plants now!",  plantBlockCode, Pos, plantsGrowingDays, growingDays );
                    growPlants(Api.World.BlockAccessor, wildcraft.rndn);
                    plantsGrowingDays = 0;
                }
            } 
            else
            {
                // We're currently fruiting, check when our last update was
                if (Api.World.Calendar.TotalDays - lastUpdateTotalDays > 0.1)
                {
                    lastUpdateTotalDays = Api.World.Calendar.TotalDays;

                    // Feel to see how many of the rhizome's stored plant blocks still contain their plants
                    for (int i = 0; i < grownRhizomeOffsets.Length; i++)
                    {
                        var offset = grownRhizomeOffsets[i];
                        var pos = Pos.AddCopy(offset);
                        var chunk = Api.World.BlockAccessor.GetChunkAtBlockPos(pos);
                        if (chunk == null) return;
                        
                        // If the offset entry no longer contains a plant remove it from the list
                        if (!Api.World.BlockAccessor.GetBlock(pos).Code.Equals(plantBlockCode))
                        {
                            Api.Logger.Notification("Rhizome's plant {0} rhizome at {1} absent, removing it from the list!",  plantBlockCode, pos);
                            grownRhizomeOffsets = grownRhizomeOffsets.RemoveEntry(i);
                            i--;
                        }
                    }
                }
            }
        }

        public void Regrow()
        {
            Api.Logger.Notification("{0} rhizome commanded to regrow!",  plantBlockCode);
            DestroyGrownPlants();
            growPlants(Api.World.BlockAccessor, wildcraft.rndn);
        }
        

        // Kill the plant
        private void DestroyGrownPlants()
        {
            
            // Record when the plants all died (why?) then go through each stored position and if the plant's still there then replace it with air
            plantsDiedTotalDays = Api.World.Calendar.TotalDays;
            Api.Logger.Notification("{0} rhizome at {1} destroying plants, attempting to delete {2} plants...",plantBlockCode,Pos,grownRhizomeOffsets.Length);
            int plants = grownRhizomeOffsets.Length;
            int plantsDone = 1;
            foreach (var offset in grownRhizomeOffsets)
            {
                BlockPos pos = Pos.AddCopy(offset);
                var block = Api.World.BlockAccessor.GetBlock(pos);
                Api.Logger.Notification("Plant {0}/{1}- block {2} at position {3}",plantsDone,plants,block.Code,pos);
                if (block.Variant["type"] == wcPlantBlock.Variant["type"])
                {
                    Api.Logger.Notification("{0} rhizome plant at position {1} killed!",plantBlockCode,pos);
                    Api.World.BlockAccessor.SetBlock(0, pos);
                }
                plantsDone++;
            }
            // Reset the offsets array to a blank state
            grownRhizomeOffsets = new Vec3i[0];
        }

        bool setwcPlantBlock(Block block)
        {
            this.wcPlantBlock = block;
            this.plantBlockCode = block.Code;

            if (Api != null)
            {
                // Only add a rhizome if our block actually has data we can read
                if (block?.Attributes?["WildcraftRhizomeProps"].Exists != true) return false;

                if (block != null) props = block.Attributes["WildcraftRhizomeProps"].AsObject<WildcraftRhizomeProps>();
                wildcraft.lcgrnd.InitPositionSeed(plantBlockCode.GetHashCode(), (int)Api.World.Calendar.GetHemisphere(Pos) + 5);

                // Define how long each rhizome grows and fruits for
                // Growingdays is set to zero once fruiting begins 
                // Apply a little randomness, now using a minimum and maximum value defined by the block's rhizome prop data
                fruitingDays = props.fruitingDaysMin + (wildcraft.lcgrnd.NextDouble() * (props.fruitingDaysMax - props.fruitingDaysMin));
                growingDays = props.growingDaysMin + (wildcraft.lcgrnd.NextDouble() * (props.growingDaysMax - props.growingDaysMin));
                Api.Logger.Notification("setwcPlantBlock called for plant {0} at position {1}, growingDays {2} of {3}-{4}, fruitingDays {5} of {6}-{7}!",plantBlockCode,Pos,growingDays,props.growingDaysMin,props.growingDaysMax,fruitingDays,props.fruitingDaysMin,props.fruitingDaysMax);
            }

            return true;
        }

        public void OnGenerated(IBlockAccessor blockAccessor, LCGRandom rnd, RhizomatusPlant block)
        {
            plantBlockCode = this.Block.Code;
            Api.Logger.Notification("Wildcraft Rhizome blockentity initialized with '{0}' as it's plant code at position {1}.", plantBlockCode, Pos);
            setwcPlantBlock(block);
            wildcraft.lcgrnd.InitPositionSeed(plantBlockCode.GetHashCode(), (int)(wcPlantBlock as RhizomatusPlant).Api.World.Calendar.GetHemisphere(Pos));
            // 33% chance of the rhizome being set to grow for a short while before fruiting
            if (wildcraft.lcgrnd.NextDouble() < 0.33)
            {
                plantsGrowingDays = wildcraft.lcgrnd.NextDouble() * props.growingDaysMin;
                return;
            }
            growPlants(blockAccessor, rnd);
        }

        // Grow our plants 
        private void growPlants(IBlockAccessor blockAccessor, IRandom rnd)
        {
            generateRhizomePlants(blockAccessor, rnd);
            plantsGrownTotalDays = (wcPlantBlock as RhizomatusPlant).Api.World.Calendar.TotalDays - rnd.NextDouble() * fruitingDays;
            Api.Logger.Notification("Rhizome growPlants called!");    
        }

        private void generateRhizomePlants(IBlockAccessor blockAccessor, IRandom rnd)
        {
            //Time to actually spawn our plants, get a random number between the minimum and maximum values to grow
            int cnt = props.grownPlantNumberMin + rnd.NextInt(props.grownPlantNumberMax - props.grownPlantNumberMin);
            int cntTotal = cnt;
            int spawnSuccesses = 0;
            BlockPos pos = new BlockPos();
            int chunkSize = blockAccessor.ChunkSize;
            List<Vec3i> offsets = new List<Vec3i>();
            Api.Logger.Notification("Rhizome generateRhizomePlants called, attempting to spawn {0} plants (out of a possible {1}-{2}):",cnt,props.grownPlantNumberMin,props.grownPlantNumberMax);

            if (!isChunkAreaLoaded(blockAccessor, props.growRange)) return;

            // While the remaining plant number is above zero attempt to grow more plants
            while (cnt-- > 0)
            {
                Api.Logger.Notification("Plant spawn attempt {0}/{1}...");
                // Grab a random location within range on both axes in either direction
                int dx = props.growRange - rnd.NextInt(2* props.growRange + 1);
                int dz = props.growRange - rnd.NextInt(2* props.growRange + 1);

                pos.Set(Pos.X + dx, 0, Pos.Z + dz);

                var mapChunk = blockAccessor.GetMapChunkAtBlockPos(pos);

                int lx = GameMath.Mod(pos.X, chunkSize);
                int lz = GameMath.Mod(pos.Z, chunkSize);

                // Grab the terrain height at that location 
                pos.Y = mapChunk.WorldGenTerrainHeightMap[lz * blockAccessor.ChunkSize + lx] + 1;

                // Check the block at that location and the block beneath it (which is hopefully soil)
                Block hereBlock = blockAccessor.GetBlock(pos);
                Block belowBlock = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
                Api.Logger.Notification("... Position selected ({0}) hereBlock: {1}, belowBlock: {2}...",pos,hereBlock.Code,belowBlock.Code);
                // Make sure that the block isn't water and the block beneath it is soil of some kind
                if (belowBlock.Fertility < 10 || hereBlock.LiquidCode != null)
                {
                    Api.Logger.Notification("Attempt failed due to invalid location: Insufficient fertility ({0}) in belowBlock or liquid present ({1})",belowBlock.Fertility,hereBlock.LiquidCode);
                    continue;
                }

                // Place a plant if there's air, grass or any other block with replaceable 6000 or over here
                if ((plantsGrownTotalDays == 0 && hereBlock.Replaceable >= 6000) || hereBlock.Id == 0)
                {
                    blockAccessor.SetBlock(wcPlantBlock.Id, pos);
                    offsets.Add(new Vec3i(dx, pos.Y - Pos.Y, dz));
                    Api.Logger.Notification("Attempt succeeded! {0} planted at {1}",wcPlantBlock.Id,pos);
                    spawnSuccesses++;
                }
            }
            Api.Logger.Notification("Rhizome plant generation complete, {0} plants successfully spawned with {1} attempts",spawnSuccesses,pos);
            this.grownRhizomeOffsets = offsets.ToArray();
        }

        private bool isChunkAreaLoaded(IBlockAccessor blockAccessor, int growRange)
        {
            int chunksize = blockAccessor.ChunkSize;
            int mincx = (Pos.X - growRange) / chunksize;
            int maxcx = (Pos.X + growRange) / chunksize;

            int mincz = (Pos.Z - growRange) / chunksize;
            int maxcz = (Pos.Z + growRange) / chunksize;

            for (int cx = mincx; cx <= maxcx; cx++)
            {
                for (int cz = mincz; cz <= maxcz; cz++)
                {
                    if (blockAccessor.GetChunk(cx, Pos.Y / chunksize, cz) == null) return false;
                }
            }

            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            plantBlockCode = new AssetLocation(tree.GetString("plantBlockCode"));
            grownRhizomeOffsets = tree.GetVec3is("grownRhizomeOffsets");

            plantsGrownTotalDays = tree.GetDouble("plantsGrownTotalDays");
            plantsDiedTotalDays = tree.GetDouble("plantsDiedTotalDays");
            lastUpdateTotalDays = tree.GetDouble("lastUpdateTotalDays");
            plantsGrowingDays = tree.GetDouble("plantsGrowingDays");

            setwcPlantBlock(worldAccessForResolve.GetBlock(plantBlockCode));
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetString("plantBlockCode", plantBlockCode.ToShortString());
            tree.SetVec3is("grownRhizomeOffsets", grownRhizomeOffsets);
            tree.SetDouble("plantsGrownTotalDays", plantsGrownTotalDays);
            tree.SetDouble("plantsDiedTotalDays", plantsDiedTotalDays);

            tree.SetDouble("lastUpdateTotalDays", lastUpdateTotalDays);
            tree.SetDouble("plantsGrowingDays", plantsGrowingDays);
        }
    }
}