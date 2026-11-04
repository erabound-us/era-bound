#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsFPSKit
{
    public class TR_SceneDiag : EditorWindow
    {
        [MenuItem("TreasureRoyale/Enable Era Map Generator")]
        static void EnableEraGen()
        {
            var scene = SceneManager.GetActiveScene();
            var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGOs)
            {
                if (go.scene != scene) continue;
                if (go.name == "EraMapGenerator")
                {
                    Undo.RecordObject(go, "Enable EraMapGenerator");
                    go.SetActive(true);
                    EditorUtility.SetDirty(go);
                    Debug.Log("[TR] EraMapGenerator enabled");
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("TreasureRoyale/Fix Ground Material")]
        static void FixGroundMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/314 Arts/MarsFPSKit/Materials/Environment/MudRocky.mat");
            if (mat == null) { Debug.LogError("[TR] MudRocky.mat not found"); return; }

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("[TR] URP Lit shader not found"); return; }

            Undo.RecordObject(mat, "Fix Ground Material");
            mat.shader = urpLit;
            mat.SetTextureScale("_BaseMap", new Vector2(4, 4));
            mat.SetTextureScale("_MainTex", new Vector2(4, 4));
            mat.SetTexture("_BumpMap", null);
            mat.DisableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TR] MudRocky fixed: shader={mat.shader.name} tiling=4x4 normalmap=removed");
        }

        [MenuItem("TreasureRoyale/Diagnose Scene")]
        static void Diag()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log($"[TR_Diag] Active scene: {scene.name} path={scene.path} loaded={scene.isLoaded}");

            var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            int terrainCount = 0, groundCount = 0, colliderCount = 0, zeroColliders = 0;

            foreach (var go in allGOs)
            {
                if (go.scene != scene) continue;
                if (go.name == "Terrain") { terrainCount++; Debug.Log($"[TR_Diag] Terrain found: active={go.activeSelf} pos={go.transform.position}"); }
                if (go.name == "Ground") { groundCount++; Debug.Log($"[TR_Diag] Ground found: active={go.activeSelf} childCount={go.transform.childCount}"); }
                var bc = go.GetComponent<BoxCollider>();
                if (bc != null) { colliderCount++; if (bc.size.y < 0.01f) zeroColliders++; }
            }

            Debug.Log($"[TR_Diag] Terrain GOs: {terrainCount}, Ground GOs: {groundCount}, BoxColliders: {colliderCount}, Zero-Y colliders: {zeroColliders}");
            Debug.Log($"[TR_Diag] Layer 8 vs 0 ignored: {Physics.GetIgnoreLayerCollision(8, 0)}");
            Debug.Log($"[TR_Diag] Layer 8 vs 8 ignored: {Physics.GetIgnoreLayerCollision(8, 8)}");
        }

        [MenuItem("TreasureRoyale/Fix Physics Layers")]
        static void FixPhysicsLayers()
        {
            var scene = SceneManager.GetActiveScene();

            // Persist to ProjectSettings via SerializedObject so it survives domain reload
            var dynManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
            if (dynManager.Length > 0)
            {
                var so = new SerializedObject(dynManager[0]);
                var matrix = so.FindProperty("m_LayerCollisionMatrix");
                if (matrix != null)
                {
                    // Layer collision matrix is stored as a flat array of 32 ints (one per layer)
                    // Each int is a bitmask of which layers that layer collides with.
                    // Layer 8 bitmask: set bit 0 (layer 0) and bit 8 (layer 8)
                    int layer8val = matrix.GetArrayElementAtIndex(8).intValue;
                    layer8val |= (1 << 0) | (1 << 8);
                    matrix.GetArrayElementAtIndex(8).intValue = layer8val;
                    // Also set layer 0's bit 8
                    int layer0val = matrix.GetArrayElementAtIndex(0).intValue;
                    layer0val |= (1 << 8);
                    matrix.GetArrayElementAtIndex(0).intValue = layer0val;
                    so.ApplyModifiedProperties();
                    Debug.Log("[TR] Physics layer matrix: layer 8 <-> layer 0 enabled and saved to ProjectSettings");
                }
                else
                    Debug.LogWarning("[TR] m_LayerCollisionMatrix property not found — open Edit > Project Settings > Physics and enable layer 8 vs layer 0 manually");
            }

            Physics.IgnoreLayerCollision(8, 0, false);
            Physics.IgnoreLayerCollision(8, 8, false);
            Physics.IgnoreLayerCollision(0, 0, false);

            Debug.Log($"[TR] After fix — Layer 8 vs 0 ignored: {Physics.GetIgnoreLayerCollision(8, 0)}");

            var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            int moved = 0, fixedC = 0;
            foreach (var go in allGOs)
            {
                if (go.scene != scene) continue;
                if (go.transform.parent != null && go.transform.parent.name == "Ground")
                {
                    if (go.layer != 8)
                    {
                        Undo.RecordObject(go, "Fix Layer");
                        go.layer = 8;
                        EditorUtility.SetDirty(go);
                        moved++;
                    }
                    var bc = go.GetComponent<BoxCollider>();
                    if (bc != null && bc.size.y < 0.01f)
                    {
                        Undo.RecordObject(bc, "Fix Collider");
                        bc.size = new Vector3(bc.size.x, 0.1f, bc.size.z);
                        bc.center = new Vector3(bc.center.x, -0.05f, bc.center.z);
                        EditorUtility.SetDirty(bc);
                        fixedC++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[TR] Fix Physics Layers done: moved={moved} to layer 8, colliders fixed={fixedC}");
        }

        [MenuItem("TreasureRoyale/Force Fix All")]
        static void ForceFix()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded) { Debug.LogError("[TR] Scene not loaded"); return; }

            var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            int fixedColliders = 0;
            Terrain terrain = null;

            foreach (var go in allGOs)
            {
                if (go.scene != scene) continue;

                var t = go.GetComponent<Terrain>();
                if (t != null)
                {
                    terrain = t;
                    var size = t.terrainData.size;
                    var newPos = new Vector3(-size.x / 2f, 0f, -size.z / 2f);
                    Undo.RecordObject(go.transform, "Fix Terrain Pos");
                    go.transform.position = newPos;
                    EditorUtility.SetDirty(go.transform);
                    Debug.Log($"[TR] Terrain moved to {newPos}");
                }

                var bc = go.GetComponent<BoxCollider>();
                if (bc != null && bc.size.y < 0.01f)
                {
                    Undo.RecordObject(bc, "Fix Collider");
                    bc.size = new Vector3(bc.size.x, 0.1f, bc.size.z);
                    bc.center = new Vector3(bc.center.x, -0.05f, bc.center.z);
                    EditorUtility.SetDirty(bc);
                    fixedColliders++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[TR] Force Fix done: {fixedColliders} colliders fixed, terrain={terrain != null}");
        }
    }
}
#endif
