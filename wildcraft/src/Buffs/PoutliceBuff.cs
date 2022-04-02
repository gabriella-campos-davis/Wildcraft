using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using ProtoBuf;
using BuffStuff;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class PoulticeBuff : Buff {
    [ProtoIgnore]
    private static int DURATION_TICKS = 4 * 8;
    public double hpPerTick;
    public void init(double totalHealthChange) {
      hpPerTick = totalHealthChange / DURATION_TICKS;
      SetExpiryInTicks(DURATION_TICKS);
    }
    public override void OnDeath() {
      SetExpiryImmediately();
    }

    public override void OnExpire()
    {
      SetExpiryImmediately();
    }
    public override void OnTick() {
      Entity.ReceiveDamage(new DamageSource {
        Source = EnumDamageSource.Internal,
        Type = hpPerTick < 0 ? EnumDamageType.Heal : EnumDamageType.Heal
      }, (float)hpPerTick);
    }
  }