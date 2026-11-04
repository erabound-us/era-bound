using UnityEngine;
using Photon.Pun;
using MarsFPSKit;
using ExitGames.Client.Photon;

namespace KillstreaksPlugin
{
    public class Killstreaks_KillstreakAirstrikeBomb : MonoBehaviourPun
    {
        /// <summary>
        /// Our rigidbody
        /// </summary>
        [Header("Components")]
        public Rigidbody body;
        /// <summary>
        /// Explosion prefab
        /// </summary>
        public GameObject explosionPrefab;

        #region Runtime
        [HideInInspector]
        public bool ownerIsBot;
        /// <summary>
        /// ID of the owner
        /// </summary>
        [HideInInspector]
        public int ownerId;
        /// <summary>
        /// Main reference
        /// </summary>
        private Kit_IngameMain main;
        #endregion

        #region Unity Calls
        private void Start()
        {
            //Get owner
            ownerIsBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];

            main = FindObjectOfType<Kit_IngameMain>();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                body.isKinematic = false;

                if (transform.position.y < main.mapDeathThreshold)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else
            {
                body.isKinematic = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (photonView.IsMine)
            {
                Hashtable table = new Hashtable();
                table["ownerIsBot"] = ownerIsBot;
                table["ownerId"] = ownerId;
                table["position"] = transform.position;
                PhotonNetwork.RaiseEvent(30, table, new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.All }, new SendOptions { Reliability = true });

                PhotonNetwork.Destroy(gameObject);
            }
        }
        #endregion
    }
}