using MarsFPSKit;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

namespace KillstreaksPlugin
{
    /// <summary>
    /// This implements Helicopter AI
    /// </summary>
    public class Killstreaks_KillstreakHelicopterAI : MonoBehaviourPunCallbacks, IPunObservable, IKitDamageable
    {
        [Header("Components")]
        /// <summary>
        /// Helicopter Anim
        /// </summary>
        public Animator anim;
        /// <summary>
        /// Prefab for replacement
        /// </summary>
        public GameObject replacementPrefab;
        /// <summary>
        /// Which killstreak does this prefab belong to?
        /// </summary>
        public Killstreak_KillstreakBase belongsToKillstreak;


        /// <summary>
        /// Transform used to rotate minigun up and down
        /// </summary>
        [Header("Minigun")]
        public Transform minigunUpDown;
        /// <summary>
        /// Min max for up down rotation
        /// </summary>
        public Vector2 minigunUpDownMinMax;
        /// <summary>
        /// Transform used to rotate mingun left and right
        /// </summary>
        public Transform minigunLeftRight;
        /// <summary>
        /// Min max for left right rotation
        /// </summary>
        public Vector2 minigunLeftRightMinMax;
        /// <summary>
        /// Muzzle Flash for minigun
        /// </summary>
        public ParticleSystem minigunMuzzleFlash;
        /// <summary>
        /// Audio for Minigun
        /// </summary>
        public AudioSource minigunAudio;
        /// <summary>
        /// From where the minigun's raycast is fired
        /// </summary>
        public Transform minigunShootGo;
        /// <summary>
        /// Min/Max wait time between the minigun checking for enemies and shooting them
        /// </summary>
        public Vector2 minigunWaitTimeBetweenAttemptedKills;
        /// <summary>
        /// Layers that are checked for visibility
        /// </summary>
        public LayerMask minigunLookLayers;
        /// <summary>
        /// Time between shots fired
        /// </summary>
        public float minigunFireRate = 0.1f;
        /// <summary>
        /// How much time does the minigun have for a kill?
        /// </summary>
        public Vector2 minigunTimeGivenForKill;
        /// <summary>
        /// Range at which we can shoot things
        /// </summary>
        public float minigunRange = 1000f;
        /// <summary>
        /// Layer mask for shooting things
        /// </summary>
        public LayerMask minigunHitMask;
        /// <summary>
        /// Ragdoll force
        /// </summary>
        public float minigunRagdollForce = 15000f;
        /// <summary>
        /// Damage per shot
        /// </summary>
        public float minigunDamage = 30f;

        #region Velocity Calc
        //Speed
        //To calculate movement
        private Vector3 position;
        private Vector3 oldPosition;
        private float rawSpeed;
        private Vector3 direction;
        private Vector3 localDirection;
        #endregion

        #region Runtime
        /// <summary>
        /// Settings
        /// </summary>
        private Killstreaks_KillstreakHelicopterMapSettings settings;
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
        /// If owner is not bot, this is the photon player
        /// </summary>
        public Photon.Realtime.Player ownerPlayer;
        [HideInInspector]
        /// <summary>
        /// 0 = Just spawned, nothing happened yet (should actually never be the case)
        /// 1 = Going to hover point
        /// 2 = Staying at hover point, devastating enemies
        /// 3 = Evacuating
        /// </summary>
        public int helicopterState;
        /// <summary>
        /// Current hover point for helicopter.
        /// </summary>
        private int helicopterCurrentHoverPoint = -1;
        /// <summary>
        /// Timer for staying somewhere!
        /// </summary>
        private double helicopterTimer;
        /// <summary>
        /// When did we spawn? For despawn.
        /// </summary>
        private double helicopterCameIntoExistance;
        /// <summary>
        /// Minigun timer
        /// </summary>
        private double helicopterMinigunTimer;
        /// <summary>
        /// When we can shoot (:
        /// </summary>
        private double helicopterMinigunKillTimer;
        [HideInInspector]
        /// <summary>
        /// Target
        /// </summary>
        public Kit_PlayerBehaviour currentTarget;
        /// <summary>
        /// When was the minigun lastly fired?
        /// </summary>
        private float minigunLastFire;
        /// <summary>
        /// Reference velocity for SmoothDAmp
        /// </summary>
        private Vector3 refVel;
        /// <summary>
        /// Rotation to set to left right
        /// </summary>
        private Quaternion rotationLeftRight;
        /// <summary>
        /// Rotation to set to up down
        /// </summary>
        private Quaternion rotationUpDown;
        /// <summary>
        /// Hit points of helicopter :)
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
            settings = FindObjectOfType<Killstreaks_KillstreakHelicopterMapSettings>();
            if (!settings) throw new System.Exception("No Helicopter Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            main = FindObjectOfType<Kit_IngameMain>();
        }

