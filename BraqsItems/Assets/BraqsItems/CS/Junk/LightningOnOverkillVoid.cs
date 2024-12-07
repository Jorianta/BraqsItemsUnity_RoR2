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
    public class LightningOnOverkillVoid
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.LightningOnOverkill_isEnabled.Value) return;

            Log.Info("Initializing Sunken Chains Item");

            //ITEM//
            itemDef = GetItemDef("LightningOnOverkillVoid");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Sunken Chains Initialized");
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0 && (bool)damageInfo.attacker &&
                damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.TryGetComponent(out BraqsItems_LightningOnOverkillVoidBehavior component) && victim.TryGetComponent(out CharacterBody victimBody))
            {
                component.FireVoidLightning(damageInfo, attackerBody, victimBody);
            }

            orig(self, damageInfo, victim);
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            if(obj.attackerBody && obj.attackerBody.TryGetComponent(out BraqsItems_LightningOnOverkillVoidBehavior component))
            {
                int stack = obj.attackerBody.inventory.GetItemCount(itemDef);
                if (stack <= 0) return;
                
                float damage = RoR2.Util.OnHitProcDamage((obj.damageDealt - obj.combinedHealthBeforeDamage), obj.attackerBody.baseDamage, 
                    (stack - 1) * ConfigManager.LightningOnOverkill_damagePercentPerStack.Value + ConfigManager.LightningOnOverkill_damagePercentBase.Value);

                if (damage <= 0) return;
                
                component.StoreDeathDamage(damage);

            }
        }

        public class BraqsItems_LightningOnOverkillVoidBehavior : CharacterBody.ItemBehavior
        {
            private class DamageBonus
            {
                public float damage;
                public int hitsLeft;
            }

            private List<DamageBonus> damageBonuses;

            private float totalDamageBonus;

            public void StoreDeathDamage(float damage)
            {
                totalDamageBonus += damage;

                damageBonuses.Add(new DamageBonus 
                {
                    damage = damage,
                    hitsLeft = stack,
                });
            }

            public void FireVoidLightning(DamageInfo damageInfo, CharacterBody attackerBody, CharacterBody victimBody)
            {
                //Split the damage across void lightning to prevent runaway one shotting. Its conveniently thematically appropriate!

                int bonusCount = damageBonuses.Count;
                float fractionalDamage = totalDamageBonus / bonusCount;

                VoidLightningOrb voidLightningOrb = new VoidLightningOrb
                {
                    origin = damageInfo.position,
                    damageValue = fractionalDamage,
                    isCrit = damageInfo.crit,
                    totalStrikes = bonusCount - 1,
                    teamIndex = attackerBody.teamComponent.teamIndex,
                    attacker = damageInfo.attacker,
                    //for now...
                    procCoefficient = 0f,

                    procChainMask = damageInfo.procChainMask,
                    damageColorIndex = DamageColorIndex.Void,
                    secondsPerStrike = 0.1f,
                };
                HurtBox hurtBox = victimBody.mainHurtBox;
                if ((bool)hurtBox)
                {
                    voidLightningOrb.target = hurtBox;
                    OrbManager.instance.AddOrb(voidLightningOrb);
                }

                OnBonusProcced();
                
            }

            private void OnBonusProcced()
            {
                for (int num = damageBonuses.Count - 1; num >= 0; num--)
                {
                    DamageBonus bonus = damageBonuses[num];

                    if (--bonus.hitsLeft <= 0)
                    {
                        totalDamageBonus -= bonus.damage;
                        damageBonuses.RemoveAt(num);
                    }
                }
            }
        }
    }
}
