using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedModConfig;
using HarmonyLib;

namespace CustomGameStats
{
    public class StatManager : MonoBehaviour
    {
        public static StatManager instance;

        public Character splitPlayer;
        public ModConfig currentPlayerSyncInfo;
        public ModConfig currentAiSyncInfo;
        public Action syncHandler = SyncHandler;
        public bool splitStarted = false;
        public bool isInfoSynced = false;

        private static readonly string _dir = @"Mods\ModConfigs\";
        private static readonly string _file = _dir + Settings.modName;
        private static readonly string _ext = ".json";

        private readonly Dictionary<string, VitalsInfo> _lastVitals = new Dictionary<string, VitalsInfo>();

        private string _currentHostUID = "";
        private bool _vitalsUpdated = false;
        private float _lastVitalsUpdate = -12f;

        internal void Awake()
        {
            instance = this;
        }

        internal void Start()
        {
            Main.playerConfig.OnSettingsSaved += syncHandler;
            Main.aiConfig.OnSettingsSaved += syncHandler;
        }

        internal void Update()
        {
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused || NetworkLevelLoader.Instance.IsGameplayLoading)
            {
                return;
            }

            if (!PhotonNetwork.offlineMode && !PhotonNetwork.isMasterClient)
            {
                if (UpdateSyncInfo())
                {
                    RPCManager.instance.RequestSync();
                }
            }

            if (splitStarted)
            {
                if (SplitPlayerInstantiated())
                {
                    splitStarted = false;
                    CharacterStats_ApplyCoopStats.Prefix(splitPlayer.Stats);
                }
            }

            if (UpdateVitalsInfo() && _vitalsUpdated)
            {
                SaveVitalsInfo();
            }
        }

        public void SetSyncInfo()  //client
        {
            instance.isInfoSynced = true;
            instance._currentHostUID = CharacterManager.Instance.GetWorldHostCharacter()?.UID;
            UpdateCustomStats(instance.currentAiSyncInfo, instance.currentPlayerSyncInfo);
        }

        public void SetSyncBoolInfo(string _name, bool _bool, string _flag)  //client
        {
            if (_flag == "player")
            {
                instance.currentPlayerSyncInfo.SetValue(_name, _bool);
            }

            if (_flag == "ai")
            {
                instance.currentAiSyncInfo.SetValue(_name, _bool);
            }
        }

