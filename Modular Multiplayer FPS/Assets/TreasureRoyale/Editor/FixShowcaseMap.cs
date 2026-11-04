using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MarsFPSKit
{
    static class FixShowcaseMap
    {
        [MenuItem("TreasureRoyale/Fix Showcase Map (Use Real Terrain)")]
        static void Run()
        {
            string scenePath = "Assets/314 Arts/MarsFPSKit/Scenes/Showcase_Map.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            // URP terrain material
            string matPath = "Assets/TreasureRoyale/Terrain_URP.mat";
            var urpShader = Shader.Find("Universal Render Pipeline/Terrain/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Lit");
            Material terrainMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (terrainMat == null && urpShader != null)
            {
                terrainMat = new Material(urpShader) { name = "Terrain_URP" };
                AssetDatabase.CreateAsset(terrainMat, matPath);
            }

            // Correct TerrainData
            var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/New Terrain.asset");
            if (terrainData == null)
                Debug.LogWarning("[TR] New Terrain.asset not found!");

            int changes = 0;
            foreach (var go in scene.GetRootGameObjects())
            {
                // Disable procedural objects
                if (go.name == "EraMapGenerator" || go.name == "ERA_Structures")
                {
                    if (go.activeSelf) { go.SetActive(false); EditorUtility.SetDirty(go); changes++; Debug.Log("[TR] Disabled " + go.name); }
                    continue;
                }

                // Enable map geometry
                if (go.name == "Ground" || go.name == "Map")
                    if (!go.activeSelf) { go.SetActive(true); EditorUtility.SetDirty(go); changes++; }

                // Fix Terrain
                var terrain = go.GetComponentInChildren<Terrain>(true);
                if (terrain != null)
                {
                    if (!terrain.gameObject.activeSelf)
                    { terrain.gameObject.SetActive(true); EditorUtility.SetDirty(terrain.gameObject); changes++; }

                    if (terrainData != null && terrain.terrainData != terrainData)
                    { terrain.terrainData = terrainData; EditorUtility.SetDirty(terrain); changes++; Debug.Log("[TR] Fixed TerrainData on Terrain"); }

                    var tc = terrain.GetComponent<TerrainCollider>();
                    if (tc != null && terrainData != null && tc.terrainData != terrainData)
                    { tc.terrainData = terrainData; EditorUtility.SetDirty(tc); changes++; }

                    if (terrain.transform.position != Vector3.zero)
                    { terrain.transform.position = Vector3.zero; EditorUtility.SetDirty(terrain.transform); changes++; Debug.Log("[TR] Moved Terrain to origin"); }

                    if (terrainMat != null && terrain.materialTemplate != terrainMat)
                    { terrain.materialTemplate = terrainMat; EditorUtility.SetDirty(terrain); changes++; }
                }
            }

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TR] FixShowcaseMap: {changes} changes saved.");
        }
    }
}
