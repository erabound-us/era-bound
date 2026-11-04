using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach once per scene on a persistent manager object.
    // Divides the map into interest zones (Photon interest groups).
    // Local player subscribes only to nearby zones, cutting bandwidth
    // for large era maps with 16 players spread across the map.
    //
    // Each PhotonView on creatures/artifacts/containers should have its
    // interest group set to match its zone's groupId.
    public class TR_InterestManager : MonoBehaviour
    {
        [System.Serializable]
        public class InterestZone
        {
            public byte groupId;        // Photon interest group (1–255; 0 = global)
            public Vector3 center;
            public float radius;
        }

        [SerializeField] InterestZone[] zones;
        [SerializeField] float updateInterval = 2f;

        Kit_IngameMain _main;
        float _nextUpdate;
        readonly HashSet<byte> _subscribedGroups = new HashSet<byte>();

        void Start()
        {
            _main = FindObjectOfType<Kit_IngameMain>();
            // Always subscribe to group 0 (global — beacons, match events)
            PhotonNetwork.SetInterestGroups(new byte[0], new byte[] { 0 });
            _subscribedGroups.Add(0);
        }

        void Update()
        {
            if (!PhotonNetwork.InRoom) return;
            if (_main?.myPlayer == null) return;
            if (Time.time < _nextUpdate) return;
            _nextUpdate = Time.time + updateInterval;

            UpdateSubscriptions(_main.myPlayer.transform.position);
        }

        void UpdateSubscriptions(Vector3 playerPos)
        {
            var desired = new HashSet<byte> { 0 };
            foreach (var zone in zones)
                if (Vector3.Distance(playerPos, zone.center) <= zone.radius)
                    desired.Add(zone.groupId);

            var toAdd    = new List<byte>();
            var toRemove = new List<byte>();

            foreach (byte g in desired)
                if (!_subscribedGroups.Contains(g)) toAdd.Add(g);

            foreach (byte g in _subscribedGroups)
                if (!desired.Contains(g)) toRemove.Add(g);

            if (toAdd.Count == 0 && toRemove.Count == 0) return;

            PhotonNetwork.SetInterestGroups(toRemove.ToArray(), toAdd.ToArray());

            foreach (byte g in toRemove) _subscribedGroups.Remove(g);
            foreach (byte g in toAdd)    _subscribedGroups.Add(g);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (zones == null) return;
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
            foreach (var z in zones)
                Gizmos.DrawSphere(z.center, z.radius);
        }
#endif
    }
}
