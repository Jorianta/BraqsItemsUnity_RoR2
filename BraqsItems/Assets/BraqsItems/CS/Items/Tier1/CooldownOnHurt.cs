using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static BraqsItems.Util.Helpers;
using BraqsItems.Misc;
using System.Runtime.CompilerServices;

namespace BraqsItems
{
    public static class CooldownOnHurt
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.BiggerExplosions_isEnabled.Value) return;

            Log.Info("Initializing Reptile Brain Item");
            //ITEM//
            itemDef = GetItemDef("CooldownOnHurt");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Reptile Brain Initialized");
        }

        private static void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (self && self.body && self.body.inventory)
            {
                int stacks = self.body.inventory.GetItemCount(itemDef);

                if (stacks > 0)
                {
                    float reduction = 0.5f * (stacks);
                    foreach (GenericSkill skill in self.body.skillLocator.allSkills)
                    {
                        skill.RunRecharge(reduction);
                    }
                }
            }
        }
    }
}
