using UnityEngine;
using UnityEditor;

namespace MarsFPSKit
{
    static class FixPlayerModel
    {
        [MenuItem("TreasureRoyale/Fix Player Model Prefab Reference")]
        static void Fix()
        {
            string infoPath   = "Assets/314 Arts/MarsFPSKit/GameData/Player/Player Models/Player_ThirdPerson_Outfit1_Hairstyle3_Info.asset";
            string prefabPath = "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Prefabs/Outfit1/Outfit1_Hairstyle3.prefab";
            string gameModeAssetPath = "Assets/314 Arts/MarsFPSKit/Required/New Kit_Pv E_Treasure Royale.asset";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogError("[TR] Prefab not found: " + prefabPath); return; }

            var info = AssetDatabase.LoadAssetAtPath<Kit_PlayerModelInformation>(infoPath);
            if (info == null) { Debug.LogError("[TR] Info asset not found: " + infoPath); return; }

            // Fix prefab on info asset
            if (info.prefab != prefab)
            {
                info.prefab = prefab;
                EditorUtility.SetDirty(info);
                Debug.Log("[TR] Fixed player model prefab -> " + prefab.name);
            }
            else
            {
                Debug.Log("[TR] Player model prefab already correct: " + prefab.name);
            }

            // Also verify the game mode asset spawnPlayerModels references the info asset
            var gm = AssetDatabase.LoadAssetAtPath<Kit_PvE_TreasureRoyale>(gameModeAssetPath);
            if (gm == null) { Debug.LogError("[TR] Game mode asset not found."); return; }

            Debug.Log("[TR] spawnPlayerModels count: " + gm.spawnPlayerModels.Length);
            for (int i = 0; i < gm.spawnPlayerModels.Length; i++)
            {
                var pm = gm.spawnPlayerModels[i];
                Debug.Log("[TR] spawnPlayerModels[" + i + "] information=" + (pm.information == null ? "NULL" : pm.information.name) +
                          " prefab=" + (pm.information?.prefab == null ? "NULL" : pm.information.prefab.name));
                if (pm.information == null)
                {
                    gm.spawnPlayerModels[i] = new PlayerModelConfig { information = info, customization = pm.customization };
                    EditorUtility.SetDirty(gm);
                    Debug.Log("[TR] Fixed null information reference in spawnPlayerModels[" + i + "]");
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
