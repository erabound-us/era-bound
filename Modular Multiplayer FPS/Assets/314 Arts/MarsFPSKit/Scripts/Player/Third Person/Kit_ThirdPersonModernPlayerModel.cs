using System;
using System.Collections.Generic;
using MarsFPSKit.Weapons;
using UnityEngine;
using UnityEngine.Rendering;
using Photon.Pun;
using System.Collections;

namespace MarsFPSKit
{
    /// <summary>
    /// A modern animating player model, which calculates velocity locally. It will read data from <see cref="Kit_Movement_BootsOnGround"/>
    /// </summary>
    public class Kit_ThirdPersonModernPlayerModel : Kit_ThirdPersonPlayerModel
    {
        public AnimatorStateName animatorStates;

        /// <summary>
        /// These are all renderers that will be set to "shadows only" when using first person
        /// </summary>
        public Renderer[] fpShadowOnlyRenderers;
        /// <summary>
        /// These are colliders for hit detection
        /// </summary>
        public Collider[] raycastColliders;

        public Rigidbody[] rigidbodies;

        /// <summary>
        /// Implements the death camera
        /// </summary>
        [Header("Death Camera")]
        public Kit_DeathCameraBase deathCamera;
        /// <summary>
        /// How long does our ragdoll live?
        /// </summary>
        [HideInInspector]
        public float ragdollLiveTime = 3600f;

        [HideInInspector]
        /// <summary>
        /// This is the reference to our player
        /// </summary>
        public Kit_PlayerBehaviour kpb;
        //Cache movement transform
        private Transform movementTransform;
        //To calculate movement
        private Vector3 position;
        private Vector3 oldPosition;
        private string oldAnimType;

        //Speed
        public float smoothedSpeed;
        private float rawSpeed;
        public Vector3 direction;
        public Vector3 localDirection;

