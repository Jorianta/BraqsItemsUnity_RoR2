//using HarmonyLib;
using R2API;
using RoR2;
using System;
using UnityEngine;


namespace BraqsItems.Misc
{
    //for use with goobo sr.
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

            //ItemCatalog.availability.CallWhenAvailable(() =>
            //{
            //    ItemCatalog.itemRelationships.Add(brokenItemRelationship, Array.Empty<ItemDef.Pair>());

            //    addBrokenItemRelationship(RoR2Content.Items.ExtraLife, RoR2Content.Items.ExtraLifeConsumed);
            //    addBrokenItemRelationship(DLC1Content.Items.FragileDamageBonus, DLC1Content.Items.FragileDamageBonusConsumed);
            //    addBrokenItemRelationship(DLC1Content.Items.HealingPotion, DLC1Content.Items.HealingPotionConsumed);
            //    addBrokenItemRelationship(DLC1Content.Items.ExtraLifeVoid, DLC1Content.Items.ExtraLifeVoidConsumed);

            //    BraqsItemsContent.BraqsItems_ContentPack.itemRelationshipProviders.Add(new ItemRelationshipProvider[] { itemRelationshipProvider });
            //});
        }

        //Make sure the second item is the broken version
        //public static void addBrokenItemRelationship(ItemDef normalitem, ItemDef brokenItem)
        //{
        //    ItemDef.Pair pair = new ItemDef.Pair()
        //    {
        //        itemDef1 = normalitem,
        //        itemDef2 = brokenItem,
        //    };
        //    itemRelationshipProvider.relationships = itemRelationshipProvider.relationships.AddToArray(pair);
        //}
    }
}
