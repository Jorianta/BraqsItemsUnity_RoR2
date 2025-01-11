using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Orbs;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public static class LightningOnOverkill
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.LightningOnOverkill_isEnabled.Value) return;

            Log.Info("Initializing Jumper Cables Item");

            //ITEM//
            itemDef = GetItemDef("LightningOnOverkill");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Jumper Cables Initialized");
        }

        private static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            if(obj.attackerBody && obj.attackerBody.inventory)
            {
                int stack = obj.attackerBody.inventory.GetItemCount(itemDef);
                if (stack <= 0) return;
                
                float damage = RoR2.Util.OnHitProcDamage((obj.damageDealt - obj.combinedHealthBeforeDamage), obj.attackerBody.baseDamage, 
                    (stack - 1) * ConfigManager.LightningOnOverkill_damagePercentPerStack.Value + ConfigManager.LightningOnOverkill_damagePercentBase.Value);

                if (damage <= 0) return;

                LightningOrb lightningOrb = new LightningOrb
                {
                    origin = obj.victimBody.corePosition,
                    damageValue = damage,
                    isCrit = obj.damageInfo.crit,
                    bouncesRemaining = ConfigManager.LightningOnOverkill_bounceBase.Value + (stack-1) * ConfigManager.LightningOnOverkill_bouncePerStack.Value,
                    teamIndex = obj.attackerBody.teamComponent.teamIndex,
                    attacker = obj.attacker,
                    procCoefficient = 0.2f,
                    bouncedObjects = new List<HealthComponent> { obj.victim },
                    lightningType = LightningOrb.LightningType.Ukulele,
                    damageColorIndex = DamageColorIndex.Item,
                    range = ConfigManager.LightningOnOverkill_rangeBase.Value + (stack-1) * ConfigManager.LightningOnOverkill_rangePerStack.Value,
                };
                HurtBox hurtBox = lightningOrb.PickNextTarget(obj.victimBody.corePosition);
                if ((bool)hurtBox)
                {
                    lightningOrb.target = hurtBox;
                    OrbManager.instance.AddOrb(lightningOrb);
                }                
            }
        }
    }
}
