//using HarmonyLib;
using R2API;
using RoR2;
using On;
using System;
using UnityEngine;
using System.Linq;


namespace BraqsItems.Misc
{
    //for use with refabricator.
    public static class BrokenItemRelationships
    {

        public static ItemRelationshipType brokenItemRelationship;
        public static ItemRelationshipProvider itemRelationshipProvider;
        internal static void CreateBrokenItemProvider()
        {
            brokenItemRelationship = BraqsItemsMain.assetBundle.LoadAsset<ItemRelationshipType>("BrokenItemRelationship");
            ContentAddition.AddItemRelationshipType(brokenItemRelationship);

            itemRelationshipProvider = BraqsItemsMain.assetBundle.LoadAsset<ItemRelationshipProvider>("BrokenItemProvider");
            ContentAddition.AddItemRelationshipProvider(itemRelationshipProvider);


            ItemCatalog.availability.CallWhenAvailable(() =>
            {
                if(!ItemCatalog.itemRelationships.ContainsKey(brokenItemRelationship)) ItemCatalog.itemRelationships.Add(brokenItemRelationship, Array.Empty<ItemDef.Pair>());

                ItemDef.Pair[] relationships = new ItemDef.Pair[]
                {
            new() {itemDef1 = RoR2Content.Items.ExtraLife, itemDef2 = RoR2Content.Items.ExtraLifeConsumed },
            new() {itemDef1 = DLC1Content.Items.FragileDamageBonus, itemDef2 = DLC1Content.Items.FragileDamageBonusConsumed },
            new() {itemDef1 = DLC1Content.Items.HealingPotion, itemDef2 = DLC1Content.Items.HealingPotionConsumed },
            new() {itemDef1 = DLC1Content.Items.ExtraLifeVoid, itemDef2 = DLC1Content.Items.ExtraLifeVoidConsumed },
                };

                ItemCatalog.itemRelationships[brokenItemRelationship] = ItemCatalog.itemRelationships[brokenItemRelationship].Union(relationships).ToArray();

                foreach (ItemDef.Pair itemDef in ItemCatalog.itemRelationships[brokenItemRelationship])
                {
                    Debug.Log("BrokenItemRelationships added: (" + itemDef.itemDef1 + ", " + itemDef.itemDef2 + ")");
                }
            });

        }
    }
}
