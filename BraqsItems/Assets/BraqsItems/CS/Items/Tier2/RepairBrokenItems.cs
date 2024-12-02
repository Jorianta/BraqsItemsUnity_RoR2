﻿using BraqsItems.Misc;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public class RepairBrokenItems
    {
        public static ItemDef itemDef;

        public static bool isEnabled = true;
        public static int baseRepairs = 2;
        public static int repairsPerStack = 2;
        public static float whiteRepairChance = 100f;
        public static float greenRepairChance = 75f;
        public static float redRepairChance = 50f;

        public static Dictionary<ItemTier, int> tierWeights;

        internal static void Init()
        {
            if (!isEnabled) return;

            Log.Info("Initializing Goobo Sr. Item");

            //ITEM//
            itemDef = GetItemDef("RepairBrokenItems");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Goobo Sr. Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
        }

        private static void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);

            int totalRepairAttempts = self.inventory.GetItemCount(itemDef);
            if (totalRepairAttempts <= 0 || !self ||!stage) return;

            totalRepairAttempts = (totalRepairAttempts - 1) * repairsPerStack + baseRepairs;

            Log.Debug("RepairBrokenItems: Attempting " + totalRepairAttempts + " repairs");

            HG.ReadOnlyArray<ItemDef.Pair> relationships = ItemCatalog.GetItemPairsForRelationship(BrokenItemRelationships.brokenItemRelationship);

            int relationshipCount = ItemCatalog.itemRelationships[BrokenItemRelationships.brokenItemRelationship].Length;
            float[] weights = new float[relationshipCount];
            float[] chances = new float[relationshipCount];
            float totalWeight = 0;

            for(int i = 0; i < relationshipCount; i++)
            {
                ItemDef.Pair pair = relationships[i];
                int temp = self.inventory.GetItemCount(pair.itemDef2);

                if (pair.itemDef1.tier == ItemTier.Tier1 || pair.itemDef1.tier == ItemTier.VoidTier1) chances[i] = whiteRepairChance;
                else if (pair.itemDef1.tier == ItemTier.Tier3 || pair.itemDef1.tier == ItemTier.VoidTier3) chances[i] = redRepairChance;
                else chances[i] = greenRepairChance;

                weights[i] = temp;
                totalWeight += temp;
            }

            if(totalWeight <= 0) return;

            int[] repairs = new int[relationshipCount];


            for (int i = 0; i < totalRepairAttempts; i++)
            {
                float cursor = 0;
                float random = UnityEngine.Random.Range(0f, totalWeight);

                //Find a random broken item to repair
                for (int j = 0; j < relationshipCount; j++)
                {
                    cursor += weights[j];
                    if (cursor >= random)
                    {   
                        //Try to repair
                        if(RoR2.Util.CheckRoll(chances[i], self)) repairs[j]++;
                        weights[j] -= 1;
                        break; 
                    }
                }
            }


            for (int i = 0; i < repairs.Length; i++)
            {
                tryRepairItems(self, relationships[i], repairs[i]);
            }
        }

        public static void tryRepairItems(CharacterMaster master, ItemDef.Pair pair, int count)
        {
            if (master == null || count <= 0)
            {
                return;
            }
            Log.Debug("RepairBrokenItems: Repairing " + count + " " + pair.itemDef2.name);

            count = Math.Min(count, master.inventory.GetItemCount(pair.itemDef2));

            master.inventory.RemoveItem(pair.itemDef2, count);
            master.inventory.GiveItem(pair.itemDef1, count);

            CharacterMasterNotificationQueue.SendTransformNotification(master, pair.itemDef2.itemIndex, pair.itemDef1.itemIndex, CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);

            return;
        }
    }
}