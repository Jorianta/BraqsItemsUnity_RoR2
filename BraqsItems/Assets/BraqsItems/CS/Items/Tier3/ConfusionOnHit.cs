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
        public static DotController.DotIndex dotIndex;

        private static GameObject distractionOrbEffect;

        internal static void Init()
        {
            if (!ConfigManager.ConfusionOnHit_isEnabled.Value) return;

            Log.Info("Initializing Pieces of Silver Item");
            //ITEM//
            itemDef = GetItemDef("ConfusionOnHit");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //DEBUFF//
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "Betrayed";
            buffDef.canStack = false;
            buffDef.isHidden = false;
            buffDef.isDebuff = true;
            buffDef.buffColor = Color.white;
            buffDef.isCooldown = false;
            buffDef.iconSprite = BraqsItemsMain.assetBundle.LoadAsset<Sprite>("texBuffBetrayedIcon");

            ContentAddition.AddBuffDef(buffDef);

            //Does no damage, but annoys enemies.
            DotController.DotDef dotDef = new DotController.DotDef()
            {
                interval = 0.2f,
                damageCoefficient = 0,
                damageColorIndex = DamageColorIndex.Item,
                associatedBuff = buffDef,
            };
            DotAPI.RegisterDotDef(dotDef);

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
                    new GradientColorKey{color = new Color(0.62f, 0.52f, 0.62f), time = 0.3f},
                    new GradientColorKey{color = new Color(0.43f, 0.42f, 0.66f), time = 1f},
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
                coinParticleMaterial.SetColor("_Color", new Color(0.53f, 0.52f, 0.76f));
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
            if (obj.damageInfo.procCoefficient > 0 && !obj.damageInfo.rejected && obj.attacker && obj.attackerMaster && obj.attackerMaster.inventory && obj.victim && obj.victimBody)
            {
                int stack = obj.attackerMaster.inventory.GetItemCount(itemDef);

                if(stack > 0)
                {    
                    float duration = ConfigManager.ConfusionOnHit_durationBase.Value + (stack - 1) * ConfigManager.ConfusionOnHit_durationPerStack.Value;
                    int distractions = ConfigManager.ConfusionOnHit_enemiesAggroed.Value * stack;

                    DistractionOrb.FireDistractionOrbs(obj.victimBody.gameObject, obj.attacker, distractions, duration);
                    obj.victimBody.AddTimedBuff(buffDef, duration);
                    
                }
            }
        }

        //"Damage" nearby enemies to get their attention.
        private class DistractionOrb : RoR2.Orbs.GenericDamageOrb
        {
            public float overrideDuration = 0.6f;
            public float range = 50f;
            public float buffDurationOnArrival = 0f;
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

            public override void OnArrival()
            {
                base.OnArrival();
                
                //if (target && target.healthComponent && target.healthComponent.body)
                //{
                //    target.healthComponent.body.AddTimedBuff(buffDef, buffDurationOnArrival);
                //}
            }

            //Every target is told the last target was the attacker. returns the last targeted enemy.
            public static void FireDistractionOrbs(GameObject firstHit, GameObject distractor, int count, float buffDuration = 0)
            {
                List<HealthComponent> targets = new List<HealthComponent>();
                if (firstHit.TryGetComponent(out HealthComponent healthComponent)) targets.Add(healthComponent);

                BullseyeSearch search = new BullseyeSearch();

                for (int i = 0; i < count; i++)
                {
                    DistractionOrb distractionOrb = new DistractionOrb();
                    distractionOrb.search = search;
                    distractionOrb.origin = firstHit.transform.position;
                    distractionOrb.buffDurationOnArrival = buffDuration;
                    distractionOrb.attacker = firstHit;
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