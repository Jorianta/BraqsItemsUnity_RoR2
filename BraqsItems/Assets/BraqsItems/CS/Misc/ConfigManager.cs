using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;


namespace BraqsItems
{
    public class ConfigManager
    {
        public static ConfigEntry<string> ConfigVersion;

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


        //Bomblet
        public static ConfigEntry<bool> ExplodeAgain_isEnabled;
        public static ConfigEntry<float> ExplodeAgain_chance;
        public static ConfigEntry<float> ExplodeAgain_radiusCoefficient;
        public static ConfigEntry<float> ExplodeAgain_damageCoefficient;
        public static ConfigEntry<int> ExplodeAgain_maxBombsBase;
        public static ConfigEntry<int> ExplodeAgain_maxBombsPerStack;
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


        public static void Init(string configPath)
        {
            //Thanks to Hex3, who's mod I referenced heavily for this.

            var Config = new ConfigFile(Path.Combine(configPath, "braquen-BraqsItems.cfg"), true);

            //Remake Config on major update
            ConfigVersion = Config.Bind("Version", "Version", BraqsItemsMain.VERSION, "Don't touch. Used to determine if a config file is out of date.");
            var ConfigMajor = ConfigVersion.Value.Split('.')[0];
            var Major = BraqsItemsMain.VERSION.Split('.')[0];
            if (ConfigMajor != Major)
            {
                Config.Clear();
                Config.Save();
                ConfigVersion = Config.Bind("Version", "Version", BraqsItemsMain.VERSION, "Don't touch. Used to determine if a config file is out of date.");
            }

            //White
            AttackSpeedOnHit_isEnabled = Config.Bind("ATTACKSPEEDONHIT", "Enable", true, "Load this item.");
            AttackSpeedOnHit_percentBase = Config.Bind("ATTACKSPEEDONHIT", "Base Attack Speed Bonus", 0.01f, "Attack speed bonus gained on hit with one stack.");
            AttackSpeedOnHit_percentPerStack = Config.Bind("ATTACKSPEEDONHIT", "Attack Speed Bonus Per Stack", 0.01f, "Attack speed bonus gained on hit per additional stack.");

            BiggerExplosions_isEnabled = Config.Bind("BIGGEREXPLOSIONS", "Enable", true, "Load this item.");
            BiggerExplosions_percentBase = Config.Bind("BIGGEREXPLOSIONS", "Base Blast Radius Increase", 0.05f, "Blast radius increase with one stack.");
            BiggerExplosions_percentPerStack = Config.Bind("BIGGEREXPLOSIONS", "Attack Speed Bonus Per Stack", 0.05f, "Blast radius increase per additional stack.");

            LightningOnOverkill_isEnabled = Config.Bind("LIGHTNINGONOVERKILL", "Enable", true, "Load this item.");
            LightningOnOverkill_damagePercentPerStack = Config.Bind("LIGHTNINGONOVERKILL", "Damage Per Stack", 0.50f, "Excess damage per additional stack.");
            LightningOnOverkill_damagePercentBase = Config.Bind("LIGHTNINGONOVERKILL", "Base Damage", 1f, "Fraction of excess damage from a kill dealt to nearby enemies with one stack.");
            LightningOnOverkill_bouncePerStack = Config.Bind("LIGHTNINGONOVERKILL", "Bounces Per Stack", 1, "Number of enemies hit per additional stack.");
            LightningOnOverkill_bounceBase = Config.Bind("LIGHTNINGONOVERKILL", "Base Bounces", 3, "Number of enemies hit with one stack.");
            LightningOnOverkill_rangePerStack = Config.Bind("LIGHTNINGONOVERKILL", "Range Per Stack", 2f, "Range of the lightning per additional stack.");
            LightningOnOverkill_rangeBase = Config.Bind("LIGHTNINGONOVERKILL", "Range", 15f, "Range of the lightning with one stack.");

            //Green
            ExplodeAgain_isEnabled = Config.Bind("EXPLODEAGAIN", "Enable", true, "Load this item.");
            ExplodeAgain_chance = Config.Bind("EXPLODEAGAIN", "Bomblet Chance", 25f, "The chance to fire a bomblet. This chance is rolled repeatedly to determine how many bomblets to fire.");
            ExplodeAgain_maxBombsBase = Config.Bind("EXPLODEAGAIN", "Number of Bombs", 3, "The maximum number of bomblets that can be fired with one stack.");
            ExplodeAgain_maxBombsPerStack = Config.Bind("EXPLODEAGAIN", "Number of Bombs Per Stack", 2, "The number of additinal bomblets that can be fired per additional stack.");
            ExplodeAgain_radiusCoefficient = Config.Bind("EXPLODEAGAIN", "Radius Coefficient", 0.75f, "Multiplied by the radius of the initial explosion to determine the bomblet's blast radius.");
            ExplodeAgain_damageCoefficient = Config.Bind("EXPLODEAGAIN", "Damage Coefficient", 0.75f, "Multiplied by the damage of the initial explosion to determine the bomblet's damage.");

            HealFromBleed_isEnabled = Config.Bind("HEALFROMBLEED", "Enable", true, "Load this item.");
            HealFromBleed_percentBase = Config.Bind("HEALFROMBLEED", "Percent Health Healed", 0.01f, "Fraction of health healed for every 100% bleed damage with one stack.");
            HealFromBleed_percentPerStack = Config.Bind("HEALFROMBLEED", "Percent Health Healed Per Stack", 0.01f, "Fraction of health healed for every 100% bleed damage per additional stack.");

            RepairBrokenItems_isEnabled = Config.Bind("REPAIRBROKENITEMS", "Enable", true, "Load this item.");
            RepairBrokenItems_repairsBase = Config.Bind("REPAIRBROKENITEMS", "Base Repair Attempts", 3, "Number of repairs attempted on stage advance with one stack.");
            RepairBrokenItems_repairsPerStack = Config.Bind("REPAIRBROKENITEMS", "Repair Attempts Per Stack", 2, "Number of repairs attempted on stage advance per additional stack.");
            RepairBrokenItems_whiteChance = Config.Bind("REPAIRBROKENITEMS", "White Success Chance", 100f, "Repair success chance for white/void white items.");
            RepairBrokenItems_redChance = Config.Bind("REPAIRBROKENITEMS", "Red Success Chance", 50f, "Repair success chance for red/void red items.");
            RepairBrokenItems_defaultChance = Config.Bind("REPAIRBROKENITEMS", "Default Success Chance", 75f, "Repair success chance for all other items.");

            //Red
            ExplosionFrenzy_isEnabled = Config.Bind("EXPLOSIONFRENZY", "Enable", true, "Load this item.");
            ExplosionFrenzy_igniteDamageBase = Config.Bind("EXPLOSIONFRENZY", "Base Total Damage", 0.5f, "Total Damage dealt by burn effects from explosions.");
            ExplosionFrenzy_igniteDamagePerStack = Config.Bind("EXPLOSIONFRENZY", "Total Damage Per Stack", 0.5f, "Additional Total Damage dealt by burn effects from explosions per additional stack.");
            ExplosionFrenzy_bonusPerBurn = Config.Bind("EXPLOSIONFRENZY", "Blast Radius Bonus", 0.1f, "Increase in blast radius per burning enemy.");
            ExplosionFrenzy_bonusCapBase = Config.Bind("EXPLOSIONFRENZY", "Base Maximum Bonus", 10, "The maximum number of bonuses with one stack.");
            ExplosionFrenzy_bonusCapPerStack = Config.Bind("EXPLOSIONFRENZY", "Maximum Bonus Per Stack", 10, "The number of possible bonuses per additional stack.");

            HundredRendingFists_isEnabled = Config.Bind("HUNDREDRENDINGFISTS", "Enable", true, "Load this item.");
            HundredRendingFists_rendDuration = Config.Bind("HUNDREDRENDINGFISTS", "Buff Duration", 3f, "The window of time before stored damage is applied.");
            HundredRendingFists_storedDamagePerStack = Config.Bind("HUNDREDRENDINGFISTS", "Base Total Damage", 0.30f, "Total damage stored on hit per additional stack.");
            HundredRendingFists_storedDamageBase = Config.Bind("HUNDREDRENDINGFISTS", "Total Damage Per Stack", 0.30f, "Total damage stored on hit with one stack.");
            HundredRendingFists_storeBonus = Config.Bind("HUNDREDRENDINGFISTS", "Bonus Total Damage", 0.05f, "Extra total damage applied for every instance of stored damage.");

            InductionCoil_isEnabled = Config.Bind("LIGHTNINGDAMAGEBOOST", "Enable", true, "Load this item.");
            InductionCoil_damageBonusBase = Config.Bind("LIGHTNINGDAMAGEBOOST", "Base Damage Increase", 0.50f, "Damage increase per bounce with one stack.");
            InductionCoil_damageBonusPerStack = Config.Bind("LIGHTNINGDAMAGEBOOST", "Percent Damage Increase", 0.20f, "Damage increase per bounce per additional stack.");

            foreach (ConfigDefinition def in Config.Keys)
            {
                ConfigEntryBase configEntryBase = Config[def];
                if (configEntryBase.DefaultValue.GetType() == typeof(bool))
                {
                    ModSettingsManager.AddOption(new CheckBoxOption((ConfigEntry<bool>)configEntryBase));
                }
                if (configEntryBase.DefaultValue.GetType() == typeof(int))
                {
                    ModSettingsManager.AddOption(new IntSliderOption((ConfigEntry<int>)configEntryBase, new IntSliderConfig() { min = 0, max = ((ConfigEntry<int>)configEntryBase).Value * 10 }));
                }
                if (configEntryBase.DefaultValue.GetType() == typeof(float))
                {
                    ModSettingsManager.AddOption(new StepSliderOption((ConfigEntry<float>)configEntryBase, new StepSliderConfig() { min = 0, max = ((ConfigEntry<float>)configEntryBase).Value * 10f, increment = ((ConfigEntry<float>)configEntryBase).Value / 10f }));
                }
            }
        }    
    }
}