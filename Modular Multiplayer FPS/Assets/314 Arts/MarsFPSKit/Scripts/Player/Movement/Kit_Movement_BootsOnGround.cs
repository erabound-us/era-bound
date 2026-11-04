using Photon.Pun;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MarsFPSKit
{
    //The runtime data. It will be stored as an object in the player. Never store runtime data in a scriptable object.
    public class BootsOnGroundRuntimeData
    {
        public Vector3 desiredMoveDirection = Vector3.zero;

        public Vector3 swimmingWorldMoveDirection = Vector3.zero;

        public Vector3 moveDirection = Vector3.zero;

        /// <summary>
        /// The local movement direction
        /// </summary>
        public Vector3 localMoveDirection = Vector3.zero;

        /// <summary>
        /// Are we grounded?
        /// </summary>
        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// <para>2 = Prone (not implemented right now.)</para>
        /// <para>3 = Swimming</para>
        /// </summary>
        public int state;
        /// <summary>
        /// Helper bool to see if we're currently swimming
        /// </summary>
        public bool isSwimming
        {
            get
            {
                return state == 3;
            }
        }
        /// <summary>
        /// Are we underwater?
        /// </summary>
        public bool isUnderwater;
        /// <summary>
        /// How much air do we have left?
        /// </summary>
        public float airLeft = 100f;
        /// <summary>
        /// When do we take the next damage?
        /// </summary>
        public float nextAirDamageAppliedAt;
        /// <summary>
        /// Current
        /// </summary>
        public Kit_SwimmingTrigger swimmingCurrent;
        /// <summary>
        /// When was the last swimming sound played?
        /// </summary>
        public float lastSwimmingSoundPlayed;
        /// <summary>
        /// Cooldown to prevent immediate swim re-entry after exit
        /// </summary>
        public float swimExitCooldown;
        /// <summary>
        /// Minimum time we must be in swim state before OnTriggerExit can exit it
        /// </summary>
        public float swimEnteredAt;

        /// <summary>
        /// Did we jump and have not been grounded since?
        /// </summary>
        public bool isJumping;
        /// <summary>
        /// How many times have we jumped since last ground?
        /// </summary>
        public int jumpCount;
        /// <summary>
        /// Are we currently sprinting?
        /// </summary>
        public bool isSprinting;
        /// <summary>
        /// Were we swimming (true until grounded)
        /// </summary>
        public bool wasSwimming;
        /// <summary>
        /// Slot of swimming weapon :)
        /// </summary>
        public int swimmingWeaponSlot;
        /// <summary>
        /// This is where we return afterwards!
        /// </summary>
        public int[] swimmingPreviousWeaponSlot;
        /// <summary>
        /// Material type for footsteps
        /// </summary>
        public string currentMaterial;
        /// <summary>
        /// When can we make our next footstep?
        /// </summary>
        public float nextFootstep;

        /// <summary>
        /// Last state of the crouch input
        /// </summary>
        public bool lastCrouch = false;
        /// <summary>
        /// Last state of the jump input
        /// </summary>
        public bool lastJump = false;

        /// <summary>
        /// Should we play slow (aiming) walk animation?
        /// </summary>
        public bool playSlowWalkAnimation;

        #region Fall Damage
        /// <summary>
        /// Are we falling?
        /// </summary>
        public bool falling;
        /// <summary>
        /// How far have we fallen?
        /// </summary>
        public float fallDistance;
        /// <summary>
        /// From where did we fall?
        /// </summary>
        public float fallHighestPoint;
        #endregion

        #region Stamina
        /// <summary>
        /// Our currently left stamina
        /// </summary>
        public float staminaLeft = 100f;
        /// <summary>
        /// After which time can we regenerate stamina?
        /// </summary>
        public float staminaRegenerationTime = 0f;
        /// <summary>
        /// If <see cref="Time.time"/> is smaller than this, sprinting is blocked because stamina is depleted
        /// </summary>
        public float staminaDepletedSprintingBlock;
        #endregion
    }

    public class BootsOnGroundSyncRuntimeData
    {
        public Vector3 velocity;
        /// <summary>
        /// Smoothed velocity
        /// </summary>
        public Vector3 velocitySmoothed;
        /// <summary>
        /// Are we grounded?
        /// </summary>
        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// </summary>
        public int state;
        /// <summary>
        /// Are we currently sprinting?
        /// </summary>
        public bool isSprinting;
        /// <summary>
        /// Are we currently swimming?
        /// </summary>
        public bool isSwimming;
        /// <summary>
        /// When was the last swimming sound played?
        /// </summary>
        public float lastSwimmingSoundPlayed;
        /// <summary>
        /// Material type for footsteps
        /// </summary>
        public string currentMaterial;
        /// <summary>
        /// When can we make our next footstep?
        /// </summary>
        public float nextFootstep;
        /// <summary>
        /// Should we play slow (aiming) walk animation?
        /// </summary>
        public bool playSlowWalkAnimation;
        /// <summary>
        /// Our currently left stamina
        /// </summary>
        public float staminaLeft = 100f;
        /// <summary>
        /// How much air do we have left?
        /// </summary>
        public float airLeft = 100f;
        /// <summary>
        /// Synced move direction
        /// </summary>
        public Vector3 moveDirection = Vector3.zero;
        /// <summary>
        /// The local movement direction
        /// </summary>
        public Vector3 localMoveDirection = Vector3.zero;
    }

    /// <summary>
    /// Helper class for Footsteps
    /// </summary>
    [System.Serializable]
    public class Footstep
    {
        /// <summary>
        /// Sounds for this footstep material
        /// </summary>
        public AudioClip[] clips;
        /// <summary>
        /// Max audio distance for this footstep material
        /// </summary>
        public float maxDistance = 20f;
        /// <summary>
        /// Audio rolloff for this footstep material
        /// </summary>
        public AnimationCurve rollOff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
    }

    [System.Serializable]
    public class StringFootstepDictionary : SerializableDictionary<string, Footstep> { }

    [System.Serializable]
    public class NestedSound
    {
        public AudioClip[] clips;
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Movement/Boots on ground")]
    public class Kit_Movement_BootsOnGround : Kit_MovementBase
    {
        [Header("Stats")]
        [Tooltip("Sprinting speed")]
        public float sprintSpeed = 6f;
        [Tooltip("Normal walk speed")]
        public float walkSpeed = 6f;
        [Tooltip("Crouch walk speed")]
        public float crouchSpeed = 6f;

        /// <summary>
        /// If this is set to true, we can move while we are not grounded
        /// </summary>
        public bool airControlEnabled = false;
        /// <summary>
        /// Air control can control sprint?
        /// </summary>
        public bool airControlSprintControl = true;

        [Tooltip("Multiplier that is applied to Physics.gravity for the character controller")]
        /// <summary>
        /// Multiplier for Physics.gravity
        /// </summary>
        public float gravityMultiplier = 1f;

        [Tooltip("The speed that is applied when you try to jump")]
        public float jumpSpeed = 8f;
        /// <summary>
        /// How many times can we jump
        /// </summary>
        [Tooltip("How many times can we jump?")]
        public int jumpMaxCount = 1;
        [Tooltip("How much stamina does the jump cost?")]
        /// <summary>
        /// How much stamina does the jump cost?
        /// </summary>
        public float jumpStaminaCost = 10f;

        /// <summary>
        /// Which jump animation to play for a certain jump count?
        /// </summary>
        public string[] jumpAnimations;

        /// <summary>
        /// Which jump sound to play for a certain jump?
        /// </summary>
        public NestedSound[] jumpSound;

        /// <summary>
        /// Which land sound to play for a certain jump?
        /// </summary>
        public NestedSound[] jumpLandSound;

        [Header("Character Heights")]
        public float standHeight = 1.8f; //State 0 height
        public float crouchHeight = 1.2f; //State 1 height

        [Header("Camera Positions")]
        public float camPosSmoothSpeed = 6f; //The lerp speed of changing the camera position
        public Vector3 camPosStand = new Vector3(0, 1.65f, 0f);
        public Vector3 camPosCrouch = new Vector3(0, 1.05f, 0f);

        [Header("Fall Damage")]
        /// <summary>
        /// After how many units of falling will we take damage?
        /// </summary>
        public float fallDamageThreshold = 10;
        /// <summary>
        /// With which value will the fall damage be multiplied with?
        /// </summary>
        public float fallDamageMultiplier = 5f;

        [Header("Others")]
        [Tooltip("How many units should we be moved down by default? To be able to walk down stairs properly")]
        public float defaultYmove = -2f; //How many units should we be moved down by default? To be able to walk down stairs properly

        [Header("Footsteps")]
        public float footstepsRunTime = 0.25f; //Time between footsteps when we're running
        public float footstepsWalkTime = 0.4f; //Time between footsteps when we're standing
        public float footstepsCrouchTime = 0.7f; //Time between footsteps when we're crouching

        public float footstepsRunVolume = 0.8f; //Volume for footsteps when we're running
        public float footstepsWalkVolume = 0.4f; //Volume for footsteps when we're walking
        public float footstepsCrouchVolume = 0.1f; //Volume for footsteps when we're crouching

        public StringFootstepDictionary allFootsteps; //All footstep materials

        [Header("Fall Down effect")]
        public float fallDownAmount = 10.0f;
        public float fallDownMinOffset = -6.0f;
        public float fallDownMaxoffset = 6.0f;
        public float fallDownTime = 0.3f;
        public float fallDownReturnSpeed = 1f;

        /// <summary>
        /// Is stamina enabled?
        /// </summary>
        [Header("Stamina")]
        public bool staminaSystemEnabled = true;
        /// <summary>
        /// How fast will stamina decrease when we are running?
        /// </summary>
        public float staminaDecreaseRate = 5f;
        /// <summary>
        /// How fast will stamina increase when we are not running?=
        /// </summary>
        public float staminaIncreaseRate = 3f;
        /// <summary>
        /// How long until regen starts when stamina was not depleted?
        /// </summary>
        public float staminaPauseRateNotEmpty = 2f;
        /// <summary>
        /// How long until regen starts when stamina was depleted?
        /// </summary>
        public float staminaPauseRateEmpty = 5f;
        /// <summary>
        /// How long will sprinting be blocked if stamina was depleted
        /// </summary>
        public float staminaDepletedSprintBlockTime = 7f;
        /// <summary>
        /// Sounds that will play when stamina was depleted
        /// </summary>
        public AudioClip[] staminaExhaustedSound;

        /// <summary>
        /// How fast do we swim?
        /// </summary>
        [Header("Swimming")]
        public float swimmingSpeed = 4f;
        /// <summary>
        /// How fast does it react to our input?
        /// </summary>
        public float swimmingInputLerpSpeed = 1f;
        /// <summary>
        /// Swimming height
        /// </summary>
        public float swimmingHeight = 0.6f;
        /// <summary>
        /// Weapon to select when swimming!
        /// </summary>
        public Weapons.Kit_WeaponBase swimmingWeapon;
        /// <summary>
        /// Should we be able to drown?
        /// </summary>
        public bool swimmingEnableDrowning;
        /// <summary>
        /// How long does it take to drown?
        /// </summary>
        public float swimmingDrowningTime = 5f;
        /// <summary>
        /// How quickly do we recover air?
        /// </summary>
        public float swimmingDrowningRecoveryTime = 5f;
        /// <summary>
        /// How much time between drowning
        /// </summary>
        public Vector2 swimmingDrowningDamageTimes = new Vector2(1, 3);
        /// <summary>
        /// Damage we take when drowning
        /// </summary>
        public Vector2 swimmingDrowningDamage = new Vector2(10, 20f);

        static void SetEraGroundIgnore(CharacterController cc, bool ignore)
        {
            var go = GameObject.Find("ERA_Ground"); if (go == null) return;
            foreach (var col in go.GetComponents<Collider>()) Physics.IgnoreCollision(cc, col, ignore);
            var rf = GameObject.Find("ERA_RagdollFloor"); if (rf == null) return;
            var rfc = rf.GetComponent<Collider>(); if (rfc != null) Physics.IgnoreCollision(cc, rfc, ignore);
        }

        public override void CalculateMovementUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.photonView.IsMine)
            {
                //Check if the object is correct
                if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundRuntimeData))
                {
                    pb.customMovementData = new BootsOnGroundRuntimeData();
                    ((BootsOnGroundRuntimeData)pb.customMovementData).swimExitCooldown = float.MaxValue; // block swim until first update

                    if (pb.isFirstPersonActive)
                    {
                        pb.main.hud.DisplayMovementState(0);
                    }
                }

                BootsOnGroundRuntimeData data = pb.customMovementData as BootsOnGroundRuntimeData;

                //Move transform back
                pb.playerCameraFallDownTransform.localRotation = Quaternion.Slerp(pb.playerCameraFallDownTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                // If swimming but no longer over any water trigger, force exit swim (landed on bank)
                if (data.isSwimming)
                {
                    bool stillOverWater = false;
                    foreach (var st in UnityEngine.Object.FindObjectsOfType<Kit_SwimmingTrigger>())
                    {
                        if (st.col == null || st.gameObject.name == "ERA_OceanTrigger" || st.gameObject.name == "ERA_OceanDrown") continue;
                        var bcSw = st.col as BoxCollider; if (bcSw == null) continue;
                        Vector3 locSw = bcSw.transform.InverseTransformPoint(new Vector3(pb.transform.position.x, 0f, pb.transform.position.z));
                        Vector3 halfSw = bcSw.size * 0.5f;
                        if (Mathf.Abs(locSw.x - bcSw.center.x) <= halfSw.x && Mathf.Abs(locSw.z - bcSw.center.z) <= halfSw.z)
                        { stillOverWater = true; break; }
                    }
                    if (!stillOverWater)
                    {
                        data.wasSwimming = false; data.state = 0; data.isUnderwater = false; data.swimmingCurrent = null;
                        data.moveDirection = data.swimmingWorldMoveDirection; data.moveDirection.y = 0f; data.swimmingWorldMoveDirection.y = 0f;
                        data.swimExitCooldown = Time.time + 2.0f;
                        pb.cc.height = standHeight; pb.cc.center = new Vector3(0f, standHeight / 2f, 0f);
                        SetEraGroundIgnore(pb.cc, false);
                        Vector3 forceExitPos = pb.transform.position; forceExitPos.y = 1.5f; pb.transform.position = forceExitPos;
                        if (swimmingWeapon && data.swimmingPreviousWeaponSlot != null) pb.weaponManager.PluginSelectWeapon(pb, data.swimmingPreviousWeaponSlot[0], data.swimmingPreviousWeaponSlot[1], false);
                    }
                }

                // If player is standing over a water trigger, ignore ERA_Ground so they sink in
                if (!data.isSwimming && data.swimExitCooldown != float.MaxValue && Time.time >= data.swimExitCooldown && pb.transform.position.y < 0.5f)
                {
                    bool overWater = false;
                    foreach (var st in UnityEngine.Object.FindObjectsOfType<Kit_SwimmingTrigger>())
                    {
                        if (st.col == null || st.gameObject.name == "ERA_OceanTrigger" || st.gameObject.name == "ERA_OceanDrown") continue;
                        var bcOw = st.col as BoxCollider; if (bcOw == null) continue;
                        Vector3 locOw = bcOw.transform.InverseTransformPoint(new Vector3(pb.transform.position.x, 0f, pb.transform.position.z));
                        Vector3 halfOw = bcOw.size * 0.5f;
                        if (Mathf.Abs(locOw.x - bcOw.center.x) <= halfOw.x && Mathf.Abs(locOw.z - bcOw.center.z) <= halfOw.z)
                        { overWater = true; break; }
                    }
                    var eraGndCheck = GameObject.Find("ERA_Ground");
                    if (eraGndCheck != null) { foreach (var col in eraGndCheck.GetComponents<Collider>()) Physics.IgnoreCollision(pb.cc, col, overWater); }
                }

                // Poll for swim trigger overlap — only enter when player is below water surface
                if (!data.isSwimming && data.swimExitCooldown != float.MaxValue && Time.time >= data.swimExitCooldown && pb.transform.position.y < -0.1f)
                {
                    foreach (var st in UnityEngine.Object.FindObjectsOfType<Kit_SwimmingTrigger>())
                    {
                        if (st.col == null) continue;
                        if (st.gameObject.name == "ERA_OceanTrigger" || st.gameObject.name == "ERA_OceanDrown") continue;
                        if (st.col.bounds.Contains(pb.transform.position))
                        {
                            data.state = 3; data.swimEnteredAt = Time.time; data.swimmingCurrent = st;
                            data.swimmingWorldMoveDirection = data.moveDirection;
                            SetEraGroundIgnore(pb.cc, true);
                            break;
                        }
                    }
                }
                // Poll-based swim exit - exit when player has surfaced or reached a bank
                if (data.isSwimming && Time.time > data.swimEnteredAt + 0.5f)
                {
                    bool surfaced = pb.transform.position.y >= 0f;
                    if (surfaced)
                    {

                        data.wasSwimming = false; data.state = 0; data.isUnderwater = false; data.swimmingCurrent = null;
                        data.moveDirection = data.swimmingWorldMoveDirection; data.moveDirection.y = 0f; data.swimmingWorldMoveDirection.y = 0f;
                        data.swimExitCooldown = Time.time + 2.0f;
                        pb.cc.height = standHeight; pb.cc.center = new Vector3(0f, standHeight / 2f, 0f);
                        SetEraGroundIgnore(pb.cc, false);
                        Vector3 exitPos = pb.transform.position; exitPos.y = 1.5f; pb.transform.position = exitPos;
                        if (swimmingWeapon && data.swimmingPreviousWeaponSlot != null) pb.weaponManager.PluginSelectWeapon(pb, data.swimmingPreviousWeaponSlot[0], data.swimmingPreviousWeaponSlot[1], false);
                    }
                }

                //Swim logic
                if (data.isSwimming)
                {
                    //Reset "ground" bools
                    data.isGrounded = false;
                    data.isJumping = false;
                    data.isSprinting = false;
                    data.falling = false;

                    pb.cc.height = swimmingHeight;
                    pb.cc.center = new Vector3(0f, swimmingHeight / 2f, 0f);
                    SetEraGroundIgnore(pb.cc, true);

                    #region Main Input
                    //Only get input if the cursor is locked
                    if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        //Check if we want to move
                        if (data.isGrounded && data.desiredMoveDirection.sqrMagnitude > 0.005f)
                        {
                            //Call for spawn protection
                            if (pb.spawnProtection)
                            {
                                pb.spawnProtection.PlayerMoved(pb);
                            }
                        }

                        if (Mathf.Abs(pb.input.ver) > 0.05f)
                        {
                            data.desiredMoveDirection.x = 0f;
                            data.desiredMoveDirection.y = 0f;
                            data.desiredMoveDirection.z = pb.input.ver;
                            //Transform
                            data.desiredMoveDirection = pb.playerCameraTransform.TransformDirection(data.desiredMoveDirection * swimmingSpeed);
                            //Add that
                            data.swimmingWorldMoveDirection += data.desiredMoveDirection * Time.deltaTime * swimmingInputLerpSpeed;
                            data.swimmingWorldMoveDirection = Vector3.ClampMagnitude(data.swimmingWorldMoveDirection, swimmingSpeed);
                        }

                        if (Mathf.Abs(pb.input.hor) > 0.05f)
                        {
                            data.desiredMoveDirection.x = pb.input.hor;
                            data.desiredMoveDirection.y = 0f;
                            data.desiredMoveDirection.z = 0f;
                            //Transform
                            data.desiredMoveDirection = pb.playerCameraTransform.TransformDirection(data.desiredMoveDirection * swimmingSpeed);
                            //Add that
                            data.swimmingWorldMoveDirection += data.desiredMoveDirection * Time.deltaTime * swimmingInputLerpSpeed;
                            data.swimmingWorldMoveDirection = Vector3.ClampMagnitude(data.swimmingWorldMoveDirection, swimmingSpeed);
                        }

                        if (Mathf.Abs(pb.input.ver) < 0.05f && Mathf.Abs(pb.input.hor) < 0.05f)
                        {
                            data.swimmingWorldMoveDirection = Vector3.Lerp(data.swimmingWorldMoveDirection, Vector3.zero, Time.deltaTime * swimmingInputLerpSpeed);
                        }
                    }
                    //If not, don't move
                    else
                    {
                        data.swimmingWorldMoveDirection = Vector3.Lerp(data.swimmingWorldMoveDirection, Vector3.zero, Time.deltaTime * swimmingInputLerpSpeed);
                    }

                    //Move
                    CollisionFlags swimCollision = pb.cc.Move(data.swimmingWorldMoveDirection * Time.deltaTime);
                    // Per known-working doc: depth clamp at y=-4
                    if (pb.transform.position.y < -4f)
                    {
                        Vector3 clampPos = pb.transform.position; clampPos.y = -4f; pb.transform.position = clampPos;
                        data.swimmingWorldMoveDirection.y = 0f;
                    }
                    // Boulder/tree exit: only exit if grounded well above water surface (on a rock/tree)
                    if (pb.cc.isGrounded && pb.transform.position.y > 0.5f)
                    {
                        data.wasSwimming = false; data.state = 0; data.swimmingCurrent = null;
                        data.swimExitCooldown = Time.time + 2.0f;
                        pb.cc.height = standHeight; pb.cc.center = new Vector3(0f, standHeight / 2f, 0f);
                        SetEraGroundIgnore(pb.cc, false);
                        Vector3 boulderExitPos = pb.transform.position; boulderExitPos.y = 1.5f; pb.transform.position = boulderExitPos;
                        if (swimmingWeapon && data.swimmingPreviousWeaponSlot != null) pb.weaponManager.PluginSelectWeapon(pb, data.swimmingPreviousWeaponSlot[0], data.swimmingPreviousWeaponSlot[1], false);
                    }
                    //Get local movement direction
                    data.localMoveDirection = pb.playerCameraTransform.InverseTransformDirection(data.moveDirection);
                    #endregion

                    if (data.isUnderwater)
                    {
                        //Check if drowning is enabled
                        if (swimmingEnableDrowning)
                        {
                            if (data.airLeft > 0)
                            {
                                data.airLeft -= (100 / swimmingDrowningTime) * Time.deltaTime;
                            }
                            else
                            {
                                if (Time.time > data.nextAirDamageAppliedAt)
                                {
                                    //Apply damage
                                    pb.LocalDamage(Random.Range(swimmingDrowningDamage.x, swimmingDrowningDamage.y), "Water", pb.transform.position, Vector3.zero, 0f, pb.transform.position, 0, pb.isBot, pb.id);
                                    //Set time
                                    data.nextAirDamageAppliedAt = Time.time + Random.Range(swimmingDrowningDamageTimes.x, swimmingDrowningDamageTimes.y);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (swimmingEnableDrowning)
                        {
                            if (data.airLeft < 100f)
                            {
                                data.airLeft += (100 / swimmingDrowningRecoveryTime) * Time.deltaTime;
                            }
                        }
                    }
                }
                else
                {

                    if (pb.cc.isGrounded || airControlEnabled && !data.wasSwimming)
                    {
                        float yMovement = data.moveDirection.y;
                        #region Main Input
                        //Only get input if the cursor is locked
                        if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                        {
                            //Calculate move direction based on input
                            data.moveDirection.x = pb.input.hor;
                            //if (pb.cc.isGrounded)
                            data.moveDirection.y = 0f;
                            data.moveDirection.z = pb.input.ver;

                            //Check if we want to move
                            if (data.isGrounded && data.moveDirection.sqrMagnitude > 0.005f)
                            {
                                //Call for spawn protection
                                if (pb.spawnProtection)
                                {
                                    pb.spawnProtection.PlayerMoved(pb);
                                }
                            }

                            //Correct strafe
                            data.moveDirection = Vector3.ClampMagnitude(data.moveDirection, 1f);

                            if (pb.cc.isGrounded)
                            {
                                data.moveDirection.y = defaultYmove;
                            }
                            else
                            {
                                data.moveDirection.y = 0f;
                            }

                            if (airControlSprintControl || data.isGrounded)
                            {
                                //Get sprinting input
                                if (pb.input.sprint && data.moveDirection.z > 0.3f && pb.weaponManager.CanRun(pb))
                                {
                                    //Check if we can sprint
                                    if (data.state == 0 && (!staminaSystemEnabled || staminaSystemEnabled && data.staminaLeft >= 0f && Time.time > data.staminaDepletedSprintingBlock))
                                    {
                                        data.isSprinting = true;

                                        if (staminaSystemEnabled)
                                        {
                                            data.staminaLeft -= Time.deltaTime * staminaDecreaseRate;

                                            if (data.staminaLeft <= 0f)
                                            {
                                                //Stamina is now depleted. Set times
                                                data.staminaRegenerationTime = Time.time + staminaPauseRateEmpty;
                                                data.staminaDepletedSprintingBlock = Time.time + staminaDepletedSprintBlockTime;

                                                //Play sound.
                                                if (staminaExhaustedSound.Length > 0)
                                                {
                                                    //Send RPC
                                                    pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 0, 0, Random.Range(0, staminaExhaustedSound.Length));
                                                }
                                            }
                                            else
                                            {
                                                //Set time for regen
                                                data.staminaRegenerationTime = Time.time + staminaPauseRateNotEmpty;
                                            }
                                        }

                                    }
                                    //We cannot sprint
                                    else
                                    {
                                        data.isSprinting = false;
                                    }
                                }
                                else
                                {
                                    //We are not sprinting
                                    data.isSprinting = false;
                                }
                            }
                        }
                        //If not, don't move
                        else
                        {
                            //Reset move direction
                            data.moveDirection = new Vector3(0f, defaultYmove, 0f);
                            //Reset sprinting
                            data.isSprinting = false;
                        }
                        #endregion

                        //Take rotation in consideration (local to world)
                        data.moveDirection = pb.transform.TransformDirection(data.moveDirection);
                        //Apply speed based on state
                        //Standing
                        if (data.state == 0)
                        {
                            //Sprinting
                            if (data.isSprinting)
                            {
                                data.moveDirection *= sprintSpeed;
                            }
                            //Not sprinting
                            else
                            {
                                data.moveDirection *= walkSpeed;
                            }
                        }
                        //Crouching
                        else if (data.state == 1)
                        {
                            data.moveDirection *= crouchSpeed;
                        }

                        if (!pb.cc.isGrounded)
                        {
                            data.moveDirection.y = yMovement;
                        }
                        else
                        {
                            //Mouse Look multiplier
                            data.moveDirection *= pb.looking.GetSpeedMultiplier(pb);
                            //Weapon multiplier
                            data.moveDirection *= pb.weaponManager.CurrentMovementMultiplier(pb); //Retrive from weapon manager
                                                                                                  //Should play slow animation?
                            data.playSlowWalkAnimation = pb.weaponManager.IsAiming(pb); //Retrive from weapon manager
                        }
                    }

                    //Air recovery
                    if (swimmingEnableDrowning)
                    {
                        if (data.airLeft < 100f)
                        {
                            data.airLeft += (100 / swimmingDrowningRecoveryTime) * Time.deltaTime;
                        }
                    }

                    if (pb.cc.isGrounded)
                    {
                        #region Fall Damage
                        if (data.falling)
                        {
                            //Calculate distance we have fallen
                            data.fallDistance = data.fallHighestPoint - pb.transform.position.y;
                            data.falling = false;
                            if (data.fallDistance > fallDamageThreshold)
                            {
                                pb.ApplyFallDamage((data.fallDistance - fallDamageThreshold) * fallDamageMultiplier);
                                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                                //Tell weapon manager
                                pb.weaponManager.FallDownEffect(pb, true);
                            }
                            else if (data.fallDistance > 0.1f)
                            {
                                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                                //Tell weapon manager
                                pb.weaponManager.FallDownEffect(pb, false);
                            }
                        }
                        #endregion

                        #region Crouch Input
                        if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                        {
                            if (Kit_GameSettings.isCrouchToggle)
                            {
                                if (data.lastCrouch != pb.input.crouch)
                                {
                                    data.lastCrouch = pb.input.crouch;
                                    //Get crouch input
                                    if (pb.input.crouch)
                                    {
                                        //Change state
                                        if (data.state == 0)
                                        {
                                            //We are standing, crouch
                                            data.state = 1;
                                        }
                                        else if (data.state == 1)
                                        {
                                            //We are crouching, stand up
                                            data.state = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (pb.input.crouch)
                                {
                                    data.state = 1;
                                }
                                else
                                {
                                    data.state = 0;
                                }
                                data.lastCrouch = pb.input.crouch;
                            }
                        }
                        #endregion

                        #region Jump
                        //Reset jump counter
                        if (data.jumpCount > 0)
                        {
                            //We Landed
                            pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 2, data.jumpCount - 1, Random.Range(0, jumpLandSound[data.jumpCount - 1].clips.Length));
                            data.jumpCount = 0;
                            data.isJumping = false;
                        }

                        if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                        {
                            if (data.lastJump != pb.input.jump)
                            {
                                data.lastJump = pb.input.jump;
                                //Get Jump input
                                if (pb.input.jump)
                                {
                                    //Check if we can jump
                                    if (data.state == 0)
                                    {
                                        if (!staminaSystemEnabled || data.staminaLeft >= jumpStaminaCost)
                                        {
                                            data.moveDirection.y = jumpSpeed;

                                            data.isJumping = true;

                                            //Play Animation and Jump Sound
                                            pb.photonView.RPC("MovementPlayAnimation", RpcTarget.All, 0, data.jumpCount);
                                            pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 1, data.jumpCount, Random.Range(0, jumpSound[data.jumpCount].clips.Length));

                                            data.jumpCount = 1;

                                            //Call for spawn protection
                                            if (pb.spawnProtection)
                                            {
                                                pb.spawnProtection.PlayerMoved(pb);
                                            }

                                            if (staminaSystemEnabled)
                                            {
                                                data.staminaLeft -= jumpStaminaCost;

                                                if (data.staminaLeft <= 0f)
                                                {
                                                    //Stamina is now depleted. Set times
                                                    data.staminaRegenerationTime = Time.time + staminaPauseRateEmpty;
                                                    data.staminaDepletedSprintingBlock = Time.time + staminaDepletedSprintBlockTime;

                                                    //Play sound.
                                                    if (staminaExhaustedSound.Length > 0)
                                                    {
                                                        //Send RPC
                                                        pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 0, 0, Random.Range(0, staminaExhaustedSound.Length));
                                                    }
                                                }
                                                else
                                                {
                                                    //Set time for regen
                                                    data.staminaRegenerationTime = Time.time + staminaPauseRateNotEmpty;
                                                }
                                            }
                                        }
                                    }
                                    //If we try to jump and we try to jump, stand up
                                    else if (data.state == 1)
                                    {
                                        data.state = 0;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //Save initial falling point
                        if (!data.falling)
                        {
                            data.fallHighestPoint = pb.transform.position.y;
                            data.falling = true;
                        }
                        //Check if we moved higher for some reason
                        if (pb.transform.position.y > data.fallHighestPoint)
                        {
                            data.fallHighestPoint = pb.transform.position.y;
                        }

                        if (data.isJumping)
                        {
                            if (data.jumpCount < jumpMaxCount)
                            {
                                if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                                {
                                    if (data.lastJump != pb.input.jump)
                                    {
                                        data.lastJump = pb.input.jump;
                                        //Get Jump input
                                        if (pb.input.jump)
                                        {
                                            //Check if we can jump
                                            if (data.state == 0)
                                            {
                                                if (!staminaSystemEnabled || data.staminaLeft >= jumpStaminaCost)
                                                {
                                                    data.moveDirection.y = jumpSpeed;
                                                    data.isJumping = true;

                                                    pb.photonView.RPC("MovementPlayAnimation", RpcTarget.All, 0, data.jumpCount);
                                                    pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 1, data.jumpCount, Random.Range(0, jumpSound[data.jumpCount].clips.Length));

                                                    data.jumpCount++;

                                                    if (staminaSystemEnabled)
                                                    {
                                                        data.staminaLeft -= jumpStaminaCost;

                                                        if (data.staminaLeft <= 0f)
                                                        {
                                                            //Stamina is now depleted. Set times
                                                            data.staminaRegenerationTime = Time.time + staminaPauseRateEmpty;
                                                            data.staminaDepletedSprintingBlock = Time.time + staminaDepletedSprintBlockTime;

                                                            //Play sound.
                                                            if (staminaExhaustedSound.Length > 0)
                                                            {
                                                                //Send RPC
                                                                pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 0, 0, Random.Range(0, staminaExhaustedSound.Length));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //Set time for regen
                                                            data.staminaRegenerationTime = Time.time + staminaPauseRateNotEmpty;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #region Character Height
                    //Change character height based on the state
                    //Standing
                    if (data.state == 0)
                    {
                        pb.cc.height = standHeight; //Set height
                        pb.cc.center = new Vector3(0f, standHeight / 2, 0f); //Set center
                    }
                    //Crouch
                    else if (data.state == 1)
                    {
                        pb.cc.height = crouchHeight; //Set height
                        pb.cc.center = new Vector3(0f, crouchHeight / 2, 0f); //Set center
                    }
                    #endregion

                    //Apply gravity
                    data.moveDirection += Physics.gravity * Time.deltaTime * gravityMultiplier;
                    //Move
                    CollisionFlags collision = pb.cc.Move(data.moveDirection * Time.deltaTime);
                    //Get local movement direction
                    data.localMoveDirection = pb.transform.InverseTransformDirection(pb.cc.velocity);
                    //Check grounded
                    data.isGrounded = pb.cc.isGrounded;

                    if (data.isGrounded)
                    {
                        data.wasSwimming = false;
                        if (data.swimExitCooldown == float.MaxValue)
                            data.swimExitCooldown = 0f;
                    }
                    // ERA_Ground collision is managed per swim state only
                    if (collision.HasFlag(CollisionFlags.Above))
                    {
                        if (data.moveDirection.y > 0f)
                        {
                            data.moveDirection.y = -data.moveDirection.y;
                        }
                    }
                }

                //Move the camer to the correct position
                #region CameraMove
                //Standing
                if (data.state == 0)
                {
                    //Smoothly lerp to the correct state
                    pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosStand + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
                }
                //Crouching
                else if (data.state == 1)
                {
                    //Smoothly lerp to the correct state
                    pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosCrouch + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
                }
                //Swimming — use standing camera position, no offset (per known-working doc: no separate swim camera block causes conjoined twin)
                else if (data.state == 3)
                {
                    pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosStand + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
                }
                #endregion

                #region Character Height
                //Change character height based on the state
                //Standing
                if (data.state == 0)
                {
                    pb.cc.height = standHeight; //Set height
                    pb.cc.center = new Vector3(0f, standHeight / 2, 0f); //Set center
                }
                //Crouch
                else if (data.state == 1)
                {
                    pb.cc.height = crouchHeight; //Set height
                    pb.cc.center = new Vector3(0f, crouchHeight / 2, 0f); //Set center
                }
                #endregion

                #region Stamina regen
                if (staminaSystemEnabled)
                {
                    //Check if we can  regen
                    if (Time.time > data.staminaRegenerationTime)
                    {
                        //Check if we need to regen
                        if (data.staminaLeft < 100f)
                        {
                            //Now, regen
                            data.staminaLeft += Time.deltaTime * staminaIncreaseRate;
                        }
                    }

                    if (pb.isFirstPersonActive)
                    {
                        if ((data.isSwimming && swimmingEnableDrowning && data.airLeft < 100f) || data.airLeft < 100f && !data.isSprinting && data.staminaLeft >= 99)
                        {
                            pb.main.hud.DisplayStamina(data.airLeft);
                        }
                        else
                        {
                            //Display stamina
                            pb.main.hud.DisplayStamina(data.staminaLeft);
                        }
                    }
                }
                else
                {
                    if (swimmingEnableDrowning)
                    {
                        pb.main.hud.DisplayStamina(data.airLeft);
                    }
                }
                #endregion

                if (pb.isFirstPersonActive)
                {
                    if (data.state == 0)
                    {
                        pb.main.hud.DisplayMovementState(0);
                    }
                    else if (data.state == 1)
                    {
                        pb.main.hud.DisplayMovementState(1);
                    }
                }
            }
            else
            {
                //Check if the object is correct
                if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundSyncRuntimeData))
                {
                    pb.customMovementData = new BootsOnGroundSyncRuntimeData();
                }

                BootsOnGroundSyncRuntimeData data = pb.customMovementData as BootsOnGroundSyncRuntimeData;

                //Smooth velocity
                data.velocitySmoothed = Vector3.Lerp(data.velocitySmoothed, data.velocity, Time.deltaTime * 15f);

                //Standing
                if (data.state == 0)
                {
                    //Smoothly lerp to the correct state
                    pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosStand + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
                }
                //Crouching
                else if (data.state == 1)
                {
                    //Smoothly lerp to the correct state
                    pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosCrouch + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
                }

                #region Stamina HUD
                if (pb.isFirstPersonActive)
                {
                    if (staminaSystemEnabled)
                    {
                        if ((data.isSwimming && swimmingEnableDrowning && data.airLeft < 100f) || data.airLeft < 100f && !data.isSprinting && data.staminaLeft >= 99)
                        {
                            pb.main.hud.DisplayStamina(data.airLeft);
                        }
                        else
                        {
                            //Display stamina
                            pb.main.hud.DisplayStamina(data.staminaLeft);
                        }
                    }
                    else
                    {
                        if (swimmingEnableDrowning)
                        {
                            pb.main.hud.DisplayStamina(data.airLeft);
                        }
                    }

                    if (data.state == 0)
                    {
                        pb.main.hud.DisplayMovementState(0);
                    }
                    else if (data.state == 1)
                    {
                        pb.main.hud.DisplayMovementState(1);
                    }
                }
                #endregion
            }
        }

        public override int GetCurrentWeaponMoveAnimation(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.5f)
                    {
                        //Check if we're sprinting, if return 2
                        if (bogrd.isSprinting) return 2;
                        //If not return 1
                        else
                            return 1;
                    }
                }
            }
            else if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
            {
                BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (bogrd.velocity.sqrMagnitude > 0.5f)
                    {
                        //Check if we're sprinting, if return 2
                        if (bogrd.isSprinting) return 2;
                        //If not return 1
                        else
                            return 1;
                    }
                }
            }
            return 0;
        }

        public override float GetCurrentWalkAnimationSpeed(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.1f)
                    {
                        //Check if we're sprinting, if return speed divided by sprintSpeed
                        if (bogrd.isSprinting) return pb.cc.velocity.magnitude / sprintSpeed;
                        //If not return speed divided by normal walking speed
                        else
                            return pb.cc.velocity.magnitude / walkSpeed;
                    }
                }
            }
            else if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.1f)
                    {
                        //Check if we're sprinting, if return speed divided by sprintSpeed
                        if (bogrd.isSprinting) return bogrd.velocity.magnitude / sprintSpeed;
                        //If not return speed divided by normal walking speed
                        else
                            return bogrd.velocity.magnitude / walkSpeed;
                    }
                }
            }
            return 1f;
        }

        public override Vector3 GetMovementDirection(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                return bogrd.localMoveDirection.normalized;
            }
            else if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
            {
                BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                return bogrd.localMoveDirection.normalized;
            }
            return Vector3.zero;
        }

        public override bool CanFire(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Just based on our spriting value, if we are sprinting we cannot fire
                if (bogrd.isSprinting && bogrd.isGrounded) return false;
                else return true;
            }
            return false;
        }

        public override bool IsRunning(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Just based on our spriting value, if we are sprinting we cannot fire
                if (bogrd.isSprinting) return true;
                else return false;
            }
            else if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
            {
                BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Just based on our spriting value, if we are sprinting we cannot fire
                if (bogrd.isSprinting) return true;
                else return false;
            }
            return false;
        }

        public override void CalculateFootstepsUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Get velMag
                    if (pb.cc.velocity.magnitude > 0.5f)
                    {
                        //We're moving
                        //Check if enough time has passed since the last footstep
                        if (Time.time >= bogrd.nextFootstep)
                        {
                            if (bogrd.currentMaterial == null || !allFootsteps.ContainsKey(bogrd.currentMaterial)) bogrd.currentMaterial = "Concrete";
                            //Set next footstep sound
                            pb.footstepSource.clip = allFootsteps[bogrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogrd.currentMaterial].clips.Length)];
                            //Set footstep source rolloff and distance
                            pb.footstepSource.maxDistance = allFootsteps[bogrd.currentMaterial].maxDistance;
                            pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogrd.currentMaterial].rollOff);
                            //Set volume and time
                            if (bogrd.state == 0)
                            {
                                if (bogrd.isSprinting) //Sprinting
                                {
                                    pb.footstepSource.volume = footstepsRunVolume;
                                    bogrd.nextFootstep = Time.time + footstepsRunTime;
                                }
                                else //Normal walking
                                {
                                    pb.footstepSource.volume = footstepsWalkVolume;
                                    bogrd.nextFootstep = Time.time + footstepsWalkTime;
                                }
                            }
                            else if (bogrd.state == 1) //Crouching
                            {
                                pb.footstepSource.volume = footstepsCrouchVolume;
                                bogrd.nextFootstep = Time.time + footstepsCrouchTime;
                            }
                            //Play
                            pb.footstepSource.Play();
                        }
                    }
                }
            }
            else if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
            {
                BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogsrd.isGrounded)
                {
                    //Get velMag
                    if (bogsrd.velocity.magnitude > 0.5f)
                    {
                        //We're moving
                        //Check if enough time has passed since our last footstep
                        if (Time.time >= bogsrd.nextFootstep)
                        {
                            //Set next footstep sound
                            pb.footstepSource.clip = allFootsteps[bogsrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogsrd.currentMaterial].clips.Length)];
                            //Set footstep source rolloff and distance
                            pb.footstepSource.maxDistance = allFootsteps[bogsrd.currentMaterial].maxDistance;
                            pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogsrd.currentMaterial].rollOff);
                            //Set volume and time
                            if (bogsrd.state == 0)
                            {
                                if (bogsrd.isSprinting) //Sprinting
                                {
                                    pb.footstepSource.volume = footstepsRunVolume;
                                    bogsrd.nextFootstep = Time.time + footstepsRunTime;
                                }
                                else //Normal walking
                                {
                                    pb.footstepSource.volume = footstepsWalkVolume;
                                    bogsrd.nextFootstep = Time.time + footstepsWalkTime;
                                }
                            }
                            else if (bogsrd.state == 1) //Crouching
                            {
                                pb.footstepSource.volume = footstepsCrouchVolume;
                                bogsrd.nextFootstep = Time.time + footstepsCrouchTime;
                            }
                            //Play
                            pb.footstepSource.Play();
                        }
                    }
                }
            }
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    //Send velocity
                    stream.SendNext(pb.cc.velocity);
                    //Send grounded
                    stream.SendNext(bogrd.isGrounded);
                    //Send state
                    stream.SendNext(bogrd.state);
                    //Send sprinting
                    stream.SendNext(bogrd.isSprinting);
                    //Send material type
                    stream.SendNext(bogrd.currentMaterial);
                    //Send slow walk animation
                    stream.SendNext(bogrd.playSlowWalkAnimation);
                    //Send stamina
                    stream.SendNext(bogrd.staminaLeft);
                    //Send air left
                    stream.SendNext(bogrd.airLeft);
                    //Send directions
                    stream.SendNext(bogrd.moveDirection);
                    stream.SendNext(bogrd.localMoveDirection);
                }
                else
                {
                    //Send dummies
                    //Send velocity
                    stream.SendNext(Vector3.zero);
                    //Send grounded
                    stream.SendNext(true);
                    //Send state
                    stream.SendNext(0);
                    //Send sprinting
                    stream.SendNext(false);
                    //Send material type
                    stream.SendNext(0);
                    //Send slow walk animation
                    stream.SendNext(false);
                    //Send stamina left
                    stream.SendNext(0f);
                    //Send air left
                    stream.SendNext(0f);
                    //Send directions
                    stream.SendNext(Vector3.zero);
                    stream.SendNext(Vector3.zero);
                }
            }
            else if (stream.IsReading) //To avoid errors before data arrives
            {
                //Check if the object is correct
                if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundSyncRuntimeData))
                {
                    pb.customMovementData = new BootsOnGroundSyncRuntimeData();
                }
                BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Read velocity
                bogsrd.velocity = (Vector3)stream.ReceiveNext();
                //Read grounded
                bogsrd.isGrounded = (bool)stream.ReceiveNext();
                //Read state
                bogsrd.state = (int)stream.ReceiveNext();
                //Read isSprinting
                bogsrd.isSprinting = (bool)stream.ReceiveNext();
                //Read material type
                bogsrd.currentMaterial = (string)stream.ReceiveNext();
                //Read slow animation
                bogsrd.playSlowWalkAnimation = (bool)stream.ReceiveNext();
                //Read stamina left
                bogsrd.staminaLeft = (float)stream.ReceiveNext();
                //Read air left
                bogsrd.airLeft = (float)stream.ReceiveNext();
                //Read directions
                bogsrd.moveDirection = (Vector3)stream.ReceiveNext();
                bogsrd.localMoveDirection = (Vector3)stream.ReceiveNext();
            }
        }

        public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;

                Terrain terrain = hit.collider.GetComponent<Terrain>();
                Kit_TerrainFootstepConverter convertedFootsteps = hit.collider.GetComponent<Kit_TerrainFootstepConverter>();

                if (terrain && convertedFootsteps)
                {
                    //Get texture from helper
                    int texture = GetMainTexture(pb.transform.position, terrain);

                    if (texture < convertedFootsteps.textureToString.Length)
                    {
                        //Convert texture id to 'tag' equivalent
                        if (allFootsteps.ContainsKey(convertedFootsteps.textureToString[texture]))
                        {
                            bogrd.currentMaterial = convertedFootsteps.textureToString[texture];
                        }
                        else
                        {
                            bogrd.currentMaterial = "Concrete";
                        }
                    }
                    else
                    {
                        Debug.LogError("Terrain texture outside of texture to foosteps conversion array! Playing concrete footstep.");
                        bogrd.currentMaterial = "Concrete";
                    }
                }
                else
                {
                    //Use tag if it exists
                    if (allFootsteps.ContainsKey(hit.collider.tag))
                    {
                        bogrd.currentMaterial = hit.collider.tag;
                    }
                    //Use default 'concrete'
                    else
                    {
                        bogrd.currentMaterial = "Concrete";
                    }
                }
            }
        }

        public override Vector3 GetVelocity(Kit_PlayerBehaviour pb)
        {
            if (pb.isController)
            {
                //If we are the controller, just get velocity from the character controller
                return pb.cc.velocity;
            }
            else
            {
                //If we are not, get it from the sync data
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                {
                    BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                    return bogsrd.velocitySmoothed;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        public override void PlaySound(Kit_PlayerBehaviour pb, int soundID, int id2, int arrayID)
        {
            //Check if sound isnt playing right now
            //Check for id
            if (soundID == 0) //Exhausted
            {
                if (!pb.movementSoundSource.isPlaying)
                {
                    //Set clip
                    pb.movementSoundSource.clip = staminaExhaustedSound[arrayID];
                    //Play
                    pb.movementSoundSource.Play();
                }
            }
            else if (soundID == 1)
            {
                //Set clip
                pb.movementSoundSource.clip = jumpSound[id2].clips[arrayID];
                //Play
                pb.movementSoundSource.Play();
            }
            else if (soundID == 2)
            {
                //Set clip
                pb.movementSoundSource.clip = jumpLandSound[id2].clips[arrayID];
                //Play
                pb.movementSoundSource.Play();
            }
        }

        public override void PlayAnimation(Kit_PlayerBehaviour pb, int id, int id2)
        {
            if (id == 0)
            {
                pb.thirdPersonPlayerModel.anim.SetTrigger(jumpAnimations[Mathf.Clamp(id2, 0, jumpAnimations.Length - 1)]);
            }
        }

        public override void OnTriggerEnterRelay(Kit_PlayerBehaviour pb, Collider col)
        {
            if (col.GetComponent<Kit_SwimmingTrigger>())
            {
                //Get
                Kit_SwimmingTrigger trigger = col.GetComponent<Kit_SwimmingTrigger>();
                //Check if its ours
                if (pb.photonView.IsMine)
                {
                    if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                    {
                        BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                        Debug.Log($"[SwimDBG] swimExitCooldown={bogrd.swimExitCooldown} Time.time={Time.time} state={bogrd.state} playerY={pb.transform.position.y}");
                        // Don't enter swim from ocean boundary trigger while inside playable area
                        if (col.gameObject.name == "ERA_OceanTrigger" || col.gameObject.name == "ERA_OceanDrown" || col.gameObject.name == "ERA_WaterDrown")
                        {
                            float distFromCenter = new Vector2(pb.transform.position.x, pb.transform.position.z).magnitude;
                            if (col.gameObject.name == "ERA_WaterDrown" || distFromCenter < 750f) return;
                        }
                        if (Time.time < bogrd.swimExitCooldown) { Debug.Log($"[SwimDBG] BLOCKED by cooldown"); return; }
                        //Only play this sound upon entering!
                        if (bogrd.state != 3)
                        {
                            //Check if a sound should be played
                            if (trigger.playerEnterSound)
                            {
                                if (Time.time - 0.5f > bogrd.lastSwimmingSoundPlayed)
                                {
                                    AudioSource.PlayClipAtPoint(trigger.playerEnterSound, pb.playerCameraTransform.position);
                                    bogrd.lastSwimmingSoundPlayed = Time.time;
                                }
                            }
                        }
                        //Set state
                        if (bogrd.state != 3)
                        {
                            bogrd.state = 3;
                            bogrd.swimEnteredAt = Time.time;
                        }
                        bogrd.swimmingCurrent = trigger;
                        // Ignore ERA_Ground FIRST, then teleport below surface
                        SetEraGroundIgnore(pb.cc, true);
                        Vector3 ep = pb.transform.position; ep.y = -0.5f; pb.transform.position = ep;
                        // Push player downward so they sink below ERA_Ground
                        bogrd.swimmingWorldMoveDirection = bogrd.moveDirection;
                        bogrd.swimmingWorldMoveDirection.y = -1f;
                        if (swimmingWeapon)
                        {
                            int[] weps = pb.weaponManager.GetCurrentlySelectedWeapon(pb);
                            if (weps[0] != bogrd.swimmingWeaponSlot)
                            {
                                bogrd.swimmingPreviousWeaponSlot = weps;
                            }
                            //Select swimming weapon
                            pb.weaponManager.PluginSelectWeapon(pb, bogrd.swimmingWeaponSlot, 0, true);
                        }
                    }
                }
                else
                {
                    if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                    {
                        BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                        //Only play this sound upon entering!
                        if (!bogrd.isSwimming)
                        {
                            //Check if a sound should be played
                            if (trigger.playerEnterSound)
                            {
                                if (Time.time - 0.5f > bogrd.lastSwimmingSoundPlayed)
                                {
                                    AudioSource.PlayClipAtPoint(trigger.playerEnterSound, pb.playerCameraTransform.position);
                                    bogrd.lastSwimmingSoundPlayed = Time.time;
                                }
                            }
                            bogrd.isSwimming = true;
                        }
                    }
                }
            }
        }

        public override void OnTriggerExitRelay(Kit_PlayerBehaviour pb, Collider col)
        {
            Kit_SwimmingTrigger swim = col.GetComponent<Kit_SwimmingTrigger>();
            if (swim)
            {
                if (pb.photonView.IsMine)
                {
                    if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                    {
                        BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                        if (bogrd.state == 3)
                        {
                            // Only exit swim if player position is outside ALL swim triggers
                            bool stillInWater = false;
                            foreach (var other in UnityEngine.Object.FindObjectsOfType<Kit_SwimmingTrigger>())
                            {
                                if (other.col == null) continue;
                                if (other.gameObject.name == "ERA_OceanTrigger" || other.gameObject.name == "ERA_OceanDrown") continue;
                                // Check if player position is inside trigger bounds (more reliable than CC bounds intersection)
                                if (other.col.bounds.Contains(pb.transform.position)) { stillInWater = true; break; }
                                // Also check slightly below player (CC bottom)
                                Vector3 ccBottom = pb.transform.position; ccBottom.y -= pb.cc.height * 0.5f;
                                if (other.col.bounds.Contains(ccBottom)) { stillInWater = true; break; }
                            }
                            if (stillInWater)
                            {
                                bogrd.swimmingCurrent = null;
                                foreach (var other in UnityEngine.Object.FindObjectsOfType<Kit_SwimmingTrigger>())
                                {
                                    if (other == swim || other.col == null) continue;
                                    if (other.col.bounds.Intersects(pb.cc.bounds)) { bogrd.swimmingCurrent = other; break; }
                                }
                                return;
                            }
                            Debug.Log("[Player] Exited swimming trigger. Exiting swim state.");
                            bogrd.wasSwimming = false;
                            bogrd.state = 0;
                            bogrd.isUnderwater = false;
                            bogrd.swimmingCurrent = null;
                            bogrd.moveDirection = bogrd.swimmingWorldMoveDirection;
                            bogrd.moveDirection.y = 0f;
                            bogrd.swimmingWorldMoveDirection.y = 0f;
                            bogrd.swimExitCooldown = Time.time + 2.0f;
                            pb.cc.height = standHeight; pb.cc.center = new Vector3(0f, standHeight / 2f, 0f);
                            SetEraGroundIgnore(pb.cc, false);
                            Vector3 exitPos = pb.transform.position; exitPos.y = 1.5f; pb.transform.position = exitPos;
                            if (swimmingWeapon)
                            {
                                pb.weaponManager.PluginSelectWeapon(pb, bogrd.swimmingPreviousWeaponSlot[0], bogrd.swimmingPreviousWeaponSlot[1], false);
                            }
                        }
                    }
                }
                else
                {
                    if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                    {
                        BootsOnGroundSyncRuntimeData bogrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                        bogrd.isSwimming = false;
                    }
                }
            }
        }

        public override void OnCameraTriggerEnterRelay(Kit_PlayerBehaviour pb, Collider col)
        {
            Kit_SwimmingTrigger swim = col.GetComponent<Kit_SwimmingTrigger>();
            //Check for swimming trigger
            if (swim)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    if (bogrd.swimmingCurrent == swim)
                    {
                        //Camera is underwater - player is underwater too
                        bogrd.isUnderwater = true;
                        //Check if that's really "us" and not a bot
                        if (pb.isFirstPersonActive)
                        {
                            //Show
                            pb.main.hud.DisplayUnderwater(true);
                        }
                    }
                }
            }
        }

        public override void OnCameraTriggerExitRelay(Kit_PlayerBehaviour pb, Collider col)
        {
            Kit_SwimmingTrigger swim = col.GetComponent<Kit_SwimmingTrigger>();
            //Check for swimming trigger
            if (swim)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    if (bogrd.swimmingCurrent == swim)
                    {
                        //Camera is not underwater - player is not underwater anymore
                        bogrd.isUnderwater = false;
                        //Check if that's really "us" and not a bot
                        if (pb.isFirstPersonActive)
                        {
                            //Hide
                            pb.main.hud.DisplayUnderwater(false);
                        }
                    }
                }
            }
        }

        #region Weapon Injection
        public override WeaponsFromPlugin WeaponsToInjectIntoWeaponManager(Kit_PlayerBehaviour player)
        {
            if (swimmingWeapon)
            {
                WeaponsFromPlugin toReturn = new WeaponsFromPlugin();
                toReturn.weaponsInSlot = new WeaponAttachmentBundle[1];
                toReturn.weaponsInSlot[0] = new WeaponAttachmentBundle();
                toReturn.weaponsInSlot[0].weapon = swimmingWeapon;
                toReturn.weaponsInSlot[0].attachments = new int[0];

                return toReturn;
            }
            else
            {
                return base.WeaponsToInjectIntoWeaponManager(player);
            }
        }

        public override void ReportSlotOfInjectedWeapons(Kit_PlayerBehaviour pb, int slotWhereTheyWereInjected)
        {
            //Check if the object is correct
            if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundRuntimeData))
            {
                pb.customMovementData = new BootsOnGroundRuntimeData();

                if (!pb.isBot)
                {
                    pb.main.hud.DisplayMovementState(0);
                }
            }

            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                bogrd.swimmingWeaponSlot = slotWhereTheyWereInjected;
            }
        }
        #endregion

        #region Helpers for Terrain
        public static float[] GetTextureMix(Vector3 worldPos, Terrain terrain)
        {
            // returns an array containing the relative mix of textures
            // on the main terrain at this world position.
            // The number of values in the array will equal the number
            // of textures added to the terrain.
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            // calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            // extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
            for (int n = 0; n < cellMix.Length; ++n)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }

            return cellMix;
        }

        public static int GetMainTexture(Vector3 worldPos, Terrain terrain)
        {
            // returns the zero-based index of the most dominant texture
            // on the main terrain at this world position.
            float[] mix = GetTextureMix(worldPos, terrain);
            float maxMix = 0;
            int maxIndex = 0;
            // loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; ++n)
            {
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }
            return maxIndex;
        }
        #endregion
    }
}
