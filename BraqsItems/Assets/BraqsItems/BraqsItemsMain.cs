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

namespace BraqsItems
{
    #region Dependencies
    [BepInDependency("___riskofthunder.RoR2BepInExPack")]
    #endregion
    [BepInPlugin(GUID, MODNAME, VERSION)]

    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]

    [BepInDependency("com.rune580.riskofoptions")]

    public class BraqsItemsMain : BaseUnityPlugin
    {
        public const string GUID = "com.Braquen.BraqsItems";
        public const string MODNAME = "Braqs Items";
        public const string VERSION = "0.0.1";

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

            Helpers.Init();
            Stats.Init();
            CharacterEvents.Init();

            BraqsItemsExpansion = assetBundle.LoadAsset<ExpansionDef>("BraqsItemsExpansion");
            ContentAddition.AddExpansionDef(BraqsItemsExpansion);

            BrokenItemRelationships.CreateBrokenItemProvider();

            BiggerExplosions.Init();
            AttackSpeedOnHit.Init();
            LightningOnOverkill.Init();
            ExplodeAgain.Init();
            RepairBrokenItems.Init();
            HealFromBleed.Init();
            ExplosionFrenzy.Init();
            HundredRendingFists.Init();
            LightningDamageBoost.Init();
        }

        private void Update()
        {
            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(HundredRendingFists.itemDef.itemIndex), transform.position, transform.forward * 20f);
            }
        }
    }
}
