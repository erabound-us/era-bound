using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MarsFPSKit
{
    // Attach to each creature prefab alongside a NavMeshAgent.
    // MasterClient-only: pauses NavMeshAgent and AI updates when no player
    // is within activationRadius, saving CPU in creature-heavy eras.
    [RequireComponent(typeof(NavMeshAgent))]
    public class TR_CreatureAIThrottle : MonoBehaviour
    {
        [SerializeField] float activationRadius = 60f;
        [SerializeField] float checkInterval    = 1f;

        NavMeshAgent _agent;
        Kit_IngameMain _main;
        float _nextCheck;
        bool _active = true;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _main  = FindObjectOfType<Kit_IngameMain>();
        }

        void Update()
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient) return;
            if (Time.time < _nextCheck) return;
            _nextCheck = Time.time + checkInterval;

            bool shouldBeActive = AnyPlayerInRange();
            if (shouldBeActive == _active) return;

            _active = shouldBeActive;
            _agent.isStopped = !_active;
            _agent.enabled   = _active;
        }

        bool AnyPlayerInRange()
        {
            if (_main == null) return true;
            foreach (var pb in _main.allActivePlayers)
            {
                if (pb == null) continue;
                if (Vector3.Distance(transform.position, pb.transform.position) <= activationRadius)
                    return true;
            }
            return false;
        }
    }
}
