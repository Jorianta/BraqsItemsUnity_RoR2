using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static BraqsItems.Util.Helpers;
using System;
using UnityEngine.Networking;
using System.Linq;
using RoR2.Orbs;

namespace BraqsItems
{
    public class LunarAOE
    {
        public static ItemDef itemDef;

        private static GameObject lunarMissile;
        private static GameObject SparkEffect;

        public static ModdedProcType procType;


        internal static void Init()
        {
            if (!ConfigManager.AttackSpeedOnHit_isEnabled.Value) return;

            Log.Info("Initializing LunarAOE Item");

            //ITEM//
            itemDef = GetItemDef("LunarAOE");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            procType = ProcTypeAPI.ReserveProcType();

            GenerateEffect();

            Hooks();

            Log.Info("LunarAOE Initialized");
        }

        private static void GenerateEffect()
        {

            lunarMissile = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MissileVoid/MissileVoidOrbEffect.prefab").WaitForCompletion().InstantiateClone("LunarMissile");
            SparkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab").WaitForCompletion().InstantiateClone("LunarAoESpark");
            Texture lunarRamp = Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHelfire.png").WaitForCompletion();

            try
            {

                UnityEngine.Object.Destroy(SparkEffect.transform.Find("Lightning, Spark Center").gameObject);
                UnityEngine.Object.Destroy(SparkEffect.transform.Find("Nova Sphere").gameObject);

            }
            catch (Exception e)
            {
                Log.Warning("Stats:GenerateEffect() ; Could not create lunar aoe effect." +
                    "\n" + e);
            }
            ContentAddition.AddEffect(SparkEffect);

            try
            {
                var orb = lunarMissile.GetComponent<OrbEffect>();
                orb.endEffect = SparkEffect;
            }
            catch (Exception e)
            {
                Log.Warning("LunarAOE:GenerateEffect() ; Could not edit missile impact." +
                    "\n" + e);
            }
            try
            {
                var trail = lunarMissile.transform.Find("MissileVoidGhost/Trail").gameObject.GetComponent<TrailRenderer>();
                trail.time = 1f;
                var trailMaterial = trail.material;

                trailMaterial.SetTexture("_RemapTex", lunarRamp);
            }
            catch (Exception e)
            {
                Log.Warning("LunarAOE:GenerateEffect() ; Could not edit missile trail." +
                    "\n" + e);
            }

            try
            {
                var flash = lunarMissile.transform.Find("MissileVoidGhost/Point Light").gameObject.GetComponent<Light>();
                flash.color = new Color(0.53f, 0.52f, 0.76f);

            }
            catch (Exception e)
            {
                Log.Warning("LunarAOE:GenerateEffect() ; Could not edit missile light." +
                    "\n" + e);
            }
            try
            {
                var flare = lunarMissile.transform.Find("MissileVoidGhost/Flare").gameObject.GetComponent<ParticleSystemRenderer>();

                var flareMat = flare.material;
                flareMat.SetTexture("_RemapTex", lunarRamp);

                //Gradient silverGradient = new Gradient();
                //silverGradient.mode = flareColor.color.gradient.mode; ;
                //silverGradient.alphaKeys = flareColor.color.gradient.alphaKeys;
                //silverGradient.colorKeys = new GradientColorKey[]
                //{
                //    new GradientColorKey{color = new Color(1f, 1f, 1f), time = 0f},
                //    new GradientColorKey{color = new Color(0.62f, 0.52f, 0.62f), time = 0.3f},
                //    new GradientColorKey{color = new Color(0.43f, 0.42f, 0.66f), time = 1f},
                //};

                //flareColor.color = new ParticleSystem.MinMaxGradient(silverGradient);
            }
            catch (Exception e)
            {
                Log.Warning("LunarAOE:GenerateEffect() ; Could not edit missile flare." +
                "\n" + e);
            }

            ContentAddition.AddEffect(lunarMissile);
            
        }

        private static void Hooks()
        {

            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            //On.RoR2.GlobalEventManager.OnHitAllProcess += GlobalEventManager_OnHitAllProcess;
        }

        private static void GlobalEventManager_OnHitAllProcess(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);

            if (damageInfo.procCoefficient == 0f || damageInfo.rejected)
            {
                return;
            }
            if (!damageInfo.attacker)
            {
                return;
            }
            //prevent double dipping
            if(hitObject.GetComponent<HealthComponent>())
            {
                return;
            }