        private void Start()
        {
            //Get owner
            ownerIsBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];
            ownerInstanceId = (int)photonView.InstantiationData[2];

            if (!ownerIsBot)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].ActorNumber == ownerId)
                    {
                        int id = i;
                        ownerPlayer = PhotonNetwork.PlayerList[id];
                        break;
                    }
                }
            }

            if (photonView.IsMine)
            {
                helicopterCameIntoExistance = PhotonNetwork.Time;
                helicopterCurrentHoverPoint = Random.Range(0, settings.hoveringPoints.Length);
                helicopterState = 1;
            }

            //Call the commander!
            CallCommander();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (PhotonNetwork.Time < helicopterCameIntoExistance + settings.helicopterLiveTime)
                {
                    if (helicopterState == 1)
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, settings.hoveringPoints[helicopterCurrentHoverPoint], ref refVel, settings.helicopterSmooth);
                        //Check for distance
                        if (Vector3.Distance(transform.position, settings.hoveringPoints[helicopterCurrentHoverPoint]) < 10f)
                        {
                            //State
                            helicopterState = 2;
                            //Set timer
                            helicopterTimer = PhotonNetwork.Time + Random.Range(settings.helicopterHoveringTime.x, settings.helicopterHoveringTime.y);
                        }
                    }
                    else if (helicopterState == 2)
                    {
                        if (PhotonNetwork.Time > helicopterTimer)
                        {
                            //Get new!
                            helicopterCurrentHoverPoint = Random.Range(0, settings.hoveringPoints.Length);
                            helicopterState = 1;
                        }
                    }
                }
                else
                {
                    //Check if we are evacuating yet
                    if (helicopterState != 3)
                    {
                        helicopterCurrentHoverPoint = Random.Range(0, settings.spawnPoints.Length);
                        helicopterState = 3;
                        currentTarget = null;
                    }
                    else
                    {
                        //Slerp
                        transform.position = Vector3.SmoothDamp(transform.position, settings.spawnPoints[helicopterCurrentHoverPoint].transform.position, ref refVel, settings.helicopterSmooth * 5);
                        //Check for distance
                        if (Vector3.Distance(transform.position, settings.spawnPoints[helicopterCurrentHoverPoint].transform.position) < 25f)
                        {
                            //Destroy
                            PhotonNetwork.Destroy(gameObject);
                        }
                    }
                }

                if (rawSpeed > 5f)
                {
                    transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 0.4f);
                }

                #region Minigun Logic
                if (PhotonNetwork.Time > helicopterMinigunTimer && helicopterState != 3)
                {
                    helicopterMinigunTimer = PhotonNetwork.Time + Random.Range(minigunWaitTimeBetweenAttemptedKills.x, minigunWaitTimeBetweenAttemptedKills.y);
                    helicopterMinigunKillTimer = PhotonNetwork.Time + Random.Range(minigunTimeGivenForKill.x, minigunTimeGivenForKill.y);

                    for (int i = 0; i < main.allActivePlayers.Count; i++)
                    {
                        if (main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, ownerId, ownerIsBot, main.allActivePlayers[i], false))
                        {
                            if (CanSeePlayer(main.allActivePlayers[i]))
                            {
                                int id = i;
                                currentTarget = main.allActivePlayers[id];
                                break;
                            }
                        }
                    }
                }

                if (currentTarget)
                {
                    //Look
                    Vector3 lookPos = (currentTarget.transform.position + Vector3.up * 1f) - minigunUpDown.position;
                    rotationUpDown = Quaternion.LookRotation(lookPos);
                    Vector3 eulerAngles = rotationUpDown.eulerAngles;
                    eulerAngles.z = ClampAngle(eulerAngles.z, minigunUpDownMinMax.x, minigunUpDownMinMax.y);
                    rotationUpDown = Quaternion.Euler(eulerAngles);
                    lookPos.y = 0;
                    rotationLeftRight = Quaternion.LookRotation(lookPos);

                    if (helicopterMinigunKillTimer > PhotonNetwork.Time)
                    {
                        if (Time.time > minigunLastFire + minigunFireRate)
                        {
                            minigunLastFire = Time.time;
                            //Fire
                            photonView.RPC("FireMinigun", RpcTarget.All);
                            MinigunFireRaycast();
                        }
                    }
                }
                else
                {
                    Vector3 lookPos = (transform.position + transform.forward * 2000f) - transform.position;
                    rotationUpDown = Quaternion.LookRotation(lookPos);
                    Vector3 eulerAngles = rotationUpDown.eulerAngles;
                    eulerAngles.z = ClampAngle(eulerAngles.z, -13f, 90f);
                    rotationUpDown = Quaternion.Euler(eulerAngles);
                    lookPos.y = 0;
                    rotationLeftRight = Quaternion.LookRotation(lookPos);
                }

                minigunLeftRight.rotation = Quaternion.Slerp(minigunLeftRight.rotation, rotationLeftRight, Time.deltaTime * 18f);
                minigunUpDown.rotation = Quaternion.Slerp(minigunUpDown.rotation, rotationUpDown, Time.deltaTime * 16f);
                #endregion
            }
            else
            {
                minigunLeftRight.rotation = Quaternion.Slerp(minigunLeftRight.rotation, rotationLeftRight, Time.deltaTime * 18f);
                minigunUpDown.rotation = Quaternion.Slerp(minigunUpDown.rotation, rotationUpDown, Time.deltaTime * 16f);
            }

            anim.SetFloat("moveV", localDirection.z / 8f, 0.1f, Time.deltaTime);
            anim.SetFloat("moveH", localDirection.x / 8f, 0.1f, Time.deltaTime);
        }

        void LateUpdate()
        {
            //Pause Error
            if (Time.deltaTime <= float.Epsilon)
            {
                rawSpeed = 0f;
                return;
            }

            //Update position
            position = transform.position;

            //Calculate speed
            rawSpeed = (position - oldPosition).magnitude / Time.deltaTime;

            //Update direction
            if (position != oldPosition)
            {
                direction = Vector3.Normalize(position - oldPosition);
            }
            else
            {
                direction = Vector3.zero;
            }

            //Update local direction
            localDirection = transform.InverseTransformDirection(direction);

            //Update old position
            oldPosition = position;
        }

        private void OnDestroy()
        {
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

        #region RPCs
        [PunRPC]
        public void DisplayHitmarker()
        {
            //Since we hit a player, show the hitmarker
            main.hud.DisplayHitmarker();
        }

        [PunRPC]
        public void FireMinigun()
        {
            minigunMuzzleFlash.Play(true);
            minigunAudio.Play();
        }

        [PunRPC]
        public void MinigunRaycastHit(Vector3 pos, Vector3 normal, string material)
        {
            //Relay to impact processor
            main.impactProcessor.ProcessImpact(main, pos, normal, material);
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

        void MinigunFireRaycast()
        {
            RaycastHit hit;
            Vector3 dir = minigunShootGo.forward;
            Vector3 pos = minigunShootGo.position;

            RaycastHit[] hits = Physics.RaycastAll(pos, dir, minigunRange, minigunHitMask.value).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];
                //Check if we hit ourselves
                if (hit.transform.root != transform.root)
                {
                    if (hit.collider.GetComponent<Kit_PlayerDamageMultiplier>())
                    {
                        Kit_PlayerDamageMultiplier adm = hit.collider.GetComponent<Kit_PlayerDamageMultiplier>();
                        if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            hit.transform.root.GetComponent<Kit_PlayerBehaviour>().LocalDamage(minigunDamage * adm.damageMultiplier, "Helicopter", transform.position, dir, minigunRagdollForce, hit.point, adm.ragdollId, ownerIsBot, ownerId);
                            //Send to hit processor
                            main.impactProcessor.ProcessImpact(main, hit.point, hit.normal, hit.transform.tag);
                            //Tell other players we hit something
                            photonView.RPC("MinigunRaycastHit", RpcTarget.Others, hit.point, hit.normal, hit.transform.tag);

                            if (!ownerIsBot)
                            {
                                photonView.RPC("DisplayHitmarker", ownerPlayer);

                            }
                        }
                    }
                    else
                    {
                        //Send to hit processor
                        main.impactProcessor.ProcessImpact(main, hit.point, hit.normal, hit.transform.tag);
                        //Tell other players we hit something
                        photonView.RPC("MinigunRaycastHit", RpcTarget.Others, hit.point, hit.normal, hit.transform.tag);
                    }
                }
            }
        }

        public bool CanSeePlayer(Kit_PlayerBehaviour pb)
        {
            RaycastHit hit;
            if (Physics.Linecast(minigunLeftRight.transform.position, pb.transform.position, out hit, minigunLookLayers.value, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.root == pb.transform.root)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
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
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!ownerIsBot && otherPlayer == ownerPlayer)
            {
                helicopterCameIntoExistance = 0f;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(rotationLeftRight);
                stream.SendNext(rotationUpDown);
                stream.SendNext(helicopterCameIntoExistance);
                stream.SendNext(helicopterCurrentHoverPoint);
                stream.SendNext(helicopterMinigunKillTimer);
                stream.SendNext(helicopterMinigunTimer);
                stream.SendNext(helicopterState);
                stream.SendNext(helicopterTimer);
                stream.SendNext(hitPoints);
            }
            else
            {
                rotationLeftRight = (Quaternion)stream.ReceiveNext();
                rotationUpDown = (Quaternion)stream.ReceiveNext();
                helicopterCameIntoExistance = (double)stream.ReceiveNext();
                helicopterCurrentHoverPoint = (int)stream.ReceiveNext();
                helicopterMinigunKillTimer = (double)stream.ReceiveNext();
                helicopterMinigunTimer = (double)stream.ReceiveNext();
                helicopterState = (int)stream.ReceiveNext();
                helicopterTimer = (double)stream.ReceiveNext();
                hitPoints = (float)stream.ReceiveNext();
            }
        }
        #endregion

        #region Helper
        public static float ClampAngle(float angle, float min, float max)
        {
            angle = Mathf.Repeat(angle, 360);
            min = Mathf.Repeat(min, 360);
            max = Mathf.Repeat(max, 360);
            bool inverse = false;
            var tmin = min;
            var tangle = angle;
            if (min > 180)
            {
                inverse = !inverse;
                tmin -= 180;
            }
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            var result = !inverse ? tangle > tmin : tangle < tmin;
            if (!result)
                angle = min;

            inverse = false;
            tangle = angle;
            var tmax = max;
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            if (max > 180)
            {
                inverse = !inverse;
                tmax -= 180;
            }

            result = !inverse ? tangle < tmax : tangle > tmax;
            if (!result)
                angle = max;
            return angle;
        }
        #endregion
    }
}