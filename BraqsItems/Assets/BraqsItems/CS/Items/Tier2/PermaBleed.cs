using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using RoR2.Orbs;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using static RoR2.DotController;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class PermaBleed
    {
        public static ItemDef itemDef;
        public static BuffDef leechReady;
        public static BuffDef leechDebuff;

        public static bool isEnabled = true;


        internal static void Init()
        {
            if (!isEnabled) return;

            Log.Info("Initializing Leech Jar Item");

            //ITEM//
            itemDef = GetItemDef("PermaBleed");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //BUFFS//
            leechReady = ScriptableObject.CreateInstance<BuffDef>();
            leechReady.name = "Leech Ready";
            leechReady.canStack = true;
            leechReady.isHidden = false;
            leechReady.isDebuff = false;
            leechReady.buffColor = Color.yellow;
            leechReady.isCooldown = false;
            leechReady.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LifestealOnHit/texBuffLifestealOnHitIcon.tif").WaitForCompletion();

            leechDebuff = ScriptableObject.CreateInstance<BuffDef>();
            leechDebuff.name = "Leeched";
            leechDebuff.canStack = false;
            leechDebuff.isHidden = false;
            leechDebuff.isDebuff = true;
            leechDebuff.buffColor = Color.magenta;
            leechDebuff.isCooldown = false;
            leechDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LifestealOnHit/texBuffLifestealOnHitIcon.tif").WaitForCompletion();

            ContentAddition.AddBuffDef(leechReady);
            ContentAddition.AddBuffDef(leechDebuff);

            Hooks();

            Log.Info("Leech Jar Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.DotController.AddDot += DotController_AddDot;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += CharacterBody_AddBuff_BuffIndex;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;

            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void CharacterBody_AddBuff_BuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (buffType == leechDebuff.buffIndex)
            {
                if(DotController.dotControllerLocator.TryGetValue(self.gameObject.GetInstanceID(), out DotController controller))
                {
                    ref List<DotStack> dots = ref controller.dotStackList;
                    int k = 0;
                    for (int count3 = dots.Count; k < count3; k++)
                    {
                        if (dots[k].dotIndex == DotIndex.Bleed || dots[k].dotIndex == DotIndex.SuperBleed)
                        {
                            dots[k].timer = Mathf.Max(dots[k].timer, float.PositiveInfinity);
                        }
                    }
                }
            }

            orig(self, buffType);
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            obj.victim?.GetComponent<BraqsItems_LeechController>()?.OnDeath();
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo.rejected && damageInfo.dotIndex == DotController.DotIndex.Bleed || damageInfo.dotIndex == DotController.DotIndex.SuperBleed)
            {
                if (self.body && self.body.master && self.gameObject.TryGetComponent(out BraqsItems_LeechController component) && component.ownerCharacterBody)
                {
                    //heal 1% for 100% damage dealt
                    component.storeHeal(component.ownerCharacterBody.maxHealth *  damageInfo.damage/(100 * component.ownerCharacterBody.damage));
                }
            }

            orig(self, damageInfo);
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_LeechMaster>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasBuff(leechReady))
            {
                if(attackerBody.TryGetComponent(out BraqsItems_LeechMaster component))
                {
                    component.ApplyLeech(victim);
                }

                //u aint got no leeches
                else attackerBody.SetBuffCount(leechReady.buffIndex, 0);
            }

            orig(self, damageInfo, victim);
        }

        //Maybe change this so it just resets the timer when dot is removed.
        private static void DotController_AddDot(On.RoR2.DotController.orig_AddDot orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            if (self.victimBody.HasBuff(leechDebuff) && (dotIndex == DotController.DotIndex.Bleed || dotIndex == DotController.DotIndex.SuperBleed))
            {
                //if total damage is used, you get a 0 damage bleed, so get that outta here
                orig(self, attackerObject, float.PositiveInfinity, dotIndex, damageMultiplier, maxStacksFromAttacker, null, preUpgradeDotIndex);
            }
            else orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }

        public class BraqsItems_LeechMaster : CharacterBody.ItemBehavior
        {
            private int remainingLeeches;

            private List<BraqsItems_LeechController> victims = new List<BraqsItems_LeechController>();

            private void Start()
            {
                Log.Debug("PermaBleedBehavior:Start()");
                remainingLeeches = stack;
                body.SetBuffCount(leechReady.buffIndex, stack);
            }

            public void FixedUpdate()
            {
                remainingLeeches = Mathf.Max(0, stack - victims.Count);
                body.SetBuffCount(leechReady.buffIndex, remainingLeeches);
            }

            private void OnDestroy()
            {
                Log.Debug("PermaBleedBehavior:OnDestroy()");
                body.SetBuffCount(leechReady.buffIndex, 0);
            }

            public void ApplyLeech(GameObject victim)
            {
                if (remainingLeeches <= 0) return;

                if(!victim.GetComponent<BraqsItems_LeechController>())
                {
                    BraqsItems_LeechController component = BraqsItems_LeechController.AddController(victim, body.gameObject);

                    victims.Add(component);
                }
            }

            public void ReturnLeech(BraqsItems_LeechController victim)
            {
                if (victims.Contains(victim))
                {
                    Log.Debug("Returning leech");
                    victims.Remove(victim);
                }
            }
        }

        public class BraqsItems_LeechController : MonoBehaviour
        {
            public float healingTimer = 0.5f;
            public float storedHeal = 0f;

            public GameObject ownerBodyObject;
            private HealthComponent ownerHealthComponent;
            public CharacterBody ownerCharacterBody;

            private BraqsItems_LeechMaster leechMaster;

            public GameObject targetBodyObject;
            private HealthComponent targetHealthComponent;
            private CharacterBody targetCharacterBody;

            public static BraqsItems_LeechController AddController(GameObject victim, GameObject attacker)
            {
                BraqsItems_LeechController component = victim.AddComponent<BraqsItems_LeechController>();
                component.targetBodyObject = victim;
                component.ownerBodyObject = attacker;
                return component;
            }

            private void Start()
            {
                targetHealthComponent = (targetBodyObject ? targetBodyObject.GetComponent<HealthComponent>() : null);
                targetCharacterBody = (targetBodyObject ? targetBodyObject.GetComponent<CharacterBody>() : null);

                leechMaster = (ownerBodyObject ? ownerBodyObject.GetComponent<BraqsItems_LeechMaster>() : null);
                
                ownerHealthComponent = (ownerBodyObject ? ownerBodyObject.GetComponent<HealthComponent>() : null);
                ownerCharacterBody = (ownerBodyObject ? ownerBodyObject.GetComponent<CharacterBody>() : null);

                targetCharacterBody.AddBuff(leechDebuff);
            }

            private void OnDestroy()
            {
                Log.Debug("Attempting to return leech");
                healAttacker();
                leechMaster.ReturnLeech(this);
            }

            public void OnDeath()
            {
                Destroy(this);
            }

            private void FixedUpdate()
            {

                if (!targetCharacterBody || !targetHealthComponent || !targetCharacterBody.HasBuff(leechDebuff)) Destroy(this);

                healingTimer -= Time.fixedDeltaTime;
                if (healingTimer <= 0f)
                {
                    healingTimer = 0.5f;
                    healAttacker();
                }
            }

            public void storeHeal(float heal)
            {
                storedHeal += heal;
            }

            private void healAttacker()
            {
                if(ownerHealthComponent && storedHeal > 0)
                {
                    HealOrb healOrb = new HealOrb();
                    healOrb.origin = targetBodyObject.transform.position;
                    healOrb.target = ownerCharacterBody.mainHurtBox;
                    healOrb.healValue = storedHeal;

                    OrbManager.instance.AddOrb(healOrb);
                    storedHeal = 0f;
                }
            }
        }
    }
}
