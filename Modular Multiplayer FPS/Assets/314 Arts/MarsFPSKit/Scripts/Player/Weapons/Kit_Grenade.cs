using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This script is the actual grenade
    /// </summary>
    public class Kit_Grenade : Photon.Pun.MonoBehaviourPun
    {
        /// <summary>
        /// Time until the grenade explodes!
        /// </summary>
        public float explosionTime = 5f;

        /// <summary>
        /// Rigidbody of this grenade!
        /// </summary>
        public Rigidbody rb;

        /// <summary>
        /// Explosion that this grenade makes!
        /// </summary>
        public GameObject explosionPrefab;

        [HideInInspector]
        /// <summary>
        /// Gun this was fired from!
        /// </summary>
        public int gunId;
        /// <summary>
        /// cached owner id
        /// </summary>
        private int ownerId;
        /// <summary>
        /// Is the owner a bot?
        /// </summary>
        private bool ownerBot;

        private bool hasExploded;
        private bool wasThrown;
        private bool isMine;

        IEnumerator Start()
        {
            ownerBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];
            isMine = photonView.IsMine;
            rb.isKinematic = !isMine;
            wasThrown = true;
            if (isMine)
            {
                yield return new WaitForSeconds(explosionTime);
                PhotonNetwork.Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (!wasThrown || hasExploded || !Application.isPlaying) return;
            hasExploded = true;
            if (explosionPrefab)
            {
                GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation);
                Kit_Explosion exp = go.GetComponent<Kit_Explosion>();
                if (exp) exp.Explode(isMine, ownerBot, ownerId, gunId);
                Kit_FlashbangExplosion flash = go.GetComponent<Kit_FlashbangExplosion>();
                if (flash) flash.Explode(isMine, ownerBot, ownerId, gunId);
            }
        }
    }
}