        public void SetSyncFloatInfo(string _name, float _float, string _flag)  //client
        {
            if (_flag == "player")
            {
                instance.currentPlayerSyncInfo.SetValue(_name, _float);
            }

            if (_flag == "ai")
            {
                instance.currentAiSyncInfo.SetValue(_name, _float);
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

        private static void SyncHandler()  //host
        {
            if (Main.IsHost())
            {
                instance.isInfoSynced = false;
                RPCManager.instance.Sync();
            }
            
            if (PhotonNetwork.offlineMode)
            {
                instance.UpdateCustomStats(Main.aiConfig, Main.playerConfig);
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
                    if (!isInfoSynced)
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
                return true;
            }
        }

        private IEnumerator CO_InvokeOrig(CharacterStats _instance)  //client
        {
            Debug.Log("Starting delayed invoke...");
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading && (instance.currentPlayerSyncInfo == null || instance.currentAiSyncInfo == null))
            {
                yield return new WaitForSeconds(1.0f);
            }
            
            if (instance.currentPlayerSyncInfo == null || instance.currentAiSyncInfo == null || !(bool)instance.currentPlayerSyncInfo.GetValue(Settings.toggleSwitch) || !(bool)instance.currentAiSyncInfo.GetValue(Settings.toggleSwitch))
            {
                try
                {
                    Debug.Log("Trying reverse patch...");
                    CharacterStats_ApplyCoopStats.ReversePatch(_instance);
                }
                catch { }
            }
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

        private void SetCustomStat(CharacterStats _stats, string _stackSource, Tag _tag, float _val, bool _mult, ModConfig _config)
        {
            Stat[] _dmg = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageTypesModifier");
            Stat[] _pro = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageProtection");
            Stat[] _res = (Stat[])AT.GetValue(typeof(CharacterStats), _stats, "m_damageResistance");

            _stats.RefreshVitalMaxStat();
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

        private void UpdateCustomStats(ModConfig _aiConfig, ModConfig _playerConfig)
        {
            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                if (c.IsAI)
                {
                    Debug.Log("Updating AI stats...");
                    ApplyCustomStats(c, _aiConfig, Settings.aiStats);
                }
                else
                {
                    Debug.Log("Updating Player stats...");
                    ApplyCustomStats(c, _playerConfig, Settings.playerStats);
                }
            }
        }

        private void ApplyCustomStats(Character _char, ModConfig _config, string _stackSource)
        {
            foreach (BBSetting _bbs in _config.Settings)
            {
                if (_bbs is FloatSetting _f)
                {
                    Tag _tag = TagSourceManager.Instance.GetTag(AT.GetTagUID(_f.Name));
                    float _val = _f.m_value;
                    bool _mult = (bool)_config.GetValue(_f.Name + Settings.modMult);

                    if (_mult)
                    {
                        _val = _f.m_value / 100f;
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
                        UpdateVitals(_char.Stats, _ratios, _config);
                    }
                    else
                    {
                        if (_lastVitals.ContainsKey(_char.UID))
                        {
                            _lastVitals.Remove(_char.UID);
                        }

                        _lastVitals.Add(_char.UID, _ratios);
                        UpdateVitals(_char.Stats, _ratios, _config);
                        SaveVitalsInfo();
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

            if (Time.time - _lastVitalsUpdate > 12f || _forced)
            {
                Debug.Log("Checking vitals info...");
                foreach (Character c in CharacterManager.Instance.Characters.Values)
                {
                    if (!c.IsAI && c.HealthRatio != _lastVitals.GetValueSafe(c.UID).healthRatio && c.HealthRatio <= 1)
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
                var _init = AT.GetValue(typeof(Character), _char, "m_startInitDone");
                var _late = AT.GetValue(typeof(Character), _char, "m_lateInitDone");

                if ((!(bool)Main.playerConfig.GetValue(_flag) && !(bool)Main.aiConfig.GetValue(_flag)) || NetworkLevelLoader.Instance.IsGameplayPaused || (!(bool)_init && !(bool)_late))
                {
                    return true;
                }

                if (SplitScreenManager.Instance.IsSplitActive && _char.Name.Contains("Prefab"))
                {
                    instance.splitPlayer = _char;
                    instance.splitStarted = true;
                    return true;
                }

                if (PhotonNetwork.isMasterClient)
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
                    if (instance.currentPlayerSyncInfo == null || instance.currentAiSyncInfo == null)
                    {
                        instance.StartCoroutine(instance.CO_InvokeOrig(__instance));
                    }
                    //else
                    //{
                    //    if ((!(bool)instance.currentPlayerSyncInfo.GetValue(_flag) && !(bool)instance.currentAiSyncInfo.GetValue(_flag)) || NetworkLevelLoader.Instance.IsGameplayPaused || (!(bool)_init && !(bool)_late))
                    //    {
                    //        return true;
                    //    }

                    //    if (!_char.IsAI)
                    //    {
                    //        if ((bool)instance.currentPlayerSyncInfo.GetValue(_flag))
                    //        {
                    //            Debug.Log("Client applying synced custom player stats...");
                    //            instance.ApplyCustomStats(_char, instance.currentPlayerSyncInfo, Settings.playerStats);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if ((bool)instance.currentAiSyncInfo.GetValue(_flag))
                    //        {
                    //            Debug.Log("Client applying synced custom ai stats...");
                    //            instance.ApplyCustomStats(_char, instance.currentAiSyncInfo, Settings.aiStats);
                    //        }
                    //    }
                    //}
                }

                return false;
            }
        }
    }
}
