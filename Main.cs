using System.Collections.Generic;
using SharedModConfig;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CustomGameStats
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(DEPENDANT, BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string GUID = "com.insterstice.customgamestats";
        public const string NAME = "Custom Game Stats";
        public const string VERSION = "1.3.1";

        public const string DEPENDANT = "com.sinai.SharedModConfig";

        public static Main instance;

        public static ModConfig playerConfig;
        public static ModConfig aiConfig;

        internal void Awake()
        {
            instance = this;

            var _obj = gameObject;
            _obj.AddComponent<StatManager>();
            _obj.AddComponent<RPCManager>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        internal void Start()
        {
            playerConfig = SetupConfig(Settings.playerStatsTitle);
            playerConfig.Register();

            aiConfig = SetupConfig(Settings.aiStatsTitle);
            aiConfig.Register();

            Logger.Log(LogLevel.Message, NAME + " " + VERSION + " instantiated!");
        }

        private ModConfig SetupConfig(string _set)
        {
            List<BBSetting> _BB = new List<BBSetting>();

            if (_set == Settings.aiStatsTitle)
            {
                var _settings = new Settings();
                _BB.AddRange(_settings.rulesSettings);
                _BB.AddRange(_settings.characterSettings);
            }

            if (_set == Settings.playerStatsTitle)
            {
                var _settings = new Settings();
                _BB.AddRange(_settings.rulesSettings);
                _BB.AddRange(_settings.playerSettings);
                _BB.AddRange(_settings.characterSettings);
            }

            ModConfig _config = new ModConfig
            {
                ModName = Settings.modName + _set,
                SettingsVersion = 1.0,
                Settings = _BB
            };

            return _config;
        }
    }
}
