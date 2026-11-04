using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MarsFPSKit
{
    [InitializeOnLoad]
    static class EraAssetAutoWirer
    {
        const string ConfigPath = "Assets/TreasureRoyale/Themes/EraAssetConfig.asset";

        static readonly string[] MapScenes =
        {
            "Assets/314 Arts/MarsFPSKit/Scenes/Showcase_Map.unity",
            "Assets/314 Arts/MarsFPSKit/Scenes/Test_Map_1.unity",
            "Assets/314 Arts/MarsFPSKit/Scenes/Test_Map_3.unity"
        };

        static readonly string[] TreePaths =
        {
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree1_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree1_2_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree2_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree2_2_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree3_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree3_2_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree4_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree4_2_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree5_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree5_2_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree6_1_prefab.prefab",
            "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/tree6_2_prefab.prefab",
            // smalltree, bush, flower, mushroom all use Shader Graphs/TerrainGrass — render as black flat quads, excluded
            // Mountain Terrain rocks and tree — Standard shader, _MainTex, converts correctly
            "Assets/Mountain Terrain rocks and tree/Prefab/tree_01.prefab",
        };

        static readonly string[] PalmPaths =
        {
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm1.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm2.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm3.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm4.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm5.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm6.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Palm7.prefab",
        };

        static readonly string[] RockPaths =
        {
            // Rocks and Boulders 2 — proper textures, good quality
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock1A.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock1B.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock1C.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock2.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock3.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock4A.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock4B.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock5A.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock6A.prefab",
            "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock6B.prefab",
            // LowPolyMedieval — textured, era-appropriate
            "Assets/LowPolyMedievalStarterPack/Prefabs/MossyBoulder.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/RockCluster.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/SmallRock.prefab",
            // AncientRuins — textured
            "Assets/AncientRuins/Prefabs/P_AncientRuins_Rock1.prefab",
            "Assets/AncientRuins/Prefabs/P_AncientRuins_Rock2.prefab",
            // Mountain Terrain rocks and tree — Standard shader, converts correctly
            // NOTE: rock_set prefabs removed — Standard shader MeshColliders cause walk-through on non-static objects
        };

        static readonly string[] MountainPaths = System.Array.Empty<string>();

        // Only real buildings and large standalone ruins — no props, fence segments, or plain walls
        static readonly string[] StructuresAncient =
        {
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins1.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins2.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins3.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins4.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins5.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins6.prefab",
            // Symphonie URP prefab removed — URP Lit shader renders black without proper lighting
        };

        static readonly string[] StructuresMedieval =
        {
            // Full buildings — good as standalone scattered structures
            "Assets/LowPolyMedievalStarterPack/Prefabs/CastleTower.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/MedievalHouse.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/SmallMedievalHouse.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/TwoStoryMedievalHouse.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/TwoStoryMedievalHouse_2.prefab",
            "Assets/LowPolyMedievalStarterPack/Prefabs/RuinedArchway.prefab",
        };

        static readonly string[] StructuresModern =
        {
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins1.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins2.prefab",
            "Assets/PolyRonin/Desert Oasis Pack/Prefabs/Ruins3.prefab",
        };

        static EraAssetAutoWirer() => EditorApplication.delayCall += AutoWire;

        [MenuItem("TreasureRoyale/Rewire Era Assets")]
        public static void AutoWire()
        {
            var config = BuildOrUpdateConfig();
            if (config == null) return;
            AssignToScenes(config);
            Debug.Log("[TR] EraAssetAutoWirer: done.");
        }

        static EraAssetConfig BuildOrUpdateConfig()
        {
            var trees   = Load(TreePaths);
            var palms   = Load(PalmPaths);
            var rocks   = Load(RockPaths);
            var mountains = Load(MountainPaths);
            var water   = LoadOne("Assets/WaterWorks/Water_Plane.prefab");

            var ancientStructures  = Load(StructuresAncient);
            var medievalStructures = Load(StructuresMedieval);
            var modernStructures   = Load(StructuresModern);

            var treesWithPalms = new List<GameObject>(trees);
            treesWithPalms.AddRange(palms);

            var config = AssetDatabase.LoadAssetAtPath<EraAssetConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EraAssetConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            var entries = new List<EraAssetConfig.EraAssets>
            {
                Make("deep_prehistoric", treesWithPalms.ToArray(), rocks, ancientStructures,  water, mountains),
                Make("prehistoric",      treesWithPalms.ToArray(), rocks, ancientStructures,  water, mountains),
                Make("ancient",          treesWithPalms.ToArray(), rocks, ancientStructures,  water, mountains),
                Make("medieval",         trees,                    rocks, medievalStructures, water, mountains),
                Make("renaissance",      trees,                    rocks, medievalStructures, water, mountains),
                Make("industrial",       trees,                    rocks, modernStructures,   water, mountains),
                Make("modern",           trees,                    rocks, modernStructures,   water, mountains),
                Make("near_future",      trees,                    rocks, modernStructures,   water, mountains),
                Make("mid_future",       trees,                    rocks, modernStructures,   water, mountains),
                Make("far_future",       trees,                    rocks, modernStructures,   water, mountains),
                Make("deep_future",      trees,                    rocks, modernStructures,   water, mountains),
            };

            config.eras = entries.ToArray();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[TR] EraAssetConfig — trees:{trees.Length}+palms:{palms.Length} rocks:{rocks.Length} " +
                      $"water:{(water != null ? water.name : "NULL")}");
            return config;
        }

        static EraAssetConfig.EraAssets Make(string era, GameObject[] trees, GameObject[] rocks,
                                              GameObject[] structures, GameObject water, GameObject[] mountains)
        {
            var ssrMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/WaterWorks/Materials/SSR_Water_Bright.mat");
            var mountain = mountains != null && mountains.Length > 0 ? mountains[0] : null;
            return new EraAssetConfig.EraAssets
            {
                era              = era,
                treePrefabs      = trees,
                rockPrefabs      = rocks,
                structurePrefabs = structures,
                cavePrefabs      = System.Array.Empty<GameObject>(),
                waterPrefab      = water,
                waterMaterial    = ssrMat,
                mountainPrefab   = mountain,
            };
        }

        static GameObject[] Load(string[] paths)
        {
            var list = new List<GameObject>();
            foreach (var p in paths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go != null) list.Add(go);
                else Debug.LogWarning($"[TR] AutoWirer: prefab not found: {p}");
            }
            return list.ToArray();
        }

        static GameObject LoadOne(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) Debug.LogWarning($"[TR] AutoWirer: prefab not found: {path}");
            return go;
        }

        static void AssignToScenes(EraAssetConfig config)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string activePath = EditorSceneManager.GetActiveScene().path;
            foreach (string scenePath in MapScenes)
            {
                string full = Path.Combine(Application.dataPath, "..", scenePath);
                if (!File.Exists(full)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool dirty = false;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var gen = root.GetComponentInChildren<EraMapGenerator>(true);
                    if (gen == null) continue;
                    if (gen.assetConfig != config) { gen.assetConfig = config; EditorUtility.SetDirty(gen); dirty = true; }
                }
                if (dirty) EditorSceneManager.SaveScene(scene);
            }
            if (!string.IsNullOrEmpty(activePath) && File.Exists(activePath))
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
        }
    }
}
