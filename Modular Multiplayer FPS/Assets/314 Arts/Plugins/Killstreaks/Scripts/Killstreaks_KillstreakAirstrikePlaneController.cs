using MarsFPSKit;
using Photon.Pun;
using UnityEngine;

namespace KillstreaksPlugin
{
    public class Killstreaks_KillstreakAirstrikePlaneController : MonoBehaviourPun, IPunObservable
    {
        #region Runtime
        /// <summary>
        /// Settings
        /// </summary>
        private Killstreaks_KillstreakAirstrikeMapSettings settings;
        [HideInInspector]
        /// <summary>
        /// Access to kit main
        /// </summary>
        public Kit_IngameMain main;
        [HideInInspector]
        public bool ownerIsBot;
        /// <summary>
        /// ID of the owner
        /// </summary>
        [HideInInspector]
        public int ownerId;
        /// <summary>
        /// Instance ID of owner when he called this
        /// </summary>
        public int ownerInstanceId;
        /// <summary>
        /// Where bombs must drop.
        /// </summary>
        private Vector3 targetLocation;
        /// <summary>
        /// How many planes were spawned
        /// </summary>
        private int amountOfPlanesSpawned;
        /// <summary>
        /// Timer used to spawn planes (:
        /// </summary>
        private double planeSpawnTimer;
        #endregion

        #region Unity Calls
        private void Awake()
        {
            //Find settings
            settings = FindObjectOfType<Killstreaks_KillstreakAirstrikeMapSettings>();
            if (!settings) throw new System.Exception("No Airstrike Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            main = FindObjectOfType<Kit_IngameMain>();
        }

        private void Start()
        {
            //Get owner
            ownerIsBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];
            ownerInstanceId = (int)photonView.InstantiationData[2];
            targetLocation = (Vector3)photonView.InstantiationData[3];
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (PhotonNetwork.Time > planeSpawnTimer)
                {
                    if (amountOfPlanesSpawned < settings.amountOfPlanes)
                    {
                        //Spawn Plane
                        Vector3 spawnPos = targetLocation;
                        spawnPos -= (transform.forward).normalized * settings.spawnPointDistance;
                        spawnPos.y = settings.planeAltitude;

                        object[] instData = new object[4];
                        instData[0] = ownerIsBot;
                        instData[1] = ownerId;
                        instData[2] = ownerInstanceId;
                        Vector3 targetForPlanes = targetLocation;
                        //We need to set this somewhere
                        targetForPlanes.y = settings.planeAltitude;
                        instData[3] = targetForPlanes;

                        //Spawn plane and let it fly! :D
                        PhotonNetwork.Instantiate(settings.planePrefab.name, spawnPos, transform.rotation, 0, instData);

                        amountOfPlanesSpawned++;
                        planeSpawnTimer = PhotonNetwork.Time + settings.planeSpawnTime;
                    }
                    else
                    {
                        //Destroy
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }
        #endregion

        #region Photon Calls
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(amountOfPlanesSpawned);
                stream.SendNext(planeSpawnTimer);
            }
            else
            {
                amountOfPlanesSpawned = (int)stream.ReceiveNext();
                planeSpawnTimer = (double)stream.ReceiveNext();
            }
        }
        #endregion
    }
}