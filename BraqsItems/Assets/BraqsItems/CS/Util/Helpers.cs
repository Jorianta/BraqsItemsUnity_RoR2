using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BraqsItems.Util
{
    static class Helpers
    {
        public static GameObject ExplosionEffect;
        public static void Init()
        {
            ExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StickyBomb/BehemothVFX.prefab").WaitForCompletion();
        }
        public static void doExtraExplosionEffect(Vector3 position, float scale)
        {
            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
            {
                origin = position,
                scale = scale,
            }, transmit: true);
            EffectManager.SpawnEffect(ExplosionEffect, new EffectData
            {
                origin = position,
                scale = scale,
            }, transmit: true);
        }

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

            ModelParams.minDistance = 5;
            ModelParams.maxDistance = 10;

            return itemDef;
        }
    }
}
