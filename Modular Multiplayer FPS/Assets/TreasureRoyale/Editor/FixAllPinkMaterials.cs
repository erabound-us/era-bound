using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class FixAllPinkMaterials
    {
        static readonly HashSet<string> BadShaders = new HashSet<string>
        {
            "Standard",
            "Standard (Specular setup)",
            "Custom/SurfaceShader_VC",
            "Custom/SimpleGrabPassBlur",
            "Custom/MaskedUIBlur",
            "Double sided",
            "Hidden/InternalErrorShader",
        };

        [MenuItem("TreasureRoyale/Fix ALL Pink Materials (Project-Wide)")]
        static void Run()
        {
            var urpLit      = Shader.Find("Universal Render Pipeline/Lit");
            var urpUnlit    = Shader.Find("Universal Render Pipeline/Unlit");
            var urpParticle = Shader.Find("Universal Render Pipeline/Particles/Lit");

            if (urpLit == null) { Debug.LogError("[TR] URP Lit not found."); return; }

            int fixed_ = 0;
            var log = new System.Text.StringBuilder();

            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/")) continue;
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;
                if (!BadShaders.Contains(mat.shader.name)) continue;

                Texture albedo = null;
                if (mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");
                if (albedo == null && mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
                Color col = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                          : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                          : Color.white;

                string sn = mat.shader.name.ToLower();
                Shader target = urpLit;
                if (sn.Contains("particle") || sn.Contains("alpha") || sn.Contains("splash"))
                    target = urpParticle ?? urpLit;
                else if (sn.Contains("blur") || sn.Contains("unlit"))
                    target = urpUnlit ?? urpLit;

                mat.shader = target;
                if (albedo != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);

                EditorUtility.SetDirty(mat);
                fixed_++;
                log.AppendLine("  " + System.IO.Path.GetFileName(path));
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[TR] Fixed " + fixed_ + " pink materials:\n" + log);
        }
    }
}