        public override void FirstPerson()
        {
            //Set all renderers to shadow only
            for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
            {
                fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            for (int i = 0; i < customizationSlots.Length; i++)
            {
                for (int o = 0; o < customizationSlots[i].customizations.Length; o++)
                {
                    for (int p = 0; p < customizationSlots[i].customizations[o].customizationBehaviours.Length; p++)
                    {
                        if (customizationSlots[i].customizations[o].customizationBehaviours[p] is Kit_PlayerModelCustomizationRenderer)
                        {
                            Kit_PlayerModelCustomizationRenderer render = customizationSlots[i].customizations[o].customizationBehaviours[p] as Kit_PlayerModelCustomizationRenderer;
                            for (int l = 0; l < render.renderers.Length; l++)
                            {
                                render.renderers[l].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                            }
                        }
                    }
                }
            }
        }

        public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective)
        {
            if (perspective == Kit_GameInformation.Perspective.FirstPerson)
            {
                //Set all renderers to shadow only
                for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
                {
                    fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }

                for (int i = 0; i < customizationSlots.Length; i++)
                {
                    for (int o = 0; o < customizationSlots[i].customizations.Length; o++)
                    {
                        for (int p = 0; p < customizationSlots[i].customizations[o].customizationBehaviours.Length; p++)
                        {
                            if (customizationSlots[i].customizations[o].customizationBehaviours[p] is Kit_PlayerModelCustomizationRenderer)
                            {
                                Kit_PlayerModelCustomizationRenderer render = customizationSlots[i].customizations[o].customizationBehaviours[p] as Kit_PlayerModelCustomizationRenderer;
                                for (int l = 0; l < render.renderers.Length; l++)
                                {
                                    render.renderers[l].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Set all renderers to shadow only
                for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
                {
                    fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.On;
                }

                for (int i = 0; i < customizationSlots.Length; i++)
                {
                    for (int o = 0; o < customizationSlots[i].customizations.Length; o++)
                    {
                        for (int p = 0; p < customizationSlots[i].customizations[o].customizationBehaviours.Length; p++)
                        {
                            if (customizationSlots[i].customizations[o].customizationBehaviours[p] is Kit_PlayerModelCustomizationRenderer)
                            {
                                Kit_PlayerModelCustomizationRenderer render = customizationSlots[i].customizations[o].customizationBehaviours[p] as Kit_PlayerModelCustomizationRenderer;
                                for (int l = 0; l < render.renderers.Length; l++)
                                {
                                    render.renderers[l].shadowCastingMode = ShadowCastingMode.On;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SetupModel(Kit_PlayerBehaviour newKpb)
        {
            //Store the reference to our player
            kpb = newKpb;
            //Cache movement transform
            movementTransform = kpb.transform;
            //Store position
            position = movementTransform.position;
            oldPosition = movementTransform.position;

            enabled = true;
        }

        public override void ThirdPerson()
        {
            //Set all renderers to normal shadows
            for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
            {
                fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            }

            for (int i = 0; i < customizationSlots.Length; i++)
            {
                for (int o = 0; o < customizationSlots[i].customizations.Length; o++)
                {
                    for (int p = 0; p < customizationSlots[i].customizations[o].customizationBehaviours.Length; p++)
                    {
                        if (customizationSlots[i].customizations[o].customizationBehaviours[p] is Kit_PlayerModelCustomizationRenderer)
                        {
                            Kit_PlayerModelCustomizationRenderer render = customizationSlots[i].customizations[o].customizationBehaviours[p] as Kit_PlayerModelCustomizationRenderer;
                            for (int l = 0; l < render.renderers.Length; l++)
                            {
                                render.renderers[l].shadowCastingMode = ShadowCastingMode.On;
                            }
                        }
                    }
                }
            }
        }

        public override void SetAnimType(string animType, bool noTrans = false)
        {
            /*
            Debug.Log(animType);
            string getAnimation = animatorStates.GetAnimatorState(anim.GetCurrentAnimatorStateInfo(0), oldAnimType);
            if (getAnimation != "")
            {
                if (noTrans)
                {
                    anim.Play(animType + "." + getAnimation, 0);
                }
                else
                {
                    anim.CrossFade(animType + "." + getAnimation, 0.1f, 0);
                }
            }
            else
            {
                if (noTrans)
                {
                    anim.Play(animType, 0);
                }
                else
                {
                    anim.CrossFade(animType, 0.1f, 0);
                }
            }
            oldAnimType = animType;
            */

            Kit_GameInformation game = null;
            if (kpb) game = kpb.main.gameInformation;
            else
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                if (main)
                {
                    game = main.gameInformation;
                }
                else
                {
                    UI.Kit_MenuManager mm = FindObjectOfType<UI.Kit_MenuManager>();
                    if (mm)
                    {
                        game = mm.game;
                    }
                }
            }

            if (game)
            {
                for (int i = 0; i < game.allAnimatorAnimationSets.Length; i++)
                {
                    if (game.allAnimatorAnimationSets[i].prefix == animType)
                    {
                        anim.SetInteger("animType", game.allAnimatorAnimationSets[i].type);
                        anim.SetTrigger("ChangeState");
                        break;
                    }
                }
            }
        }

        public override void PlayWeaponFireAnimation(string animType)
        {
            if (anim)
            {
                anim.Play(animType + " Fire", 2, 0f);
            }
        }

        public override void PlayWeaponReloadAnimation(string animType, int type)
        {
            if (anim)
            {
                if (type == 0)
                {
                    anim.CrossFade(animType + " Reload", 0.1f, 1, 0f);
                }
                else if (type == 1)
                {
                    anim.CrossFade(animType + " Reload Empty", 0.1f, 1, 0f);
                }
            }
        }

        public override void PlayWeaponChangeAnimation(string animType, bool draw, float length)
        {
            anim.SetFloat("actionSpeed", 1 / length);

            if (draw)
            {
                anim.CrossFade(animType + " Draw", 0.1f, 1, 0f);
            }
            else
            {
                anim.CrossFade(animType + " Holster", 0.1f, 1, 0f);
            }
        }

        public override void PlayMeleeAnimation(int animation, int state)
        {
            if (anim)
            {
                if (animation == 0)
                {
                    if (state == 0)
                    {
                        anim.CrossFade("Stab Windup", 0.1f, 1, 0f);
                    }
                    else if (state == 1)
                    {
                        anim.CrossFade("Stab Miss", 0.1f, 1, 0f);
                    }
                    else if (state == 2)
                    {
                        anim.CrossFade("Stab Hit", 0.1f, 1, 0f);
                    }
                    else if (state == 3)
                    {
                        anim.CrossFade("Stab Hit Object", 0.1f, 1, 0f);
                    }
                }
                else if (animation == 1)
                {
                    if (state == 0)
                    {
                        anim.CrossFade("Charge", 0.1f, 1, 0f);
                    }
                    else if (state == 1)
                    {
                        anim.CrossFade("Charge Windup", 0.1f, 1, 0f);
                    }
                    else if (state == 2)
                    {
                        anim.CrossFade("Charge Miss", 0.1f, 1, 0f);
                    }
                    else if (state == 3)
                    {
                        anim.CrossFade("Charge Hit", 0.1f, 1, 0f);
                    }
                    else if (state == 4)
                    {
                        anim.CrossFade("Charge Hit Object", 0.1f, 1, 0f);
                    }
                }
                else if (animation == 2)
                {
                    anim.CrossFade("Heal", 0.1f, 1, 0f);
                }
            }
        }

        public override void PlayGrenadeAnimation(int animation)
        {
            if (anim)
            {
                if (animation == 0)
                {
                    anim.CrossFade("PullPin", 0.1f, 1, 0f);
                }
                else if (animation == 1)
                {
                    anim.CrossFade("Throw", 0.1f, 1, 0f);
                }
            }
        }

        public override void AbortWeaponAnimations()
        {
            if (anim)
            {
                anim.CrossFade("Null", 0.05f, 1, 0f); //Reload layer
                anim.CrossFade("Null", 0.05f, 2, 0f); //Fire layer
            }
            //Also stop reload sounds
            soundReload.Stop();
        }

        public override void CreateRagdoll()
        {
            //Play Death sound
            if (kpb.voiceManager)
            {
                kpb.voiceManager.PlayDeathSound(kpb, kpb.deathSoundCategory, kpb.deathSoundID);
            }

            // Disable the CharacterController immediately so it cannot push ragdoll bones through ERA_Ground
            if (kpb.cc != null)
            {
                var eraGndCC = GameObject.Find("ERA_Ground");
                if (eraGndCC != null) { foreach (var col in eraGndCC.GetComponents<Collider>()) Physics.IgnoreCollision(kpb.cc, col, false); }
                var rfCC = GameObject.Find("ERA_RagdollFloor");
                if (rfCC != null) { var rfc = rfCC.GetComponent<Collider>(); if (rfc != null) Physics.IgnoreCollision(kpb.cc, rfc, false); }
                kpb.cc.enabled = false;
            }

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].isKinematic = false;
                rigidbodies[i].useGravity = true;
                rigidbodies[i].velocity = Vector3.zero;
                rigidbodies[i].angularVelocity = Vector3.zero;
                rigidbodies[i].collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidbodies[i].gameObject.layer = 11;
                var boneCol = rigidbodies[i].GetComponent<Collider>();
                if (boneCol != null) boneCol.isTrigger = false;
                // Remove all joint limits so gravity can flatten the body freely
                var cj = rigidbodies[i].GetComponent<CharacterJoint>();
                if (cj != null)
                {
                    cj.enablePreprocessing = false;
                    cj.enableCollision = false;
                    var free = new SoftJointLimit { limit = 177f, bounciness = 0f, contactDistance = 0f };
                    var freeNeg = new SoftJointLimit { limit = -177f, bounciness = 0f, contactDistance = 0f };
                    var zeroSpring = new SoftJointLimitSpring { spring = 0f, damper = 0f };
                    cj.twistLimitSpring = zeroSpring;
                    cj.swingLimitSpring = zeroSpring;
                    cj.lowTwistLimit = freeNeg;
                    cj.highTwistLimit = free;
                    cj.swing1Limit = free;
                    cj.swing2Limit = free;
                }
                // Disable inter-bone collisions so bones don't push each other up
                for (int j = 0; j < rigidbodies.Length; j++)
                {
                    if (i == j) continue;
                    var colI = rigidbodies[i].GetComponent<Collider>();
                    var colJ = rigidbodies[j].GetComponent<Collider>();
                    if (colI != null && colJ != null) Physics.IgnoreCollision(colI, colJ, true);
                }
            }
            // Ensure ragdoll layer (11) collides with Default layer (0) so it lands on ERA_Ground
            Physics.IgnoreLayerCollision(11, 0, false);
            for (int L = 0; L < 32; L++) Physics.IgnoreLayerCollision(11, L, false);
            var eraGroundGO = GameObject.Find("ERA_Ground");
            MeshCollider eraGroundMC = eraGroundGO != null ? eraGroundGO.GetComponent<MeshCollider>() : null;
            for (int i = 0; i < raycastColliders.Length; i++)
            {
                raycastColliders[i].isTrigger = false;
                raycastColliders[i].gameObject.layer = 11;
                if (eraGroundMC != null) Physics.IgnoreCollision(raycastColliders[i], eraGroundMC, false);
                var rb = raycastColliders[i].GetComponent<Rigidbody>();
                if (rb != null) { rb.isKinematic = false; rb.useGravity = true; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; }
                var colRend = raycastColliders[i].GetComponent<Renderer>();
                if (colRend != null) colRend.enabled = false;
            }

            // Initial velocity already zeroed when setting non-kinematic above

            if (kpb.photonView.IsMine && !kpb.isBot)
            {
                if (deathCamera)
                {
                    deathCamera.SetupDeathCamera(this);
                }
                else
                {
                    // Enable ragdoll renderers, skipping bone GOs that have no real material (grey physics shapes)
                    var skipGOs = new System.Collections.Generic.HashSet<GameObject>();
                    foreach (var c in raycastColliders) skipGOs.Add(c.gameObject);
                    foreach (var rb in rigidbodies) skipGOs.Add(rb.gameObject);
                    var allRenderers = GetComponentsInChildren<Renderer>(true);
                    foreach (var r in allRenderers)
                    {
                        if (skipGOs.Contains(r.gameObject)) continue;
                        if (r.sharedMaterial == null || r.sharedMaterial.shader == null) continue;
                        r.enabled = true;
                        r.shadowCastingMode = ShadowCastingMode.On;
                    }
                    var camAnchorGo = new GameObject("TR_DeathCamAnchor");
                    var follower = camAnchorGo.AddComponent<TR_RagdollCamFollower>();
                    Transform followTarget = transform;
                    if (rigidbodies != null && rigidbodies.Length > 0)
                        followTarget = rigidbodies[0].transform;
                    follower.target = followTarget;
                    follower.offset = new Vector3(0f, 1.5f, -3f);
                    kpb.main.activeCameraTransform = camAnchorGo.transform;
                    Destroy(camAnchorGo, ragdollLiveTime);
                }
            }

            transform.parent = null;
            float cachedForce = kpb.ragdollForce;
            Vector3 cachedPoint = kpb.ragdollPoint;
            StartCoroutine(ApplyForce(cachedForce, cachedPoint));
            StartCoroutine(LockJointsOnLand());
        }

        IEnumerator ApplyForce(float ragdollForce, Vector3 ragdollPoint)
        {
            Destroy(weaponsInHandsGo.gameObject);
            Destroy(anim);
            yield return new WaitForFixedUpdate();
            if (rigidbodies != null)
            {
                float force = Mathf.Clamp(ragdollForce * 0.4f, 20f, 38f);
                Vector3 outward = (rigidbodies[0].transform.position - ragdollPoint).normalized;
                Vector3 kick = outward * force * 0.5f + Vector3.up * force * 0.55f;
                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    rigidbodies[i].velocity = kick;
                    rigidbodies[i].drag = 0f;
                }
                // Drive head down so it doesn't stay propped up by its joint rest axis
                if (rigidbodies.Length > 10)
                    rigidbodies[10].AddForce(Vector3.down * 50f + outward * -10f, ForceMode.Impulse);
                StartCoroutine(ExtraGravity());
            }
        }

        IEnumerator LockJointsOnLand()
        {
            // Wait minimum time so body is actually in the air before checking landing
            yield return new WaitForSeconds(0.8f);
            float timeout = 4f;
            while (timeout > 0f)
            {
                yield return new WaitForFixedUpdate();
                timeout -= Time.fixedDeltaTime;
                if (rigidbodies == null || rigidbodies[0] == null) yield break;
                if (rigidbodies[0].velocity.magnitude < 1.5f)
                {
                    for (int i = 0; i < rigidbodies.Length; i++)
                    {
                        if (rigidbodies[i] == null) continue;
                        var cj = rigidbodies[i].GetComponent<CharacterJoint>();
                        if (cj == null) continue;
                        var connectedBody = cj.connectedBody;
                        Destroy(cj);
                        var fj = rigidbodies[i].gameObject.AddComponent<FixedJoint>();
                        fj.connectedBody = connectedBody;
                        fj.breakForce = Mathf.Infinity;
                        fj.breakTorque = Mathf.Infinity;
                    }
                    yield break;
                }
            }
        }

        IEnumerator ExtraGravity()
        {
            // Wait until body has peaked and is falling before adding extra gravity
            yield return new WaitForSeconds(0.4f);
            float timer = 0f;
            while (timer < 3f && rigidbodies != null)
            {
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    if (rigidbodies[i] == null || rigidbodies[i].isKinematic) continue;
                    rigidbodies[i].AddForce(Vector3.down * 15f, ForceMode.Acceleration);
                }
            }
        }

        void Update()
        {
            if (!anim || !kpb) return;
            //Smooth speed
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * 10f);

            #region Animator Update
            //Update the animator with our locally calculated values
            //Speed
            anim.SetFloat("walkSpeed", smoothedSpeed, 0.1f, Time.deltaTime);
            //Direction
            anim.SetFloat("walkX", localDirection.x, 0.1f, Time.deltaTime);
            anim.SetFloat("walkZ", localDirection.z, 0.1f, Time.deltaTime);
            #endregion

            #region Movement Data
            if (kpb)
            {
                if (kpb.customMovementData != null)
                {
                    //Local data
                    if (kpb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                    {
                        BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)kpb.customMovementData;
                        //Update state
                        anim.SetInteger("state", bogrd.state);
                        //Update blend
                        anim.SetFloat("stateBlend", (float)bogrd.state, 0.1f, Time.deltaTime);
                        //Update aiming
                        anim.SetBool("aiming", bogrd.playSlowWalkAnimation);
                        //Update grounded
                        anim.SetBool("grounded", bogrd.isGrounded);
                    }
                    //Synced data
                    else if (kpb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                    {
                        BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)kpb.customMovementData;
                        //Update state
                        anim.SetInteger("state", bogsrd.state);
                        //Update blend
                        anim.SetFloat("stateBlend", (float)bogsrd.state, 0.1f, Time.deltaTime);
                        //Update aiming
                        anim.SetBool("aiming", bogsrd.playSlowWalkAnimation);
                        //Update grounded
                        anim.SetBool("grounded", bogsrd.isGrounded);
                    }
                }
            }
            #endregion

            #region Looking Data
            if (kpb.customMouseLookData != null)
            {
                if (kpb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    BasicMouseLookRuntimeData bmlrd = (BasicMouseLookRuntimeData)kpb.customMouseLookData;
                    //Smoothly update looking value
                    anim.SetFloat("lookY", bmlrd.finalMouseY, 0.1f, Time.deltaTime);
                    //Smoothly update leaning value
                    anim.SetFloat("leaning", bmlrd.leaningState, 0.1f, Time.deltaTime);
                }
            }
            #endregion
        }

        void LateUpdate()
        {
            if (!movementTransform) return;
            //Pause Error
            if (Time.deltaTime <= float.Epsilon)
            {
                smoothedSpeed = 0f;
                rawSpeed = 0f;
                return;
            }

            //Update position
            position = movementTransform.position;

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
            localDirection = movementTransform.InverseTransformDirection(direction);

            //Update old position
            oldPosition = position;
        }

        public void OnAnimatorIKRelay()
        {
            if (enabled && kpb && kpb.weaponManager)
            {
                kpb.weaponManager.OnAnimatorIKCallback(kpb, anim);
            }
        }
    }
}
