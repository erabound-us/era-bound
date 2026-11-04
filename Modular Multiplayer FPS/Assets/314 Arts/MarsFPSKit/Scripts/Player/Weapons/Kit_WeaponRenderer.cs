using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public enum CameraAnimationType { Copy, LookAt }

        public class Kit_WeaponRenderer : MonoBehaviour
        {
            /// <summary>
            /// The weapon animator
            /// </summary>
            public Animator anim;

            /// <summary>
            /// Support for legacy animation
            /// </summary>
            public Animation legacyAnim;
            /// <summary>
            /// Name of animations to play
            /// </summary>
            public Kit_WeaponRendererLegacyAnimations legacyAnimData;

            /// <summary>
            /// The weapon renderers that are not of attachments
            /// </summary>
            [Tooltip("Asssign all weapon renderers here that are not of attachments")]
            public Renderer[] allWeaponRenderers;
            /// <summary>
            /// These rendererers will be disabled in the customization menu. E.g. arms
            /// </summary>
            public Renderer[] hideInCustomiazionMenu;

            [Tooltip("Enable use of player dependent arms. Please note that the rigs need to match up. If you don't know what that means, you probably shouldn't use this.")]
            /// <summary>
            /// Do we use player model dependent arms?
            /// </summary>
            [Header("Player Model Dependent Arms")]
            public bool playerModelDependentArmsEnabled = false;
            [Tooltip("This is where the player model dependent arms will get parented to")]
            /// <summary>
            /// This is where the player model dependent arms will get parented to
            /// </summary>
            public Transform playerModelDependentArmsRoot;

            [Header("Shell Ejection")]
            /// <summary>
            /// This is where the ejected shell will spawn, if assigned
            /// </summary>
            public Transform shellEjectTransform;

            [Header("Muzzle Flash")]
            /// <summary>
            /// The muzzle flash particle system to use
            /// </summary>
            public ParticleSystem muzzleFlash;

            [Header("Aiming")]
            /// <summary>
            /// Which position to move to when we are aiming
            /// </summary>
            public Vector3 aimingPos;
            /// <summary>
            /// Which rotation to rotate to when we are aming
            /// </summary>
            public Vector3 aimingRot;
            /// <summary>
            /// Which fov to go to when we are aiming
            /// </summary>
            public float aimingFov = 40f;
            /// <summary>
            /// Should we use fullscreen aiming?
            /// </summary>
            public bool aimingFullscreen;

            [Header("Run position / rotation")]
            /// <summary>
            /// Determines if the weapon should be moved when we are running
            /// </summary>
            public bool useRunPosRot;
            /// <summary>
            /// The run position to use
            /// </summary>
            public Vector3 runPos;
            /// <summary>
            /// The run rotation to use. Will be converted to Quaternion using <see cref="Quaternion.Euler(Vector3)"/>
            /// </summary>
            public Vector3 runRot;
            /// <summary>
            /// How fast is the weapon going to move / rotate towards the run pos / run rot?
            /// </summary>
            public float runSmooth = 3f;

            [Header("Camera Animation")]
            public bool cameraAnimationEnabled;
            /// <summary>
            /// If camera animation is enabled, which one should be used?
            /// </summary>
            public CameraAnimationType cameraAnimationType;
            /// <summary>
            /// The bone for the camera animation
            /// </summary>
            public Transform cameraAnimationBone;
            /// <summary>
            /// If the type is LookAt, this is the target
            /// </summary>
            public Transform cameraAnimationTarget;
            /// <summary>
            /// The reference rotation to add movemment to
            /// </summary>
            public Vector3 cameraAnimationReferenceRotation;

            [Header("Attachments")]
            /// <summary>
            /// All attachments
            /// </summary>
            public AttachmentSlot[] attachmentSlots;

            /// <summary>
            /// These are all attachments that need syncing
            /// </summary>
            [HideInInspector]
            public Kit_AttachmentBehaviour[] cachedSyncAttachments;

            /// <summary>
            /// These are all attachments that need interaction
            /// </summary>
            [HideInInspector]
            public Kit_AttachmentBehaviour[] cachedInteractionAttachments;

            [HideInInspector]
            /// <summary>
            /// The currently selected attachments
            /// </summary>
            public int[] cachedAttachments;

            #region Cached values
            //This caches values from the attachments!
            /// <summary>
            /// Which position to move to when we are aiming
            /// </summary>
            [HideInInspector]
            public Vector3 cachedAimingPos;
            /// <summary>
            /// Which rotation to rotate to when we are aming
            /// </summary>
            [HideInInspector]
            public Vector3 cachedAimingRot;
            [HideInInspector]
            /// <summary>
            /// Aiming FOV
            /// </summary>
            public float cachedAimingFov = 40f;
            /// <summary>
            /// Should we use fullscreen scope?
            /// </summary>
            [HideInInspector]
            public bool cachedUseFullscreenScope;
            [HideInInspector]
            public bool cachedMuzzleFlashEnabled;
            /// <summary>
            /// Cached player
            /// </summary>
            public Kit_PlayerBehaviour cachedPlayer;
            /// <summary>
            /// Fire sound used for first person
            /// </summary>
            [HideInInspector]
            public AudioClip cachedFireSound;
            /// <summary>
            /// Fire sound used for third person
            /// </summary>
            [HideInInspector]
            public AudioClip cachedFireSoundThirdPerson;

            /// <summary>
            /// Max sound distance for third person fire
            /// </summary>
            [HideInInspector]
            public float cachedFireSoundThirdPersonMaxRange = 300f;
            /// <summary>
            /// Sound rolloff for third person fire
            /// </summary>
            [HideInInspector]
            public AnimationCurve cachedFireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);
            #endregion

            [Header("Loadout")]
            /// <summary>
            /// Use this to correct the position in the customization menu
            /// </summary>
            public Vector3 customizationMenuOffset;

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Weapon renderer from " + gameObject.name + " at index " + i + " not assigned.");
                    }
                }
            }
