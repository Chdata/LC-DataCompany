using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DataCompanyMod.Patches;
using DataCompanyMod.ScrapItems;
using HarmonyLib;
using UnityEngine;

namespace DataCompanyMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        internal static new ManualLogSource Logger;

        private ConfigEntry<int> configStaleBreadRarity;

        public static AssetBundle ChdataAssetBundle;
        public static Item StaleBreadItem;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            Logger.LogInfo("DATA COMPANY LOADED");
            Logger.LogInfo("DATA COMPANY LOADED");
            Logger.LogInfo("DATA COMPANY LOADED");

            configStaleBreadRarity = Config.Bind("Settings", "Stale Bread Rarity", 30, new ConfigDescription("How rare Stale Bread is to find. Lower values = More rare. 0 to MAYBE prevent spawning.", new AcceptableValueRange<int>(0, 100)));

            LoadAssets();

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(Cheats));
            harmony.PatchAll(typeof(StaleBread));
        }

        private void LoadAssets()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "datacompany.assetbundle");
            Logger.LogDebug(path);

            ChdataAssetBundle = AssetBundle.LoadFromFile(path);
            if (ChdataAssetBundle == null)
            {
                Logger.LogError("Failed to load Chdata's Assets Bundle. File \"datacompany.assetbundle\" belongs in the same folder as the .dll");
            }
            else
            {
                Logger.LogDebug("Succeeded in loading Chdata Assets Bundle.");
            }

            StaleBreadItem = ChdataAssetBundle.LoadAsset<Item>("Assets/Import/StaleBread/StaleBread.asset");
            if (StaleBreadItem == null)
            {
                Logger.LogError("Failed to load Stale Bread item. File \"datacompany.assetbundle\" belongs in the same folder as the .dll");
            }
            else
            {
                Logger.LogDebug("Succeeded in loading Stale Bread item.");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(StaleBreadItem.spawnPrefab);
                LethalLib.Modules.Items.RegisterScrap(StaleBreadItem, configStaleBreadRarity.Value, LethalLib.Modules.Levels.LevelTypes.All);
            }
        }
    }
}
