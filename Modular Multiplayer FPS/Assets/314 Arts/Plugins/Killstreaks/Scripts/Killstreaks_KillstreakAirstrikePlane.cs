using UnityEngine;
using Photon.Pun;
using MarsFPSKit;

namespace KillstreaksPlugin
{
    public class Killstreaks_KillstreakAirstrikePlane : MonoBehaviourPun, IPunObservable, IKitDamageable
    {
        /// <summary>
        /// Where bombs are instantiated
        /// </summary>
        [Header("Components")]
        public Transform bombDropTransform;
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
        private Killstreaks_KillstreakAirstrikeMapSettings settings;
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
        public int ownerInstanceId;
        /// <summary>
        /// 0 = Flying towards drop zone
        /// 1 - (1 + amount of bombs) dropping  bombs
        /// 
        /// </summary>
        private int planeState;
        /// <summary>
        /// Target location
        /// </summary>
        private Vector3 targetLocation;
        /// <summary>
        /// Timer used for bomb drop
        /// </summary>
        private double bombDropTimer;
        /// <summary>
        /// Hit points of Plane
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

            //Call the commander!
            CallCommander();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                transform.position += transform.forward * Time.deltaTime * settings.planeSpeed;

                if (planeState == 0)
                {
                    if (Vector3.Distance(transform.position, targetLocation) < settings.distanceAtWhichDropStats)
                    {
                        planeState = 1;
                    }
                }
                else if (planeState > 0 && planeState < settings.amountOfBombs + 1)
                {
                    if (PhotonNetwork.Time > bombDropTimer)
                    {
                        bombDropTimer = PhotonNetwork.Time + settings.timeBetweenBombDrops;

                        //Drop bomb
                        planeState++;

                        object[] instData = new object[2];
                        instData[0] = ownerIsBot;
                        instData[1] = ownerId;

                        GameObject go = PhotonNetwork.Instantiate(settings.bombPrefab.name, bombDropTransform.position, bombDropTransform.rotation, 0, instData);
                        Rigidbody body = go.GetComponent<Rigidbody>();
                        body.isKinematic = false;
                        body.velocity = settings.bombDropStartVelocity;
                    }
                }
                else if (planeState >= settings.amountOfBombs + 1)
                {
                    if (Vector3.Distance(transform.position, targetLocation) > settings.spawnPointDistance)
                    {
                        //Destroy us
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }

        private void OnDestroy()
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
        #endregion

        #region Custom Calls
        public void CallCommander()
        {
            if (!main) main = FindObjectOfType<Kit_IngameMain>();
            if (main.assignedTeamID != 2)
            {
                if (main.currentPvPGameModeBehaviour.isTeamGameMode)
                {
                    if (!main.currentPvPGameModeBehaviour.AreWeEnemies(main, ownerIsBot, ownerId) || !ownerIsBot && ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
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
        #endregion

        #region Photon Calls
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(planeState);
                stream.SendNext(bombDropTimer);
                stream.SendNext(hitPoints);
            }
            else
            {
                planeState = (int)stream.ReceiveNext();
                bombDropTimer = (double)stream.ReceiveNext();
                hitPoints = (float)stream.ReceiveNext();
            }
        }
        #endregion
    }
}
