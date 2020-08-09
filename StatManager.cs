using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedModConfig;
using HarmonyLib;

namespace CustomGameStats
{
    //let's try pushing to git once again....
    public class StatManager : MonoBehaviour
    {
        public static StatManager instance;

        public float timeOfLastSync = -1f;
        public bool splitStarted = false;
        public Character splitPlayer;

        private static readonly string _dir = @"Mods\ModConfigs\";
        private static readonly string _file = _dir + Settings.modName;
        private static readonly string _ext = ".json";

        private readonly List<string> _modCharacters = new List<string>();
        private readonly List<string> _modPCs = new List<string>();
        private readonly Dictionary<string, VitalsInfo> _lastVitals = new Dictionary<string, VitalsInfo>();

        private string _currentScene = "";
        private bool _sceneChanged = false;
        private ModConfig _currentPlayerSyncInfo;
        private ModConfig _currentAiSyncInfo;
        private string _currentHostUID = "";
        private float _timeOfLastSyncRequest = -5f;
        private bool _vitalsUpdated = false;
        private float _lastVitalsUpdate = -5f;

        internal void Awake()
        {
            instance = this;
        }

        internal void Update()
        {
            if (_currentScene != SceneManagerHelper.ActiveSceneName)
            {
                _sceneChanged = true;
                _currentScene = SceneManagerHelper.ActiveSceneName;
            }

            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                return;
            }

            if (_sceneChanged)
            {
                _sceneChanged = false;
                _currentHostUID = "";
                _modCharacters.Clear();
            }

            if (splitStarted)
            {
                if (SplitPlayerInstantiated())
                {
                    splitStarted = false;
                    CharacterStats_ApplyCoopStats.Prefix(splitPlayer.Stats);
                }
            }

            if (UpdateVitalsInfo() && !NetworkLevelLoader.Instance.IsGameplayLoading && _vitalsUpdated)
            {
                SaveVitalsInfo();
            }
        }

