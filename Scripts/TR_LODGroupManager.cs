using UnityEngine;

namespace MarsFPSKit
{
    // Attach once per scene on a persistent manager object.
    // Centralises LOD bias control for all era maps.
    // Call SetQualityLevel from a graphics settings menu or on platform detection.
    public class TR_LODGroupManager : MonoBehaviour
    {
        // LOD bias presets: higher = higher quality (less aggressive LOD switching)
        static readonly float[] LodBiasPresets = { 0.4f, 0.7f, 1.0f, 1.5f };

        // Labels matching Kit_GameSettings quality levels or your own enum
        public enum MapQuality { Low, Medium, High, Ultra }

        [SerializeField] MapQuality defaultQuality = MapQuality.Medium;

        void Start() => Apply(defaultQuality);

        public void SetQualityLevel(MapQuality quality) => Apply(quality);

        static void Apply(MapQuality quality)
        {
            int idx = Mathf.Clamp((int)quality, 0, LodBiasPresets.Length - 1);
            QualitySettings.lodBias = LodBiasPresets[idx];
        }
    }
}
