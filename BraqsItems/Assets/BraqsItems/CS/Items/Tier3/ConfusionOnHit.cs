using BraqsItems.Misc;
using IL.RoR2.Orbs;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using static BraqsItems.Misc.CharacterEvents;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public class ConfusionOnHit
    {
        public static ItemDef itemDef;
        public static BuffDef buffDef;

        private static GameObject distractionOrbEffect;

        internal static void Init()
        {
            if (!ConfigManager.ExplosionFrenzy_isEnabled.Value) return;

            Log.Info("Initializing Pieces of Silver Item");
            //ITEM//
            itemDef = GetItemDef("ConfusionOnHit");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //DEBUFF//
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "Branded";
            buffDef.canStack = false;
            buffDef.isHidden = false;
            buffDef.isDebuff = true;
            buffDef.buffColor = Color.white;
            buffDef.isCooldown = false;
            buffDef.iconSprite = BraqsItemsMain.assetBundle.LoadAsset<Sprite>("texBuffRendIcon");

            ContentAddition.AddBuffDef(buffDef);

            //EFFECT// 
            distractionOrbEffect = GenerateEffect();
            ContentAddition.AddEffect(distractionOrbEffect);

            Hooks();

            Log.Info("Pieces of Silver Initialized");
        }

        private static GameObject GenerateEffect()
        {

            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GoldOrbEffect.prefab").WaitForCompletion().InstantiateClone("DistractionCoins");
            Texture silverRamp = Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampBrotherPillar.png").WaitForCompletion();

            try
            {
                var trail = prefab.transform.Find("TrailParent/Trail").gameObject.GetComponent<TrailRenderer>();
                var trailMaterial = trail.material;
                trailMaterial.SetTexture("_RemapTex", silverRamp);
            }
            catch(Exception e) { 
                Log.Warning("ConfusionOnHit:GenerateEffect() ; Could not edit coin orb trail." +
                    "\n" + e);
            }

            try
            {
                var core = prefab.transform.Find("VFX/Core").gameObject.GetComponent<ParticleSystem>();
                var coreCol = core.colorOverLifetime;
                Log.Debug(coreCol.color);

                Gradient silverGradient = new Gradient();
                silverGradient.mode = coreCol.color.gradient.mode; ;
                silverGradient.alphaKeys = coreCol.color.gradient.alphaKeys;
                silverGradient.colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey{color = new Color(1f, 1f, 1f), time = 0f},
                    new GradientColorKey{color = new Color(0.94f, 0.84f, 0.88f), time = 0.1f},
                    new GradientColorKey{color = new Color(0.28f, 0.22f, 0.23f), time = 1f},
                };

                coreCol.color = new ParticleSystem.MinMaxGradient(silverGradient);
            }
            catch (Exception e) { Log.Warning("ConfusionOnHit:GenerateEffect() ; Could not edit coin orb core." +
                    "\n" + e); 
            }

            try
            {
                var coinParticle = prefab.transform.Find("VFX/CoinParticle").gameObject.GetComponent<ParticleSystemRenderer>();
                var coinParticleMaterial = coinParticle.material;
                coinParticleMaterial.SetColor("_MainColor", new Color(0.94f, 0.84f, 0.88f));
            }
            catch (Exception e) { Log.Warning("ConfusionOnHit:GenerateEffect() ; Could not edit coin orb particles." + 
                "\n" + e); 
            }

            return prefab;
        }

        private static void Hooks()
        {
            On.RoR2.FriendlyFireManager.ShouldDirectHitProceed += FriendlyFireManager_ShouldDirectHitProceed;
            On.RoR2.FriendlyFireManager.ShouldSplashHitProceed += FriendlyFireManager_ShouldSplashHitProceed;
            On.RoR2.FriendlyFireManager.ShouldSeekingProceed += FriendlyFireManager_ShouldSeekingProceed;

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private static bool FriendlyFireManager_ShouldSeekingProceed(On.RoR2.FriendlyFireManager.orig_ShouldSeekingProceed orig, HealthComponent victim, TeamIndex attackerTeamIndex)
        {
            if (victim.body.HasBuff(buffDef)) return true;
            return orig(victim, attackerTeamIndex);
        }

        private static bool FriendlyFireManager_ShouldSplashHitProceed(On.RoR2.FriendlyFireManager.orig_ShouldSplashHitProceed orig, HealthComponent victim, TeamIndex attackerTeamIndex)
        {
            if (victim.body.HasBuff(buffDef)) return true;
            return orig(victim, attackerTeamIndex);
        }

        private static bool FriendlyFireManager_ShouldDirectHitProceed(On.RoR2.FriendlyFireManager.orig_ShouldDirectHitProceed orig, HealthComponent victim, TeamIndex attackerTeamIndex)
        {
            if (victim.body.HasBuff(buffDef)) return true;
            return orig(victim, attackerTeamIndex);
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (obj.damageInfo.procCoefficient > 0 && !obj.damageInfo.rejected && obj.attacker && obj.attackerMaster && obj.attackerMaster.inventory && obj.victimBody)
            {
                int stack = obj.attackerMaster.inventory.GetItemCount(itemDef);
                if(stack > 0 && RoR2.Util.CheckRoll(5f, obj.attackerMaster))
                {
                    float duration = 5f + (stack - 1) * 2f;
                    int distractions = 10 + (stack - 1) * 5;

                    obj.victimBody.AddTimedBuff(buffDef, duration);

                    DistractionOrb.FireDistractionOrbs(obj.victimBody.gameObject, obj.attacker, distractions);
                }
            }
        }

        //"Damage" nearby enemies to get their attention.
        private class DistractionOrb : RoR2.Orbs.GenericDamageOrb
        {
            public float overrideDuration = 0.6f;
            public float range = 50f;
            private BullseyeSearch search;
            public List<HealthComponent> bouncedObjects;

            public GameObject distractor;
            public TeamIndex distractorTeamIndex;

            public override void Begin()
            {
                duration = overrideDuration;
                scale = 1f;

                EffectData effectData = new EffectData
                {
                    scale = scale,
                    origin = origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(GetOrbEffect(), effectData, transmit: true);

                isCrit = false;
                procCoefficient = 0;
                damageValue = 0;
                damageType = DamageType.Generic;
            }

            public override GameObject GetOrbEffect()
            {
                return distractionOrbEffect;
            }

            public HurtBox PickTarget()
            {
                if(origin == null) return null;
                if (search == null)
                {
                    search = new BullseyeSearch();
                }
                search.searchOrigin = origin;
                search.searchDirection = Vector3.zero;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(distractorTeamIndex);
                search.filterByLoS = false;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.maxDistanceFilter = range;
                search.RefreshCandidates();
                HurtBox hurtBox = (from v in search.GetResults()
                                   where !bouncedObjects.Contains(v.healthComponent)
                                   select v).FirstOrDefault();
                if ((bool)hurtBox)
                {
                    bouncedObjects.Add(hurtBox.healthComponent);
                }
                return hurtBox;
            }

            //The distraction is marked as the attacker to draw attention.
            public static void FireDistractionOrbs(GameObject distraction, GameObject distractor, int count)
            {
                List<HealthComponent> targets = new List<HealthComponent>();
                if (distraction.TryGetComponent(out HealthComponent healthComponent)) targets.Add(healthComponent);

                BullseyeSearch search = new BullseyeSearch();

                for (int i = 0; i < count; i++)
                {
                    DistractionOrb distractionOrb = new DistractionOrb();
                    distractionOrb.search = search;
                    distractionOrb.origin = distraction.transform.position;
                    distractionOrb.attacker = distraction;
                    distractionOrb.distractor = distractor;
                    distractionOrb.distractorTeamIndex = distractor.TryGetComponent(out CharacterBody body) ? body.teamComponent.teamIndex : TeamIndex.None;
                    distractionOrb.bouncedObjects = targets;
                    HurtBox hurtBox = distractionOrb.PickTarget();
                    if ((bool)hurtBox)
                    {
                        distractionOrb.target = hurtBox;
                        targets.Add(hurtBox.healthComponent);
                        RoR2.Orbs.OrbManager.instance.AddOrb(distractionOrb);
                    }

                    else break;

                }
            }
        }
    }
}