        public void SetSyncInfo(ModConfig _playerSyncInfo, ModConfig _aiSyncInfo)
        {
            _currentHostUID = CharacterManager.Instance.GetWorldHostCharacter()?.UID;

            _currentPlayerSyncInfo = _playerSyncInfo;
            _currentAiSyncInfo = _aiSyncInfo;

            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                if (c.IsAI)
                {
                    ApplyCustomStats(c, _currentAiSyncInfo, Settings.aiStats);
                }
                else
                {
                    ApplyCustomStats(c, _currentPlayerSyncInfo, Settings.playerStats);
                }
            }
        }

        private static float ModifyLogic(bool _op, float _base, float _value, float _limiter)
        {
            if (_op)
            {
                if (_value < 0)
                {
                    if ((_limiter - _base) / _base > _value)
                    {
                        return (_limiter - _base) / _base;
                    }
                    else
                    {
                        return _value;
                    }
                }
                else
                {
                    return _value;
                }
            }
            else
            {
                return Math.Max(_limiter - _base, _value);
            }
        }

        private static float Modify(bool _op, float _base, float _value, float _limiter, ModConfig _config)
        {
            bool _strict = (bool)_config.GetValue(Settings.strictMinimum);
            bool _behaviour = (bool)_config.GetValue(Settings.gameBehaviour);

            if (_behaviour)
            {
                return ModifyLogic(_op, _base, _value, _limiter);
            }
            else
            {
                if (_strict)
                {
                    switch (_limiter)
                    {
                        case 50f:
                            _limiter = 1f;
                            break;
                        case 0.01f:
                            _limiter = 0f;
                            break;
                        default:
                            _limiter = 0f;
                            break;
                    }

                    return ModifyLogic(_op, _base, _value, _limiter);
                }
                else
                {
                    return _value;
                }
            }
        }

        private static UID GetTagUIDForName(string _tagName)
        {
            TagSourceManager instance = TagSourceManager.Instance;
            foreach (Tag tag in (Tag[])AT.GetValue(typeof(TagSourceManager), instance, "m_tags"))
            {
                if (tag.TagName.Equals(_tagName))
                {
                    return tag.UID;
                }
            }
            return null;
        }

        private bool UpdateSyncInfo()
        {
            if (CharacterManager.Instance.GetWorldHostCharacter() is Character host)
            {
                if (host.UID != _currentHostUID)
                {
                    if (PhotonNetwork.isNonMasterClientInRoom)
                    {
                        if (Time.time - _timeOfLastSyncRequest > 5f)
                        {
                            Debug.Log("Sync info updated!");
                            _timeOfLastSyncRequest = Time.time;
                            _currentPlayerSyncInfo = null;
                            _currentAiSyncInfo = null;
                            RPCManager.instance.RequestSettings();
                        }
                    }
                    else
                    {
                        _currentHostUID = host.UID;
                        _currentPlayerSyncInfo = Main.playerConfig;
                        _currentAiSyncInfo = Main.aiConfig;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private IEnumerator DelayedInvoke(CharacterStats _instance)
        {
            float start = Time.time;

            while (Time.time - start  < 5f && _currentPlayerSyncInfo == null && _currentAiSyncInfo == null)
            {
                if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                {
                    start += 1f;
                }

                yield return new WaitForSeconds(1.0f);
            }

            if (_currentPlayerSyncInfo == null || _currentAiSyncInfo == null ||  !(bool)_currentPlayerSyncInfo.GetValue(Settings.toggleSwitch) || !(bool)_currentAiSyncInfo.GetValue(Settings.toggleSwitch))
            {
                try
                {
                    CharacterStats_ApplyCoopStats.ReversePatch(_instance);
                }
                catch{}
            }
        }

        private void UpdateVitals(CharacterStats _stats, VitalsInfo _ratios)
        {
            _stats.RefreshVitalMaxStat();
            _stats.SetHealth(_stats.MaxHealth * _ratios.healthRatio);
            AT.SetValue(_stats.BurntHealth * _ratios.burntHealthRatio, typeof(CharacterStats), _stats, "m_burntHealth");
            _stats.SetMana(_stats.MaxMana * _ratios.manaRatio);
            AT.SetValue(_stats.BurntMana * _ratios.burntManaRatio, typeof(CharacterStats), _stats, "m_burntMana");
            AT.SetValue(_stats.MaxStamina * _ratios.staminaRatio, typeof(CharacterStats), _stats, "m_stamina");
            AT.SetValue(_stats.BurntStamina * _ratios.burntStaminaRatio, typeof(CharacterStats), _stats, "m_burntStamina");
        }

        private void SetCustomStat(CharacterStats _stats, string _stackSource, Tag _tag, float _val, bool _mult, ModConfig _config)
        {
            Stat[] _dmg = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageTypesModifier");
            Stat[] _pro = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageProtection");
            Stat[] _res = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageResistance");

            _stats.RemoveStatStack(_tag, _stackSource, _mult);

            switch (_tag.TagName)
            {
                case "MaxHealth":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_maxHealthStat"), _val, CharacterStats.MIN_MAXHEALTH_LIMIT, _config)), _mult);
                    break;
                case "HealthRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_healthRegen"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "HealthBurn":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_burntHealthModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "MaxStamina":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_maxStamina"), _val, CharacterStats.MIN_MAXSTAMINA_LIMIT, _config)), _mult);
                    break;
                case "StaminaRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_staminaRegen"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "StaminaUse": case "StaminaCostReduction":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_staminaUseModifiers"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "StaminaBurn":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_burntStaminaModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "MaxMana":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_maxManaStat"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "ManaRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_manaRegen"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "ManaUse":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_manaUseModifiers"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "ManaBurn":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_burntManaModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "Impact":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_impactModifier"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "AllDamages":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_damageModifiers"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "PhysicalDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[0].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "EtherealDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[1].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DecayDamage": case "DarkDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[2].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "ElectricDamage": case "LightDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[3].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FrostDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[4].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FireDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[5].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DamageProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_allDamageProtection"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "PhysicalProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[0].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "EtherealProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[1].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DecayProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[2].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "ElectricProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[3].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FrostProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[4].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FireProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[5].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DarkProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[6].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "LightProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _pro[7].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "AllResistances": case "DamageResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_resistanceModifiers"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "PhysicalResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[0].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "EtherealResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[1].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DecayResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[2].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "ElectricResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[3].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FrostResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[4].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "FireResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[5].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "DarkResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[6].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "LightResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _res[7].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "ImpactResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_impactResistance"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "StabilityRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_stabilityRegen"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "EnvColdProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_coldProtection"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "EnvHeatProtection":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_heatProtection"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "ColdRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_coldRegenRate"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "HeatRegen":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_heatRegenRate"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "Waterproof":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_waterproof"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "CorruptionResistance":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_corruptionResistance"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "TemperatureModifier":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_temperatureModifier"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "MovementSpeed":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_movementSpeed"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "Speed":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_speedModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "AttackSpeed":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_attackSpeedModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "DodgeInvulnerabilityModifier":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_dodgeInvulneratiblityModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "Detectability":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_detectability"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "VisualDetectability":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_visualDetectability"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "PouchCapacity":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_pouchCapacity"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "FoodEffectEfficiency":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_foodEffectEfficiency"), _val, Settings.minimum, _config)), _mult);
                    break;
                case "SkillCooldownModifier":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_skillCooldownModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "BuyModifier":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_buyModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "SellModifier":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetCharacterStat(_stats, "m_sellModifier"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "FoodDepleteRate":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetPlayerStat(_stats.GetComponent<PlayerCharacterStats>(), "m_foodDepletionRate"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "DrinkDepleteRate":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetPlayerStat(_stats.GetComponent<PlayerCharacterStats>(), "m_drinkDepletionRate"), _val, Settings.minimumMod, _config)), _mult);
                    break;
                case "SleepDepleteRate":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, AT.GetPlayerStat(_stats.GetComponent<PlayerCharacterStats>(), "m_sleepDepletionRate"), _val, Settings.minimumMod, _config)), _mult);
                    break;
            }
        }

        private void ApplyCustomStats(Character _char, ModConfig _config, string _stackSource)
        {
            if (_modCharacters.Contains(_char.UID))
            {
                return;
            }

            foreach (BBSetting _setting in _config.Settings)
            {
                if (_setting is FloatSetting _mod)
                {
                    Tag _tag = TagSourceManager.Instance.GetTag(GetTagUIDForName(_mod.Name));
                    float _val = _mod.m_value;
                    bool _mult = true;

                    foreach (BBSetting _switch in _config.Settings)
                    {
                        if (_switch is BoolSetting _bool)
                        {
                            if (_bool.Name.Contains(_mod.Name + Settings.modMult))
                            {
                                _mult = _bool.m_value;

                                if (_mult)
                                {
                                    _val = _mod.m_value / 100f;
                                }
                            }
                        }
                    }
                    
                    SetCustomStat(_char.Stats, _stackSource, _tag, _val, _mult, _config);

                    VitalsInfo _ratios = LoadVitalsInfo(_char.UID) ?? new VitalsInfo
                    {
                        healthRatio = _char.HealthRatio,
                        burntHealthRatio = _char.Stats.BurntHealthRatio,
                        staminaRatio = _char.StaminaRatio,
                        burntStaminaRatio = _char.Stats.BurntStaminaRatio,
                        manaRatio = _char.ManaRatio,
                        burntManaRatio = _char.Stats.BurntManaRatio
                    };

                    
                    if (_char.IsAI)
                    {
                        UpdateVitals(_char.Stats, _ratios);
                        _modCharacters.Add(_char.UID);
                    }
                    else
                    {
                        if (_lastVitals.ContainsKey(_char.UID))
                        {
                            _lastVitals.Remove(_char.UID);
                        }

                        _lastVitals.Add(_char.UID, _ratios);
                        UpdateVitals(_char.Stats, _ratios);

                        if (!_modPCs.Contains(_char.UID))
                        {
                            _modPCs.Add(_char.UID);
                            SaveVitalsInfo();
                        }
                    }
                }
            }
        }

        private bool SplitPlayerInstantiated()
        {
            bool _boo = false;

            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                if (!SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter.Name.Contains("Prefab") && splitStarted)
                {
                    _boo = true;
                }
            }

            return _boo;
        }

        private bool UpdateVitalsInfo(bool _forced = false)
        {
            bool _boo = false;

            if (Time.time - _lastVitalsUpdate > 5f || _forced)
            {
                foreach(string u in _modPCs)
                {
                    Character c = CharacterManager.Instance.GetCharacter(u);

                    if (c.HealthRatio != _lastVitals.GetValueSafe(u).healthRatio && c.HealthRatio <= 1)
                    {
                        _boo = true;
                        _vitalsUpdated = true;
                    }
                }

                _lastVitalsUpdate = Time.time;
                return _boo;
            }
            else
            {   
                return _boo;
            }
        }

        private VitalsInfo LoadVitalsInfo(string _uid)
        {
            string _path = _file + "_" + _uid + _ext;

            if (File.Exists(_path))
            {
                if (JsonUtility.FromJson<VitalsInfo>(File.ReadAllText(_path)) is VitalsInfo _json)
                {
                    return _json;
                }
            }

            return null;
        }

        private void SaveVitalsInfo()
        {
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }

            foreach (string u in _modPCs)
            {
                Character c = CharacterManager.Instance.GetCharacter(u);

                if (c == null)
                {
                    _modPCs.Remove(u);
                    return;
                }

                string _path = _file + "_" + u + _ext;
                VitalsInfo _vitals = new VitalsInfo
                {
                    healthRatio = c.HealthRatio,
                    staminaRatio = c.StaminaRatio,
                    manaRatio = c.ManaRatio
                };

                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }

                if (!c.IsLocalPlayer)
                {
                    return;
                }

                if (_lastVitals.ContainsKey(u))
                {
                    _lastVitals.Remove(u);
                }

                _lastVitals.Add(u, _vitals);
                _vitalsUpdated = false;
                File.WriteAllText(_path, JsonUtility.ToJson(_vitals));
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "ApplyCoopStats")]
        public class CharacterStats_ApplyCoopStats
        {
            [HarmonyReversePatch]
            public static void ReversePatch(CharacterStats __instance)
            {
                throw new NotImplementedException("Harmony::ReversePatch::CharacterStats.ApplyCoopStats");
            }

            [HarmonyPrefix]
            public static bool Prefix(CharacterStats __instance)
            {
                var _char = __instance.GetComponent<Character>();
                var _flag = Settings.toggleSwitch;

                if ((!(bool)Main.playerConfig.GetValue(_flag) && !(bool)Main.aiConfig.GetValue(_flag)) || NetworkLevelLoader.Instance.IsGameplayPaused)
                {
                    return true;
                }

                if (SplitScreenManager.Instance.IsSplitActive && _char.Name.Contains("Prefab"))
                {
                    instance.splitPlayer = _char;
                    instance.splitStarted = true;
                    return true;
                }

                __instance.RefreshVitalMaxStat();

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    if (!_char.IsAI)
                    {
                        if ((bool)Main.playerConfig.GetValue(_flag))
                        {
                            Debug.Log("Applying custom stats to players...");
                            instance.ApplyCustomStats(_char, Main.playerConfig, Settings.playerStats);
                        }
                    }
                    else
                    {
                        if ((bool)Main.aiConfig.GetValue(_flag))
                        {
                            Debug.Log("Applying custom stats to ai...");
                            instance.ApplyCustomStats(_char, Main.aiConfig, Settings.aiStats);
                        }
                    }
                }
                else
                {
                    if (instance.UpdateSyncInfo() || instance._currentPlayerSyncInfo == null || instance._currentAiSyncInfo == null)
                    {
                        Debug.Log("Client requesting sync...");
                        instance.StartCoroutine(instance.DelayedInvoke(__instance));
                    }
                    else
                    {
                        if (instance._currentPlayerSyncInfo != null)
                        {
                            if ((bool)instance._currentPlayerSyncInfo.GetValue(_flag))
                            {
                                Debug.Log("Client applying synced custom player stats...");
                                instance.ApplyCustomStats(_char, instance._currentPlayerSyncInfo, Settings.playerStats);
                            }
                        }

                        if (instance._currentAiSyncInfo != null)
                        {
                            if ((bool)instance._currentAiSyncInfo.GetValue(_flag))
                            {
                                Debug.Log("Client applying synced custom ai stats...");
                                instance.ApplyCustomStats(_char, instance._currentAiSyncInfo, Settings.aiStats);
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
