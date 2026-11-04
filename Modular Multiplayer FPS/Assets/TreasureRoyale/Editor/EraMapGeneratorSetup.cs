using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MarsFPSKit
{
    public class EraMapGeneratorSetup : EditorWindow
    {
        static readonly string[] MapScenes = {
            "Assets/314 Arts/MarsFPSKit/Scenes/Showcase_Map.unity",
            "Assets/314 Arts/MarsFPSKit/Scenes/Test_Map_1.unity",
            "Assets/314 Arts/MarsFPSKit/Scenes/Test_Map_3.unity"
        };

        [MenuItem("TreasureRoyale/Setup Era Map Generator")]
        static void Run()
        {
            string currentPath = EditorSceneManager.GetActiveScene().path;
            foreach (string scenePath in MapScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                InjectIntoScene(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[TR] EraMapGenerator injected: " + scenePath);
            }
            if (!string.IsNullOrEmpty(currentPath))
                EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
            // Auto-wire any imported free package assets into EraAssetConfig
            EraAssetAutoWirer.AutoWire();
        }

        static void InjectIntoScene(Scene scene)
        {
            // Remove old MapVariantManager
            foreach (var go in scene.GetRootGameObjects())
            {
                var old = go.GetComponent<MapVariantManager>();
                if (old != null) Object.DestroyImmediate(old);
            }

            // Ensure _RuntimePinkFixer exists
            bool hasFixer = false;
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "_RuntimePinkFixer") { hasFixer = true; break; }
            if (!hasFixer)
            {
                EditorSceneManager.SetActiveScene(scene);
                var fixer = new GameObject("_RuntimePinkFixer");
                fixer.AddComponent<RuntimePinkFixer>();
                fixer.AddComponent<OfflineMatchBootstrap>();
            }

            // Skip EraMapGenerator if already injected
            foreach (var go in scene.GetRootGameObjects())
                if (go.GetComponent<EraMapGenerator>() != null) return;

            EditorSceneManager.SetActiveScene(scene);
            var mgr = new GameObject("EraMapGenerator");
            var gen = mgr.AddComponent<EraMapGenerator>();
            var structRoot = new GameObject("ERA_Structures");
            gen.structureRoot = structRoot.transform;
        }
    }
}
