using UnityEngine;

namespace KillstreaksPlugin
{
    public class Killstreaks_KillstreakAirstrikeMapSettings : MonoBehaviour
    {
        /// <summary>
        /// Prefab of plane
        /// </summary>
        public GameObject planePrefab;
        /// <summary>
        /// How many planes fly over this map
        /// </summary>
        public int amountOfPlanes = 3;
        /// <summary>
        /// Time between spawns of planes
        /// </summary>
        public float planeSpawnTime = 1.5f;
        /// <summary>
        /// At which height do the planes fly?
        /// </summary>
        public float planeAltitude = 30f;
        /// <summary>
        /// Speed of plane.
        /// </summary>
        public float planeSpeed = 100f;
        /// <summary>
        /// How far away the planes spawn :)
        /// </summary>
        public float spawnPointDistance = 400f;
        /// <summary>
        /// How far away from the center the planes drop bombs
        /// </summary>
        public float distanceAtWhichDropStats = 30f;
        /// <summary>
        /// Bomb Amount
        /// </summary>
        public int amountOfBombs = 3;
        /// <summary>
        /// How much time needs to pass between bomb drops
        /// </summary>
        public float timeBetweenBombDrops = 1f;
        /// <summary>
        /// Prefab for bomb
        /// </summary>
        public GameObject bombPrefab;
        /// <summary>
        /// Velocity of bombs
        /// </summary>
        public Vector3 bombDropStartVelocity;
        /// <summary>
        /// Xp u gain when u destroy it
        /// </summary>
        [Tooltip("Xp u gain when u destroy it")]
        public int xpGainedUponDestruction = 100;
    }
}