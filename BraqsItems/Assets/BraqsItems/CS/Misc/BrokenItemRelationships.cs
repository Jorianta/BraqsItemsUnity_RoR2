//using HarmonyLib;
using R2API;
using RoR2;
using System;
using UnityEngine;


namespace BraqsItems.Misc
{
    //for use with refabricator.
    public static class BrokenItemRelationships
    {

        public static ItemRelationshipType brokenItemRelationship;
        public static ItemRelationshipProvider itemRelationshipProvider { get; private set; }

        internal static void CreateBrokenItemProvider()
        {
            itemRelationshipProvider = BraqsItemsMain.assetBundle.LoadAsset<ItemRelationshipProvider>("BrokenItemProvider");
            brokenItemRelationship = BraqsItemsMain.assetBundle.LoadAsset<ItemRelationshipType>("BrokenItemRelationship");

            itemRelationshipProvider.relationships = new ItemDef.Pair[] 
            {
            new ItemDef.Pair() {itemDef1 = RoR2Content.Items.ExtraLife, itemDef2 = RoR2Content.Items.ExtraLifeConsumed },
            new ItemDef.Pair() {itemDef1 = DLC1Content.Items.FragileDamageBonus, itemDef2 = DLC1Content.Items.FragileDamageBonusConsumed },
            new ItemDef.Pair() {itemDef1 = DLC1Content.Items.HealingPotion, itemDef2 = DLC1Content.Items.HealingPotionConsumed },
            new ItemDef.Pair() {itemDef1 = DLC1Content.Items.ExtraLifeVoid, itemDef2 = DLC1Content.Items.ExtraLifeVoidConsumed },
            };

            ContentAddition.AddItemRelationshipProvider(itemRelationshipProvider);

        }
    }
}
