using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using SharedModConfig;
using uNature.Core.Extensions;
using UnityEngine;

namespace CustomGameStats
{
    public class StatManager : MonoBehaviour
    {
        public static StatManager instance;

        public ModConfig currentPlayerSyncInfo;
        public ModConfig currentAiSyncInfo;
        public bool isPlayerInfoSynced = false;
        public bool isAiInfoSynced = false;

        private static readonly string _dir = @"Mods\ModConfigs\";
        private static readonly string _file = _dir + Settings.modName;
        private static readonly string _ext = ".json";

        private readonly Dictionary<string, VitalsInfo> _lastVitals = new Dictionary<string, VitalsInfo>();
        private readonly Action _playerSyncHandler = PlayerSyncHandler;
        private readonly Action _aiSyncHandler = AiSyncHandler;

        private string _currentHostUID = "";
        private bool _vitalsUpdated = false;
        private bool _checkSplit = false;
        private bool _isOnline = false;
        private float _lastVitalsUpdate = -12f;

        internal void Awake()
        {
            instance = this;
        }

        internal void Start()
        {
            Main.playerConfig.OnSettingsSaved += _playerSyncHandler;
            Main.aiConfig.OnSettingsSaved += _aiSyncHandler;
        }

        internal void Update()
        {
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused || NetworkLevelLoader.Instance.IsGameplayLoading)
            {
                return;
            }

            if (Global.Lobby.PlayersInLobbyCount > 1)
            {
                if (_checkSplit)
                {
                    _checkSplit = false;
                    UpdateCustomStats(Main.playerConfig);
                }

                if (!PhotonNetwork.offlineMode && !PhotonNetwork.isMasterClient)
                {
                    if (!_isOnline)
                    {
                        _isOnline = true;
                    }

                    if (UpdateSyncInfo())
                    {
                        RPCManager.instance.RequestSync();
                    }
                }
            }
            else
            {
                _checkSplit = true;

                if (_isOnline)
                {
                    _isOnline = false;
                    isPlayerInfoSynced = false;
                    currentPlayerSyncInfo = null;
                    isAiInfoSynced = false;
                    currentAiSyncInfo = null;
                    UpdateCustomStats(Main.playerConfig);
                    UpdateCustomStats(Main.aiConfig);
                }
            }

            if (UpdateVitalsInfo() && _vitalsUpdated)
            {
                SaveVitalsInfo();
            }
        }

        public void SetPlayerSyncInfo()  //client
        {
            instance.isPlayerInfoSynced = true;
            instance._currentHostUID = CharacterManager.Instance.GetWorldHostCharacter()?.UID;
            UpdateCustomStats(instance.currentPlayerSyncInfo);
        }

        public void SetAiSyncInfo()  //client
        {
            instance.isAiInfoSynced = true;
            instance._currentHostUID = CharacterManager.Instance.GetWorldHostCharacter()?.UID;
            UpdateCustomStats(instance.currentAiSyncInfo);
        }

        public void SetSyncBoolInfo(string _name, bool _bool, string _flag)  //client
        {
            switch (_flag)
            {
                case "player":
                    instance.currentPlayerSyncInfo.SetValue(_name, _bool);
                    break;
                case "ai":
                    instance.currentAiSyncInfo.SetValue(_name, _bool);
                    break;
            }
        }

        public void SetSyncFloatInfo(string _name, float _float, string _flag)  //client
        {
            switch (_flag)
            {
                case "player":
                    instance.currentPlayerSyncInfo.SetValue(_name, _float);
                    break;
                case "ai":
                    instance.currentAiSyncInfo.SetValue(_name, _float);
                    break;
            }
        }

        private static void PlayerSyncHandler()
        {
            if (!PhotonNetwork.offlineMode && PhotonNetwork.isMasterClient)
            {
                instance.isPlayerInfoSynced = false;
                RPCManager.instance.PlayerSync();
            }

            instance.UpdateCustomStats(Main.playerConfig);
        }

