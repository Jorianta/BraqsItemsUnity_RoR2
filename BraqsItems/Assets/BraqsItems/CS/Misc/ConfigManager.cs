using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using UnityEngine;


namespace BraqsItems
{
    public class ConfigManager
    {
        public static ConfigEntry<string> ConfigVersion;
        #region white
        //HundredsAndThousands
        public static ConfigEntry<bool> AttackSpeedOnHit_isEnabled;
        public static ConfigEntry<float> AttackSpeedOnHit_percentPerStack;
        public static ConfigEntry<float> AttackSpeedOnHit_percentBase;
        //Accelerant
        public static ConfigEntry<bool> BiggerExplosions_isEnabled;
        public static ConfigEntry<float> BiggerExplosions_percentPerStack;
        public static ConfigEntry<float> BiggerExplosions_percentBase;
        //JumperCables
        public static ConfigEntry<bool> LightningOnOverkill_isEnabled;
        public static ConfigEntry<float> LightningOnOverkill_damagePercentPerStack;
        public static ConfigEntry<float> LightningOnOverkill_damagePercentBase;
        public static ConfigEntry<int> LightningOnOverkill_bouncePerStack;
        public static ConfigEntry<int> LightningOnOverkill_bounceBase;
        public static ConfigEntry<float> LightningOnOverkill_rangePerStack;
        public static ConfigEntry<float> LightningOnOverkill_rangeBase;
        //VOID
        //JumperCables
        public static ConfigEntry<bool> LightningOnOverkillVoid_isEnabled;
        public static ConfigEntry<float> LightningOnOverkillVoid_damagePercentPerStack;
        public static ConfigEntry<float> LightningOnOverkillVoid_damagePercentBase;
        public static ConfigEntry<int> LightningOnOverkillVoid_hitsPerStack;
        public static ConfigEntry<int> LightningOnOverkillVoid_hitsBase;
        #endregion
        #region green
        //Bomblet
        public static ConfigEntry<bool> ExplodeAgain_isEnabled;
        public static ConfigEntry<float> ExplodeAgain_chance;
        public static ConfigEntry<float> ExplodeAgain_radiusCoefficient;
        public static ConfigEntry<float> ExplodeAgain_damageCoefficient;
        public static ConfigEntry<int> ExplodeAgain_maxBombsBase;
        public static ConfigEntry<int> ExplodeAgain_maxBombsPerStack;
        public static ConfigEntry<bool> ExplodeAgain_ignoreProcCoefficient;
        //Leeches
        public static ConfigEntry<bool> HealFromBleed_isEnabled;
        public static ConfigEntry<float> HealFromBleed_percentBase;
        public static ConfigEntry<float> HealFromBleed_percentPerStack;
        //Refabricator
        public static ConfigEntry<bool> RepairBrokenItems_isEnabled;
        public static ConfigEntry<int> RepairBrokenItems_repairsBase;
        public static ConfigEntry<int> RepairBrokenItems_repairsPerStack;
        public static ConfigEntry<float> RepairBrokenItems_whiteChance;
        public static ConfigEntry<float> RepairBrokenItems_defaultChance;
        public static ConfigEntry<float> RepairBrokenItems_redChance;
        //Bison Pills
        public static ConfigEntry<bool> SkillSaver_isEnabled;
        public static ConfigEntry<float> SkillSaver_chance;
        //VOID
        //Tick Queen
        public static ConfigEntry<bool> HealFromBleedVoid_isEnabled;
        public static ConfigEntry<float> HealFromBleedVoid_percentBase;
        public static ConfigEntry<float> HealFromBleedVoid_percentPerStack;
        public static ConfigEntry<float> HealFromBleedVoid_radiusBase;
        public static ConfigEntry<float> HealFromBleedVoid_radiusPerStack;
        #endregion
        #region red
        //Pieces of Silver
        public static ConfigEntry<bool> ConfusionOnHit_isEnabled;
        public static ConfigEntry<float> ConfusionOnHit_chanceBase;
        public static ConfigEntry<float> ConfusionOnHit_chancePerStack;
        public static ConfigEntry<float> ConfusionOnHit_durationBase;
        public static ConfigEntry<float> ConfusionOnHit_durationPerStack;
        public static ConfigEntry<int> ConfusionOnHit_enemiesAggroed;
        public static ConfigEntry<bool> ConfusionOnHit_spreadDebuff;
        //Manifesto
        public static ConfigEntry<bool> ExplosionFrenzy_isEnabled;
        public static ConfigEntry<float> ExplosionFrenzy_igniteDamageBase;
        public static ConfigEntry<float> ExplosionFrenzy_igniteDamagePerStack;
        public static ConfigEntry<float> ExplosionFrenzy_bonusPerBurn;
        public static ConfigEntry<int> ExplosionFrenzy_bonusCapBase;
        public static ConfigEntry<int> ExplosionFrenzy_bonusCapPerStack;
        //HundredRendingFists
        public static ConfigEntry<bool> HundredRendingFists_isEnabled;
        public static ConfigEntry<float> HundredRendingFists_rendDuration;
        public static ConfigEntry<float> HundredRendingFists_storedDamagePerStack;
        public static ConfigEntry<float> HundredRendingFists_storedDamageBase;
        public static ConfigEntry<float> HundredRendingFists_storeBonus;
        //InductionCoil
        public static ConfigEntry<bool> InductionCoil_isEnabled;
        public static ConfigEntry<float> InductionCoil_damageBonusBase;
        public static ConfigEntry<float> InductionCoil_damageBonusPerStack;
        #endregion


