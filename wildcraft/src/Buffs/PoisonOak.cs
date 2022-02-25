using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using ProtoBuf;
using BuffStuff;
  
 [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class PoisonOak : Buff {
    private static float HP_PER_TICK = 1f / 8f;
    private static int DURATION_IN_REAL_SECONDS = 45;
    public override void OnStart() {
      SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
    }
    public override void OnStack(Buff oldBuff) {
      SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
    }
    public override void OnTick() {
      if (TickCounter % 8 == 0) {
        Entity.ReceiveDamage(new DamageSource { Source = EnumDamageSource.Internal, Type = EnumDamageType.Poison }, HP_PER_TICK);
      }
    }
  }