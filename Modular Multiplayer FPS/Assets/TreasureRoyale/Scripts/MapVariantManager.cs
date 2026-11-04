using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MarsFPSKit
{
    // Attach once per scene on a persistent manager object.
    // Reads universeSeed from room properties and swaps prop sets,
    // lighting profiles, and terrain layers to generate era variants.
    // MasterClient reads seed; all clients apply the same swap deterministically.
    public class MapVariantManager : MonoBehaviour
    {
        [System.Serializable]
        public class PropSwapGroup
        {
            public string groupId;
            // Each entry is a root GameObject containing one variant's props.
            // Only the variant matching the seed-selected index is activated.
            public GameObject[] variants;
        }

        [System.Serializable]
        public class LightingVariant
        {
            public PostProcessProfile postProcessProfile;
            public Color ambientColor;
            public Color fogColor;
            public float fogDensity;
        }

        [Header("Prop Swap Groups")]
        [SerializeField] PropSwapGroup[] propGroups;

        [Header("Lighting Variants")]
        [SerializeField] LightingVariant[] lightingVariants;
        [SerializeField] PostProcessVolume globalVolume;

        [Header("Terrain Layers")]
        // Each entry is a full TerrainLayer array representing one surface variant.
        [SerializeField] TerrainLayerSet[] terrainLayerSets;
        [SerializeField] Terrain[] sceneTerrain;

        [System.Serializable]
        public class TerrainLayerSet
        {
            public TerrainLayer[] layers;
        }

        void Start()
        {
            int seed = 0;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("universeSeed"))
                seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["universeSeed"];
            else if (TR_OriginTimelineManager.OfflineMatchProps != null && TR_OriginTimelineManager.OfflineMatchProps.ContainsKey("universeSeed"))
                seed = (int)TR_OriginTimelineManager.OfflineMatchProps["universeSeed"];

            ApplyVariant(seed);
        }

        public void ApplyVariant(int seed)
        {
            var rng = new System.Random(seed);

            ApplyPropSwaps(rng);
            ApplyLighting(rng);
            ApplyTerrainLayers(rng);
        }

        void ApplyPropSwaps(System.Random rng)
        {
            foreach (var group in propGroups)
            {
                if (group.variants.Length == 0) continue;
                int chosen = rng.Next(group.variants.Length);
                for (int i = 0; i < group.variants.Length; i++)
                    if (group.variants[i] != null)
                        group.variants[i].SetActive(i == chosen);
            }
        }

        void ApplyLighting(System.Random rng)
        {
            if (lightingVariants.Length == 0) return;
            int chosen = rng.Next(lightingVariants.Length);
            var variant = lightingVariants[chosen];

            if (globalVolume != null && variant.postProcessProfile != null)
                globalVolume.profile = variant.postProcessProfile;

            RenderSettings.ambientLight = variant.ambientColor;
            RenderSettings.fogColor     = variant.fogColor;
            RenderSettings.fogDensity   = variant.fogDensity;
        }

        void ApplyTerrainLayers(System.Random rng)
        {
            if (terrainLayerSets.Length == 0 || sceneTerrain.Length == 0) return;
            int chosen = rng.Next(terrainLayerSets.Length);
            var layers = terrainLayerSets[chosen].layers;
            foreach (var t in sceneTerrain)
                if (t != null) t.terrainData.terrainLayers = layers;
        }
    }
}
