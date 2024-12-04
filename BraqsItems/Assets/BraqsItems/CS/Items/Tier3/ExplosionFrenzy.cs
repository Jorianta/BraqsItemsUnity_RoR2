using BraqsItems.Misc;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static BraqsItems.Misc.CharacterEvents;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class ExplosionFrenzy
    {
        public static ItemDef itemDef;


        internal static void Init()
        {
            if (!ConfigManager.ExplosionFrenzy_isEnabled.Value) return;

            Log.Info("Initializing My Manifesto Item");
            //ITEM//
            itemDef = GetItemDef("ExplosionFrenzy");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("My Manifesto Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;

            DotController.onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;


            Stats.StatsCompEvent.StatsCompRecalc += StatsCompEvent_StatsCompRecalc;
        }

        private static void DotController_onDotInflictedServerGlobal(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex == DotController.DotIndex.Burn || inflictDotInfo.dotIndex == DotController.DotIndex.StrongerBurn)
            {
                if (dotController.victimBody && dotController.victimHealthComponent && dotController.victimHealthComponent.alive && dotController.victimBody.master && dotController.victimBody.master.TryGetComponent(out BraqsItems_CharacterEventComponent eventComponent) &&
                    inflictDotInfo.attackerObject.TryGetComponent(out CharacterBody body) && body.TryGetComponent(out BraqsItems_ExplosionFrenzyBehavior component))
                {
                    component.AddVictim(dotController.victimObject);
                }
            }
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_ExplosionFrenzyBehavior>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }

        private static BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self)
        {

            //Apply burn
            BlastAttack.Result result = orig(self);

            if (self.attacker && self.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int stacks = body.inventory.GetItemCount(itemDef);
                if (stacks > 0 && result.hitCount > 0)
                {
                    float damage = ((stacks - 1) * ConfigManager.ExplosionFrenzy_igniteDamagePerStack.Value + ConfigManager.ExplosionFrenzy_igniteDamageBase.Value) * self.baseDamage;

                    for (int i = 0; i < result.hitCount; i++)
                    {
                        HurtBox hurtBox = result.hitPoints[i].hurtBox;

                        if ((bool)hurtBox.healthComponent)
                        {
                            InflictDotInfo inflictDotInfo = default(InflictDotInfo);
                            inflictDotInfo.victimObject = hurtBox.healthComponent.gameObject;
                            inflictDotInfo.attackerObject = self.attacker;
                            inflictDotInfo.totalDamage = damage;
                            inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
                            inflictDotInfo.damageMultiplier = 1f;
                            InflictDotInfo dotInfo = inflictDotInfo;

                            StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                            
                            DotController.InflictDot(ref dotInfo);
                        }
                    }
                }
            }
            
            return result;
        }

        public static void StatsCompEvent_StatsCompRecalc(object sender, Stats.StatsCompRecalcArgs args)
        {
            if (args.Stats && NetworkServer.active)
            {
                if (args.Stats.body && args.Stats.body.TryGetComponent(out BraqsItems_ExplosionFrenzyBehavior component))
                {
                    int bonus = component.bonus;
                    if (bonus > 0)
                    {
                        args.Stats.blastRadiusBoostAdd *= (bonus) * ConfigManager.ExplosionFrenzy_bonusPerBurn.Value + 1;
                    }
                }
            }
        }

        public class BraqsItems_ExplosionFrenzyBehavior : CharacterBody.ItemBehavior
        {
            private int maxBonus;
            public int bonus;
            private List<BraqsItems_BurnTracker> victims = new List<BraqsItems_BurnTracker>();

            private void Start()
            {
                Log.Debug("ExplosionFrenzyBehavior:Start()");
                maxBonus = ConfigManager.ExplosionFrenzy_bonusCapPerStack.Value * (stack-1) + ConfigManager.ExplosionFrenzy_bonusCapBase.Value;
            }

            private void OnDestroy()
            {
                Log.Debug("ExplosionFrenzyBehavior:OnDestroy()");
            }

            public void AddVictim(GameObject victim)
            {
                victim.TryGetComponent(out BraqsItems_BurnTracker component);

                //If they dont have a tracker, add it. If they do and we have it already, quit.
                if (!component)
                {
                    component = victim.AddComponent<BraqsItems_BurnTracker>();
                    component.victimBodyObject = victim;
                }
                else if (victims.Contains(component)) return;

                component.AddAttacker(this);
                victims.Add(component);
                ApplyBonus();
            }

            //Its a little odd these funtion take different types, but its better this way
            public void RemoveVictim(BraqsItems_BurnTracker victim)
            {
                victims.Remove(victim);
                ApplyBonus();
            }

            private void ApplyBonus()
            {
                bonus = Math.Min(victims.Count, maxBonus);
                body.RecalculateStats();
            }
        }

        public class BraqsItems_BurnTracker : MonoBehaviour
        {
            public List<BraqsItems_ExplosionFrenzyBehavior> attackers = new List<BraqsItems_ExplosionFrenzyBehavior> { };

            public GameObject victimBodyObject;
            private CharacterBody victimCharacterBody;


            public void Start()
            {
                victimCharacterBody = (victimBodyObject ? victimBodyObject.GetComponent<CharacterBody>() : null);
            }

            public void OnDestroy()
            {
                foreach(BraqsItems_ExplosionFrenzyBehavior a in attackers)
                {
                    a.RemoveVictim(this);
                }
            }

            public void FixedUpdate()
            {
                if (!victimCharacterBody || !victimCharacterBody.HasBuff(RoR2Content.Buffs.OnFire)) Destroy(this);
            }

            public void AddAttacker(BraqsItems_ExplosionFrenzyBehavior attacker)
            {
                if(attackers.Contains(attacker)) return;
                attackers.Add(attacker);
            }
        }
    }
}
