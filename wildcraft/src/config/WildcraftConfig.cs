using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace wildcraft.config
{
    class WildcraftConfig 
    {
        public float berryBushDamage = 0.5f;
        public float berryBushDamageTick = 0.7f;
        public string[] berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public bool useKnifeForClipping = true;
        public bool useShearsForClipping = true;

        public bool plantsCanPoison = true;

        public bool poulticeHealOverTime = true;


        public WildcraftConfig()
        {}

        public static WildcraftConfig Current { get; set; }

        public static WildcraftConfig GetDefault()
        {
            WildcraftConfig defaultConfig = new();

            defaultConfig.berryBushDamage = 0.5f;
            defaultConfig.berryBushDamageTick = 0.7f;
            defaultConfig.berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
            defaultConfig.useKnifeForClipping = true;
            defaultConfig.useShearsForClipping = true;

            defaultconfig.plantsCanPoison = true;

            defaultConfig.poulticeHealOverTime = true;

            return defaultConfig;
        }
    }
}