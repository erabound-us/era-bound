using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace MarsFPSKit
{
    public static class TR_BuildCleanScene
    {
        const string SRC  = "Assets/314 Arts/MarsFPSKit/Scenes/Showcase_Map.unity";
        const string DEST = "Assets/TreasureRoyale/Scenes/TreasureRoyale_Map.unity";

        [MenuItem("TreasureRoyale/Build Clean TR Map Scene")]
        static void Build()
        {
            // Save current scene
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Copy Showcase_Map to new path (preserves all kit prefab instances)
            AssetDatabase.CopyAsset(SRC, DEST);
            AssetDatabase.Refresh();

            // Open the new scene
            var scene = EditorSceneManager.OpenScene(DEST, OpenSceneMode.Single);

            // Names of root GOs to KEEP (everything else gets deleted)
            var keep = new System.Collections.Generic.HashSet<string> {
                "MarsFPSKit_IngamePrefab",
                "Kit_SceneSyncer",
                "Directional Light",
            };

            // Delete all root GOs not in keep list
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                if (!keep.Contains(go.name))
                    Object.DestroyImmediate(go);
            }

            // Add _RuntimePinkFixer
            var pinkFixer = new GameObject("_RuntimePinkFixer");
            pinkFixer.AddComponent<RuntimePinkFixer>();

            // Add ERA_Structures root
            var eraStructures = new GameObject("ERA_Structures");

            // Add EraMapGenerator GO with component pre-wired
            var emgGO = new GameObject("EraMapGenerator");
            var emg = emgGO.AddComponent<EraMapGenerator>();
            emg.structureRoot = eraStructures.transform;
            // Wire EraAssetConfig
            var config = AssetDatabase.LoadAssetAtPath<EraAssetConfig>(
                "Assets/TreasureRoyale/Themes/EraAssetConfig.asset");
            if (config != null)
                emg.assetConfig = config;
            else
                Debug.LogWarning("[TR] BuildCleanScene: EraAssetConfig not found at expected path");

            // Add 8 spawn points in a ring at Y=1.5
            var spawnScript = typeof(Kit_PlayerSpawn);
            var trMode = AssetDatabase.LoadAssetAtPath<Kit_PvE_TreasureRoyale>(
                "Assets/314 Arts/MarsFPSKit/Required/New Kit_Pv E_Treasure Royale.asset");
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float r = 20f;
                var spawnGO = new GameObject("TR_Spawn_" + i);
                spawnGO.transform.position = new Vector3(
                    Mathf.Cos(angle) * r, 1.5f, Mathf.Sin(angle) * r);
                spawnGO.transform.rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);
                var sp = spawnGO.AddComponent<Kit_PlayerSpawn>();
                if (trMode != null)
                    sp.coopGameModes = new Kit_PvE_GameModeBase[] { trMode };
            }

            // Register scene in build settings
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);
            bool alreadyAdded = false;
            foreach (var s in buildScenes)
                if (s.path == DEST) { alreadyAdded = true; break; }
            if (!alreadyAdded)
                buildScenes.Add(new EditorBuildSettingsScene(DEST, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            // Save
            EditorSceneManager.SaveScene(scene, DEST);
            AssetDatabase.Refresh();

            // Update TR game mode asset to use new map
            var trGameMode = AssetDatabase.LoadAssetAtPath<Kit_PvE_TreasureRoyale>(
                "Assets/314 Arts/MarsFPSKit/Required/New Kit_Pv E_Treasure Royale.asset");
            var newMap = AssetDatabase.LoadAssetAtPath<Kit_MapInformation>(
                "Assets/314 Arts/MarsFPSKit/GameData/Maps/TreasureRoyale_Map.asset");
            if (trGameMode != null && newMap != null)
            {
                trGameMode.maps = new Kit_MapInformation[] { newMap };
                EditorUtility.SetDirty(trGameMode);
                AssetDatabase.SaveAssets();
                Debug.Log("[TR] Updated TR game mode to use TreasureRoyale_Map");
            }
            else
                Debug.LogWarning("[TR] Could not update game mode maps — load assets manually");

            Debug.Log("[TR] Clean scene built at " + DEST);
            EditorUtility.DisplayDialog("Done",
                "TreasureRoyale_Map.unity created and wired into the TR game mode.",
                "OK");
        }
    }
}
