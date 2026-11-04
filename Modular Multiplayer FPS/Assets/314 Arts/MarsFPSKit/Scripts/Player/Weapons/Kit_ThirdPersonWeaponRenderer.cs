using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class Kit_ThirdPersonWeaponRenderer : MonoBehaviour
        {
            /// <summary>
            /// The weapon renderers that are not of attachments
            /// </summary>
            [Tooltip("Asssign all weapon renderers here that are not of attachments")]
            public Renderer[] allWeaponRenderers;

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

            /// <summary>
            /// An array of inverse kinematic positions for individual player models
            /// </summary>
            [Header("Inverse Kinematics")]
            public Transform[] leftHandIK;

            [Header("Attachments")]
            [Tooltip("Make sure they MATCH the first person attachment slots!")]
            public AttachmentSlot[] attachmentSlots;

            /// <summary>
            /// These are all attachments that need syncing
            /// </summary>
            [HideInInspector]
            public Kit_AttachmentBehaviour[] cachedSyncAttachments;

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
            public bool cachedMuzzleFlashEnabled;

            /// <summary>
            /// Tick this if weapon is silenced
            /// </summary>
            public bool isWeaponSilenced;
            /// <summary>
            /// Cached player reference
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

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Third person weapon renderer from " + gameObject.name + " at index " + i + " not assigned.");
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
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].SetVisibility(cachedPlayer, AttachmentUseCase.ThirdPerson, value);
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
            /// Is this weapon set to shadows only?
            /// </summary>
            public bool shadowsOnly
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (allWeaponRenderers[i].shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) return false;
                    }
                    return true;
                }
                set
                {
                    if (value)
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        }
                        //Attachment renderers
                        for (int i = 0; i < attachmentSlots.Length; i++)
                        {
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentRenderer))
                                    {
                                        Kit_AttachmentRenderer ar = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentRenderer;
                                        for (int a = 0; a < ar.renderersToActivate.Length; a++)
                                        {
                                            ar.renderersToActivate[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        }
                        //Attachment renderers
                        for (int i = 0; i < attachmentSlots.Length; i++)
                        {
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentRenderer))
                                    {
                                        Kit_AttachmentRenderer ar = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentRenderer;
                                        for (int a = 0; a < ar.renderersToActivate.Length; a++)
                                        {
                                            ar.renderersToActivate[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Enables the given attachments.
            /// </summary>
            /// <param name="enabledAttachments"></param>
            public void SetAttachments(int[] enabledAttachments, Kit_ModernWeaponScript ws, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Set default cached values
                cachedAttachments = enabledAttachments;
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
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Selected(pb, AttachmentUseCase.ThirdPerson);

                                        //Check for sync
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].RequiresSyncing())
                                        {
                                            int add = i;
                                            int addTwo = o;
                                            int addThree = p;
                                            syncAttachments.Add(attachmentSlots[add].attachments[addTwo].attachmentBehaviours[addThree]);

                                            if (pb && pb.isController && data != null)
                                            {
                                                for (int a = 0; a < data.weaponRenderer.attachmentSlots[i].attachments[o].attachmentBehaviours.Length; a++)
                                                {
                                                    //Woah this got pretty complex real quick
                                                    if (data.weaponRenderer.attachmentSlots[i].attachments[o].attachmentBehaviours[a].GetType() == attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType())
                                                    {
                                                        data.weaponRenderer.attachmentSlots[i].attachments[o].attachmentBehaviours[a].thirdPersonEquivalent = attachmentSlots[add].attachments[addTwo].attachmentBehaviours[addThree];
                                                    }
                                                }
                                            }
                                        }

                                        //Check what it is
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentAimOverride))
                                        {
                                            Kit_AttachmentAimOverride aimOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentAimOverride;
                                            //Override aim
                                            cachedAimingPos = aimOverride.aimPos;
                                            cachedAimingRot = aimOverride.aimRot;
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
                                            isWeaponSilenced = soundOverride.silencesWeapon;
                                        }
                                    }
                                }
                                else
                                {
                                    //Tell the behaviours they are not active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Unselected(pb, AttachmentUseCase.ThirdPerson);
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
            }
        }
    }
}