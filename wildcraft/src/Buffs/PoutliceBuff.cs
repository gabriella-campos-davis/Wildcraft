using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using System;
using ProtoBuf;
using BuffStuff;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class PoulticeBuff : Buff {
    [ProtoIgnore]

    public double totalTimeChange = 8;
    private int DURATION_TICKS = 4 * 8 ; //8 seconds
    public double hpPerTick;
    public void init(double totalHealthChange, double totalTime) {
      totalTimeChange = totalTime;
      hpPerTick = totalHealthChange / totalTimeChange;
      DURATION_TICKS = 4 * Convert.ToInt32(totalTimeChange);
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
      if (TickCounter % 4 == 0) {
        Entity.ReceiveDamage(new DamageSource {
          Source = EnumDamageSource.Internal,
          Type = hpPerTick < 0 ? EnumDamageType.Heal : EnumDamageType.Heal
        }, (float)hpPerTick);
      }
    }
  }