using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace wildcraft.config
{
    class WildcraftConfig 
    {
        public bool plantsCanDamage = true;
        public bool plantsCanPoison = true;
        public string[] plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

        public bool berryBushCanDamage = true;
        public float berryBushDamage = 0.5f;
        public float berryBushDamageTick = 0.7f;
        public string[] berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
        public bool useKnifeForClipping = true;
        public bool useShearsForClipping = true;

        public bool poulticeHealOverTime = true;


        public WildcraftConfig()
        {}

        public static WildcraftConfig Current { get; set; }

        public static WildcraftConfig GetDefault()
        {
            WildcraftConfig defaultConfig = new();

            defaultConfig.plantsCanDamage = true;
            defaultConfig.plantsCanPoison = true;
            defaultConfig.plantsWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};

            defaultConfig.berryBushCanDamage = true;
            defaultConfig.berryBushDamage = 0.5f;
            defaultConfig.berryBushDamageTick = 0.7f;
            defaultConfig.berryBushWillDamage = new string[]{"game:wolf", "game:bear", "game:drifter", "game:player"};
            defaultConfig.useKnifeForClipping = true;
            defaultConfig.useShearsForClipping = true;
            
            defaultConfig.poulticeHealOverTime = true;

            return defaultConfig;
        }
    }
}