            damageInfo.damage = DamageShareOrb.TryDistributeDamage(damageInfo);
        }

        //TODO: make aoe effect teammates in chaos
        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active || damageInfo.procChainMask.HasModdedProc(procType))
            {
                orig(self, damageInfo);
                return;
            }

            damageInfo.damage = DamageShareOrb.TryDistributeDamage(damageInfo, self);

            orig(self, damageInfo);
        }

        private class DamageShareOrb : RoR2.Orbs.Orb
        {
            private const float speed = 100f;

            public float damageValue;

            public GameObject attacker;

            public TeamIndex teamIndex;

            public List<HealthComponent> bouncedObjects;

            public bool isCrit;

            public float scale;

            public ProcChainMask procChainMask;

            public float procCoefficient;

            public DamageColorIndex damageColorIndex;
            public DamageTypeCombo damageTypeCombo;

            public override void Begin()
            {
                base.duration = base.distanceToTarget / speed;
                EffectData effectData = new EffectData
                {
                    scale = scale,
                    origin = origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(lunarMissile, effectData, transmit: true);
            }

            public override void OnArrival()
            {
                if ((bool)target)
                {
                    HealthComponent healthComponent = target.healthComponent;
                    if ((bool)healthComponent)
                    {

                        Vector3 position = target.transform.position;
                        GameObject gameObject = healthComponent.gameObject;
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageValue;
                        damageInfo.damageType = damageTypeCombo;
                        damageInfo.attacker = attacker;
                        damageInfo.inflictor = null;
                        damageInfo.crit = isCrit;
                        damageInfo.procChainMask = procChainMask;
                        damageInfo.procCoefficient = procCoefficient;
                        damageInfo.position = position;
                        damageInfo.damageColorIndex = damageColorIndex;

                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, gameObject);
                    }
                }
            }

            public static float TryDistributeDamage(DamageInfo damageInfo, HealthComponent victim = null)
            {
                Log.Debug("DamageShareOrb:TryDistributeDamage()");

                if(!damageInfo.attacker.TryGetComponent(out CharacterBody body) || damageInfo.procChainMask.HasModdedProc(procType)) return damageInfo.damage;
                int stack = body.inventory.GetItemCountEffective(itemDef);
                if(stack <= 0) return damageInfo.damage;

                float radius = 10f * stack;

                TeamIndex team = body.teamComponent.teamIndex;

                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(team);
                bullseyeSearch.maxDistanceFilter = radius;
                bullseyeSearch.searchOrigin = damageInfo.position;
                bullseyeSearch.searchDirection = Vector3.zero;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.None;
                bullseyeSearch.filterByLoS = false;

                bullseyeSearch.RefreshCandidates();

                if(victim) bullseyeSearch.FilterOutGameObject(victim.gameObject);

                IEnumerable<HurtBox> enumerable = bullseyeSearch.GetResults().Where(RoR2.Util.IsValid).Distinct(default(HurtBox.EntityEqualityComparer));

                //add one to account for the original victim
                float damage = damageInfo.damage / (enumerable.Count() + (victim?1:0));

                EffectData effectData = new EffectData
                {
                    origin = damageInfo.position,
                };
                EffectManager.SpawnEffect(SparkEffect, effectData, transmit: true);

                //may change
                float procCoefficient = damageInfo.procCoefficient / 2;

                Log.Debug("Distributing " +damageInfo.damage+" damage across "+enumerable.Count()+" extra characters, for "+damage+" damage each.");

                foreach (HurtBox item in enumerable)
                {

                    DamageShareOrb DamageShareOrb = new DamageShareOrb();
                    DamageShareOrb.origin = damageInfo.position;
                    DamageShareOrb.damageValue = damage;
                    DamageShareOrb.damageTypeCombo = damageInfo.damageType;
                    DamageShareOrb.isCrit = damageInfo.crit;
                    DamageShareOrb.teamIndex = team;
                    DamageShareOrb.attacker = damageInfo.attacker;
                    DamageShareOrb.procChainMask = damageInfo.procChainMask;
                    DamageShareOrb.procChainMask.AddModdedProc(procType);
                    DamageShareOrb.procCoefficient = procCoefficient;
                    DamageShareOrb.damageColorIndex = DamageColorIndex.Item;
                    DamageShareOrb.target = item;
                    OrbManager.instance.AddOrb(DamageShareOrb);

                }

                return damage;
            }
        }
    }
}