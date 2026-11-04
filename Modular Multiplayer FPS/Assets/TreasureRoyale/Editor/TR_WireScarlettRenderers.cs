using UnityEngine;
using UnityEditor;

namespace MarsFPSKit
{
    public static class TR_WireScarlettRenderers
    {
        [MenuItem("TreasureRoyale/Wire Scarlett Shadow Renderers")]
        static void Wire()
        {
            string[] wrapperPaths = {
                "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models/Player_ThirdPerson_Outfit4_Hairstyle1.prefab",
                "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models/Player_ThirdPerson_Outfit1_Hairstyle3.prefab",
                "Assets/314 Arts/MarsFPSKit/Prefabs/Player/Third Person Models/Player_ThirdPerson_Outfit5_Hairstyle1.prefab",
            };

            int total = 0;
            foreach (var path in wrapperPaths)
            {
                if (!System.IO.File.Exists(path)) { Debug.LogWarning($"[TR] File not found: {path}"); continue; }
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    // Log everything we find
                    var allComponents = root.GetComponentsInChildren<Component>(true);
                    Debug.Log($"[TR] {System.IO.Path.GetFileName(path)}: {allComponents.Length} components total");

                    var tpModel = root.GetComponentInChildren<Kit_ThirdPersonModernPlayerModel>(true);
                    if (tpModel == null)
                    {
                        Debug.LogWarning($"[TR] No Kit_ThirdPersonModernPlayerModel in {path} - checking all GOs:");
                        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                            Debug.Log($"  GO: {t.name}");
                        continue;
                    }
                    Debug.Log($"[TR] Found tpModel on: {tpModel.gameObject.name}, anim={tpModel.anim}, fpShadow count={tpModel.fpShadowOnlyRenderers?.Length}");

                    // Wire Animator
                    var animators = root.GetComponentsInChildren<Animator>(true);
                    Debug.Log($"[TR] Found {animators.Length} Animators");
                    if (animators.Length > 0)
                    {
                        tpModel.anim = animators[0];
                        Debug.Log($"[TR] Wired Animator: {animators[0].gameObject.name}");
                    }

                    // Wire all SkinnedMeshRenderers
                    var smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    Debug.Log($"[TR] Found {smrs.Length} SkinnedMeshRenderers");
                    if (smrs.Length > 0)
                    {
                        var renderers = new Renderer[smrs.Length];
                        for (int i = 0; i < smrs.Length; i++) renderers[i] = smrs[i];
                        tpModel.fpShadowOnlyRenderers = renderers;
                    }

                    // Wire weaponsInHandsGo
                    if (tpModel.weaponsInHandsGo == null)
                    {
                        Transform hand = FindBone(root.transform, "RightHand", "Hand_R", "hand_r", "R_Hand", "Bip01 R Hand", "mixamorig:RightHand");
                        if (hand != null)
                        {
                            tpModel.weaponsInHandsGo = hand;
                            Debug.Log($"[TR] Wired weaponsInHandsGo to: {hand.name}");
                        }
                        else
                        {
                            var placeholder = new GameObject("WeaponsInHands");
                            placeholder.transform.SetParent(root.transform, false);
                            tpModel.weaponsInHandsGo = placeholder.transform;
                            Debug.Log("[TR] Created WeaponsInHands placeholder");
                        }
                    }

                    // Wire soundFire
                    if (tpModel.soundFire == null)
                    {
                        var sources = root.GetComponentsInChildren<AudioSource>(true);
                        if (sources.Length > 0)
                            tpModel.soundFire = sources[0];
                        else
                        {
                            var audioGO = new GameObject("SoundFire");
                            audioGO.transform.SetParent(root.transform, false);
                            tpModel.soundFire = audioGO.AddComponent<AudioSource>();
                        }
                        Debug.Log($"[TR] Wired soundFire");
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    total++;
                    Debug.Log($"[TR] Saved {System.IO.Path.GetFileName(path)}");
                }
                catch (System.Exception e) { Debug.LogError($"[TR] Error processing {path}: {e.Message}"); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", $"Wired {total} Scarlett prefabs. Check console for details.", "OK");
        }

        static Transform FindBone(Transform root, params string[] names)
        {
            foreach (var name in names)
            {
                var found = FindInChildren(root, name);
                if (found != null) return found;
            }
            return null;
        }

        static Transform FindInChildren(Transform t, string name)
        {
            if (t.name.ToLower().Contains(name.ToLower())) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var result = FindInChildren(t.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
