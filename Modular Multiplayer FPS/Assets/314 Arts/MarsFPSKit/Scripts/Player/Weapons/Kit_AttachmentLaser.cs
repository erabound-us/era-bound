using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Implements a synced laser (FP, TP)
        /// This needs to be on both, FP AND TP! Onesided will not work.
        /// Thanks to Ciulama for sponsoring this!
        /// </summary>
        public class Kit_AttachmentLaser : Kit_AttachmentBehaviour
        {
            /// <summary>
            /// Light used for laser
            /// </summary>
            public Light laser;

            /// <summary>
            /// Line Renderer used for laser
            /// </summary>
            public LineRenderer laserLine;

            /// <summary>
            /// Game Object from which raycast is fired
            /// </summary>
            public Transform laserGO;

            /// <summary>
            /// Maximum distance for raycast
            /// </summary>
            public float maxLaserDistance = 500f;

            /// <summary>
            /// Layer mask for laser ;)
            /// </summary>
            public LayerMask laserMask;

            /// <summary>
            /// Raycast hit..
            /// </summary>
            public RaycastHit hit;

            /// <summary>
            /// Is the flashlight enabled?
            /// </summary>
            private bool isLaserEnabled;

            /// <summary>
            /// Player reference
            /// </summary>
            private Kit_PlayerBehaviour myPlayer;

            /// <summary>
            /// Use
            /// </summary>
            private AttachmentUseCase myUse;

            /// <summary>
            /// Get input!;
            /// </summary>
            private bool lastLaserInput;

            public override bool RequiresSyncing()
            {
                return true;
            }

            public override void SyncFromFirstPerson(object obj)
            {
                isLaserEnabled = (bool)obj;
            }

            public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int index)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(isLaserEnabled);
                }
                else
                {
                    isLaserEnabled = (bool)stream.ReceiveNext();
                }
            }

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                if (pb)
                {
                    enabled = true;
                    myPlayer = pb;
                }
                else
                {
                    myPlayer = null;
                }

                myUse = auc;
            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                enabled = false;
            }

            void Update()
            {
                if (myPlayer)
                {
                    if (myPlayer.isController)
                    {
                        if (lastLaserInput != myPlayer.input.laser)
                        {
                            lastLaserInput = myPlayer.input.laser;
                            if (myPlayer.input.laser)
                            {
                                //Switch...
                                isLaserEnabled = !isLaserEnabled;
                            }
                        }
                    }
                }

                UpdateLaser();
            }

            void LateUpdate()
            {
                UpdateLaser();
            }

            void UpdateLaser()
            {
                if (Physics.Raycast(laserGO.position, laserGO.forward, out hit, maxLaserDistance, laserMask, QueryTriggerInteraction.Ignore))
                {
                    laserLine.SetPosition(0, laserGO.position);
                    laserLine.SetPosition(1, hit.point);
                    laser.transform.position = hit.point + hit.normal * 0.03f;
                }
                else
                {
                    laserLine.SetPosition(0, laserGO.position);
                    laserLine.SetPosition(1, laserGO.position + laserGO.forward * maxLaserDistance);
                    laser.transform.position = laserGO.position + laserGO.forward * maxLaserDistance;
                }

                if (myPlayer && myPlayer.isController)
                {
                    if (myPlayer.isBot)
                    {
                        //Bots will only ever use third person (until I decide to add spectating, sigh)
                        if (myUse == AttachmentUseCase.FirstPerson)
                        {
                            laser.enabled = false;
                            laserLine.enabled = false;
                        }
                        else if (myUse == AttachmentUseCase.ThirdPerson)
                        {
                            laser.enabled = isLaserEnabled;
                            laserLine.enabled = isLaserEnabled;
                        }
                    }
                    else
                    {
                        //If first person, only enable when third person is not active
                        if (myUse == AttachmentUseCase.FirstPerson)
                        {
                            if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                            {
                                laser.enabled = false;
                                laserLine.enabled = false;
                            }
                            else
                            {
                                laser.enabled = isLaserEnabled;
                                laserLine.enabled = isLaserEnabled;
                            }
                        }
                        //If third person, only enable when third person mode is active
                        else if (myUse == AttachmentUseCase.ThirdPerson)
                        {
                            if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                            {
                                laser.enabled = isLaserEnabled;
                                laserLine.enabled = isLaserEnabled;
                            }
                            else
                            {
                                laser.enabled = false;
                                laserLine.enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    laser.enabled = isLaserEnabled;
                    laserLine.enabled = isLaserEnabled;
                }
            }

            public override void SetVisibility(Kit_PlayerBehaviour pb, AttachmentUseCase auc, bool visible)
            {
                if (visible)
                {
                    UpdateLaser();

                    enabled = true;
                }
                else
                {
                    laserLine.enabled = false;
                    laser.enabled = false;

                    enabled = false;
                }
            }
        }
    }
}