#endif

            /// <summary>
            /// Visibility state of the weapon
            /// </summary>
            public bool visible
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (!allWeaponRenderers[i].enabled) return false;
                    }
                    return true;
                }
                set
                {
                    //Set renderers
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        allWeaponRenderers[i].enabled = value;
                    }

                    //Loop through all slots
                    for (int i = 0; i < cachedAttachments.Length; i++)
                    {
                        if (i < attachmentSlots.Length)
                        {
                            //Loop through all attachments for that slot
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                //Check if this attachment is enabled
                                if (o == cachedAttachments[i])
                                {
                                    //Tell the behaviours they are active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].SetVisibility(cachedPlayer, AttachmentUseCase.FirstPerson, value);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Something must have gone wrong with the attachments. Enabled attachments is longer than all slots.");
                        }
                    }
                }
            }

            /// <summary>
            /// Enables the given attachments.
            /// </summary>
            /// <param name="enabledAttachments"></param>
            public void SetAttachments(int[] enabledAttachments, Kit_ModernWeaponScript ws, Kit_PlayerBehaviour pb)
            {
                //Set default cached values
                cachedAttachments = enabledAttachments;
                cachedAimingPos = aimingPos;
                cachedAimingRot = aimingRot;
                cachedAimingFov = aimingFov;
                cachedUseFullscreenScope = aimingFullscreen;
                cachedMuzzleFlashEnabled = true;
                cachedPlayer = pb;
                if (ws)
                {
                    cachedFireSound = ws.fireSound;
                    cachedFireSoundThirdPerson = ws.fireSoundThirdPerson;
                    cachedFireSoundThirdPersonMaxRange = ws.fireSoundThirdPersonMaxRange;
                    cachedFireSoundThirdPersonRolloff = ws.fireSoundThirdPersonRolloff;
                }

                //Create temporary list of synced attachments
                List<Kit_AttachmentBehaviour> syncAttachments = new List<Kit_AttachmentBehaviour>();
                List<Kit_AttachmentBehaviour> interactionAttachments = new List<Kit_AttachmentBehaviour>();

                try
                {
                    //Loop through all slots
                    for (int i = 0; i < enabledAttachments.Length; i++)
                    {
                        if (i < attachmentSlots.Length)
                        {
                            //Loop through all attachments for that slot
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                //Check if this attachment is enabled
                                if (o == enabledAttachments[i])
                                {
                                    //Tell the behaviours they are active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Selected(pb, AttachmentUseCase.FirstPerson);

                                        //Check for sync
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].RequiresSyncing())
                                        {
                                            int add = i;
                                            int addTwo = o;
                                            int addThree = p;
                                            syncAttachments.Add(attachmentSlots[add].attachments[addTwo].attachmentBehaviours[addThree]);
                                        }

                                        //Check for interaction
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].RequiresInteraction())
                                        {
                                            int add = i;
                                            int addTwo = o;
                                            int addThree = p;
                                            interactionAttachments.Add(attachmentSlots[add].attachments[addTwo].attachmentBehaviours[addThree]);
                                        }

                                        //Check what it is
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentAimOverride))
                                        {
                                            Kit_AttachmentAimOverride aimOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentAimOverride;
                                            //Override aim
                                            cachedAimingPos = aimOverride.aimPos;
                                            cachedAimingRot = aimOverride.aimRot;
                                            cachedAimingFov = aimOverride.aimFov;
                                            cachedUseFullscreenScope = aimOverride.useFullscreenScope;
                                        }
                                        else if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentDisableMuzzleFlash))
                                        {
                                            cachedMuzzleFlashEnabled = false;
                                        }
                                        else if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentOverrideSounds))
                                        {
                                            Kit_AttachmentOverrideSounds soundOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentOverrideSounds;
                                            cachedFireSound = soundOverride.fireSound;
                                            cachedFireSoundThirdPerson = soundOverride.fireSoundThirdPerson;
                                            cachedFireSoundThirdPersonMaxRange = soundOverride.fireSoundThirdPersonMaxRange;
                                            cachedFireSoundThirdPersonRolloff = soundOverride.fireSoundThirdPersonRolloff;
                                        }
                                        else if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentAnimatorOverride))
                                        {
                                            Kit_AttachmentAnimatorOverride animatorOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentAnimatorOverride;
                                            if (anim)
                                            {
                                                anim.runtimeAnimatorController = animatorOverride.animatorOverride;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Tell the behaviours they are not active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Unselected(pb, AttachmentUseCase.FirstPerson);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Something must have gone wrong with the attachments. Enabled attachments is longer than all slots.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("There was an error with the attachments: " + e);
                }

                cachedSyncAttachments = syncAttachments.ToArray();
                cachedInteractionAttachments = interactionAttachments.ToArray();
            }
        }
    }
}