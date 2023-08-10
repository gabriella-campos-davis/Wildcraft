using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API;
using System;
using System.Reflection;
using System.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using wildcraft.Gui;
using wildcraft.config;
using BuffStuff;

[assembly: ModInfo( "Wildcraft",
	Description = "Adds new plants to the game",
	Website     = "",
	Authors     = new []{ "gabb", "CATASTEROID" } )]

namespace wildcraft
{
    public class wildcraft : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        // Stuff for rhizomes, seeds for random number calls etc
        ICoreAPI Api;
        public static LCGRandom lcgrnd;
        public static NormalRandom rndn;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Api = api;
            api.RegisterBlockClass("WildcraftBerryBush", typeof(WildcraftBerryBush));
            api.RegisterBlockClass("PricklyBerryBush", typeof(PricklyBerryBush));
            api.RegisterBlockClass("ShrubBerryBush", typeof(ShrubBerryBush));
            api.RegisterBlockClass("GroundBerryPlant", typeof(GroundBerryPlant));
            api.RegisterBlockClass("WildcraftPlant", typeof(WildcraftPlant));
            api.RegisterBlockClass("RhizomatusPlant", typeof(RhizomatusPlant));
            api.RegisterBlockClass("WaterHerb", typeof(WaterHerb));
            api.RegisterBlockClass("SimpleWaterPlant", typeof(SimpleWaterPlant));
            api.RegisterBlockClass("AquaticPlant", typeof(AquaticPlant));
            api.RegisterBlockClass("DuckWeed", typeof(DuckWeed));
            api.RegisterBlockClass("DuckWeedRoot", typeof(DuckWeedRoot));
            api.RegisterBlockClass("BlockMossCoating", typeof(BlockMossCoating));
            api.RegisterBlockClass("WildcraftMoss", typeof(WildcraftMoss));
            api.RegisterBlockClass("GiantKelp", typeof(GiantKelp));

            api.RegisterBlockEntityClass("BEWildcraftBerryBush", typeof(BEWildcraftBerryBush));
            api.RegisterBlockEntityClass("BEShrubBerryBush", typeof(BEShrubBerryBush));
            api.RegisterBlockEntityClass("BETallBerryBush", typeof(BETallBerryBush));
            api.RegisterBlockEntityClass("BEClipping", typeof(BEClipping));
            api.RegisterBlockEntityClass("BEGroundBerryPlant", typeof(BEGroundBerryPlant));
            api.RegisterBlockEntityClass("BEWildcraftRhizome", typeof(BEWildcraftRhizome));
            api.RegisterBlockEntityClass("BESeedling", typeof(BESeedling));
            api.RegisterBlockEntityClass("BERhizome", typeof(BERhizome));
            api.RegisterBlockEntityClass("BEDuckWeedRoot", typeof(BEDuckWeedRoot));

            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemRoot", typeof(ItemRoot));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            api.RegisterItemClass("ItemBerrySeed", typeof(ItemBerrySeed));
            api.RegisterItemClass("ItemHerb", typeof(ItemHerb));
            api.RegisterItemClass("WildcraftPoultice", typeof(WildcraftPoultice));

            try
            {
                var Config = api.LoadModConfig<WildcraftConfig>("wildcraftconfig.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    WildcraftConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    WildcraftConfig.Current = WildcraftConfig.GetDefault();
                }
            }
            catch
            {
                WildcraftConfig.Current = WildcraftConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                if (WildcraftConfig.Current.plantsCanDamage == null)
                    WildcraftConfig.Current.plantsCanDamage = WildcraftConfig.GetDefault().plantsCanDamage;

                if (WildcraftConfig.Current.plantsCanPoison == null)
                    WildcraftConfig.Current.plantsCanPoison = WildcraftConfig.GetDefault().plantsCanPoison;

                if (WildcraftConfig.Current.plantsWillDamage == null)
                    WildcraftConfig.Current.plantsWillDamage = WildcraftConfig.GetDefault().plantsWillDamage;

                if (WildcraftConfig.Current.berryBushCanDamage == null)
                    WildcraftConfig.Current.berryBushCanDamage = WildcraftConfig.GetDefault().berryBushCanDamage;

                if (WildcraftConfig.Current.berryBushDamage == null)
                    WildcraftConfig.Current.berryBushDamage = WildcraftConfig.GetDefault().berryBushDamage;

                if (WildcraftConfig.Current.berryBushDamageTick == null)
                    WildcraftConfig.Current.berryBushDamageTick = WildcraftConfig.GetDefault().berryBushDamageTick;

                if (WildcraftConfig.Current.berryBushWillDamage == null)
                    WildcraftConfig.Current.berryBushWillDamage = WildcraftConfig.GetDefault().berryBushWillDamage;

                if (WildcraftConfig.Current.useKnifeForClipping == null)
                    WildcraftConfig.Current.useKnifeForClipping = WildcraftConfig.GetDefault().useKnifeForClipping;

                if (WildcraftConfig.Current.useShearsForClipping == null)
                    WildcraftConfig.Current.useShearsForClipping = WildcraftConfig.GetDefault().useShearsForClipping;

                if (WildcraftConfig.Current.poulticeHealOverTime == null)
                    WildcraftConfig.Current.poulticeHealOverTime = WildcraftConfig.GetDefault().poulticeHealOverTime;

                api.StoreModConfig(WildcraftConfig.Current, "wildcraftconfig.json");
            }
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            BuffManager.Initialize(api, this);
            BuffManager.RegisterBuffType("RashDebuff", typeof(RashDebuff));
            BuffManager.RegisterBuffType("PoulticeBuff", typeof(PoulticeBuff));

            api.Event.SaveGameLoaded += Event_SaveGameLoaded;

            this.Api = api;
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            //capi.Gui.RegisterDialog(new HudElementBuffs(capi));
            
        }

        private void Event_SaveGameLoaded()
        {
            lcgrnd = new LCGRandom(Api.World.Seed);
            rndn = new NormalRandom(Api.World.Seed);
        }
    }
}
