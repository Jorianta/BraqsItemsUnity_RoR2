using BraqsItems.Misc;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static RoR2.OverlapAttack;

namespace BraqsItems.Junk
{
    public class OnKillOnHit
    {
        public static ItemDef itemDef;

        public static bool isEnabled = true;

        static GameObject fmpEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/DeathProjectileTickEffect.prefab").WaitForCompletion();

        internal static void Init()
        {
            Log.Info("Initializing OnKillOnHit Item");
            //ITEM//
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "ONKILLONHIT";
            itemDef.nameToken = "ITEM_ONKILLONHIT_NAME";
            itemDef.pickupToken = "ITEM_ONKILLONHIT_PICKUP";
            itemDef.descriptionToken = "ITEM_ONKILLONHIT_DESC";
            itemDef.loreToken = "ITEM_ONKILLONHIT_LORE";

            itemDef.AutoPopulateTokens();

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
            };

            itemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathProjectile/texDeathProjectileIcon.png").WaitForCompletion();
            itemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/PickupDeathProjectile.prefab").WaitForCompletion();

            ModelPanelParameters ModelParams = itemDef.pickupModelPrefab.AddComponent<ModelPanelParameters>();

            ModelParams.minDistance = 5;
            ModelParams.maxDistance = 10;
            // itemDef.pickupModelPrefab.GetComponent<ModelPanelParameters>().cameraPositionTransform.localPosition = new Vector3(1, 1, -0.3f); 
            // itemDef.pickupModelPrefab.GetComponent<ModelPanelParameters>().focusPointTransform.localPosition = new Vector3(0, 1, -0.3f);
            // itemDef.pickupModelPrefab.GetComponent<ModelPanelParameters>().focusPointTransform.localEulerAngles = new Vector3(0, 0, 0);



            itemDef.canRemove = true;
            itemDef.hidden = false;

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();

            Log.Info("My Manifesto Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        //This actually works great, just too great. It successfully procs your onkill effects, it just ALSO procs the enemies ondeath effects.
        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker &&  damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.inventory
                && victim.TryGetComponent(out CharacterBody victimBody) && (bool)victimBody.healthComponent)
            {

                Log.Debug("OnKillOnHit:GlobalEventManager_OnHitEnemy");

                int stacks = attackerBody.inventory.GetItemCount(itemDef);
                float procChance = (damageInfo.procCoefficient * 100f * 0.07f * stacks)/(0.07f * stacks + 1);

                if (stacks > 0 && attackerBody.master && RoR2.Util.CheckRoll(procChance, attackerBody.master)) {
 

                    EffectData effectData = new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Quaternion.identity
                    };
                    EffectManager.SpawnEffect(fmpEffectPrefab, effectData, false);
                    DamageInfo damageInfo2 = new DamageInfo
                    {
                        attacker = damageInfo.attacker,
                        crit = damageInfo.crit,
                        damage = damageInfo.damage,
                        position = damageInfo.position,
                        procCoefficient = 0f,
                        damageType = DamageType.Generic,
                        damageColorIndex = DamageColorIndex.Item
                    };

                    DamageReport damageReport = new DamageReport(damageInfo2, victimBody.healthComponent, damageInfo.damage, victimBody.healthComponent.combinedHealth);
                    GlobalEventManager.instance.OnCharacterDeath(damageReport);
                  }
            }



            orig(self, damageInfo, victim);
        }
    }
}
