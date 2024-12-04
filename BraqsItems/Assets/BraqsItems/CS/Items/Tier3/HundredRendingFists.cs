
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static RoR2.DotController;
using System.Collections.Generic;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class HundredRendingFists
    {

        public static ItemDef itemDef;
        public static BuffDef RendDebuff;
        public static GameObject delayedDamageEffect;

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
            RendDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/BleedOnHitVoid/texBuffFractureIcon.tif").WaitForCompletion();

            ContentAddition.AddBuffDef(RendDebuff);

            //EFFECTS//
            delayedDamageEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/DelayedDamage/DelayedDamageIndicator.prefab").WaitForCompletion();

            Hooks();

            Log.Info("North Star Hand Wraps Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.inventory
                && victim.TryGetComponent(out CharacterBody victimBody) && (bool)victimBody.healthComponent)
            {
                int stacks = attackerBody.inventory.GetItemCount(itemDef);
                float procChance = (damageInfo.procCoefficient * 100f);

                if (stacks > 0 && attackerBody.master && RoR2.Util.CheckRoll(procChance, attackerBody.master))
                {
                    BraqsItems_RendController.ApplyRend(damageInfo.attacker, victimBody, damageInfo.damage, stacks);

                }
            }

            orig(self, damageInfo, victim);
        }

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
            public int stores;
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
                Log.Debug("RendController:OnDestroy()");
            }

            private void FixedUpdate()
            {
                if (!body.HasBuff(RendDebuff) && firstStackApplied)
                {
                    DealStoredDamage();
                }
            }

            public static void ApplyRend(GameObject attacker, CharacterBody victim, float damage, int itemStacks)
            {
                //Rend
                Log.Debug("RendController:ApplyRend()");
                var duration = ConfigManager.HundredRendingFists_rendDuration.Value;

                victim.TryGetComponent(out BraqsItems_RendController rendController);
                if (!rendController)
                {
                    rendController = victim.gameObject.AddComponent<BraqsItems_RendController>();
                    rendController.body = victim;
                }

                for (var i = 0; i < victim.timedBuffs.Count; i++)
                {
                    var timedBuff = victim.timedBuffs[i];
                    if (timedBuff.buffIndex == RendDebuff.buffIndex)
                    {
                        timedBuff.timer = duration;
                    }
                }
                victim.AddTimedBuff(RendDebuff, duration);
                rendController.firstStackApplied = true;

                rendController.StoreDamage(attacker, damage, itemStacks);

                rendController.UpdateDelayedDamageEffect();

            }

            private void StoreDamage(GameObject attacker, float damage, int itemStacks)
            {
                //store damage per attacker, so proper credit can be given. 
                var damageCoefficient = (itemStacks - 1) * ConfigManager.HundredRendingFists_storedDamagePerStack.Value + ConfigManager.HundredRendingFists_storedDamageBase.Value;

                bool temp = true;
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
                stores++;
            }

            private void DealStoredDamage()
            {
                Log.Debug("RendController:DealStoredDamage()");
                if (body && healthComponent)
                {
                    for (int i = 0; i < damageStores.Count; i++)
                    {
                        DamageStore damageStore = damageStores[i];

                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.crit = false;
                        damageInfo.damage = damageStore.damage * (damageStore.damageCoefficient + (stores * ConfigManager.HundredRendingFists_storeBonus.Value));
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

            private void UpdateDelayedDamageEffect()
            {
                if(delayedDamageIndicator)
                {
                    Destroy(delayedDamageIndicator);
                }
                EffectData effectData = new EffectData()
                {
                    origin = body.mainHurtBox ? body.mainHurtBox.transform.position : body.transform.position,
                    rotation = Quaternion.identity,
                    rootObject = body.gameObject
                };
               EffectManager.SpawnEffect(delayedDamageEffect, effectData, true);

            }
        }
    }
}
