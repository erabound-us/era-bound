using UnityEngine;

namespace KillstreaksPlugin
{
    [System.Serializable]
    public class UavSettings
    {
        /// <summary>
        /// This is where the UAV initially starts and then moves to the start lerp position
        /// </summary>
        [Tooltip("This is where the UAV initially starts and then moves to the start lerp position")]
        public PositionRotation startPosition;
        /// <summary>
        /// This is where the UAV slowly starts flying over the map
        /// </summary>
        [Tooltip("This is where the UAV slowly starts flying over the map")]
        public PositionRotation circlingStartPosition;
        /// <summary>
        /// At the end of its lifetime this is where it will be.
        /// </summary>
        [Tooltip("At the end of its lifetime this is where it will be.")]
        public PositionRotation circlingEndPosition;
        /// <summary>
        /// Where the UAV flies to die
        /// </summary>
        [Tooltip("Where the UAV flies to die")]
        public PositionRotation endPosition;
        /// <summary>
        /// The time it takes for the UAV to arrive
        /// </summary>
        [Tooltip("The time it takes for the UAV to arrive")]
        public float uavArrivalTime = 5f;
        /// <summary>
        /// The actual lifetime of the UAV where it goes over the map
        /// </summary>
        [Tooltip("The actual lifetime of the UAV where it goes over the map")]
        public float uavLifeTime = 20f;
        /// <summary>
        /// The time it takes for the UAV to fly away woohoo
        /// </summary>
        [Tooltip("The time it takes for the UAV to fly away woohoo")]
        public float uavDepartureTime = 5f;
        /// <summary>
        /// Xp u gain when u destroy it
        /// </summary>
        [Tooltip("Xp u gain when u destroy it")]
        public int xpGainedUponDestruction = 100;

        [Header("UAV Scanner")]

        public PositionRotation scannerStartPos;

        public PositionRotation scannerEndPos;

        public float scannerTime = 5f;
    }

    [System.Serializable]
    public class HelicopterSettings
    {
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
        /// So that its not as abrupt
        /// </summary>
        public float helicopterSpeedEvenSetAtDistance = 30f;
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

    [System.Serializable]
    public class AirstrikeSettings
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

    public class Killstreaks_MapSettingsLoaderData : ScriptableObject
    {
        public string sceneName;

        public UavSettings uav;

        public AirstrikeSettings airstrike;

        public HelicopterSettings helicopter;
    }
}