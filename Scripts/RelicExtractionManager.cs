using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    public class RelicExtractionManager : MonoBehaviourPun
    {
        public static RelicExtractionManager Instance { get; private set; }

        [SerializeField] float teamEligibleRadius       = 20f;
        [SerializeField] float soloChannelDuration      = 20f;
        [SerializeField] float soloEliteChannelDuration = 14f;
        [SerializeField] float teamChannelDuration      = 10f;
        [SerializeField] float repeatBroadcastInterval  = 3f;

        bool _relicInPlay;
        string _relicId;
        Coroutine _repeatBroadcast;
        Kit_IngameMain _main;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _main = FindObjectOfType<Kit_IngameMain>();
        }

        public void OnRelicPickedUp(string relicId)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            _relicInPlay = true;
            _relicId = relicId;
            photonView.RPC(nameof(RPC_RelicPickedUp), RpcTarget.All, relicId);
        }

        public float GetChannelDuration()
        {
            bool isTeam = IsInTeam();
            bool hasEliteGear = GearLoadout.Instance != null && GearLoadout.Instance.HasRelicExtractionGear();
            if (isTeam) return teamChannelDuration;
            return hasEliteGear ? soloEliteChannelDuration : soloChannelDuration;
        }

        public bool DamageSlowsInsteadOfCancels() =>
            !IsInTeam() && GearLoadout.Instance != null && GearLoadout.Instance.HasRelicExtractionGear();

        public void OnRelicExtractionComplete(int channelingActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient || !_relicInPlay) return;

            bool isTeam = IsActorInTeam(channelingActorNumber);
            if (!isTeam) { AwardRelic(channelingActorNumber); return; }

            var eligible = GetEligibleTeamMembers(channelingActorNumber);
            int winnerActor = eligible.Count > 0
                ? eligible[Random.Range(0, eligible.Count)]
                : channelingActorNumber;

            AwardRelic(winnerActor);

            foreach (int actor in eligible.Where(a => a != winnerActor))
                photonView.RPC(nameof(RPC_AwardConsolation),
                    PhotonNetwork.CurrentRoom.GetPlayer(actor), _relicId);
        }

        void AwardRelic(int actorNumber)
        {
            var winner = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            photonView.RPC(nameof(RPC_RelicAwarded), RpcTarget.All,
                actorNumber, winner?.NickName ?? "Unknown", _relicId);
            _relicInPlay = false;
            if (_repeatBroadcast != null) StopCoroutine(_repeatBroadcast);
        }

        // Uses kit's allActivePlayers for proximity — no FindObjectsOfType
        List<int> GetEligibleTeamMembers(int channelingActor)
        {
            var eligible = new List<int> { channelingActor };
            if (_main == null) return eligible;
            var beaconPos = ExtractionBeacon.Instance.transform.position;

            foreach (var pb in _main.allActivePlayers)
            {
                if (pb == null || pb.photonView.Owner == null) continue;
                int actor = pb.photonView.Owner.ActorNumber;
                if (actor == channelingActor) continue;
                if (!IsActorInTeam(actor)) continue;
                if (Vector3.Distance(pb.transform.position, beaconPos) <= teamEligibleRadius)
                    eligible.Add(actor);
            }
            return eligible;
        }

        public void StartRepeatBroadcast(int actorNumber)
        {
            if (!IsInTeam() && GearLoadout.Instance != null && !GearLoadout.Instance.HasRelicExtractionGear())
                _repeatBroadcast = StartCoroutine(RepeatBroadcastRoutine(actorNumber));
        }

        public void StopRepeatBroadcast()
        {
            if (_repeatBroadcast != null) StopCoroutine(_repeatBroadcast);
        }

        IEnumerator RepeatBroadcastRoutine(int actorNumber)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            while (true)
            {
                yield return new WaitForSeconds(repeatBroadcastInterval);
                photonView.RPC(nameof(RPC_RepeatBroadcast), RpcTarget.All,
                    player?.NickName ?? "Unknown");
            }
        }

        [PunRPC] void RPC_RelicPickedUp(string relicId) =>
            ExtractionHUD.Instance?.OnRelicPickedUp(relicId);

        [PunRPC]
        void RPC_RelicAwarded(int actorNumber, string playerName, string relicId)
        {
            bool isLocal = PhotonNetwork.LocalPlayer.ActorNumber == actorNumber;
            ExtractionHUD.Instance?.OnRelicAwarded(playerName, relicId, isLocal);
            if (isLocal) PlayerInventory.LocalInstance?.CommitRelicExtraction(relicId);
        }

        [PunRPC] void RPC_AwardConsolation(string relicId) =>
            PlayerInventory.LocalInstance?.CommitConsolationReward(relicId);

        [PunRPC] void RPC_RepeatBroadcast(string playerName) =>
            ExtractionHUD.Instance?.OnSoloRelicBroadcast(playerName);

        static bool IsInTeam()
        {
            var props = PhotonNetwork.LocalPlayer.CustomProperties;
            return props.ContainsKey("teamId") && props["teamId"] != null;
        }

        static bool IsActorInTeam(int actorNumber)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            return player != null
                && player.CustomProperties.ContainsKey("teamId")
                && player.CustomProperties["teamId"] != null;
        }
    }
}
