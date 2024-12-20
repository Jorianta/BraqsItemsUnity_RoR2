using BraqsItems.Misc;
using R2API;
using RoR2;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;

namespace BraqsItems.Util
{
    static class Helpers
    {
        public static ItemDef GetItemDef(string name)
        {
            ItemDef itemDef = BraqsItemsMain.assetBundle.LoadAsset<ItemDef>(name);

            name = name.ToUpper();

            itemDef.name = name;
            itemDef.nameToken = "ITEM_"+name+"_NAME";
            itemDef.pickupToken = "ITEM_"+name+"_PICKUP";
            itemDef.descriptionToken = "ITEM_"+name+"_DESC";
            itemDef.loreToken = "ITEM_"+name+"_LORE";

            ModelPanelParameters ModelParams = itemDef.pickupModelPrefab.AddComponent<ModelPanelParameters>();

            ModelParams.minDistance = 3;
            ModelParams.maxDistance = 6;

            ModelParams.cameraPositionTransform = new GameObject("CameraPosition").transform;
            ModelParams.cameraPositionTransform.SetParent(itemDef.pickupModelPrefab.transform);

            ModelParams.focusPointTransform = new GameObject("FocusPoint").transform;
            ModelParams.focusPointTransform.SetParent(itemDef.pickupModelPrefab.transform);

            return itemDef;
        }

        public static void AddContagiousRelationship(this ItemDef itemDef, ItemDef itemToTransform)
        {

            On.RoR2.Items.ContagiousItemManager.Init += (orig) =>
            {
                ItemDef.Pair pair = new() { itemDef1 = itemToTransform, itemDef2 = itemDef };
                ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].Union(new ItemDef.Pair[] { pair }).ToArray();
                orig();
            };

            //ItemCatalog.availability.CallWhenAvailable(() =>
            //{
            //    ItemDef.Pair pair = new() { itemDef1 = itemDef, itemDef2 = itemToTransform };
            //    ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].Union(new ItemDef.Pair[] { pair }).ToArray();
            //});
        }

        internal static void ErrorHookFailed(string name, Exception e)
        {
            Log.Error(name + " hook failed: " + e.Message);
        }
    }
}
