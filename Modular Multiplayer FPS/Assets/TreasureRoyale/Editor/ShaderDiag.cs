using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class ShaderDiag
    {
        static readonly string[] KeepPrefixes = {
            "Universal Render Pipeline/",
            "Skybox/", "Sprites/", "UI/", "TextMeshPro/", "GUI/",
            "Unlit/", "Hidden/", "Shader Graphs/Ruins_URP",
        };

        static bool IsGood(string name)
        {
            foreach (var p in KeepPrefixes)
                if (name.StartsWith(p)) return true;
            return false;
        }

        [MenuItem("TreasureRoyale/Diagnose Available Shaders")]
        static void RunShaders()
        {
            string[] candidates = {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Unlit/Color",
                "Skybox/Procedural",
            };
            var sb = new System.Text.StringBuilder();
            foreach (var name in candidates)
            {
                var s = Shader.Find(name);
                bool ok = s != null && s.name != "Hidden/InternalErrorShader";
                sb.AppendLine("  " + (ok ? "OK" : "MISSING") + ": " + name);
            }
            Debug.Log("[TR] Shaders:\n" + sb);
        }

        [MenuItem("TreasureRoyale/Report All Non-URP Material Shaders")]
        static void ReportNonURP()
        {
            var counts = new Dictionary<string, int>();
            var examples = new Dictionary<string, string>();

            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/")) continue;
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;
                string sname = mat.shader.name;
                if (IsGood(sname)) continue;
                if (sname.StartsWith("TextMeshPro/") || sname.StartsWith("GUI/")) continue;

                if (!counts.ContainsKey(sname)) { counts[sname] = 0; examples[sname] = path; }
                counts[sname]++;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Non-URP materials (" + counts.Count + " unique shaders):");
            foreach (var kv in counts)
                sb.AppendLine("  [" + kv.Value + "x] " + kv.Key + "\n      e.g. " + examples[kv.Key]);
            Debug.Log("[TR] " + sb);
        }

        [MenuItem("TreasureRoyale/Fix ALL Non-URP Materials (Nuclear Option)")]
        static void NuclearFix()
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
                if (IsGood(mat.shader.name)) continue;
                if (mat.shader.name.StartsWith("TextMeshPro/") || mat.shader.name.StartsWith("GUI/")) continue;

                Texture albedo = null;
                if (mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");
                if (albedo == null && mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
                Color col = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                          : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                          : Color.white;

                string sn = mat.shader.name.ToLower();
                Shader target = urpLit;
                if (sn.Contains("particle") || sn.Contains("alpha") || sn.Contains("smoke") || sn.Contains("splash"))
                    target = urpParticle ?? urpLit;
                else if (sn.Contains("unlit"))
                    target = urpUnlit ?? urpLit;

                mat.shader = target;
                if (albedo != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);

                EditorUtility.SetDirty(mat);
                fixed_++;
                log.AppendLine("  " + System.IO.Path.GetFileName(path) + "  was: " + mat.shader.name);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[TR] Nuclear fix: " + fixed_ + " materials fixed.\n" + log);
        }
    }
}
