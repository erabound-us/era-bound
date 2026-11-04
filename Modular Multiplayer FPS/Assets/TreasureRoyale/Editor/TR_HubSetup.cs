#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MarsFPSKit
{
    public static class TR_HubSetup
    {
        const string MAIN_MENU_SCENE = "Assets/314 Arts/MarsFPSKit/Scenes/MainMenu.unity";

        [MenuItem("Tools/Treasure Royale/Open Hub Scene")]
        static void OpenHubScene()
        {
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);
        }

        [MenuItem("Tools/Treasure Royale/2 - Setup Hub (Run Once)")]
        static void SetupHub()
        {
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            // Remove old TR objects
            foreach (var name in new[] { "TR_HubCanvas", "TR_HubManager", "TR_PersistentManager" })
                DestroyIfExists(name);

            // TR_HubManager on its own GO — builds the entire canvas at runtime
            var hubGO = new GameObject("TR_HubManager");
            AddComp(hubGO, "MarsFPSKit.TR_HubManager");

            // Persistent services
            var persistent = new GameObject("TR_PersistentManager");
            AddComp(persistent, "MarsFPSKit.TR_HubApi");
            AddComp(persistent, "MarsFPSKit.TR_OriginTimelineManager");
            AddComp(persistent, "MarsFPSKit.TR_PlatformQualityBootstrap");
            AddComp(persistent, "MarsFPSKit.TR_HubBootstrap");

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
            Debug.Log($"[TR Setup] Done. Saved={saved}. TR_HubManager will build UI at runtime.");
        }

        static void DestroyIfExists(string name)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null) continue;
                try { if (go.name == name && go.scene.isLoaded) Object.DestroyImmediate(go); } catch { }
            }
        }

        static void AddComp(GameObject go, string typeName)
        {
            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) { Debug.LogWarning($"[TR Setup] Type not found: {typeName}"); return; }
            if (go.GetComponent(type) == null) go.AddComponent(type);
        }
    }
}
#endif
