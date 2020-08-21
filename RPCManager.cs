using System.Collections;
using System.Collections.Generic;
using SharedModConfig;
using UnityEngine;

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

        public void RequestPlayerSync()  //client
        {
            InitPlayerSyncInfo();
            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("RequestPlayerSyncRPC", PhotonNetwork.masterClient, new object[0]);
            }
        }

        public void RequestAiSync()  //client
        {
            InitAiSyncInfo();
            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("RequestAiSyncRPC", PhotonNetwork.masterClient, new object[0]);
            }
        }

        public void PlayerSync() //host
        {
            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("PlayerSyncRPC", PhotonTargets.All, new object[0]);
            }
        }

        public void AiSync() //host
        {
            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("AiSyncRPC", PhotonTargets.All, new object[0]);
            }
        }

        [PunRPC]
        private void RequestPlayerSyncRPC()  //host
        {
            StartCoroutine(CO_SetPlayerSyncInfo());
        }

        [PunRPC]
        private void RequestAiSyncRPC()  //host
        {
            StartCoroutine(CO_SetAiSyncInfo());
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
            StatManager.instance.currentPlayerSyncInfo = new ModConfig
            {
                ModName = Settings.modName + "_Player",
                SettingsVersion = 1.0,
                Settings = InitSyncSettings("player")
            };

            Settings.SoftRegister(StatManager.instance.currentPlayerSyncInfo);
        }

        private void InitAiSyncInfo()  //client
        {
            StatManager.instance.currentAiSyncInfo = new ModConfig
            {
                ModName = Settings.modName + "_Ai",
                SettingsVersion = 1.0,
                Settings = InitSyncSettings("ai")
            };

            Settings.SoftRegister(StatManager.instance.currentAiSyncInfo);
        }

        private void SendBoolSetting(ModConfig _config, string _flag)  //host
        {
            foreach (BBSetting _bbs in _config.Settings)
            {
                if (_bbs is BoolSetting _b && !PhotonNetwork.offlineMode)
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
                if (_bbs is FloatSetting _f && !PhotonNetwork.offlineMode)
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

        private IEnumerator CO_SetPlayerSyncInfo()  //host
        {
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }

            SendSyncSettings(Main.playerConfig, "player");

            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("SetPlayerSyncInfoRPC", PhotonTargets.All, new object[0]);
            }
        }

        private IEnumerator CO_SetAiSyncInfo()  //host
        {
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }

            SendSyncSettings(Main.aiConfig, "ai");

            if (!PhotonNetwork.offlineMode)
            {
                photonView.RPC("SetAiSyncInfoRPC", PhotonTargets.All, new object[0]);
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
            StatManager.instance.currentPlayerSyncInfo = null;
        }

        [PunRPC]
        private void SetPlayerSyncInfoRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                if (!StatManager.instance.playerSyncInit)
                {
                    StatManager.instance.playerSyncInit = true;
                }

                StatManager.instance.SetPlayerSyncInfo();
            }
        }

        [PunRPC]
        private void AiSyncRPC()  //client
        {
            StatManager.instance.currentAiSyncInfo = null;
        }

        [PunRPC]
        private void SetAiSyncInfoRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                if (!StatManager.instance.aiSyncInit)
                {
                    StatManager.instance.aiSyncInit = true;
                }

                StatManager.instance.SetAiSyncInfo();
            }
        }
    }
}
