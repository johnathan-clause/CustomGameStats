using System.Collections;
using UnityEngine;
using SharedModConfig;

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

        public void RequestSettings()
        {
            Debug.Log("Requesting sync....");
            photonView.RPC("RequestSettingsRPC", PhotonNetwork.masterClient, new object[0]);
        }

        [PunRPC]
        private void RequestSettingsRPC()
        {
            StartCoroutine(DelayedSendSettings());
        }

        private void IterateSettings(ModConfig _config, string _set)
        {
            foreach (FloatSetting _f in _config.Settings)
            {
                foreach (BoolSetting _b in _config.Settings)
                {
                    if (_b.Name.Contains(_f.Name + Settings.modMult))
                    {
                        photonView.RPC("SendBoolSettingRPC", PhotonTargets.All, new object[]
                        {
                                _b.m_value,
                                _set
                        });
                    }
                }

                photonView.RPC("SendFloatSettingRPC", PhotonTargets.All, new object[]
                {
                        _f.m_value,
                        _set
                });
            }
        }

        private IEnumerator DelayedSendSettings()
        {
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }

            if (!PhotonNetwork.isNonMasterClientInRoom && (StatManager.instance.timeOfLastSync < 0 || Time.time - StatManager.instance.timeOfLastSync > 10f))
            {
                StatManager.instance.timeOfLastSync = Time.time;
                IterateSettings(Main.playerConfig, "player");
                IterateSettings(Main.aiConfig, "ai");
            }
        }

        [PunRPC]
        private void SendBoolSettingRPC(bool _bool, string _set)
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncBoolInfo(_bool, _set);
            }
        }

        [PunRPC]
        private void SendFloatSettingRPC(float _float, string _set)
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncFloatInfo(_float, _set);
            }
        }
    }
}
