
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static RoR2.DotController;
using System.Collections.Generic;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public static class HundredRendingFists
    {

        public static ItemDef itemDef;
        public static BuffDef RendDebuff;
        private static GameObject delayedDamageActivateEffect;

        //unused
        private static GameObject delayedDamageEffect;
        

        internal static void Init()
        {
            if (!ConfigManager.HundredRendingFists_isEnabled.Value) return;

            Log.Info("Initializing North Star Hand Wraps Item");
            //ITEM//
            itemDef = GetItemDef("HundredRendingFists");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //DEBUFF//
            RendDebuff = ScriptableObject.CreateInstance<BuffDef>();
            RendDebuff.name = "Rend";
            RendDebuff.canStack = true;
            RendDebuff.isHidden = false;
            RendDebuff.isDebuff = true;
            RendDebuff.buffColor = Color.white;
            RendDebuff.isCooldown = false;
            RendDebuff.iconSprite = BraqsItemsMain.assetBundle.LoadAsset<Sprite>("texBuffRendIcon");

            ContentAddition.AddBuffDef(RendDebuff);

            //EFFECTS//
            delayedDamageEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/DelayedDamage/DelayedDamageIndicator.prefab").WaitForCompletion();
            delayedDamageActivateEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion();



            Hooks();

            Log.Info("North Star Hand Wraps Initialized");
        }

        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.inventory
                && victim.TryGetComponent(out CharacterBody victimBody) && (bool)victimBody.healthComponent)
            {
                int stacks = attackerBody.inventory.GetItemCount(itemDef);

                if (stacks > 0 && attackerBody.master)
                {
                    var damageCoefficient = (stacks - 1) * ConfigManager.HundredRendingFists_storedDamagePerStack.Value + ConfigManager.HundredRendingFists_storedDamageBase.Value;
                    BraqsItems_RendController.ApplyRend(damageInfo.attacker, victimBody, damageInfo, damageCoefficient);

                }
            }

            orig(self, damageInfo, victim);
        }

        //public, so others can stack rend if they need to.
        public class BraqsItems_RendController : MonoBehaviour
        {
            private class DamageStore
            {
                public GameObject attackerObject;
                public float damage;
                public float damageCoefficient;
            }

            private GameObject delayedDamageIndicator;

            private List<DamageStore> damageStores = new List<DamageStore>();
            public float storeBonus;
            private bool firstStackApplied = false;

            public CharacterBody body;
            public HealthComponent healthComponent;


            private void Start()
            {
                if (gameObject.TryGetComponent(out CharacterBody component))
                {
                    Log.Debug("RendController:Start()");
                    body = component;
                    healthComponent = body.healthComponent;
                }
                else Destroy(this);
            }

            private void OnDestroy()
            {
                if (delayedDamageIndicator)
                {
                    Object.Destroy(delayedDamageIndicator);
                }
                Log.Debug("RendController:OnDestroy()");
            }

            private void FixedUpdate()
            {
                if (!body.HasBuff(RendDebuff) && firstStackApplied)
                {
                    DealStoredDamage();
                }
            }

            public static void ApplyRend(GameObject attacker, CharacterBody victim, DamageInfo damage, float damageCoefficient)
            {
                //Rend
                Log.Debug("RendController:ApplyRend()");

                victim.TryGetComponent(out BraqsItems_RendController rendController);
                if (!rendController)
                {
                    rendController = victim.gameObject.AddComponent<BraqsItems_RendController>();
                    rendController.body = victim;
                }

                rendController.StoreDamage(attacker, damage, damageCoefficient);
                rendController.firstStackApplied = true;

            }

            private void StoreDamage(GameObject attacker, DamageInfo damageInfo, float damageCoefficient)
            {
                //store damage per attacker, so proper credit can be given. 

                bool temp = true;
                float damage = damageInfo.damage;

                for (int i = 0; i < damageStores.Count; i++)
                {
                    if (damageStores[i].attackerObject == attacker)
                    {
                        damageStores[i].damage += damage;
                        damageStores[i].damageCoefficient = damageCoefficient;
                        temp = false;
                    }
                }
                if (temp)
                {
                    damageStores.Add(new DamageStore
                    {
                        attackerObject = attacker,
                        damage = damage,
                        damageCoefficient = damageCoefficient,
                    });
                }

                //the extra damage is based on TOTAL stores, rather than per attacker stores. Team up for big bonuses!
                //Lower proc coefficents give lower bonuses.
                storeBonus += damageInfo.procCoefficient;
                UpdateBuffStacks();
            }

            private void DealStoredDamage()
            {
                Log.Debug("RendController:DealStoredDamage()");
                if (body && healthComponent)
                {

                    //RoR2.Util.PlaySound("Play_bleedOnCritAndExplode_explode", body.gameObject);
                    float percentBonus = storeBonus * ConfigManager.HundredRendingFists_storeBonus.Value;

                    //don't think this does anything
                    float effectScale = Mathf.Min(ConfigManager.HundredRendingFists_storedDamageBase.Value + percentBonus, 3);

                    EffectManager.SpawnEffect(delayedDamageActivateEffect, new EffectData()
                    {
                        origin = body.corePosition,
                        color = Color.white,
                        scale = effectScale,
                    }, true);

                    for (int i = 0; i < damageStores.Count; i++)
                    {
                        DamageStore damageStore = damageStores[i];

                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.crit = false;
                        damageInfo.damage = damageStore.damage * (damageStore.damageCoefficient + percentBonus);
                        damageInfo.force = Vector3.zero;
                        damageInfo.inflictor = base.gameObject;
                        damageInfo.position = body.corePosition;
                        damageInfo.procCoefficient = 0f;
                        damageInfo.damageColorIndex = DamageColorIndex.SuperBleed;
                        damageInfo.damageType = DamageType.DoT;
                        damageInfo.attacker = damageStore.attackerObject;

                        healthComponent.TakeDamage(damageInfo);
                    }
                }


                Destroy(this);
            }

            private void UpdateBuffStacks()
            {
                var duration = ConfigManager.HundredRendingFists_rendDuration.Value;
                //Refresh all stacks
                for (var i = 0; i < body.timedBuffs.Count; i++)
                {
                    var timedBuff = body.timedBuffs[i];
                    if (timedBuff.buffIndex == RendDebuff.buffIndex)
                    {
                        timedBuff.timer = duration;
                    }
                }

                //bool effectsDirty = false;
                //Add 1 stack per store rounded up. This way, x stacks = x * 5% bonus
                for (var i = 0; i < storeBonus - body.GetBuffCount(RendDebuff); i++)
                {
                    body.AddTimedBuff(RendDebuff, duration);
                    //effectsDirty = true;
                }

                //if(effectsDirty) UpdateDelayedDamageEffect();
            }
            
            //private void UpdateDelayedDamageEffect()
            //{

            //    if (delayedDamageIndicator)
            //    {
            //        Object.Destroy(delayedDamageIndicator);
            //    }
            //    delayedDamageIndicator = UnityEngine.Object.Instantiate(delayedDamageEffect, body.mainHurtBox ? body.mainHurtBox.transform.position : body.transform.position, Quaternion.identity, body.mainHurtBox ? body.mainHurtBox.transform : body.transform);
                
            //}
        }
    }
}
