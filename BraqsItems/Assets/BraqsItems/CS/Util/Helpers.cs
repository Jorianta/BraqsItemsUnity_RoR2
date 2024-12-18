﻿using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    }
}
