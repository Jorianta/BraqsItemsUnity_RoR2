using RoR2;
using R2API;
using UnityEngine;
using RoR2.UI;
using static BraqsItems.Misc.CharacterEvents;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    public class RandomSkillBoost
    {
        public static ItemDef itemDef;
        public static BuffDef primaryBoost;
        public static BuffDef secondaryBoost;
        public static BuffDef utilityBoost;
        public static BuffDef specialBoost;
        public static BuffDef hiddenPrimaryBoost;
        public static BuffDef hiddenSecondaryBoost;
        public static BuffDef hiddenUtilityBoost;
        public static BuffDef hiddenSpecialBoost;

        internal static void Init()
        {
            if (!ConfigManager.AttackSpeedOnHit_isEnabled.Value) return;

            Log.Info("Initializing Simon Says Item");

            //ITEM//
            itemDef = GetItemDef("RandomSkillBoost");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //BUFFS//
            primaryBoost = InitializeBuff(primaryBoost,"Primary");
            secondaryBoost = InitializeBuff(secondaryBoost,"Secondary");
            utilityBoost = InitializeBuff(utilityBoost, "Utility");
            specialBoost = InitializeBuff(specialBoost, "Special");
            hiddenPrimaryBoost = InitializeHiddenBuff("Primary");
            hiddenSecondaryBoost = InitializeHiddenBuff("Secondary");
            hiddenUtilityBoost = InitializeHiddenBuff("Utility");
            hiddenSpecialBoost = InitializeHiddenBuff("Special");

            Hooks();

            Log.Info("Simon Says Initialized");
        }

        private static BuffDef InitializeBuff(BuffDef def, string name)
        {
            def = ScriptableObject.CreateInstance<BuffDef>();
            def.name = name +" Boost";
            def.canStack = false;
            def.isHidden = false;
            def.isDebuff = false;
            def.buffColor = Color.white;
            def.isCooldown = false;
            def.iconSprite = BraqsItemsMain.assetBundle.LoadAsset<Sprite>("texBuff"+name+"BoostIcon");
            ContentAddition.AddBuffDef(def);
            return def;
        }
        private static BuffDef InitializeHiddenBuff(string name)
        {
            BuffDef def = new BuffDef();
            def = ScriptableObject.CreateInstance<BuffDef>();
            def.name = name + " Boost";
            def.canStack = false;
            def.isHidden = true;
            def.ignoreGrowthNectar = true;
            def.isDebuff = false;
            def.buffColor = Color.white;
            def.isCooldown = false;
            ContentAddition.AddBuffDef(def);
            return def;
        }


        private static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            //keep reduced cooldown until next restock so it isn't useless.
            if (self.skillLocator)
            {
                if (buffDef == primaryBoost) self.AddTimedBuff(hiddenPrimaryBoost, self.skillLocator.primary.cooldownRemaining);
                
                if (buffDef == secondaryBoost) self.AddTimedBuff(hiddenSecondaryBoost, self.skillLocator.secondary.cooldownRemaining);
                
                if (buffDef == utilityBoost) self.AddTimedBuff(hiddenUtilityBoost, self.skillLocator.utility.cooldownRemaining);
                
                if (buffDef == specialBoost) self.AddTimedBuff(hiddenSpecialBoost, self.skillLocator.special.cooldownRemaining);
                
            }

            orig(self, buffDef);
        }

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo != null && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int stack = body.inventory.GetItemCount(itemDef);
                if (stack > 0)
                {
                    float multiplier = 1f + 0.2f * stack;
                    if (SkillMatch(damageInfo.damageType.damageSource, body))
                    {
                        damageInfo.damage *= multiplier;
                        damageInfo.damageColorIndex = DamageColorIndex.Nearby;
                    }
                }
            }

            orig(self, damageInfo);
        }

        private static bool SkillMatch(DamageSource source, CharacterBody body)
        {
            if (source.HasFlag(DamageSource.Primary) && body.HasBuff(primaryBoost)) return true;
            if (source.HasFlag(DamageSource.Secondary) && body.HasBuff(secondaryBoost)) return true;
            if (source.HasFlag(DamageSource.Utility) && body.HasBuff(utilityBoost)) return true;
            if (source.HasFlag(DamageSource.Special) && body.HasBuff(specialBoost)) return true;
            else return false;
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            self.AddItemBehavior<BraqsItems_RandomSkillBoostBehavior>(self.inventory.GetItemCount(itemDef));
            orig(self);
        }


        public static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                int stack = sender.inventory.GetItemCount(itemDef);
                if (stack > 0)
                {
                    float hyperbolic = (stack - 1) * 0.1f / ((stack - 1) * 0.1f + 1);
                    float cooldownReduction = -0.1f - hyperbolic;

                    if (sender.HasBuff(primaryBoost) || sender.HasBuff(hiddenPrimaryBoost)) args.primaryCooldownMultAdd += cooldownReduction;
                    if (sender.HasBuff(secondaryBoost) || sender.HasBuff(hiddenSecondaryBoost)) args.secondaryCooldownMultAdd += cooldownReduction;
                    if (sender.HasBuff(utilityBoost) || sender.HasBuff(hiddenUtilityBoost)) args.utilityCooldownMultAdd += cooldownReduction;
                    if (sender.HasBuff(specialBoost) || sender.HasBuff(hiddenSpecialBoost)) args.specialCooldownMultAdd += cooldownReduction;
                }
            }
        }

        public class BraqsItems_RandomSkillBoostBehavior : CharacterBody.ItemBehavior
        {
            private const float buffTimer = 5f;
            private SkillSlot currentBoostedSkill;

            //using our own rng to prevent differences in runs using the same seed.
            public Xoroshiro128Plus skillBoostRng;
            private HUD bodyHud;

            private void Start()
            {
                Log.Debug("RandomSkillBoostBehavior:Start()");
                foreach (HUD hud in HUD.readOnlyInstanceList)
                {
                    if (hud.targetBodyObject == body.gameObject)
                    {
                        bodyHud = hud;
                    }
                }
            }

            private void FixedUpdate()
            {
                if (!( body.HasBuff(primaryBoost) || body.HasBuff(secondaryBoost) || body.HasBuff(utilityBoost) || body.HasBuff(specialBoost) ))
                {
                    Log.Debug("Adding Random Skill Boost"); 
                    skillBoostRng ??= new(Run.instance.seed);

                    SkillSlot skill = (SkillSlot)skillBoostRng.RangeInt(0, 4);
                    Log.Debug($"Boosting {skill}");
                    switch (skill)
                    {
                        case SkillSlot.Primary:
                            { 
                                body.AddTimedBuff(primaryBoost, buffTimer);
                                body.SetBuffCount(hiddenPrimaryBoost.buffIndex, 0);
                                break; 
                            }
                        case SkillSlot.Secondary: 
                            { 
                                body.AddTimedBuff(secondaryBoost, buffTimer);
                                body.SetBuffCount(hiddenSecondaryBoost.buffIndex, 0);
                                break; 
                            }
                        case SkillSlot.Utility:
                            {
                                body.AddTimedBuff(utilityBoost, buffTimer);
                                body.SetBuffCount(hiddenUtilityBoost.buffIndex, 0);
                                break;
                            }
                        case SkillSlot.Special: 
                            { 
                                body.AddTimedBuff(specialBoost, buffTimer);
                                body.SetBuffCount(hiddenSpecialBoost.buffIndex, 0);
                                break; 
                            }
                        default: Log.Error("RandomSkillBoostBehavior: Non-skill selected for boost."); break;
                    }

                    currentBoostedSkill = skill;
                }
            }

            private void UpdateSkillIcons()
            {
                if(!bodyHud) return;
                foreach (var skill in bodyHud.skillIcons)
                {
                    if(skill.targetSkillSlot == currentBoostedSkill)
                    {

                    }
                    else
                    {

                    }
                }
            }

            private void OnDestroy()
            {
                Log.Debug("RandomSkillBoostBehavior:OnDestroy()");
                body.SetBuffCount(primaryBoost.buffIndex, 0);
                body.SetBuffCount(secondaryBoost.buffIndex, 0);
                body.SetBuffCount(utilityBoost.buffIndex, 0);
                body.SetBuffCount(specialBoost.buffIndex, 0);
            }
        }
    }
}