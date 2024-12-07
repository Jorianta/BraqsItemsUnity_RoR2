using R2API;
using RoR2;
using On.RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class LightningDamageBoost
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            if (!ConfigManager.InductionCoil_isEnabled.Value) return;

            Log.Info("Initializing Induction Coil Item");
            //ITEM//
            itemDef = GetItemDef("LightningDamageBoost");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();

            Log.Info("Induction Coil Initialized");
        }

        public static void Hooks()
        {
            LightningOrb.Begin += LightningOrb_Begin;
            VoidLightningOrb.Begin += VoidLightningOrb_Begin;
        }

        //void lightning too!
        private static void VoidLightningOrb_Begin(VoidLightningOrb.orig_Begin orig, RoR2.Orbs.VoidLightningOrb self)
        {
            Log.Debug("LightningDamageBoost:VoidLightningOrb_Begin");

            if (self.attacker && self.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int count = body.inventory.GetItemCount(itemDef);

                if (count > 0)
                {
                    self.damageValue *= 1 + (count - 1) * ConfigManager.InductionCoil_damageBonusPerStack.Value + ConfigManager.InductionCoil_damageBonusBase.Value;
                    Log.Debug("Chain damage = " + self.damageValue);
                }
            }

            orig(self);
        }

        //There IS a damagecoefficientperbounce attribute that does this.
        //Unfortunately, every bounce instantiates a new lightning orb that calls begin, so any changes to this number are applied EVERY BOUNCE
        private static void LightningOrb_Begin(LightningOrb.orig_Begin orig, RoR2.Orbs.LightningOrb self)
        {
            Log.Debug("LightningDamageBoost:LightningOrb_Begin");

            if (self.attacker && self.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int count = body.inventory.GetItemCount(itemDef);

                if (count > 0)
                {
                    self.damageValue *= 1 + (count - 1) * ConfigManager.InductionCoil_damageBonusPerStack.Value + ConfigManager.InductionCoil_damageBonusBase.Value;
                    Log.Debug("Chain damage = " + self.damageValue);
                }
            }

            orig(self);
        }
    }
}
