using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using BraqsItems.Misc;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public class BiggerExplosions
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.BiggerExplosions_isEnabled.Value) return;

            Log.Info("Initializing Accelerant Item");
            //ITEM//
            itemDef = GetItemDef("BiggerExplosions");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Acclerant Initialized");
        }

        private static void Hooks()
        {
            Stats.StatsCompEvent.StatsCompRecalc += StatsCompEvent_StatsCompRecalc;
        }

        public static void StatsCompEvent_StatsCompRecalc(object sender, Stats.StatsCompRecalcArgs args)
        {
            if (args.Stats && NetworkServer.active)
            {
                if (args.Stats.inventory)
                {
                    int stack = args.Stats.inventory.GetItemCount(itemDef);
                    if (stack > 0) {
                        args.Stats.blastRadiusBoostAdd *= 1 + (stack-1) * ConfigManager.BiggerExplosions_percentPerStack.Value + ConfigManager.BiggerExplosions_percentBase.Value;
                    }
                }
            }
        }
    }
}