        private static void AiSyncHandler()
        {
            if (!PhotonNetwork.offlineMode && PhotonNetwork.isMasterClient)
            {
                instance.isAiInfoSynced = false;
                RPCManager.instance.AiSync();
            }

            instance.UpdateCustomStats(Main.aiConfig);
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
            if ((bool)_config.GetValue(Settings.gameBehaviour))
            {
                return ModifyLogic(_op, _base, _value, _limiter);
            }
            else
            {
                if ((bool)_config.GetValue(Settings.strictMinimum))
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

        private bool UpdateSyncInfo()
        {
            if (CharacterManager.Instance.GetWorldHostCharacter() is Character host)
            {
                if (host.UID != _currentHostUID)
                {
                    return true;
                }
                else
                {
                    if (!isAiInfoSynced || !isPlayerInfoSynced)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateCustomStats(ModConfig _config)
        {
            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                bool _flag = _config.ModName.Contains("Player");

                if (_flag ? !c.IsAI : c.IsAI && (bool)_config.GetValue(Settings.toggleSwitch))
                {
                    ApplyCustomStats(c, _config, _flag ? Settings.playerStats : Settings.aiStats, true);
                }
                else
                {
                    ApplyCustomStats(c, _config, _flag ? Settings.playerStats : Settings.aiStats, false);
                }
            }
        }

        private void ApplyCustomStats(Character _char, ModConfig _config, string _stackSource, bool _flag)
        {
            VitalsInfo _ratios = LoadVitalsInfo(_char.UID) ?? new VitalsInfo
            {
                healthRatio = _char.HealthRatio,
                burntHealthRatio = _char.Stats.BurntHealthRatio,
                staminaRatio = _char.StaminaRatio,
                burntStaminaRatio = _char.Stats.BurntStaminaRatio,
                manaRatio = _char.ManaRatio,
                burntManaRatio = _char.Stats.BurntManaRatio
            };

            foreach (BBSetting _bbs in _config.Settings)
            {
                if (_bbs is FloatSetting _f)
                {
                    Tag _tag = TagSourceManager.Instance.GetTag(AT.GetTagUID(_f.Name));
                    bool _mult = (bool)_config.GetValue(_f.Name + Settings.modMult);

                    if (_flag)
                    {
                        SetCustomStat(_char.Stats, _stackSource, _tag, _mult ? _f.m_value / 100f : _f.m_value, _mult, _config);
                    }
                    else
                    {
                        ClearCustomStat(_char.Stats, _tag, _stackSource, _mult);
                    }
                    
                }
            }

            UpdateVitals(_char.Stats, _ratios, _config);

            if (!_char.IsAI)
            {
                if (_lastVitals.ContainsKey(_char.UID))
                {
                    _lastVitals.Remove(_char.UID);
                }

                _lastVitals.Add(_char.UID, _ratios);
                SaveVitalsInfo();
            }
        }

        private void SetCustomStat(CharacterStats _stats, string _stackSource, Tag _tag, float _val, bool _mult, ModConfig _config)
        {
            Stat[] _dmg = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageTypesModifier");
            Stat[] _pro = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageProtection");
            Stat[] _res = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageResistance");

            ClearCustomStat(_stats, _tag, _stackSource, _mult);
            _stats.RefreshVitalMaxStat();

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
                case "StaminaUse":
                case "StaminaCostReduction":
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
                case "DecayDamage":
                case "DarkDamage":
                    _stats.AddStatStack(_tag, new StatStack(_stackSource, Modify(_mult, _dmg[2].CurrentValue, _val, Settings.minimum, _config)), _mult);
                    break;
                case "ElectricDamage":
                case "LightDamage":
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
                case "AllResistances":
                case "DamageResistance":
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

        private void ClearCustomStat(CharacterStats _stats, Tag _tag, string _stackSource, bool _mult)
        {
            _stats.RemoveStatStack(_tag, _stackSource, !_mult);
            _stats.RemoveStatStack(_tag, _stackSource, _mult);
        }

        private void UpdateVitals(CharacterStats _stats, VitalsInfo _ratios, ModConfig _config)
        {
            float _hp, _hpb, _sp, _spb, _mp, _mpb;
            _stats.RefreshVitalMaxStat();
            if (!(bool)_config.GetValue(Settings.gameBehaviour))
            {
                _hp = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.Health;
                _hpb = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.BurntHealth;
                _sp = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.Stamina;
                _spb = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.BurntStamina;
                _mp = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.Mana;
                _mpb = SaveManager.Instance.GetCharacterSave(_stats.GetComponent<Character>().UID).PSave.BurntMana;
            }
            else
            {
                _hp = _stats.MaxHealth * _ratios.healthRatio;
                _hpb = _stats.MaxHealth * _ratios.burntHealthRatio;
                _sp = _stats.MaxStamina * _ratios.staminaRatio;
                _spb = _stats.MaxStamina * _ratios.burntStaminaRatio;
                _mp = _stats.MaxMana * _ratios.manaRatio;
                _mpb = _stats.MaxMana * _ratios.burntManaRatio;
            }
            _stats.SetHealth(_hp);
            AT.SetValue(_hpb, typeof(CharacterStats), _stats, "m_burntHealth");
            AT.SetValue(_sp, typeof(CharacterStats), _stats, "m_stamina");
            AT.SetValue(_spb, typeof(CharacterStats), _stats, "m_burntStamina");
            _stats.SetMana(_mp);
            AT.SetValue(_mpb, typeof(CharacterStats), _stats, "m_burntMana");
        }

        private bool UpdateVitalsInfo()
        {
            bool _boo = false;

            if (Time.time - _lastVitalsUpdate > 12f)
            {
                foreach (Character c in CharacterManager.Instance.Characters.Values)
                {
                    if (_lastVitals.ContainsKey(c.UID) && c.HealthRatio != _lastVitals.GetValueSafe(c.UID).healthRatio && c.HealthRatio <= 1)
                    {
                        _vitalsUpdated = true;
                        _boo = true;
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

            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                if (!c.IsAI && c.IsLocalPlayer)
                {
                    string _path = _file + "_" + c.UID + _ext;
                    VitalsInfo _vitals = new VitalsInfo
                    {
                        healthRatio = c.HealthRatio,
                        burntHealthRatio = c.Stats.BurntHealthRatio,
                        staminaRatio = c.StaminaRatio,
                        burntStaminaRatio = c.Stats.BurntStaminaRatio,
                        manaRatio = c.ManaRatio,
                        burntManaRatio = c.Stats.BurntManaRatio
                    };

                    if (File.Exists(_path))
                    {
                        File.Delete(_path);
                    }

                    if (_lastVitals.ContainsKey(c.UID))
                    {
                        _lastVitals.Remove(c.UID);
                    }

                    _lastVitals.Add(c.UID, _vitals);
                    _vitalsUpdated = false;
                    File.WriteAllText(_path, JsonUtility.ToJson(_vitals));
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "ApplyCoopStats")]
        public class CharacterStats_ApplyCoopStats
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterStats __instance)
            {
                Character _char = __instance.GetComponent<Character>();

                if ((!(bool)Main.playerConfig.GetValue(Settings.toggleSwitch) && !(bool)Main.aiConfig.GetValue(Settings.toggleSwitch)) || NetworkLevelLoader.Instance.IsGameplayPaused || (!_char.IsStartInitDone || !_char.IsLateInitDone))
                {
                    return true;
                }

                if (PhotonNetwork.isMasterClient)
                {
                    if (!_char.IsAI)
                    {
                        instance.UpdateCustomStats(Main.playerConfig);
                    }
                    else
                    {
                        instance.UpdateCustomStats(Main.aiConfig);
                    }
                }
                else
                {
                    if (!_char.IsAI)
                    {
                        instance.UpdateCustomStats(instance.currentPlayerSyncInfo);
                    }
                    else
                    {
                        instance.UpdateCustomStats(instance.currentAiSyncInfo);
                    }
                }

                return false;
            }
        }
    }
}
