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
        public static ModdedProcType procType;

        internal static void Init()
        {
            if (!ConfigManager.LightningOnOverkillVoid_isEnabled.Value) return;

            Log.Info("Initializing Sunken Chains Item");

            //ITEM//
            itemDef = GetItemDef("LightningOnOverkillVoid");
            if (ConfigManager.LightningOnOverkill_isEnabled.Value) itemDef.AddContagiousRelationship(LightningOnOverkill.itemDef);

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //PROC//
            procType = ProcTypeAPI.ReserveProcType();

            Hooks();

            Log.Info("Sunken Chains Initialized");
        }

        private static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_LightningOnOverkillVoidBehavior>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0 && !damageInfo.procChainMask.HasModdedProc(procType) && (bool)damageInfo.attacker &&
                damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.TryGetComponent(out BraqsItems_LightningOnOverkillVoidBehavior component) && victim.TryGetComponent(out CharacterBody victimBody))
            {
                if (victimBody.healthComponent && victimBody.healthComponent.alive && damageInfo.damageType.IsDamageSourceSkillBased)
                {
                    component.FireVoidLightning(damageInfo, victimBody);
                }
            }

            orig(self, damageInfo, victim);
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            if(obj.attackerBody && obj.attackerBody.TryGetComponent(out BraqsItems_LightningOnOverkillVoidBehavior component))
            {
                int stack = obj.attackerBody.inventory.GetItemCount(itemDef);
                if (stack <= 0) return;

                //up to 400% base damage, based on how much damage was wasted
                float multiplier = ConfigManager.LightningOnOverkillVoid_damagePercentBase.Value + (stack - 1) * ConfigManager.LightningOnOverkillVoid_damagePercentPerStack.Value;
                multiplier *= (obj.damageDealt - obj.combinedHealthBeforeDamage)/obj.damageDealt;
                float damage = RoR2.Util.OnHitProcDamage(obj.attackerBody.damage, obj.attackerBody.baseDamage, 
                    multiplier);

                Debug.Log("Storing " + damage + " damage from overkill.");

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

            private List<DamageBonus> damageBonuses = new List<DamageBonus>();

            private float totalDamageBonus;

            public void StoreDeathDamage(float damage)
            {
                totalDamageBonus += damage;

                damageBonuses.Add(new DamageBonus 
                {
                    damage = damage,
                    hitsLeft = ConfigManager.LightningOnOverkillVoid_hitsBase.Value + (stack - 1) * ConfigManager.LightningOnOverkillVoid_hitsPerStack.Value,
                });
            }

            public void FireVoidLightning(DamageInfo damageInfo, CharacterBody victimBody)
            {
                //Split the damage across void lightning. Its thematically appropriate!

                int bonusCount = damageBonuses.Count;
                if (bonusCount <= 0 || totalDamageBonus <=0) return;

                Log.Debug("Firing Overkill Bonus");

                float fractionalDamage = totalDamageBonus  / bonusCount;

                ProcChainMask mask = damageInfo.procChainMask;
                mask.AddModdedProc(procType);

                VoidLightningOrb voidLightningOrb = new VoidLightningOrb
                {
                    origin = damageInfo.position,
                    damageValue = fractionalDamage,
                    isCrit = damageInfo.crit,
                    totalStrikes = bonusCount,
                    teamIndex = body.teamComponent.teamIndex,
                    attacker = damageInfo.attacker,
                    procCoefficient = 0.3f,
                    procChainMask = mask,
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

                    if (--damageBonuses[num].hitsLeft <= 0)
                    {
                        totalDamageBonus -= damageBonuses[num].damage;
                        damageBonuses.RemoveAt(num);
                    }
                }
            }
        }
    }
}
