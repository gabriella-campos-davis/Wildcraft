using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

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

            api.RegisterBlockBehaviorClass("BehaviorClippable", typeof(BehaviorClippable));

            api.RegisterItemClass("ItemClipping", typeof(ItemClipping));
            api.RegisterItemClass("ItemHerbSeed", typeof(ItemHerbSeed));

            //api.RegisterItemClass("ItemFruit", typeof(ItemFruit));
        }
        public override void StartServerSide(ICoreServerAPI api)
        {

        }

        public override void StartClientSide(ICoreClientAPI api)
        {

        }
    }
}
