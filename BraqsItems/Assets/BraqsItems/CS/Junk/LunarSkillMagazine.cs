using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static BraqsItems.Misc.CharacterEvents;
using static BraqsItems.Util.Helpers;
using RoR2.Skills;
using System;
using static BraqsItems.RandomSkillBoost;

namespace BraqsItems.Junk
{
    public class LunarSkillMagazine
    {
        public static ItemDef itemDef;

        public static SkillDef skillDef;

        public static BuffDef primaryLock;
        public static BuffDef secondaryLock;
        public static BuffDef utilityLock;
        public static BuffDef specialLock;


        internal static void Init()
        {
            if (!ConfigManager.AttackSpeedOnHit_isEnabled.Value) return;

            Log.Info("LunarSkillMagazine Item");

            //ITEM//
            itemDef = GetItemDef("LunarSkillMagazine");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //SKILL//
            skillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC2/Common/DisabledSkill.asset").WaitForCompletion();

            //BUFFS//
            primaryLock = InitializeBuff("Primary");
            secondaryLock = InitializeBuff("Secondary");
            utilityLock = InitializeBuff("Utility");
            specialLock = InitializeBuff("Special");

            Hooks();

            Log.Info("LunarSkillMagazine Initialized");
        }

        private static BuffDef InitializeBuff(string name)
        {
            BuffDef def = new();

            def = ScriptableObject.CreateInstance<BuffDef>();
            def.name = name + " Skill Lock";
            def.canStack = false;
            def.isHidden = true;
            def.isDebuff = false;
            def.buffColor = Color.white;
            def.isCooldown = false;
            def.iconSprite = BraqsItemsMain.assetBundle.LoadAsset<Sprite>("texBuff" + name + "BoostIcon");
            ContentAddition.AddBuffDef(def);
            return def;
        }

        private static void Hooks()
        {
            
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }


        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self && self.inventory)
            {
                int stack = self.inventory.GetItemCount(itemDef);

                if (stack > 0)
                {
                    //self.skillLocator.primaryBonusStockSkill.bonusStockFromBody *= stack + 1;
                    if ((bool)self.skillLocator.primary && self.skillLocator.primaryBonusStockSkill)
                    {
                        self.skillLocator.primaryBonusStockSkill.SetBonusStockFromBody(0);
                        self.skillLocator.primaryBonusStockSkill.SetBonusStockFromBody(self.skillLocator.primaryBonusStockSkill.maxStock * (stack + 1));
                    }
                    if ((bool)self.skillLocator.secondary)
                    {
                        self.skillLocator.secondaryBonusStockSkill.SetBonusStockFromBody(self.skillLocator.secondaryBonusStockSkill.maxStock * (stack + 1));
                    }
                    if ((bool)self.skillLocator.utility)
                    {
                        self.skillLocator.utilityBonusStockSkill.SetBonusStockFromBody(self.skillLocator.utilityBonusStockSkill.maxStock * (stack + 1));
                    }
                    if ((bool)self.skillLocator.utility)
                    {
                        self.skillLocator.specialBonusStockSkill.SetBonusStockFromBody(self.skillLocator.specialBonusStockSkill.maxStock * (stack + 1));
                    }
                }
            }
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                int stack = sender.inventory.GetItemCount(itemDef);
                if (stack > 0)
                {
                    float cooldownReduction = -Mathf.Pow(0.5f, stack);

                    args.primaryCooldownMultAdd += cooldownReduction;
                    args.secondaryCooldownMultAdd += cooldownReduction;
                    args.utilityCooldownMultAdd += cooldownReduction;
                    args.specialCooldownMultAdd += cooldownReduction;
                }
            }
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_LunarSkillMagazineBehavior>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }

        public class BraqsItems_LunarSkillMagazineBehavior : CharacterBody.ItemBehavior
        {
            private const float buffTimer = 5f;
            private SkillSlot currentSkillLock;

            private void Start()
            {
                Log.Debug("RandomSkillBoostBehavior:Start()");
            }

            private void FixedUpdate()
            {
                if (!(body.HasBuff(primaryLock) || body.HasBuff(secondaryLock) || body.HasBuff(utilityLock) || body.HasBuff(specialLock)))
                {
                    currentSkillLock = (SkillSlot)(((int)currentSkillLock + 1) % 4);

                    switch (currentSkillLock)
                    {
                        case SkillSlot.Primary:
                            {
                                body.AddTimedBuff(primaryLock, buffTimer);
                                break;
                            }
                        case SkillSlot.Secondary:
                            {
                                body.AddTimedBuff(secondaryLock, buffTimer);
                                break;
                            }
                        case SkillSlot.Utility:
                            {
                                body.AddTimedBuff(utilityLock, buffTimer);
                                break;
                            }
                        case SkillSlot.Special:
                            {
                                body.AddTimedBuff(specialLock, buffTimer);
                                break;
                            }
                        default: Log.Error("RandomSkillBoostBehavior: Non-skill selected for boost."); break;
                    }

                    Log.Debug("LunarSkillMagazine: Locking " + currentSkillLock);
                    SetSkillLock(currentSkillLock);
                }
            }

            private void SetSkillLock(SkillSlot skillSlot)
            {
                SkillDef skillDef = LegacyResourcesAPI.Load<SkillDef>("Skills/DisabledSkills");

                if (!(skillSlot == SkillSlot.Primary)) body.skillLocator.primary.SetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                else body.skillLocator.primary.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                if (!(skillSlot == SkillSlot.Secondary)) body.skillLocator.secondary.SetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                else body.skillLocator.secondary.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                if (!(skillSlot == SkillSlot.Utility)) body.skillLocator.utility.SetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                else body.skillLocator.utility.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                if (!(skillSlot == SkillSlot.Special)) body.skillLocator.special.SetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                else body.skillLocator.special.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
            }

            private void RemoveSkillLock()
            {
                body.skillLocator.primary.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                
                body.skillLocator.secondary.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
                
                body.skillLocator.utility.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);

                body.skillLocator.special.UnsetSkillOverride(body, skillDef, GenericSkill.SkillOverridePriority.Replacement);
            }

            private void OnDestroy()
            {
                Log.Debug("RandomSkillBoostBehavior:OnDestroy()");
                body.SetBuffCount(primaryLock.buffIndex, 0);
                body.SetBuffCount(secondaryLock.buffIndex, 0);
                body.SetBuffCount(utilityLock.buffIndex, 0);
                body.SetBuffCount(specialLock.buffIndex, 0);
                RemoveSkillLock();
            }
        }
    }
}