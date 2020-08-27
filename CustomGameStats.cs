using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SharedModConfig;

namespace CustomGameStats
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(DEPENDENT, BepInDependency.DependencyFlags.HardDependency)]
    public class CustomGameStats : BaseUnityPlugin
    {
        public const string GUID = "com.theinterstice.customgamestats";
        public const string NAME = "Custom Game Stats";
        public const string VERSION = "2.1.3";
        public const string DEPENDENT = SharedModConfig.SharedModConfig.GUID;

        public static CustomGameStats Instance { get; private set; }
        public static ModConfig PlayerConfig { get; private set; }
        public static ModConfig AIConfig { get; private set; }

        internal void Awake()
        {
            Instance = this;

            var _obj = gameObject;
            _obj.AddComponent<StatManager>();
            _obj.AddComponent<RPCManager>();

            var _harmony = new Harmony(GUID);
            _harmony.PatchAll();
        }

        internal void Start()
        {
            PlayerConfig = SetupConfig(Settings.PlayerStatsTitle);
            PlayerConfig.Register();

            AIConfig = SetupConfig(Settings.AIStatsTitle);
            AIConfig.Register();

            Logger.Log(LogLevel.Message, $"{ NAME } v{ VERSION } initialized!");
        }

        private ModConfig SetupConfig(string flag)
        {
            List<BBSetting> _bbs = new List<BBSetting>();

            if (flag == Settings.AIStatsTitle)
            {
                var _settings = new Settings();
                _bbs.AddRange(_settings.RulesSettings);
                _bbs.AddRange(_settings.CharacterSettings);
            }

            if (flag == Settings.PlayerStatsTitle)
            {
                var _settings = new Settings();
                _bbs.AddRange(_settings.RulesSettings);
                _bbs.AddRange(_settings.PlayerSettings);
                _bbs.AddRange(_settings.CharacterSettings);
            }

            ModConfig _config = new ModConfig
            {
                ModName = Settings.ModName + flag,
                SettingsVersion = 1.0,
                Settings = _bbs
            };

            return _config;
        }
    }
}
