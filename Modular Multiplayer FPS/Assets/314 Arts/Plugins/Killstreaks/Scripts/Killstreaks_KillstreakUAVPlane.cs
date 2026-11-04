using MarsFPSKit;
using Photon.Pun;
using UnityEngine;

namespace KillstreaksPlugin
{
    /// <summary>
    /// This implements the actual plane for the UAV!
    /// </summary>
    public class Killstreaks_KillstreakUAVPlane : MonoBehaviourPun, IPunObservable, IKitDamageable
    {
        [Header("Components")]
        /// <summary>
        /// Prefab for replacement
        /// </summary>
        public GameObject replacementPrefab;
        /// <summary>
        /// Which killstreak does this prefab belong to?
        /// </summary>
        public Killstreak_KillstreakBase belongsToKillstreak;


        #region Runtime
        /// <summary>
        /// Settings
        /// </summary>
        private Killstreaks_KillstreakUAVMapSettings settings;
        /// <summary>
        /// Access to kit main
        /// </summary>
        private Kit_IngameMain main;
        /// <summary>
        /// Tells us if the owner is a bot
        /// </summary>
        [HideInInspector]
        public bool ownerIsBot;
        /// <summary>
        /// ID of the owner
        /// </summary>
        [HideInInspector]
        public int ownerId;
        [HideInInspector]
        /// <summary>
        /// Instance ID of owner when he called this
        /// </summary>
        public int ownerInstanceId;
        /// <summary>
        /// 0 = Arrival; 1 = Circling; 2 = Departure
        /// </summary>
        private int uavState;
        /// <summary>
        /// Progress from UAV for next step!
        /// </summary>
        private float uavProgress;
        /// <summary>
        /// Hit points of UAV
        /// </summary>
        public float hitPoints = 1000f;
        /// <summary>
        /// Did we call the commander?
        /// </summary>
        private bool wasCommanderCalled;
        #endregion

        #region Unity Calls
        private void Awake()
        {
            //Find settings
            settings = FindObjectOfType<Killstreaks_KillstreakUAVMapSettings>();
            if (!settings) throw new System.Exception("No UAV Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            main = FindObjectOfType<Kit_IngameMain>();
        }

        private void Start()
        {
            //Get owner
            ownerIsBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];
            ownerInstanceId = (int)photonView.InstantiationData[2];

            if (photonView.IsMine)
            {
                //Set initial POS.
                transform.position = settings.startPosition.position;
                transform.rotation = settings.startPosition.rotation;
            }

            Kit_Events.onTeamSwitched.AddListener(delegate { UpdatePlayerStatus(); });
            UpdatePlayerStatus();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (uavProgress < 1f)
                {
                    if (uavState == 0)
                    {
                        transform.position = Vector3.Lerp(settings.startPosition.position, settings.circlingStartPosition.position, uavProgress);
                        transform.rotation = Quaternion.Slerp(settings.startPosition.rotation, settings.circlingStartPosition.rotation, uavProgress);
                        uavProgress += Time.deltaTime / settings.uavArrivalTime;
                    }
                    else if (uavState == 1)
                    {
                        transform.position = Vector3.Lerp(settings.circlingStartPosition.position, settings.circlingEndPosition.position, uavProgress);
                        transform.rotation = Quaternion.Slerp(settings.circlingStartPosition.rotation, settings.circlingEndPosition.rotation, uavProgress);
                        uavProgress += Time.deltaTime / settings.uavLifeTime;
                    }
                    else if (uavState == 2)
                    {
                        transform.position = Vector3.Lerp(settings.circlingEndPosition.position, settings.endPosition.position, uavProgress);
                        transform.rotation = Quaternion.Slerp(settings.circlingEndPosition.rotation, settings.endPosition.rotation, uavProgress);
                        uavProgress += Time.deltaTime / settings.uavLifeTime;
                    }
                }
                else
                {
                    if (uavState == 0)
                    {
                        //Progress
                        uavState++;
                        //Reset timer
                        uavProgress = 0f;
                    }
                    else if (uavState == 1)
                    {
                        //Progress
                        uavState++;
                        //Reset timer
                        uavProgress = 0f;
                    }
                    else if (uavState == 2)
                    {
                        //Destroy UAV
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (!main) main = FindObjectOfType<Kit_IngameMain>();
            if (main.assignedTeamID != 2)
            {
                if (main.currentPvPGameModeBehaviour.isTeamGameMode)
                {
                    if (!main.currentPvPGameModeBehaviour.AreWeEnemies(main, ownerIsBot, ownerId) || !ownerIsBot && ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        settings.scannerIsOnline = false;
                    }
                }
                else
                {
                    if (!ownerIsBot && ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        settings.scannerIsOnline = false;
                    }
                }
            }

            if (main && main.mainCamera)
            {
                if (Vector3.Distance(transform.position, main.mainCamera.transform.position) < 300f)
                {
                    //Explode!
                    if (replacementPrefab)
                    {
                        GameObject go = Instantiate(replacementPrefab, transform.position, transform.rotation);
                        if (go.GetComponent<Kit_Explosion>())
                        {
                            go.GetComponent<Kit_Explosion>().Explode(photonView.IsMine, ownerIsBot, ownerId, "Helicopter");
                        }
                    }
                }
            }
        }
        #endregion

        #region Photon Calls
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(uavState);
                stream.SendNext(uavProgress);
            }
            else
            {
                uavState = (int)stream.ReceiveNext();
                uavProgress = (float)stream.ReceiveNext();
            }
        }
        #endregion

