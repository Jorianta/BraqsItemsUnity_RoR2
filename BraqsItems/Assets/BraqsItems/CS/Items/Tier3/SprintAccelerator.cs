using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static BraqsItems.ExplosionFrenzy;
using static BraqsItems.AttackSpeedOnHit;
using System.Linq;
using UnityEngine.Networking;

namespace BraqsItems
{
    internal class SprintAccelerator
    {
        public static ItemDef itemDef;

        public static bool isEnabled = true;
        public static float rangeAtFullCharge = 200f;
        public static float damageAtFullCharge = 5000f;

        private static GameObject fireEffectPrefab;

        internal static void Init()
        {
            Log.Info("Initializing Decommissioned Turbine Item");
            //ITEM//
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "SPRINTACCELERATOR";
            itemDef.nameToken = "ITEM_SPRINTACCELERATOR_NAME";
            itemDef.pickupToken = "ITEM_SPRINTACCELERATOR_PICKUP";
            itemDef.descriptionToken = "ITEM_SPRINTACCELERATOR_DESC";
            itemDef.loreToken = "ITEM_SPRINTACCELERATOR_LORE";

            itemDef.AutoPopulateTokens();

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
            };

            itemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC2/Items/IncreasePrimaryDamage/texBuffIncreasePrimaryDamageIcon.png").WaitForCompletion();
            itemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/IncreasePrimaryDamage/PickupIncreasePrimaryDamage.prefab").WaitForCompletion();

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

            //EFFECTS
            
            fireEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/ChargeSonicBoom.prefab").WaitForCompletion();

            Hooks();

            Log.Info("Decommissioned Turbine Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients1;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients1(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.TryGetComponent(out BraqsItems_SprintAccelerator component))
            {
                args.moveSpeedMultAdd += component.charge/100f;
            }
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_SprintAccelerator>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }

        public class BraqsItems_SprintAccelerator : CharacterBody.ItemBehavior
        {
            public float charge;
            public float maxCharge;
            public float chargeRate = 5f;
            private bool sprinting = false;
            private InputBankTest inputBank;

            private bool coolDown = false;
            private float baseCoolDownTimer;
            private float coolDownTimer = 0;
            private float statsUpdateTimer = 0.5f;

            private void Start()
            {
                Log.Debug("SprintAccelerator:Start()");
                inputBank = GetComponent<InputBankTest>();
                maxCharge = stack * 100f;
                baseCoolDownTimer = 10f * Mathf.Pow(0.5f, stack - 1);
            }

            private void OnDestroy()
            {
                Log.Debug("SprintAccelerator:OnDestroy()");
            }

            private void FixedUpdate()
            {
                if(body.isSprinting && !sprinting) sprinting = true;

                //Detect stop
                if(!coolDown && !body.isSprinting && sprinting)
                {
                    FireAirBlast();
                    charge = 0;
                    coolDownTimer = baseCoolDownTimer;
                    coolDown = true;
                    sprinting = false;
                    body.RecalculateStats();
                }

                //Chargeup
                if (!coolDown && sprinting && charge < maxCharge)
                {
                    float temp = charge + Time.fixedDeltaTime * chargeRate;
                    charge = Math.Min(maxCharge, temp);
                    if (charge == maxCharge) Log.Debug("SprintAccelerator: FULL CHARGE!");

                    statsUpdateTimer -= Time.fixedDeltaTime;
                    if(statsUpdateTimer <= 0)
                    {
                        Log.Debug("SprintAccelerator: Applying Boost");
                        body.RecalculateStats();
                        statsUpdateTimer = 0.5f;
                    }
                }

                //Cooldown
                if (coolDown)
                {
                    coolDownTimer -= Time.fixedDeltaTime;
                    if (coolDownTimer <= 0)
                    {
                        Log.Debug("Cooldown Complete");
                        coolDown = false;
                    }
                }
            }

            //Thanks REX!
            private void FireAirBlast()
            {
                float percentCharge = charge / maxCharge;

                float distance = rangeAtFullCharge * percentCharge;
                float damageCoefficient = 10f * percentCharge;
                float horizontalForce = 40000f * percentCharge;
                float verticalLift = 2000f * percentCharge;

                float blastConeAngle = 90f;

                //RoR2.Util.PlaySound(sound, base.gameObject);
                bool flag = body.TryGetComponent(out CharacterMotor motor);
                if (!flag) return;

                //Only the horizontal components
                Vector3 blastDirection = motor.velocity;
                blastDirection.z = 0;
                Ray blastRay = new Ray (body.corePosition, blastDirection);


                EffectManager.SpawnEffect(fireEffectPrefab, new EffectData
                {
                    origin = blastRay.origin,
                    rotation = Quaternion.LookRotation(blastRay.direction),
                    scale = charge
                }, transmit: false);

                //blastRay.origin -= blastRay.direction * backupDistance;
                if (NetworkServer.active)
                {
                    BullseyeSearch bullseyeSearch = new BullseyeSearch();
                    bullseyeSearch.teamMaskFilter = TeamMask.all;
                    bullseyeSearch.maxAngleFilter = blastConeAngle * 0.5f;
                    bullseyeSearch.maxDistanceFilter = distance;
                    bullseyeSearch.searchOrigin = blastRay.origin;
                    bullseyeSearch.searchDirection = blastRay.direction;
                    bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
                    bullseyeSearch.filterByLoS = false;
                    bullseyeSearch.RefreshCandidates();
                    bullseyeSearch.FilterOutGameObject(body.gameObject);
                    IEnumerable<HurtBox> enumerable = bullseyeSearch.GetResults().Where(RoR2.Util.IsValid).Distinct(default(HurtBox.EntityEqualityComparer));
                    TeamIndex team = body.teamComponent.teamIndex;
                    foreach (HurtBox item in enumerable)
                    {
                        if (FriendlyFireManager.ShouldSplashHitProceed(item.healthComponent, team))
                        {
                            Vector3 vector = item.transform.position - blastRay.origin;
                            float magnitude = vector.magnitude;
                            Vector3 vector2 = vector / magnitude;

                            float mass = 1f;
                            CharacterBody body = item.healthComponent.body;
                            if (body.characterMotor)
                            {
                                mass = body.characterMotor.mass;
                            }
                            else if ((bool)item.healthComponent.GetComponent<Rigidbody>())
                            {
                                mass = body.rigidbody.mass;
                            }
                            float acceleration = body.acceleration;
                            Vector3 vector3 = vector2;
                            vector3 *= horizontalForce;
                            vector3.y = verticalLift;
                            DamageInfo damageInfo = new DamageInfo
                            {
                                attacker = base.gameObject,
                                damage = body.damage * damageCoefficient,
                                position = item.transform.position,
                                procCoefficient = 1.0f
                            };
                            Log.Debug("SprintAccelerator: FIRE");
                            item.healthComponent.TakeDamageForce(vector3/mass, alwaysApply: true, disableAirControlUntilCollision: true);
                            item.healthComponent.TakeDamage(damageInfo);
                            GlobalEventManager.instance.OnHitEnemy(damageInfo, item.healthComponent.gameObject);
                        }
                    }
                }
            }
        }
    }
}
