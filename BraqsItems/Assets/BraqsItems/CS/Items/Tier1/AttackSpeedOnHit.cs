using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static BraqsItems.Misc.CharacterEvents;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public class AttackSpeedOnHit
    {
        public static ItemDef itemDef;

        public static bool isEnabled = true;
        public static float percentPerStack = 0.02f;
        public static float basePercent = 0.02f;

        internal static void Init()
        {
            if (!isEnabled) return;

            Log.Info("Initializing Hundreds and Thousands Item");

            //ITEM//
             itemDef = GetItemDef("AttackSpeedOnHit");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();

            Log.Info("Hundreds and Thousands Initialized");
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }



        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (obj.attackerBody && obj.victimMaster && obj.victimBody.healthComponent && obj.victimBody.healthComponent.alive
                && obj.attackerBody.TryGetComponent(out BraqsItems_AttackSpeedOnHitTracker component) && obj.victimMaster.TryGetComponent(out BraqsItems_CharacterEventComponent victimEvents))
            {
                if (victimEvents.body && component.TryAddVictim(victimEvents.body))
                {
                    victimEvents.OnCharacterDeath += component.RemoveVictim;
                }
            }
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_AttackSpeedOnHitTracker>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }


        public static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.TryGetComponent(out BraqsItems_AttackSpeedOnHitTracker component))
            {
                args.attackSpeedMultAdd += component.victims.Count * ((component.stack - 1) * percentPerStack + basePercent);
            }
        }

        public class BraqsItems_AttackSpeedOnHitTracker : CharacterBody.ItemBehavior
        {
            public List<CharacterBody> victims = new List<CharacterBody>();

            private void Start()
            {
                Log.Debug("AttackSpeedOnHit:Start()");
            }

            public bool TryAddVictim(CharacterBody victim)
            {
                if (!victims.Contains(victim))
                {
                    victims.Add(victim);
                    base.body.RecalculateStats();
                    Log.Debug(victims.Count + " enemies hit");
                    return true;
                }
                else return false;
            }

            public void RemoveVictim(CharacterBody victim)
            {
                if (victims.Contains(victim))
                {
                    victims.Remove(victim);
                    base.body.RecalculateStats();
                    Log.Debug(victims.Count + " enemies hit");
                }
            }

            private void OnDestroy()
            {
                Log.Debug("AttackSpeedOnHit:OnDestroy()");
               
            }
        }
    }
}