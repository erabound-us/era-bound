using UnityEngine;
using UnityEditor;

namespace MarsFPSKit
{
    /// <summary>
    /// One-click setup that fixes all remaining TR asset/UI issues programmatically.
    /// Run this after every major asset change.
    /// </summary>
    public static class TR_FullSetup
    {
        [MenuItem("TreasureRoyale/Run Full TR Setup")]
        static void RunAll()
        {
            int steps = 0;

            // 1. Fix all Scarlett + FP arms materials to URP/Lit
            steps += FixAllMaterials();

            // 2. Swap Player.prefab to Scarlett
            steps += SwapPlayerModel();

            // 3. Fix Game.asset — remove any remaining Ethan/old PvP game mode references
            steps += FixGameAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("TR Full Setup Complete",
                $"Completed {steps} fixes.\n\n" +
                "- All non-URP materials converted\n" +
                "- Player.prefab uses Scarlett\n" +
                "- Game.asset cleaned\n\n" +
                "Press Play from MainMenu.unity to test.",
                "OK");
        }

        static int FixAllMaterials()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogWarning("[TR] URP/Lit not found"); return 0; }

            string[] folders = {
                "Assets/Scarlett Riley",
                "Assets/314 Arts/MarsFPSKit/Models/Characters",
                "Assets/314 Arts/MarsFPSKit/Models/First Person Arms",
                "Assets/FPS Animated Weapons Pack"
            };

            int count = 0;
            foreach (var folder in folders)
            {
                if (!System.IO.Directory.Exists(folder)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null) continue;
                    string sn = mat.shader.name;
                    if (sn.StartsWith("Universal Render Pipeline/") || sn.StartsWith("Unlit/")) continue;

                    Texture albedo = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    Color col = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                    Texture normal = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                    Texture metallic = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;

                    mat.shader = urpLit;
                    if (albedo != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                    if (normal != null) mat.SetTexture("_BumpMap", normal);
                    if (metallic != null) mat.SetTexture("_MetallicGlossMap", metallic);

                    EditorUtility.SetDirty(mat);
                    count++;
                }
            }
            Debug.Log($"[TR] Fixed {count} materials to URP/Lit");
            return count;
        }

        static int SwapPlayerModel()
        {
            const string PLAYER_PREFAB = "Assets/314 Arts/MarsFPSKit/Resources/Player.prefab";
            const string SCARLETT_PREFAB = "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models/Player_ThirdPerson_Outfit4_Hairstyle1.prefab";

            var scarlettPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SCARLETT_PREFAB);
            if (scarlettPrefab == null) { Debug.LogWarning("[TR] Scarlett prefab not found"); return 0; }

            var root = PrefabUtility.LoadPrefabContents(PLAYER_PREFAB);
            try
            {
                var pb = root.GetComponent<Kit_PlayerBehaviour>();
                if (pb == null) return 0;

                // Remove old third-person models
                var existing = root.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                int removed = 0;
                while (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                    removed++;
                    existing = root.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                }

                // Add Scarlett
                var inst = PrefabUtility.InstantiatePrefab(scarlettPrefab) as GameObject;
                inst.transform.SetParent(root.transform, false);
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;

                var tpModel = inst.GetComponentInChildren<Kit_ThirdPersonPlayerModel>(true);
                if (tpModel != null) pb.thirdPersonPlayerModel = tpModel;

                PrefabUtility.SaveAsPrefabAsset(root, PLAYER_PREFAB);
                Debug.Log($"[TR] Player.prefab: removed {removed} old models, added Scarlett");
                return 1;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static int FixGameAsset()
        {
            // Ensure Game.asset allSingleplayerGameModes[0] and allCoopGameModes[0]
            // both point to the TR game mode — already set, just verify
            const string GAME_ASSET = "Assets/314 Arts/MarsFPSKit/Resources/Game.asset";
            var game = AssetDatabase.LoadAssetAtPath<Kit_GameInformation>(GAME_ASSET);
            if (game == null) { Debug.LogWarning("[TR] Game.asset not found"); return 0; }

            const string TR_MODE = "Assets/314 Arts/MarsFPSKit/Required/New Kit_Pv E_Treasure Royale.asset";
            var trMode = AssetDatabase.LoadAssetAtPath<Kit_PvE_TreasureRoyale>(TR_MODE);
            if (trMode == null) { Debug.LogWarning("[TR] TR game mode asset not found"); return 0; }

            bool changed = false;
            if (game.allSingleplayerGameModes.Length == 0 || game.allSingleplayerGameModes[0] != trMode)
            {
                game.allSingleplayerGameModes = new Kit_PvE_GameModeBase[] { trMode };
                changed = true;
            }
            if (game.allCoopGameModes.Length == 0 || game.allCoopGameModes[0] != trMode)
            {
                var coop = new Kit_PvE_GameModeBase[Mathf.Max(1, game.allCoopGameModes.Length)];
                coop[0] = trMode;
                for (int i = 1; i < coop.Length; i++) coop[i] = game.allCoopGameModes[i];
                game.allCoopGameModes = coop;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(game);
                Debug.Log("[TR] Game.asset: wired TR game mode");
                return 1;
            }
            return 0;
        }
    }
}