        public static void Init(string configPath)
        {
            //Thanks to Hex3, who's mod I referenced heavily for this.

            var Config = new ConfigFile(Path.Combine(configPath, "braquen-BraqsItems.cfg"), true);

            //Remake Config on minor update
            ConfigVersion = Config.Bind("Version", "Version", BraqsItemsMain.VERSION, "Don't touch. Used to determine if a config file is out of date.");
            var ConfigMinor = ConfigVersion.Value.Split('.')[0] +'.'+ ConfigVersion.Value.Split('.')[1];
            var Minor = BraqsItemsMain.VERSION.Split('.')[0] + '.' + BraqsItemsMain.VERSION.Split('.')[1];
            if (ConfigMinor != Minor)
            {
                Config.Clear();
                Config.Save();
                ConfigVersion = Config.Bind("Version", "Version", BraqsItemsMain.VERSION, "Don't touch. Used to determine if a config file is out of date.");
            }
            #region white
            AttackSpeedOnHit_isEnabled = Config.Bind("ATTACKSPEEDONHIT", "Enable", true, "Load this item.");
            AttackSpeedOnHit_percentBase = Config.Bind("ATTACKSPEEDONHIT", "Base Attack Speed Bonus", 0.01f, "Attack speed bonus gained on hit with one stack.");
            AttackSpeedOnHit_percentPerStack = Config.Bind("ATTACKSPEEDONHIT", "Attack Speed Bonus Per Stack", 0.01f, "Attack speed bonus gained on hit per additional stack.");

            BiggerExplosions_isEnabled = Config.Bind("BIGGEREXPLOSIONS", "Enable", true, "Load this item.");
            BiggerExplosions_percentBase = Config.Bind("BIGGEREXPLOSIONS", "Base Blast Radius Increase", 0.05f, "Blast radius increase with one stack.");
            BiggerExplosions_percentPerStack = Config.Bind("BIGGEREXPLOSIONS", "Attack Speed Bonus Per Stack", 0.05f, "Blast radius increase per additional stack.");

            LightningOnOverkill_isEnabled = Config.Bind("LIGHTNINGONOVERKILL", "Enable", true, "Load this item.");
            LightningOnOverkill_damagePercentPerStack = Config.Bind("LIGHTNINGONOVERKILL", "Damage Per Stack", 0f, "Excess damage per additional stack.");
            LightningOnOverkill_damagePercentBase = Config.Bind("LIGHTNINGONOVERKILL", "Base Damage", 1f, "Fraction of excess damage from a kill dealt to nearby enemies with one stack.");
            LightningOnOverkill_bouncePerStack = Config.Bind("LIGHTNINGONOVERKILL", "Bounces Per Stack", 1, "Number of enemies hit per additional stack.");
            LightningOnOverkill_bounceBase = Config.Bind("LIGHTNINGONOVERKILL", "Base Bounces", 3, "Number of enemies hit with one stack.");
            LightningOnOverkill_rangePerStack = Config.Bind("LIGHTNINGONOVERKILL", "Range Per Stack", 3f, "Range of the lightning per additional stack.");
            LightningOnOverkill_rangeBase = Config.Bind("LIGHTNINGONOVERKILL", "Range", 12f, "Range of the lightning with one stack.");

            LightningOnOverkillVoid_isEnabled = Config.Bind("LIGHTNINGONOVERKILLVOID", "Enable", true, "Load this item.");
            LightningOnOverkillVoid_damagePercentPerStack = Config.Bind("LIGHTNINGONOVERKILLVOID", "Damage Per Stack", 0.5f, "Maximum possible bonus base damage per additional stack.");
            LightningOnOverkillVoid_damagePercentBase = Config.Bind("LIGHTNINGONOVERKILLVOID", "Base Damage", 3f, "Maximum possible bonus base damage dealt on the next hit after kill.");
            LightningOnOverkillVoid_hitsPerStack = Config.Bind("LIGHTNINGONOVERKILLVOID", "Hits Per Stack", 1, "Number of hits that recieve bonus damage per additional stack.");
            LightningOnOverkillVoid_hitsBase = Config.Bind("LIGHTNINGONOVERKILLVOID", "Hits with bonus", 2, "Number of hits that recieve bonus damage after kill.");
            #endregion

            #region green
            ExplodeAgain_isEnabled = Config.Bind("EXPLODEAGAIN", "Enable", true, "Load this item.");
            ExplodeAgain_chance = Config.Bind("EXPLODEAGAIN", "Bomblet Chance", 25f, "The chance to fire a bomblet. This chance is rolled repeatedly to determine how many bomblets to fire.");
            ExplodeAgain_maxBombsBase = Config.Bind("EXPLODEAGAIN", "Number of Bombs", 3, "The maximum number of bomblets that can be fired with one stack.");
            ExplodeAgain_maxBombsPerStack = Config.Bind("EXPLODEAGAIN", "Number of Bombs Per Stack", 2, "The number of additinal bomblets that can be fired per additional stack.");
            ExplodeAgain_radiusCoefficient = Config.Bind("EXPLODEAGAIN", "Radius Coefficient", 0.75f, "Multiplied by the radius of the initial explosion to determine the bomblet's blast radius.");
            ExplodeAgain_damageCoefficient = Config.Bind("EXPLODEAGAIN", "Damage Coefficient", 0.75f, "Multiplied by the damage of the initial explosion to determine the bomblet's damage.");
            ExplodeAgain_ignoreProcCoefficient = Config.Bind("EXPLODEAGAIN", "Ignore Proc Coefficients", false, "When enabled, bomblets have the same chance to fire regardless of the proc coefficient of the initial explosion. This allows Behemoth and Sticky bomb to fire bomblets. Turn on at your own risk.");

            HealFromBleed_isEnabled = Config.Bind("HEALFROMBLEED", "Enable", true, "Load this item.");
            HealFromBleed_percentBase = Config.Bind("HEALFROMBLEED", "Percent Health Healed", 0.01f, "Fraction of health healed for every 100% bleed damage with one stack.");
            HealFromBleed_percentPerStack = Config.Bind("HEALFROMBLEED", "Percent Health Healed Per Stack", 0.01f, "Fraction of health healed for every 100% bleed damage per additional stack.");

            RepairBrokenItems_isEnabled = Config.Bind("REPAIRBROKENITEMS", "Enable", true, "Load this item.");
            RepairBrokenItems_repairsBase = Config.Bind("REPAIRBROKENITEMS", "Base Repair Attempts", 3, "Number of repairs attempted on stage advance with one stack.");
            RepairBrokenItems_repairsPerStack = Config.Bind("REPAIRBROKENITEMS", "Repair Attempts Per Stack", 3, "Number of repairs attempted on stage advance per additional stack.");
            RepairBrokenItems_whiteChance = Config.Bind("REPAIRBROKENITEMS", "White Success Chance", 100f, "Repair success chance for white/void white items.");
            RepairBrokenItems_redChance = Config.Bind("REPAIRBROKENITEMS", "Red Success Chance", 50f, "Repair success chance for red/void red items.");
            RepairBrokenItems_defaultChance = Config.Bind("REPAIRBROKENITEMS", "Default Success Chance", 75f, "Repair success chance for all other items.");
            
            SkillSaver_isEnabled = Config.Bind("SKILLSAVER", "Enable", true, "Load this item.");
            SkillSaver_chance = Config.Bind("SKILLSAVER", "Charge Save Chance", 10f, "Chance to not use a skill/equipment charge per stack of this item. Hyperbolic.");
            //Void
            HealFromBleedVoid_isEnabled = Config.Bind("HEALFROMBLEEDVOID", "Enable", true, "Load this item.");
            HealFromBleedVoid_percentBase = Config.Bind("HEALFROMBLEEDVOID", "Percent Health Healed", 0.01f, "Fraction of health healed for every 100% collapse damage with one stack.");
            HealFromBleedVoid_percentPerStack = Config.Bind("HEALFROMBLEEDVOID", "Percent Health Healed Per Stack", 0.01f, "Fraction of health healed for every 100% collapse damage per additional stack.");
            HealFromBleedVoid_radiusBase = Config.Bind("HEALFROMBLEEDVOID", "Heal Radius", 10f, "Radius of the healing burst with one stack.");
            HealFromBleedVoid_radiusPerStack = Config.Bind("HEALFROMBLEEDVOID", "Heal Radius Per Stack", 5f, "Radius of the healing burst per additional stack.");
            #endregion

            #region red
            ExplosionFrenzy_isEnabled = Config.Bind("EXPLOSIONFRENZY", "Enable", true, "Load this item.");
            ExplosionFrenzy_igniteDamageBase = Config.Bind("EXPLOSIONFRENZY", "Base Total Damage", 0.5f, "Total Damage dealt by burn effects from explosions.");
            ExplosionFrenzy_igniteDamagePerStack = Config.Bind("EXPLOSIONFRENZY", "Total Damage Per Stack", 0.5f, "Additional Total Damage dealt by burn effects from explosions per additional stack.");
            ExplosionFrenzy_bonusPerBurn = Config.Bind("EXPLOSIONFRENZY", "Blast Radius Bonus", 0.1f, "Increase in blast radius per burning enemy.");
            ExplosionFrenzy_bonusCapBase = Config.Bind("EXPLOSIONFRENZY", "Base Maximum Bonus", 10, "The maximum number of bonuses with one stack.");
            ExplosionFrenzy_bonusCapPerStack = Config.Bind("EXPLOSIONFRENZY", "Maximum Bonus Per Stack", 10, "The number of possible bonuses per additional stack.");

            HundredRendingFists_isEnabled = Config.Bind("HUNDREDRENDINGFISTS", "Enable", true, "Load this item.");
            HundredRendingFists_rendDuration = Config.Bind("HUNDREDRENDINGFISTS", "Buff Duration", 1.5f, "The window of time before stored damage is applied.");
            HundredRendingFists_storedDamagePerStack = Config.Bind("HUNDREDRENDINGFISTS", "Base Total Damage", 0.30f, "Total damage stored on hit per additional stack.");
            HundredRendingFists_storedDamageBase = Config.Bind("HUNDREDRENDINGFISTS", "Total Damage Per Stack", 0.30f, "Total damage stored on hit with one stack.");
            HundredRendingFists_storeBonus = Config.Bind("HUNDREDRENDINGFISTS", "Bonus Total Damage", 0.05f, "Extra total damage applied for every instance of stored damage.");

            InductionCoil_isEnabled = Config.Bind("LIGHTNINGDAMAGEBOOST", "Enable", true, "Load this item.");
            InductionCoil_damageBonusBase = Config.Bind("LIGHTNINGDAMAGEBOOST", "Base Damage Increase", 0.50f, "Damage increase per bounce with one stack.");
            InductionCoil_damageBonusPerStack = Config.Bind("LIGHTNINGDAMAGEBOOST", "Percent Damage Increase", 0.20f, "Damage increase per bounce per additional stack.");
            #endregion

            foreach (ConfigDefinition def in Config.Keys)
            {
                ConfigEntryBase configEntryBase = Config[def];
                if (configEntryBase.DefaultValue.GetType() == typeof(bool))
                {
                    ModSettingsManager.AddOption(new CheckBoxOption((ConfigEntry<bool>)configEntryBase));
                }
                if (configEntryBase.DefaultValue.GetType() == typeof(int))
                {
                    ModSettingsManager.AddOption(new IntSliderOption((ConfigEntry<int>)configEntryBase, new IntSliderConfig() { min = 0, max = Mathf.Max(((ConfigEntry<int>)configEntryBase).Value * 10, 10 )}));
                }
                if (configEntryBase.DefaultValue.GetType() == typeof(float))
                {
                    ModSettingsManager.AddOption(new StepSliderOption((ConfigEntry<float>)configEntryBase, new StepSliderConfig() { min = 0, max = Mathf.Max(((ConfigEntry<float>)configEntryBase).Value * 10f, 1f), increment = Mathf.Max(((ConfigEntry<float>)configEntryBase).Value / 10f , 0.001f)}));
                }
            }
        }    
    }
}