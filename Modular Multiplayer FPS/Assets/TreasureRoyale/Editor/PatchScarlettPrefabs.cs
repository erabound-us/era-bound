using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class PatchScarlettPrefabs
    {
        // Dead guid -> real material file guid
        // Dead guids found in prefab SkinnedMeshRenderers that don't exist anywhere
        static readonly Dictionary<string, string> GuidRemap = new Dictionary<string, string>
        {
            // Hair mesh slot 0 (Hairstyle3 opaque base)
            { "c5d28d83843268b4cbda24ad9a70e82b", "25d98d547ba9e3649a02284740cc872b" },
            // Hair mesh slot 1 (Hairstyle3_Alpha transparent layer)
            { "32920ce7903a9e4479afea91de9cf2d0", "899125f13ae395948a7ba75319586fef" },
            // NEWOutfit1 mesh: Clothes_Outfit1, Scarlett_Body, Scarlett_Face
            { "81ab01a18b72cc64282e68abccd75b84", "b4a4c732101821d4b982f3296bb3a94d" },
            { "8cdcedb06e5c8694f8286b58a52ce83c", "49108e6edeaf93f4199c49c51b960bb2" },
            { "f8b9333377b6cf14d82cc5d258c685b3", "2e12945846e5fd643a3d1cc937029907" },
            // Eyes mesh
            { "f34f99a1c44b20f4388dfcfd98342563", "3eb5271c27ba71d4085c304135e4b093" },
            // Accessories meshes (backpack, holster, belt)
            { "c5506c438272d8d4087ff555a68b5ba7", "17200e769b8f2a74f84f8ad8da756533" },
            // Broken texture guid anywhere in prefabs
            { "c283655c45e5ac44d85d8af5e7e151a7", "c6b3eda889fbf6a4c91ed4b56bf0d31d" },
        };

        [MenuItem("TreasureRoyale/Patch Scarlett Prefabs (Fix Dead Material Guids)")]
        static void Run()
        {
            string prefabRoot = "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett/Prefabs";
            int filesPatched = 0, replacements = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { prefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fullPath = Path.GetFullPath(path);
                string content = File.ReadAllText(fullPath);
                string original = content;

                foreach (var kv in GuidRemap)
                    content = content.Replace(kv.Key, kv.Value);

                if (content != original)
                {
                    File.WriteAllText(fullPath, content);
                    int count = 0;
                    foreach (var kv in GuidRemap)
                    {
                        int idx = 0;
                        while ((idx = original.IndexOf(kv.Key, idx)) >= 0) { count++; idx++; }
                    }
                    replacements += count;
                    filesPatched++;
                    Debug.Log($"[TR] Patched: {Path.GetFileName(path)}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TR] Patched {filesPatched} prefabs, {replacements} guid replacements.");
        }
    }
}
