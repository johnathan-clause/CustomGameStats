using System.Collections;
using UnityEngine;
using SharedModConfig;
using System.Collections.Generic;

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

            photon.viewID = 902;
        }

        public void RequestSync()  //client
        {
            Debug.Log("Initializing sync infos...");
            InitPlayerSyncInfo();
            InitAiSyncInfo();

            Debug.Log("Requesting sync....");
            photonView.RPC("RequestSyncRPC", PhotonNetwork.masterClient, new object[0]);
        }

        public void Sync() //host
        {
            Debug.Log("RPC sending client notification...");
            photonView.RPC("SyncRPC", PhotonTargets.All, new object[0]);
        }

        [PunRPC]
        private void RequestSyncRPC()  //host
        {
            Debug.Log("Client sync request recieved...");
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
            Debug.Log("Sending bool settings...");
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
            Debug.Log("Starting iteration for " + _flag + " settings...");
            SendBoolSetting(_config, _flag);
            SendFloatSetting(_config, _flag);
        }

        private IEnumerator CO_SetSyncInfo()  //host
        {
            Debug.Log("Delayed send starting...");
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }
            Debug.Log("Players done loading for delayed send...");
            Debug.Log("Checking for conditions to send...");
            if (!PhotonNetwork.isNonMasterClientInRoom && !StatManager.instance.isInfoSynced)
            {
                Debug.Log("Conditions met, stating send...");
                SendSyncSettings(Main.playerConfig, "player");
                SendSyncSettings(Main.aiConfig, "ai");
                Debug.Log("Iteration finished...");
                Debug.Log("Attempting to finalize sync...");
                photonView.RPC("SetSyncInfoRPC", PhotonTargets.All, new object[0]);
                Debug.Log("Delayed process ending...");
                StatManager.instance.isInfoSynced = true;
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
        private void SetSyncInfoRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncInfo();
            }
        }

        [PunRPC]
        private void SyncRPC()  //client
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                Debug.Log("Recieved sync notification...");
                StatManager.instance.isInfoSynced = false;
            }
        }
    }
}
