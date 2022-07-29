using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
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

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("WildcraftBerryBush", typeof(WildcraftBerryBush));
            api.RegisterBlockClass("PricklyBerryBush", typeof(PricklyBerryBush));
            api.RegisterBlockClass("ShrubBerryBush", typeof(ShrubBerryBush));
            api.RegisterBlockClass("LeafyGroundVegetable", typeof(LeafyGroundVegetable));

            api.RegisterBlockEntityClass("BEClipping", typeof(BEClipping));
            api.RegisterBlockEntityClass("BESeedling", typeof(BESeedling));
            api.RegisterBlockEntityClass("BEWildcraftBerryBush", typeof(BEWildcraftBerryBush));
            api.RegisterBlockEntityClass("BETallBerryBush", typeof(BETallBerryBush));
            api.RegisterBlockEntityClass("BEShrubBerryBush", typeof(BEShrubBerryBush));


            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            api.RegisterItemClass("ItemWCPoultice", typeof(ItemWCPoultice));

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
                if (WildcraftConfig.Current.berryBushDamage == null)
                    WildcraftConfig.Current.berryBushDamage = WildcraftConfig.GetDefault().berryBushDamage;

                if (WildcraftConfig.Current.berryBushDamageTick == null)
                    WildcraftConfig.Current.berryBushDamageTick = WildcraftConfig.GetDefault().berryBushDamageTick;

                if (WildcraftConfig.Current.plantsWillDamage == null)
                    WildcraftConfig.Current.plantsWillDamage = WildcraftConfig.GetDefault().plantsWillDamage;

                if (WildcraftConfig.Current.useKnifeForClipping == null)
                    WildcraftConfig.Current.useKnifeForClipping = WildcraftConfig.GetDefault().useKnifeForClipping;

                if (WildcraftConfig.Current.useShearsForClipping == null)
                    WildcraftConfig.Current.useShearsForClipping = WildcraftConfig.GetDefault().useShearsForClipping;

                if (WildcraftConfig.Current.plantsCanPoison == null)
                    WildcraftConfig.Current.plantsCanPoison = WildcraftConfig.GetDefault().plantsCanPoison;

                if (WildcraftConfig.Current.poulticeHealOverTime == null)
                    WildcraftConfig.Current.poulticeHealOverTime = WildcraftConfig.GetDefault().poulticeHealOverTime;

                api.StoreModConfig(WildcraftConfig.Current, "wildcraftconfig.json");
            }
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            BuffManager.Initialize(api, this);
            BuffManager.RegisterBuffType("StingingNettle", typeof(StingingNettle));
            BuffManager.RegisterBuffType("PoisonOak", typeof(PoisonOak));
            BuffManager.RegisterBuffType("PoulticeBuff", typeof(PoulticeBuff));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            //capi.Gui.RegisterDialog(new HudElementBuffs(capi));
            
        }
    }
}
