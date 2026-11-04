using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // MasterClient-only drop zone validator.
    // Attach to a persistent scene object alongside a PhotonView.
    public class TransporterDropZone : MonoBehaviourPun
    {
        public static TransporterDropZone Instance { get; private set; }

        [Header("Safety Radii")]
        [SerializeField] float playerClearRadius  = 30f;
        [SerializeField] float creatureClearRadius = 40f;
        [SerializeField] float artifactClearRadius = 25f;
        [SerializeField] float releaseDistance     = 15f;

        [Header("Layers")]
        [SerializeField] LayerMask playerLayer;
        [SerializeField] LayerMask creatureLayer;
        [SerializeField] LayerMask artifactLayer;

        DropZoneMarker[] _allZones;
        readonly HashSet<int> _reservedZones = new HashSet<int>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => _allZones = FindObjectsOfType<DropZoneMarker>();

        void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            ReleaseVacatedZones();
        }

        // Called by PlayerSpawnManager RPC (already on MasterClient)
        public void HandleDropZoneRequest(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Vector3 pos = FindValidZone(actorNumber, 1f);
            photonView.RPC(nameof(RPC_AssignDropZone),
                PhotonNetwork.CurrentRoom.GetPlayer(actorNumber),
                pos.x, pos.y, pos.z);
        }

        [PunRPC]
        void RPC_AssignDropZone(float x, float y, float z)
        {
            PlayerSpawnManager.Instance?.BeginArrival(new Vector3(x, y, z));
        }

        Vector3 FindValidZone(int actorNumber, float radiusMultiplier)
        {
            int seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["universeSeed"] ^ actorNumber;
            var indices = ShuffledIndices(_allZones.Length, seed);

            foreach (int i in indices)
            {
                if (_reservedZones.Contains(i)) continue;
                Vector3 pos = _allZones[i].transform.position;
                if (!IsClear(pos, radiusMultiplier)) continue;
                _reservedZones.Add(i);
                return pos;
            }

            if (radiusMultiplier < 1.5f) return FindValidZone(actorNumber, radiusMultiplier + 0.2f);
            return FurthestFromAllPlayers();
        }

        bool IsClear(Vector3 pos, float mult) =>
            Physics.OverlapSphere(pos, playerClearRadius   * mult, playerLayer).Length   == 0 &&
            Physics.OverlapSphere(pos, creatureClearRadius * mult, creatureLayer).Length == 0 &&
            Physics.OverlapSphere(pos, artifactClearRadius * mult, artifactLayer).Length == 0;

        void ReleaseVacatedZones()
        {
            var toRelease = new List<int>();
            foreach (int i in _reservedZones)
                if (Physics.OverlapSphere(_allZones[i].transform.position, releaseDistance, playerLayer).Length == 0)
                    toRelease.Add(i);
            foreach (int i in toRelease) _reservedZones.Remove(i);
        }

        Vector3 FurthestFromAllPlayers()
        {
            var players = FindObjectsOfType<PhotonView>();
            Vector3 best = _allZones[0].transform.position;
            float bestDist = 0f;
            foreach (var zone in _allZones)
            {
                float minDist = float.MaxValue;
                foreach (var pv in players)
                {
                    if (pv.Owner == null) continue;
                    float d = Vector3.Distance(zone.transform.position, pv.transform.position);
                    if (d < minDist) minDist = d;
                }
                if (minDist > bestDist) { bestDist = minDist; best = zone.transform.position; }
            }
            return best;
        }

        static List<int> ShuffledIndices(int count, int seed)
        {
            var list = new List<int>(count);
            for (int i = 0; i < count; i++) list.Add(i);
            var rng = new System.Random(seed);
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}
