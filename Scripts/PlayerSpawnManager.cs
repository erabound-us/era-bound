using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to a persistent scene object.
    // Bridges TransporterDropZone zone assignment with the kit's spawn + spawn-protection system.
    public class PlayerSpawnManager : MonoBehaviourPun
    {
        public static PlayerSpawnManager Instance { get; private set; }

        [SerializeField] float materializeDuration = 2f;
        [SerializeField] float orientDuration = 1.5f;
        [SerializeField] ArrivalEffect arrivalEffect;

        Kit_IngameMain _main;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _main = FindObjectOfType<Kit_IngameMain>();
        }

        void Start()
        {
            // Request a drop zone from MasterClient
            photonView.RPC(nameof(RPC_RequestDropZone), RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber);
        }

        // Called by TransporterDropZone after zone is validated
        public void BeginArrival(Vector3 position)
        {
            StartCoroutine(ArrivalSequence(position));
        }

        IEnumerator ArrivalSequence(Vector3 position)
        {
            // Kit handles spawn — we override the spawn point via a temporary spawn marker
            var tempSpawn = new GameObject("TempSpawn").transform;
            tempSpawn.position = position;

            // Disable input briefly for materialization feel
            if (_main.myPlayer != null)
                _main.myPlayer.canControlPlayer = false;

            arrivalEffect?.Play(position);
            yield return new WaitForSeconds(materializeDuration);

            // Kit's spawn protection (Kit_SpawnProtectionDefault) handles invulnerability window
            // configured on the player prefab — we just spawn
            _main.Spawn(true);

            if (_main.myPlayer != null)
            {
                _main.myPlayer.canControlPlayer = false; // orientation phase
                yield return new WaitForSeconds(orientDuration);
                _main.myPlayer.canControlPlayer = true;
            }

            Destroy(tempSpawn.gameObject);
        }

        [PunRPC]
        void RPC_RequestDropZone(int actorNumber)
        {
            TransporterDropZone.Instance?.HandleDropZoneRequest(actorNumber);
        }
    }
}
