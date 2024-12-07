using R2API;
using RoR2;
using RoR2.Orbs;
using static RoR2.DotController;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class HealFromBleed
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.HealFromBleed_isEnabled.Value) return;

            Log.Info("Initializing Leech Jar Item");

            //ITEM//
            itemDef = GetItemDef("HealFromBleed");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();

            Log.Info("Leech Jar Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        //May wan to move this to a different hook.
        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo.rejected && damageInfo.dotIndex == DotController.DotIndex.Bleed || damageInfo.dotIndex == DotController.DotIndex.SuperBleed)
            {
                if (damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.inventory)
                {
                    int stack = attackerBody.inventory.GetItemCount(itemDef);

                    if (stack > 0)
                    {
                        //heal 1% for 100% damage dealt
                        //2 bleed = 1 wungus
                        float percentHeal = (stack - 1) * ConfigManager.HealFromBleed_percentPerStack.Value + ConfigManager.HealFromBleed_percentBase.Value;
                        float heal = percentHeal * attackerBody.maxHealth * damageInfo.damage / (attackerBody.damage);

                        HealOrb healOrb = new HealOrb();
                        healOrb.origin = self.transform.position;
                        healOrb.target = attackerBody.mainHurtBox;
                        healOrb.healValue = heal;

                        OrbManager.instance.AddOrb(healOrb);
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}