        #region Custom Calls
        public bool LocalDamage(float dmg, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, bool shotBot, int shotId)
        {
            if (hitPoints > 0)
            {
                if (main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, ownerId, ownerIsBot, shotId, shotBot, false))
                {
                    photonView.RPC("ApplyDamageNetwork", RpcTarget.MasterClient, dmg, shotBot, shotId);
                    return true;
                }
            }
            return false;
        }

        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool shotBot, int shotId)
        {
            hitPoints -= dmg;
            if (hitPoints <= 0)
            {
                //Send XP
                if (!shotBot)
                {
                    PhotonNetwork.RaiseEvent(8, settings.xpGainedUponDestruction, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { shotId } }, new ExitGames.Client.Photon.SendOptions { Reliability = true });
                }

                PhotonNetwork.Destroy(gameObject);
            }
        }

        public void UpdatePlayerStatus()
        {
            if (!main) main = FindObjectOfType<Kit_IngameMain>();
            if (main.assignedTeamID != 2)
            {
                if (main.currentPvPGameModeBehaviour.isTeamGameMode)
                {
                    if (!main.currentPvPGameModeBehaviour.AreWeEnemies(main, ownerIsBot, ownerId) || !ownerIsBot && ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        settings.scannerIsOnline = true;

                        if (!wasCommanderCalled)
                        {
                            Killstreaks_Commander commander = FindObjectOfType<Killstreaks_Commander>();
                            if (commander)
                            {
                                commander.FriendlyOnline(belongsToKillstreak);
                            }
                            wasCommanderCalled = true;
                        }
                    }
                    else
                    {
                        settings.scannerIsOnline = false;

                        if (!wasCommanderCalled)
                        {
                            Killstreaks_Commander commander = FindObjectOfType<Killstreaks_Commander>();
                            if (commander)
                            {
                                commander.EnemyOnline(belongsToKillstreak);
                            }
                            wasCommanderCalled = true;
                        }
                    }
                }
                else
                {
                    if (!ownerIsBot && ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        settings.scannerIsOnline = true;
                        if (!wasCommanderCalled)
                        {
                            Killstreaks_Commander commander = FindObjectOfType<Killstreaks_Commander>();
                            if (commander)
                            {
                                commander.FriendlyOnline(belongsToKillstreak);
                            }
                            wasCommanderCalled = true;
                        }
                    }
                    else
                    {
                        settings.scannerIsOnline = false;

                        if (!wasCommanderCalled)
                        {
                            Killstreaks_Commander commander = FindObjectOfType<Killstreaks_Commander>();
                            if (commander)
                            {
                                commander.EnemyOnline(belongsToKillstreak);
                            }
                            wasCommanderCalled = true;
                        }
                    }
                }
            }
            else
            {
                settings.scannerIsOnline = false;

                if (!wasCommanderCalled)
                {
                    Killstreaks_Commander commander = FindObjectOfType<Killstreaks_Commander>();
                    if (commander)
                    {
                        commander.EnemyOnline(belongsToKillstreak);
                    }
                    wasCommanderCalled = true;
                }
            }
        }
        #endregion
    }
}