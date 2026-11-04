using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace MarsFPSKit
{
    static class FixTerrain
    {
        const string TerrainMatPath = "Assets/TreasureRoyale/Terrain_URP.mat";

        [MenuItem("TreasureRoyale/Fix Terrain Shader (URP)")]
        static void Run()
        {
            var terrains = Object.FindObjectsOfType<Terrain>();
            if (terrains.Length == 0) { Debug.Log("[TR] No Terrain objects found."); return; }

            var urpTerrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit")
                                ?? Shader.Find("Universal Render Pipeline/Lit");
            if (urpTerrainShader == null) { Debug.LogError("[TR] URP terrain shader not found."); return; }

            // Load or create a persistent terrain material asset
            var mat = AssetDatabase.LoadAssetAtPath<Material>(TerrainMatPath);
            if (mat == null)
            {
                mat = new Material(urpTerrainShader) { name = "Terrain_URP" };
                AssetDatabase.CreateAsset(mat, TerrainMatPath);
            }
            else if (mat.shader != urpTerrainShader)
            {
                mat.shader = urpTerrainShader;
                EditorUtility.SetDirty(mat);
            }

            int fixed_ = 0;
            foreach (var terrain in terrains)
            {
                if (terrain.materialTemplate != mat)
                {
                    terrain.materialTemplate = mat;
                    EditorUtility.SetDirty(terrain);
                    fixed_++;
                    Debug.Log("[TR] Fixed terrain: " + terrain.name);
                }
            }

            // Also fix terrain layers - they may have wrong shaders
            foreach (var guid in AssetDatabase.FindAssets("t:TerrainLayer"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                if (layer != null)
                {
                    EditorUtility.SetDirty(layer);
                    fixed_++;
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[TR] Fixed " + fixed_ + " terrain objects/layers.");
        }

        [MenuItem("TreasureRoyale/Disable All Terrain Objects")]
        static void DisableTerrains()
        {
            var terrains = Object.FindObjectsOfType<Terrain>(true);
            foreach (var t in terrains)
            {
                t.gameObject.SetActive(false);
                EditorUtility.SetDirty(t.gameObject);
                Debug.Log("[TR] Disabled terrain: " + t.name);
            }
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[TR] All terrains disabled.");
        }
    }
}
