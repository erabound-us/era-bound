using UnityEngine;
using UnityEditor;
using System.IO;

namespace MarsFPSKit
{
    static class FixScarlettPrefabComponents
    {
        const string PrefabRoot = "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models";
        const string FpaPrefabPath = "Assets/314 Arts/MarsFPSKit/Prefabs/Arms/FPA_HumanColor.prefab";

        [MenuItem("TreasureRoyale/Fix Scarlett Prefab Components")]
        static void Run()
        {
            var fpaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FpaPrefabPath);
            if (fpaPrefab == null) { Debug.LogError("[TR] FPA prefab not found: " + FpaPrefabPath); return; }

            int fixed_ = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fname = Path.GetFileNameWithoutExtension(path).ToLower();
                if (!fname.Contains("outfit") && !fname.Contains("scarlett")) continue;

                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;

                    // Remove blank components from previous attempts
                    foreach (var stale in root.GetComponentsInChildren<Kit_ThirdPersonModernPlayerModel>(true))
                        if (stale.anim == null) Object.DestroyImmediate(stale);

                    var pm = root.GetComponentInChildren<Kit_ThirdPersonModernPlayerModel>(true);
                    if (pm == null) { Debug.LogWarning("[TR] No Kit_ThirdPersonModernPlayerModel on " + fname); continue; }

                    bool dirty = false;

                    if (pm.anim == null)
                    {
                        pm.anim = root.GetComponentInChildren<Animator>(true);
                        dirty = true;
                    }

                    if (pm.firstPersonArmsPrefab == null)
                    {
                        pm.firstPersonArmsPrefab = fpaPrefab;
                        dirty = true;
                        Debug.Log("[TR] Wired firstPersonArmsPrefab on " + fname);
                    }

                    if (pm.anim != null)
                    {
                        var relay = pm.anim.GetComponent<Kit_ThirdPersonModernIKRelay>();
                        if (relay == null) relay = pm.anim.gameObject.AddComponent<Kit_ThirdPersonModernIKRelay>();
                        if (relay.pm != pm) { relay.pm = pm; dirty = true; }
                    }

                    if (dirty) fixed_++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[TR] FixScarlettPrefabComponents: fixed {fixed_} prefabs.");
        }
    }
}
