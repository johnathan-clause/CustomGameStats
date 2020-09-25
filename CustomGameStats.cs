using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
        public const string VERSION = "2.1.5";
        public const string DEPENDENT = SharedModConfig.SharedModConfig.GUID;

        public static CustomGameStats Instance { get; private set; }
        public static ModConfig PlayerConfig { get; private set; }
        public static ModConfig AIConfig { get; private set; }
        public static string Dir { get; private set; } = $@"Mods\ModConfigs\{ Settings.ModName }\";

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
            Init();

            PlayerConfig = SetupConfig(Settings.PlayerStatsTitle);
            PlayerConfig.Register();

            AIConfig = SetupConfig(Settings.AIStatsTitle);
            AIConfig.Register();

            Logger.Log(LogLevel.Message, $"{ NAME } v{ VERSION } initialized!");
        }

        public void ResetConfig(string flag)
        {
            if (flag == Settings.PlayerStatsTitle)
            {
                foreach (BBSetting _setting in PlayerConfig.Settings)
                {
                    _setting.SetValue(_setting.DefaultValue);
                }
            }

            if (flag == Settings.AIStatsTitle)
            {
                foreach (BBSetting _setting in AIConfig.Settings)
                {
                    _setting.SetValue(_setting.DefaultValue);
                }
            }
        }

        private void Init()
        {
            ModSettings _ms = new ModSettings();
            string _file = $@"Mods\{ Settings.ModName }.json";
            string _path = $@"Mods\ModConfigs\{ Settings.ModName}";

            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            if (!File.Exists(_file))
            {
                _ms.FirstRun = true;
                File.WriteAllText(_file, JsonUtility.ToJson(_ms));
            }

            ModSettings _settings = JsonUtility.FromJson<ModSettings>(File.ReadAllText(_file));
            if (!_settings.FirstRun) { return; }

            if (File.Exists($"{ _path }{ Settings.PlayerStatsTitle }.xml")) { File.Delete($"{ _path }{ Settings.PlayerStatsTitle }.xml"); }
            if (File.Exists($"{ _path }{ Settings.AIStatsTitle }.xml")) { File.Delete($"{ _path }{ Settings.AIStatsTitle }.xml"); }

            Directory.Delete(Dir, true);
            Directory.CreateDirectory(Dir);

            _ms.FirstRun = false;
            File.WriteAllText(_file, JsonUtility.ToJson(_ms));
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
                ModName = $"{ Settings.ModName }{ flag }",
                SettingsVersion = 1.0,
                Settings = _bbs
            };

            return _config;
        }
    }
}
