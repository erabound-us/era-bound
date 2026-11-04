using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the Player prefab alongside Kit_PlayerBehaviour.
    // Swaps renderer sets based on distance from the local camera.
    // Does NOT run on the local player (isController = true) — only remote players.
    public class TR_CharacterLOD : MonoBehaviour
    {
        [System.Serializable]
        public class LODLevel
        {
            public float maxDistance;       // switch to next level beyond this
            public Renderer[] renderers;    // renderers active at this level
            public int shadowCastingMode;   // 0=Off 1=On 2=TwoSided 3=ShadowsOnly
        }

        [SerializeField] LODLevel[] levels; // ordered near → far
        [SerializeField] float updateInterval = 0.2f;

        Kit_PlayerBehaviour _pb;
        Transform _localCam;
        float _nextUpdate;
        int _currentLevel = -1;

        void Awake() => _pb = GetComponent<Kit_PlayerBehaviour>();

        void Update()
        {
            // Only process remote players
            if (_pb == null || _pb.isController) return;
            if (Time.time < _nextUpdate) return;
            _nextUpdate = Time.time + updateInterval;

            if (_localCam == null)
            {
                var main = _pb.main;
                if (main == null || main.mainCamera == null) return;
                _localCam = main.mainCamera.transform;
            }

            float dist = Vector3.Distance(transform.position, _localCam.position);
            int target = levels.Length - 1;
            for (int i = 0; i < levels.Length; i++)
            {
                if (dist <= levels[i].maxDistance) { target = i; break; }
            }

            if (target == _currentLevel) return;
            _currentLevel = target;

            // Disable all, then enable chosen level
            for (int i = 0; i < levels.Length; i++)
            {
                bool active = i == target;
                foreach (var r in levels[i].renderers)
                {
                    if (r == null) continue;
                    r.enabled = active;
                    if (active)
                        r.shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode)levels[i].shadowCastingMode;
                }
            }
        }
    }
}
