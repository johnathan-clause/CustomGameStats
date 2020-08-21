using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SharedModConfig;

namespace CustomGameStats
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(DEPENDANT, BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string GUID = "com.theinterstice.customgamestats";
        public const string NAME = "Custom Game Stats";
        public const string VERSION = "2.1.2";

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

        private ModConfig SetupConfig(string _flag)
        {
            List<BBSetting> _bbs = new List<BBSetting>();

            if (_flag == Settings.aiStatsTitle)
            {
                var _settings = new Settings();
                _bbs.AddRange(_settings.rulesSettings);
                _bbs.AddRange(_settings.characterSettings);
            }

            if (_flag == Settings.playerStatsTitle)
            {
                var _settings = new Settings();
                _bbs.AddRange(_settings.rulesSettings);
                _bbs.AddRange(_settings.playerSettings);
                _bbs.AddRange(_settings.characterSettings);
            }

            ModConfig _config = new ModConfig
            {
                ModName = Settings.modName + _flag,
                SettingsVersion = 1.0,
                Settings = _bbs
            };

            return _config;
        }
    }
}
