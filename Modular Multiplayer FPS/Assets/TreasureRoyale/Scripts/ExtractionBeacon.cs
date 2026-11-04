using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public class ExtractionBeacon : MonoBehaviourPun
    {
        public static ExtractionBeacon Instance { get; private set; }

        public enum BeaconState { Idle, Channeling, Contested, Cooldown }

        [Header("Config")]
        [SerializeField] float channelDuration  = 10f;
        [SerializeField] float cooldownDuration = 30f;
        [SerializeField] float contestRadius    = 10f;
        [SerializeField] float cancelMoveDistance = 1f;

        [Header("Visuals")]
        [SerializeField] MonoBehaviour beaconVisual;

        public BeaconState State { get; private set; } = BeaconState.Idle;
        public float ChannelProgress { get; private set; }

        int _channelingActor = -1;
        Vector3 _channelStartPos;
        float _channelElapsed;
        Coroutine _channelRoutine;

        Kit_IngameMain _main;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _main = FindObjectOfType<Kit_IngameMain>();
        }

        public void RequestBeginChannel(int actorNumber, Vector3 playerPos)
        {
            photonView.RPC(nameof(RPC_BeginChannel), RpcTarget.MasterClient, actorNumber, playerPos);
        }

        public void RequestCancelChannel(int actorNumber)
        {
            photonView.RPC(nameof(RPC_CancelChannel), RpcTarget.MasterClient, actorNumber);
        }

        [PunRPC]
        void RPC_BeginChannel(int actorNumber, Vector3 playerPos)
        {
            if (!PhotonNetwork.IsMasterClient || State != BeaconState.Idle) return;
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player == null) return;

            _channelingActor = actorNumber;
            _channelStartPos = playerPos;
            _channelElapsed  = 0f;

            photonView.RPC(nameof(RPC_SyncState), RpcTarget.All,
                (int)BeaconState.Channeling, actorNumber, player.NickName);

            if (_channelRoutine != null) StopCoroutine(_channelRoutine);
            _channelRoutine = StartCoroutine(ChannelTick());
        }

        [PunRPC]
        void RPC_CancelChannel(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient || _channelingActor != actorNumber) return;
            AbortChannel();
        }

        IEnumerator ChannelTick()
        {
            while (_channelElapsed < channelDuration)
            {
                yield return new WaitForSeconds(0.25f);
                if (!PhotonNetwork.IsMasterClient) yield break;

                Kit_PlayerBehaviour channelingPb = GetKitPlayer(_channelingActor);
                if (channelingPb == null) { AbortChannel(); yield break; }

                if (Vector3.Distance(channelingPb.transform.position, _channelStartPos) > cancelMoveDistance)
                {
                    AbortChannel();
                    yield break;
                }

                bool contested = IsContested(channelingPb.transform.position);
                var newState = contested ? BeaconState.Contested : BeaconState.Channeling;

                if (newState != State)
                    photonView.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)newState, _channelingActor, "");

                if (!contested) _channelElapsed += 0.25f;

                photonView.RPC(nameof(RPC_SyncProgress), RpcTarget.All, _channelElapsed / channelDuration);
            }

            CompleteExtraction();
        }

        void CompleteExtraction()
        {
            photonView.RPC(nameof(RPC_ExtractionComplete), RpcTarget.All, _channelingActor);
            RelicExtractionManager.Instance?.OnRelicExtractionComplete(_channelingActor);
            _channelingActor = -1;
            StartCoroutine(CooldownRoutine());
        }

        void AbortChannel()
        {
            if (_channelRoutine != null) StopCoroutine(_channelRoutine);
            photonView.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)BeaconState.Idle, -1, "");
            _channelingActor = -1;
            _channelElapsed  = 0f;
        }

        IEnumerator CooldownRoutine()
        {
            photonView.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)BeaconState.Cooldown, -1, "");
            yield return new WaitForSeconds(cooldownDuration);
            photonView.RPC(nameof(RPC_SyncState), RpcTarget.All, (int)BeaconState.Idle, -1, "");
        }

        // Uses kit's allActivePlayers list — no extra Physics.OverlapSphere needed
        bool IsContested(Vector3 pos)
        {
            if (_main == null) return false;
            foreach (var pb in _main.allActivePlayers)
            {
                if (pb == null) continue;
                if (pb.photonView.Owner?.ActorNumber == _channelingActor) continue;
                if (Vector3.Distance(pb.transform.position, pos) <= contestRadius) return true;
            }
            return false;
        }

        [PunRPC]
        void RPC_SyncState(int state, int actorNumber, string playerName)
        {
            State = (BeaconState)state;
            beaconVisual?.SendMessage("SetState", State, SendMessageOptions.DontRequireReceiver);
            ExtractionHUD.Instance?.OnBeaconStateChanged(State, playerName);
        }

        [PunRPC]
        void RPC_SyncProgress(float progress)
        {
            ChannelProgress = progress;
            ExtractionHUD.Instance?.OnProgressChanged(progress, State);
        }

        [PunRPC]
        void RPC_ExtractionComplete(int actorNumber)
        {
            bool isLocal = PhotonNetwork.LocalPlayer.ActorNumber == actorNumber;
            ExtractionHUD.Instance?.OnExtractionComplete(actorNumber, isLocal);
            if (isLocal) PlayerInventory.LocalInstance?.CommitExtraction();
        }

        // Uses kit's allActivePlayers instead of FindObjectsOfType<PhotonView>
        Kit_PlayerBehaviour GetKitPlayer(int actorNumber)
        {
            if (_main == null) return null;
            foreach (var pb in _main.allActivePlayers)
                if (pb != null && pb.photonView.Owner?.ActorNumber == actorNumber) return pb;
            return null;
        }
    }
}
