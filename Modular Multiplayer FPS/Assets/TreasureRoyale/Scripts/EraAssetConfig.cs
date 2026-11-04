using UnityEngine;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "TreasureRoyale/Era Asset Config", fileName = "EraAssetConfig")]
    public class EraAssetConfig : ScriptableObject
    {
        [System.Serializable]
        public class EraAssets
        {
            public string era;
            [Header("Trees / Foliage")]
            public GameObject[] treePrefabs;
            [Header("Rocks / Boulders")]
            public GameObject[] rockPrefabs;
            [Header("Structures / Ruins")]
            public GameObject[] structurePrefabs;
            [Header("Cave Entrances")]
            public GameObject[] cavePrefabs;
            [Header("Water Surface (river/lake mesh)")]
            public GameObject waterPrefab;
            [Header("Water Material (overrides default Unlit/Color)")]
            public Material waterMaterial;
            [Header("Mountain / Cliff Mesh")]
            public GameObject mountainPrefab;
        }

        public EraAssets[] eras;

        public EraAssets Get(string era)
        {
            foreach (var e in eras)
                if (e.era == era) return e;
            return null;
        }
    }
}
