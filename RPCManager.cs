using System.Collections;
using System.Collections.Generic;
using SharedModConfig;
using UnityEngine;

#pragma warning disable IDE0051 // Remove unused private members
namespace CustomGameStats
{
    class RPCManager : Photon.MonoBehaviour
    {
        public static RPCManager instance;

        internal void Awake()
        {
            instance = this;
        }

        internal void Start()
        {
            var photon = gameObject.AddComponent<PhotonView>();
            photon.viewID = 777;
        }

        public void RequestSync()  //client
        {
            InitPlayerSyncInfo();
            InitAiSyncInfo();
            photonView.RPC("RequestSyncRPC", PhotonNetwork.masterClient, new object[0]);
        }

        public void PlayerSync() //host
        {
            photonView.RPC("PlayerSyncRPC", PhotonTargets.All, new object[0]);
        }

        public void AiSync() //host
        {
            photonView.RPC("AiSyncRPC", PhotonTargets.All, new object[0]);
        }

        [PunRPC]
        private void RequestSyncRPC()  //host
        {
            StatManager.instance.isPlayerInfoSynced = false;
            StatManager.instance.isAiInfoSynced = false;
            StartCoroutine(CO_SetSyncInfo());
        }

        private List<BBSetting> InitSyncSettings(string _flag)  //client
        {
            List<BBSetting> _bbs = new List<BBSetting>();
            var _settings = new Settings();
            _bbs.AddRange(_settings.rulesSettings);
            if (_flag == "player") { _bbs.AddRange(_settings.playerSettings); }
            _bbs.AddRange(_settings.characterSettings);

            return _bbs;
        }

        private void InitPlayerSyncInfo()  //client
        {
            if (StatManager.instance.currentPlayerSyncInfo == null)
            {
                StatManager.instance.currentPlayerSyncInfo = new ModConfig
                {
                    ModName = Settings.modName + "_player",
                    SettingsVersion = 1.0,
                    Settings = InitSyncSettings("player")
                };

                Settings.SoftRegister(StatManager.instance.currentPlayerSyncInfo);
            }
        }

        private void InitAiSyncInfo()  //client
        {
            if (StatManager.instance.currentAiSyncInfo == null)
            {
                StatManager.instance.currentAiSyncInfo = new ModConfig
                {
                    ModName = Settings.modName + "_ai",
                    SettingsVersion = 1.0,
                    Settings = InitSyncSettings("ai")
                };

                Settings.SoftRegister(StatManager.instance.currentAiSyncInfo);
            }
        }

        private void SendBoolSetting(ModConfig _config, string _flag)  //host
        {
            foreach (BBSetting _bbs in _config.Settings)
            {
                if (_bbs is BoolSetting _b)
                {
                    photonView.RPC("SendBoolSettingRPC", PhotonTargets.All, new object[]
                    {
                            _b.Name,
                            _b.m_value,
                            _flag
                    });
                }
            }
        }

        private void SendFloatSetting(ModConfig _config, string _flag)  //host
        {
            foreach (BBSetting _bbs in _config.Settings)
            {
                if (_bbs is FloatSetting _f)
                {
                    photonView.RPC("SendFloatSettingRPC", PhotonTargets.All, new object[]
                    {
                        _f.Name,
                        _f.m_value,
                        _flag
                    });
                }
            }
        }

        private void SendSyncSettings(ModConfig _config, string _flag)  //host
        {
            SendBoolSetting(_config, _flag);
            SendFloatSetting(_config, _flag);
        }

        private IEnumerator CO_SetSyncInfo()  //host
        {
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }

            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                if (!StatManager.instance.isAiInfoSynced)
                {
                    SendSyncSettings(Main.aiConfig, "ai");
                    photonView.RPC("SetAiSyncInfoRPC", PhotonTargets.All, new object[0]);
                    StatManager.instance.isAiInfoSynced = true;
                }

                if (!StatManager.instance.isPlayerInfoSynced)
                {
                    SendSyncSettings(Main.playerConfig, "player");
                    photonView.RPC("SetPlayerSyncInfoRPC", PhotonTargets.All, new object[0]);
                    StatManager.instance.isPlayerInfoSynced = true;
                }
            }
        }

        [PunRPC]
        private void SendBoolSettingRPC(string _name, bool _bool, string _flag)  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncBoolInfo(_name, _bool, _flag);
            }
        }

        [PunRPC]
        private void SendFloatSettingRPC(string _name, float _float, string _flag)  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncFloatInfo(_name, _float, _flag);
            }
        }

        [PunRPC]
        private void PlayerSyncRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.isPlayerInfoSynced = false;
            }
        }

        [PunRPC]
        private void SetPlayerSyncInfoRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetPlayerSyncInfo();
            }
        }

        [PunRPC]
        private void AiSyncRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.isAiInfoSynced = false;
            }
        }

        [PunRPC]
        private void SetAiSyncInfoRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetAiSyncInfo();
            }
        }
    }
}
