using UnityEngine;
using UnityEditor;
using System.IO;

namespace MarsFPSKit
{
    public static class TR_FixScarlettMaterials
    {
        [MenuItem("TreasureRoyale/Fix Scarlett Materials (Convert to URP)")]
        static void Fix()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("[TR] URP/Lit shader not found"); return; }

            // Search all Scarlett-related material folders
            string[] searchFolders = {
                "Assets/Scarlett Riley",
                "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett",
                "Assets/314 Arts/MarsFPSKit/Models/First Person Arms"
            };
            var matGuids = new System.Collections.Generic.List<string>();
            foreach (var folder in searchFolders)
                matGuids.AddRange(AssetDatabase.FindAssets("t:Material", new[] { folder }));

            int fixed_ = 0;
            foreach (var guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;
                string sn = mat.shader.name;
                if (sn.StartsWith("Universal Render Pipeline/")) continue;

                Texture albedo = null;
                Color col = Color.white;
                if (mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
                if (mat.HasProperty("_Color")) col = mat.GetColor("_Color");
                Texture normalMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                Texture metallicGloss = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;

                mat.shader = urpLit;
                if (albedo != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                if (normalMap != null) mat.SetTexture("_BumpMap", normalMap);
                if (metallicGloss != null) mat.SetTexture("_MetallicGlossMap", metallicGloss);

                EditorUtility.SetDirty(mat);
                fixed_++;
                Debug.Log($"[TR] Fixed: {mat.name} was={sn}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TR] Fixed {fixed_} Scarlett/FP-arms materials");
            EditorUtility.DisplayDialog("Done", $"Converted {fixed_} materials to URP/Lit.", "OK");
        }
    }
}
