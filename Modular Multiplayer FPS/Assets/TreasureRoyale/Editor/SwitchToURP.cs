using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class SwitchToURP
    {
        // Shader guids that are NOT URP-compatible
        static readonly HashSet<string> BadShaderGuids = new HashSet<string>
        {
            "0000000000000000f000000000000000", // Standard
            "0000000000000000e000000000000000", // Standard (Specular)
            "0e9d5a909a1f7e84882a534d0d11e49f", // HDRP Lit
            "da692e001514ec24dbc4cca1949ff7e8", // HDRP/Lit (another variant)
            "5f3a3f2429ec0be4e91ca14c10b9aee2", // HDRenderPipelineAsset shader
        };

        [MenuItem("TreasureRoyale/Fix Render Pipeline (Switch to URP)")]
        static void Run()
        {
            // 1. Find and set URP pipeline asset
            string[] candidates = {
                "Assets/HDRPDefaultResources/New Universal Render Pipeline Asset.asset",
                "Assets/TreasureRoyale/URPAsset.asset"
            };

            RenderPipelineAsset urpAsset = null;
            string urpPath = null;
            foreach (var c in candidates)
            {
                var a = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(c);
                if (a != null) { urpAsset = a; urpPath = c; break; }
            }

            if (urpAsset == null)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:RenderPipelineAsset"))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p.ToLower().Contains("hdrp") || p.ToLower().Contains("hdrender")) continue;
                    urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(p);
                    if (urpAsset != null) { urpPath = p; break; }
                }
            }

            if (urpAsset == null) { Debug.LogError("[TR] Could not find URP pipeline asset."); return; }

            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
            EditorUtility.SetDirty(urpAsset);
            AssetDatabase.SaveAssets();
            Debug.Log("[TR] Set URP pipeline: " + urpPath);

            // 2. Upgrade all materials directly
            UpgradeAllMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TR] Done. Press Play now.");
        }

        [MenuItem("TreasureRoyale/Upgrade All Materials to URP")]
        static void UpgradeAllMaterials()
        {
            var urpLit        = Shader.Find("Universal Render Pipeline/Lit");
            var urpUnlit      = Shader.Find("Universal Render Pipeline/Unlit");
            var urpParticles  = Shader.Find("Universal Render Pipeline/Particles/Lit");
            var urpSimpleLit  = Shader.Find("Universal Render Pipeline/Simple Lit");

            if (urpLit == null)
            {
                Debug.LogError("[TR] URP Lit shader not found — is URP installed?");
                return;
            }

            int fixed_ = 0, skipped = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;

                // Get the shader's asset guid
                string shaderPath = AssetDatabase.GetAssetPath(mat.shader);
                string shaderGuid = AssetDatabase.AssetPathToGUID(shaderPath);
                string shaderName = mat.shader.name.ToLower();

                bool isBad = BadShaderGuids.Contains(shaderGuid)
                    || shaderName == "standard"
                    || shaderName == "standard (specular setup)"
                    || shaderName.StartsWith("hdrp/")
                    || shaderName.StartsWith("hdrenderpipeline/")
                    || mat.shader.name == "Hidden/InternalErrorShader";

                if (!isBad) continue;

                // Preserve key textures before switching shader
                Texture albedo   = GetTex(mat, "_BaseMap", "_MainTex");
                Texture normal   = GetTex(mat, "_BumpMap");
                Texture metallic = GetTex(mat, "_MetallicGlossMap");
                Texture occlusion= GetTex(mat, "_OcclusionMap");
                Texture emission = GetTex(mat, "_EmissionMap");
                Color   baseColor= mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                                 : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                                 : Color.white;
                float   metalVal = mat.HasProperty("_Metallic")  ? mat.GetFloat("_Metallic")  : 0f;
                float   smoothVal= mat.HasProperty("_Glossiness")? mat.GetFloat("_Glossiness"): 0.5f;

                // Pick target shader
                Shader target = urpLit;
                if (shaderName.Contains("particle") && urpParticles != null) target = urpParticles;
                else if (shaderName.Contains("unlit") && urpUnlit != null)   target = urpUnlit;

                mat.shader = target;

                // Re-apply properties
                if (albedo    != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                if (normal    != null)   mat.SetTexture("_BumpMap",          normal);
                if (metallic  != null)   mat.SetTexture("_MetallicGlossMap", metallic);
                if (occlusion != null)   mat.SetTexture("_OcclusionMap",     occlusion);
                if (emission  != null)   mat.SetTexture("_EmissionMap",      emission);
                mat.SetColor("_BaseColor", baseColor);
                if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   metalVal);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothVal);

                EditorUtility.SetDirty(mat);
                fixed_++;
            }

            Debug.Log($"[TR] Upgraded {fixed_} materials to URP. Skipped (already URP): {skipped}.");
        }

        static Texture GetTex(Material mat, params string[] props)
        {
            foreach (var p in props)
                if (mat.HasProperty(p)) { var t = mat.GetTexture(p); if (t != null) return t; }
            return null;
        }
    }
}
