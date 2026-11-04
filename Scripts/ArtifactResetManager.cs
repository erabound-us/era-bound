using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to a persistent scene object alongside a PhotonView.
    // MasterClient-only: handles disconnect penalty and artifact reset.
    public class ArtifactResetManager : MonoBehaviourPun, IInRoomCallbacks
    {
        public static ArtifactResetManager Instance { get; private set; }

        [SerializeField] BossSpawner bossSpawner;

        readonly Dictionary<int, List<ArtifactResetData>> _heldArtifacts =
            new Dictionary<int, List<ArtifactResetData>>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
        void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

        public void RegisterPickup(int actorNumber, ArtifactResetData data)
        {
            if (!_heldArtifacts.ContainsKey(actorNumber))
                _heldArtifacts[actorNumber] = new List<ArtifactResetData>();
            _heldArtifacts[actorNumber].Add(data);
        }

        public void ClearPlayer(int actorNumber) => _heldArtifacts.Remove(actorNumber);

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (!_heldArtifacts.TryGetValue(otherPlayer.ActorNumber, out var artifacts)) return;
            if (artifacts.Count == 0) return;

            foreach (var data in artifacts) ResetArtifact(data);
            _heldArtifacts.Remove(otherPlayer.ActorNumber);

            photonView.RPC(nameof(RPC_BroadcastReset), RpcTarget.All,
                otherPlayer.NickName, artifacts.Count);
        }

        void ResetArtifact(ArtifactResetData data)
        {
            var go = PhotonNetwork.Instantiate(data.prefabName, data.originalPosition, Quaternion.identity);
            go.GetComponent<ArtifactPickup>()?.InitWithBumpedRarity(data.artifactId, data.originalRarity);
            bossSpawner?.SpawnBossAt(data.originalPosition);
        }

        [PunRPC]
        void RPC_BroadcastReset(string playerName, int artifactCount)
        {
            ExtractionHUD.Instance?.OnArtifactReset(playerName, artifactCount);
        }

        // Unused IInRoomCallbacks
        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable props) { }
        public void OnPlayerPropertiesUpdate(Player player, ExitGames.Client.Photon.Hashtable props) { }
        public void OnMasterClientSwitched(Player newMaster) { }
        public void OnPlayerEnteredRoom(Player newPlayer) { }
    }

    [System.Serializable]
    public struct ArtifactResetData
    {
        public string artifactId;
        public string prefabName;
        public Vector3 originalPosition;
        public string originalRarity;
    }
}
