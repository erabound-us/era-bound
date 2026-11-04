using UnityEngine;
using UnityEditor;

namespace MarsFPSKit
{
    public static class TR_SwapPlayerModel
    {
        [MenuItem("TreasureRoyale/Swap Player Model to Scarlett")]
        static void Swap()
        {
            const string PLAYER_PREFAB = "Assets/314 Arts/MarsFPSKit/Resources/Player.prefab";
            const string SCARLETT_PREFAB = "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models/Player_ThirdPerson_Outfit4_Hairstyle1.prefab";

            var scarlettPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SCARLETT_PREFAB);
            if (scarlettPrefab == null) { Debug.LogError("[TR] Scarlett prefab not found"); return; }

            // --- Fix Player.prefab ---
            var root = PrefabUtility.LoadPrefabContents(PLAYER_PREFAB);
            try
            {
                var pb = root.GetComponent<Kit_PlayerBehaviour>();
                if (pb == null) { Debug.LogError("[TR] Kit_PlayerBehaviour not found"); return; }

                var existing = root.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                int removed = 0;
                while (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                    removed++;
                    existing = root.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                }
                Debug.Log($"[TR] Removed {removed} old third-person model(s)");

                var scarlettInstance = PrefabUtility.InstantiatePrefab(scarlettPrefab) as GameObject;
                scarlettInstance.transform.SetParent(root.transform, false);
                scarlettInstance.transform.localPosition = Vector3.zero;
                scarlettInstance.transform.localRotation = Quaternion.identity;
                scarlettInstance.transform.localScale = Vector3.one;

                var tpModel = scarlettInstance.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                if (tpModel != null)
                {
                    pb.thirdPersonPlayerModel = tpModel;
                    Debug.Log($"[TR] Wired thirdPersonPlayerModel to {scarlettInstance.name}");
                }

                PrefabUtility.SaveAsPrefabAsset(root, PLAYER_PREFAB);
                Debug.Log("[TR] Player.prefab saved with Scarlett");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }

            // Note: LoadoutScreen.prefab still has a broken Ethan reference but Kit_LoadoutNew
            // is now guarded against null prefabs so it won't crash.
            // We do NOT delete LoadoutScreen.prefab as MarsFPSKit_IngamePrefab references it.

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done",
                "Scarlett is now the player model in Player.prefab.", "OK");
        }
    }
}
