using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TryConnect
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public sealed class TryConnectPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.Try-4646.TryConnect";
        public const string PluginDisplayName = "TryConnect";
        public const string PluginVersion = "1.0.0";

        internal const string MyGUID = PluginGuid;
        internal const string PluginName = PluginDisplayName;
        internal const string VersionString = PluginVersion;

        public static TryConnectPlugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        internal static ConfigEntry<bool> EnableCustomItems { get; private set; }
        internal static ConfigEntry<int> TicketFizzReplacementChance { get; private set; }
        internal static ConfigEntry<int> LoadedChipReplacementChance { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            EnableCustomItems = Config.Bind("Custom Items", "Enable", true, "Enable TryConnect runtime shop item registration.");
            TicketFizzReplacementChance = Config.Bind("Custom Items", "TicketFizzReplacementChance", 35, "Chance for shop item stamps to replace a vanilla Drink roll with Ticket Fizz.");
            LoadedChipReplacementChance = Config.Bind("Custom Items", "LoadedChipReplacementChance", 25, "Chance for shop item stamps to replace a vanilla Golden Chip roll with Loaded Chip.");

            RuntimeItemRegistry.Initialize(this);
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo(string.Format("PluginName: {0}, VersionString: {1} is loading...", PluginName, VersionString));
            _harmony = new Harmony(MyGUID);
            _harmony.PatchAll();
            RuntimeItemRegistry.TryRegisterRuntimeContent();
            Logger.LogInfo(string.Format("PluginName: {0}, VersionString: {1} is loaded.", PluginName, VersionString));
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RuntimeItemRegistry.Dispose();
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RuntimeItemRegistry.TryRegisterRuntimeContent();
        }
    }
}
