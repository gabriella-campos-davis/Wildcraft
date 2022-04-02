using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using wildcraft.Gui;
using BuffStuff;

[assembly: ModInfo( "Wildcraft",
	Description = "Adds new plants to the game",
	Website     = "",
	Authors     = new []{ "gabb" } )]

namespace wildcraft
{
    public class wildcraft : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BerryBush", typeof(BerryBush));
            api.RegisterBlockClass("LeafyGroundVegetable", typeof(LeafyGroundVegetable));

            api.RegisterBlockClass("Clipping", typeof(Clipping));

            api.RegisterBlockEntityClass("BlockEntityClipping", typeof(BlockEntityClipping));
            api.RegisterBlockEntityClass("BlockEntityWCBerryBush", typeof(BlockEntityWCBerryBush));
            api.RegisterBlockEntityClass("BlockEntityHerb", typeof(BlockEntityHerb));

            api.RegisterBlockBehaviorClass("BehaviorClippable", typeof(BehaviorClippable));

            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));
            //api.RegisterItemClass("ItemTemporalSpear", typeof(ItemTemporalSpear));
            api.RegisterItemClass("ItemWCPoultice", typeof(ItemWCPoultice));
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
