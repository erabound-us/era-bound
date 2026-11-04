using UnityEngine;

namespace KillstreaksPlugin
{
    /// <summary>
    /// This script holds map settings for helicopter!
    /// </summary>
    public class Killstreaks_KillstreakHelicopterMapSettings : MonoBehaviour
    {
        /// <summary>
        /// Where the helicopter can spawn (and despawn!)
        /// </summary>
        public Transform[] spawnPoints;
        /// <summary>
        /// Total live time of helicopter.
        /// </summary>
        public float helicopterLiveTime = 60f;
        /// <summary>
        /// How long will the helicopter stay at a given hover point?
        /// </summary>
        public Vector2 helicopterHoveringTime = new Vector2(3, 6f);
        /// <summary>
        /// Smooth speed for Helicopter
        /// </summary>
        public float helicopterSmooth = 5f;
        /// <summary>
        /// World Space Points where the helicopter will fly to and hover for a short while before flying to another hover point (AND SHOOTS YOU!)
        /// </summary>
        public Vector3[] hoveringPoints;
        /// <summary>
        /// Xp u gain when u destroy it
        /// </summary>
        [Tooltip("Xp u gain when u destroy it")]
        public int xpGainedUponDestruction = 100;
    }
}