#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsFPSKit
{
    public class TR_TerrainSizePrinter : EditorWindow
    {
        [MenuItem("TreasureRoyale/Print Terrain Size")]
        static void Print()
        {
            var t = FindObjectOfType<Terrain>();
            if (t == null) { Debug.Log("[TR] No Terrain found in scene"); return; }
            var td = t.terrainData;
            Debug.Log($"[TR] Terrain pos={t.transform.position} size={td.size} heightmapRes={td.heightmapResolution}");
        }

        [MenuItem("TreasureRoyale/Center Terrain on Origin")]
        static void CenterTerrain()
        {
            var t = FindObjectOfType<Terrain>(true);
            if (t == null) { Debug.Log("[TR] No Terrain found — open Showcase_Map scene first"); return; }
            var size = t.terrainData.size;
            var newPos = new Vector3(-size.x / 2f, 0f, -size.z / 2f);
            Undo.RecordObject(t.transform, "Center Terrain");
            t.transform.position = newPos;
            EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
            Debug.Log($"[TR] Terrain '{t.name}' moved to {newPos} (size={size})");
        }

        [MenuItem("TreasureRoyale/Fix Ground Colliders")]
        static void FixGroundColliders()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) { Debug.LogError("[TR] Ground GO not found"); return; }
            var colliders = ground.GetComponentsInChildren<BoxCollider>();
            int count = 0;
            foreach (var c in colliders)
            {
                if (c.size.y < 0.01f)
                {
                    Undo.RecordObject(c, "Fix Ground Collider");
                    c.size = new Vector3(c.size.x, 0.1f, c.size.z);
                    c.center = new Vector3(c.center.x, -0.05f, c.center.z);
                    EditorUtility.SetDirty(c);
                    count++;
                }
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[TR] Fixed {count} ground colliders");
        }

        [MenuItem("TreasureRoyale/Fix Terrain Material")]
        static void FixTerrainMaterial()
        {
            var terrain = FindObjectOfType<Terrain>();
            if (terrain == null) { Debug.LogError("[TR] No Terrain found"); return; }
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/TreasureRoyale/Terrain_URP.mat");
            if (mat == null) { Debug.LogError("[TR] Terrain_URP.mat not found"); return; }
            Undo.RecordObject(terrain, "Fix Terrain Material");
            terrain.materialTemplate = mat;
            EditorUtility.SetDirty(terrain);
            EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
            Debug.Log("[TR] Terrain material set to Terrain_URP.mat");
        }

        [MenuItem("TreasureRoyale/Fix Ground Planes Material")]
        static void FixGroundPlanesMaterial()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) { Debug.LogError("[TR] Ground GO not found"); return; }
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("[TR] URP Lit shader not found"); return; }
            int count = 0;
            foreach (var r in ground.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string sn = mats[i].shader.name;
                    if (sn.StartsWith("Universal Render Pipeline/") || sn.StartsWith("Unlit/")) continue;
                    Undo.RecordObject(mats[i], "Fix Ground Material");
                    mats[i].shader = urpLit;
                    EditorUtility.SetDirty(mats[i]);
                    count++;
                }
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log($"[TR] Fixed {count} ground plane materials to URP Lit");
        }

        [MenuItem("TreasureRoyale/Fix Spawn CoopGameModes")]
        static void FixSpawnCoopModes()
        {
            var trGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            Kit_PvE_TreasureRoyale trMode = null;
            foreach (var g in trGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<Kit_PvE_TreasureRoyale>(p);
                if (a != null) { trMode = a; break; }
            }
            if (trMode == null) { Debug.LogError("[TR] Kit_PvE_TreasureRoyale asset not found"); return; }

            var spawns = FindObjectsOfType<Kit_PlayerSpawn>();
            int count = 0;
            foreach (var s in spawns)
            {
                if (s.coopGameModes.Contains(trMode)) continue;
                Undo.RecordObject(s, "Fix Spawn CoopModes");
                s.coopGameModes = s.coopGameModes.Append(trMode).ToArray();
                EditorUtility.SetDirty(s);
                count++;
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[TR] Fixed {count} spawn points");
        }
    }
}
#endif
