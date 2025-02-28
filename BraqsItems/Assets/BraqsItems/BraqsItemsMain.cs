using BepInEx;
using BraqsItems.Misc;
using BraqsItems.Util;
using R2API;

using RoR2.ExpansionManagement;
using System.IO;
using R2API.Utils;
using RoR2;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;
using System.Diagnostics;

namespace BraqsItems
{
    #region Dependencies
    [BepInDependency("___riskofthunder.RoR2BepInExPack")]
    #endregion
    [BepInPlugin(GUID, MODNAME, VERSION)]

    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(AddressablesPlugin.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]

    [BepInDependency("com.rune580.riskofoptions")]

    public class BraqsItemsMain : BaseUnityPlugin
    {
        public const string GUID = "com.Braquen.BraqsItems";
        public const string MODNAME = "Braqs Items";
        public const string VERSION = "1.6.0";

        public static ExpansionDef BraqsItemsExpansion;

        public static PluginInfo PluginInfo { get; private set; }
        public static BraqsItemsMain instance { get; private set; }
        internal static AssetBundle assetBundle;
        internal static string assetBundleDir => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PluginInfo.Location), "braqsitemsassets");

        private void Awake()
        {
            Log.Init(Logger);
            
            instance = this;
            PluginInfo = Info;
            assetBundle = AssetBundle.LoadFromFile(assetBundleDir); // Load mainassets into stream
            ConfigManager.Init(Paths.ConfigPath);

            Stats.Init();
            CharacterEvents.Init();

            BraqsItemsExpansion = assetBundle.LoadAsset<ExpansionDef>("BraqsItemsExpansion");
            BraqsItemsExpansion.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();

            ContentAddition.AddExpansionDef(BraqsItemsExpansion);

            BrokenItemRelationships.CreateBrokenItemProvider();

            BiggerExplosions.Init();
            AttackSpeedOnHit.Init();
            LightningOnOverkill.Init();

            LightningOnOverkillVoid.Init();

            ExplodeAgain.Init();
            RepairBrokenItems.Init();
            HealFromBleed.Init();
            SkillSaver.Init();

            HealFromBleedVoid.Init();

            //ConfusionOnHit.Init();
            ExplosionFrenzy.Init();
            HundredRendingFists.Init();
            LightningDamageBoost.Init();
        }
    }
}
