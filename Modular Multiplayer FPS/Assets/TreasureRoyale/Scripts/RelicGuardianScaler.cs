using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the Relic Guardian prefab alongside a PhotonView.
    // MasterClient scales health/damage based on live player count from kit's allActivePlayers.
    public class RelicGuardianScaler : MonoBehaviourPun
    {
        [SerializeField] float baseHealth = 500f;
        [SerializeField] float healthPerPlayer = 150f;
        [SerializeField] float baseDamage = 25f;
        [SerializeField] float damagePerPlayer = 5f;

        Kit_IngameMain _main;

        void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            _main = FindObjectOfType<Kit_IngameMain>();
            int playerCount = _main != null ? _main.allActivePlayers.Count : PhotonNetwork.PlayerList.Length;
            float health = baseHealth + healthPerPlayer * (playerCount - 1);
            float damage = baseDamage + damagePerPlayer * (playerCount - 1);
            photonView.RPC(nameof(RPC_ApplyScale), RpcTarget.All, health, damage);
        }

        [PunRPC]
        void RPC_ApplyScale(float health, float damage)
        {
            var guardian = GetComponent<GuardianController>();
            if (guardian == null) return;
            guardian.SetHealth(health);
            guardian.SetDamage(damage);
        }
    }
}
