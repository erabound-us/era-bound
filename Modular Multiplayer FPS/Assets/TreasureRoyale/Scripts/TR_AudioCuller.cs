using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach once per scene on a persistent manager object.
    // Disables AudioSources on world objects beyond cullDistance from the
    // local camera. Re-enables them when the player moves close again.
    // Does NOT touch the player's own AudioSources (footsteps, weapons).
    public class TR_AudioCuller : MonoBehaviour
    {
        [SerializeField] float cullDistance  = 80f;
        [SerializeField] float updateInterval = 1f;

        struct CullEntry
        {
            public AudioSource source;
            public bool wasEnabled;
        }

        readonly List<CullEntry> _entries = new List<CullEntry>();
        Kit_IngameMain _main;
        float _nextUpdate;

        void Start()
        {
            _main = FindObjectOfType<Kit_IngameMain>();

            // Register all world AudioSources that are NOT on player prefabs
            foreach (var src in FindObjectsOfType<AudioSource>())
            {
                if (src.GetComponentInParent<Kit_PlayerBehaviour>() != null) continue;
                _entries.Add(new CullEntry { source = src, wasEnabled = src.enabled });
            }
        }

        void Update()
        {
            if (Time.time < _nextUpdate) return;
            _nextUpdate = Time.time + updateInterval;

            Transform cam = _main?.mainCamera?.transform;
            if (cam == null) return;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.source == null) { _entries.RemoveAt(i); continue; }

                bool inRange = Vector3.Distance(entry.source.transform.position, cam.position) <= cullDistance;
                entry.source.enabled = inRange && entry.wasEnabled;
                _entries[i] = entry;
            }
        }
    }
}
