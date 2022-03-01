using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using ProtoBuf;
using BuffStuff;


 namespace wildcraft
 {
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class StingingNettle : Buff {

    private static float HP_PER_TICK = 1f / 8f;
    private static int DURATION_IN_REAL_SECONDS = 15;
    public override void OnStart() {
      SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
    }
    public override void OnStack(Buff oldBuff) {
      SetExpiryInRealSeconds(DURATION_IN_REAL_SECONDS);
    }
    public override void OnDeath() {
      SetExpiryImmediately();
    }

    public override void OnExpire()
    {
      SetExpiryImmediately();
    }
    public override void OnTick() {
      Entity.ReceiveDamage(new DamageSource { Source = EnumDamageSource.Internal, Type = EnumDamageType.Poison }, HP_PER_TICK);
    }
  }}