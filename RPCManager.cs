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
            photonView.RPC("RequestSettingsRPC", PhotonNetwork.masterClient, new object[0]);
        }

        [PunRPC]
        private void RequestSettingsRPC()
        {
            StartCoroutine(DelayedSendSettings());
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

                photonView.RPC("SendSettingsRPC", PhotonTargets.All, new object[] { Main.playerConfig, Main.aiConfig });
            }
        }

        [PunRPC]
        private void SendSettingsRPC(ModConfig _playerSyncInfo, ModConfig _aiSyncInfo)
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                StatManager.instance.SetSyncInfo(_playerSyncInfo, _aiSyncInfo);
            }
        }
    }
}
