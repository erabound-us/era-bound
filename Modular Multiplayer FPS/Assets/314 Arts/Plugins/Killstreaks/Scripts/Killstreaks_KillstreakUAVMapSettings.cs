using UnityEngine;

namespace KillstreaksPlugin
{
    /// <summary>
    /// Position and Rotation that is only editable from the inspector
    /// </summary>
    [System.Serializable]
    public class PositionRotation
    {
        [SerializeField]
        private Vector3 pos;
        [SerializeField]
        private Vector3 rot;

        public Vector3 position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return Quaternion.Euler(rot);
            }
            set
            {
                rot = value.eulerAngles;
            }
        }
    }

    public class Killstreaks_KillstreakUAVMapSettings : MonoBehaviour
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
        public GameObject scanner;

        public PositionRotation scannerStartPos;

        public PositionRotation scannerEndPos;

        public float scannerTime = 5f;

        public bool scannerIsOnline;
        [HideInInspector]
        private float scannerProgress;

        private void Update()
        {
            if (scannerIsOnline)
            {
                scannerProgress += Time.deltaTime / scannerTime;
                scanner.transform.position = Vector3.Lerp(scannerStartPos.position, scannerEndPos.position, scannerProgress);
                scanner.transform.rotation = Quaternion.Slerp(scannerStartPos.rotation, scannerEndPos.rotation,scannerProgress);
                if (scannerProgress > 1f) scannerProgress = 0f;
                scanner.SetActiveOptimized(true);
            }
            else
            {
                scannerProgress = 0f;
                scanner.transform.position = Vector3.Lerp(scannerStartPos.position, scannerEndPos.position, 0f);
                scanner.transform.rotation = Quaternion.Slerp(scannerStartPos.rotation, scannerEndPos.rotation, 0f);
                scanner.SetActiveOptimized(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPosition.position, circlingStartPosition.position);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(circlingStartPosition.position, circlingEndPosition.position);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(circlingEndPosition.position, endPosition.position);
        }
    }
}