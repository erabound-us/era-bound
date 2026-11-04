using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class FixScarlettMaterials
    {
        const string SCARLETT = "Assets/314 Arts/MarsFPSKit/Models/Characters/Scarlett";

        static readonly Dictionary<string, string> TexMap = new Dictionary<string, string>
        {
            { "scarlett_body",              "Scarlett_Body.tga" },
            { "scarlett_face",              "Scarlett_Face_Style1.tga" },
            { "scarlett_face_style1",       "Scarlett_Face_Style1.tga" },
            { "scarlett_face_style2",       "Scarlett_Face_Style2.tga" },
            { "scarlett_face_style3",       "Scarlett_Face_Style3.tga" },
            { "scarlett_face_style4",       "Scarlett_Face_Style4.tga" },
            { "accessories",                "Accessories_Diffuse_Black.tga" },
            { "clothes_outfit1",            "Outfit1-5_Diffuse_Red.tga" },
            { "clothes_outfit2",            "Outfit2-3_Diffuse_Black.tga" },
            { "clothes_outfit4",            "Outfit4_Diffuse_Black.tga" },
            { "hairstyle3",                 "Hairstyle3_Red.tga" },
            { "hairstyle3_alpha",           "Hairstyle3_Red.tga" },
            { "eyes",                       "Eye_Brown.tga" },
            { "outfit1_black",              "Outfit1-5_Diffuse_Black.tga" },
            { "outfit1_orange",             "Outfit1-5_Diffuse_Orange.tga" },
            { "outfit1_pink",               "Outfit1-5_Diffuse_Pink.tga" },
            { "outfit1_red",                "Outfit1-5_Diffuse_Red.tga" },
            { "outfit1_silver",             "Outfit1-5_Diffuse_Silver.tga" },
            { "outfit1_white",              "Outfit1-5_Diffuse_White.tga" },
            { "outfit1_yellow",             "Outfit1-5_Diffuse_Yellow.tga" },
            { "outfit2-3_black",            "Outfit2-3_Diffuse_Black.tga" },
            { "outfit2-3_orange",           "Outfit2-3_Diffuse_Orange.tga" },
            { "outfit2-3_pink",             "Outfit2-3_Diffuse_Pink.tga" },
            { "outfit2-3_red",              "Outfit2-3_Diffuse_Red.tga" },
            { "outfit2-3_silver",           "Outfit2-3_Diffuse_Silver.tga" },
            { "outfit2-3_white",            "Outfit2-3_Diffuse_White.tga" },
            { "outfit2-3_yellow",           "Outfit2-3_Diffuse_Yellow.tga" },
            { "outfit4_black",              "Outfit4_Diffuse_Black.tga" },
            { "outfit4_orange",             "Outfit4_Diffuse_Orange.tga" },
            { "outfit4_pink",               "Outfit4_Diffuse_Pink.tga" },
            { "outfit4_red",                "Outfit4_Diffuse_Red.tga" },
            { "outfit4_silver",             "Outfit4_Diffuse_Silver.tga" },
            { "outfit4_white",              "Outfit4_Diffuse_White.tga" },
            { "outfit4_yellow",             "Outfit4_Diffuse_Yellow.tga" },
            { "accessories_black 1",        "Accessories_Diffuse_Black.tga" },
            { "accessories_orange",         "Accessories_Diffuse_Orange.tga" },
            { "accessories_pink",           "Accessories_Diffuse_Pink.tga" },
            { "accessories_red",            "Accessories_Diffuse_Red.tga" },
            { "accessories_silver",         "Accessories_Diffuse_Silver.tga" },
            { "accessories_white",          "Accessories_Diffuse_White.tga" },
            { "accessories_yellow",         "Accessories_Diffuse_Yellow.tga" },
            { "raygun_black",               "Raygun_Black_Diffuse.png" },
            { "raygun_orange",              "Raygun_Orange_Diffuse.png" },
            { "raygun_pink",                "Raygun_Pink_Diffuse.png" },
            { "raygun_red",                 "Raygun_Red_Diffuse.png" },
            { "raygun_silver",              "Raygun_Silver_Diffuse.png" },
            { "raygun_white",               "Raygun_White_Diffuse.png" },
            { "raygun_yellow",              "Raygun_Yellow_Diffuse.png" },
            { "hairstyle1_black 1",         "Hairstyle1_Black.tga" },
            { "hairstyle1_blue",            "Hairstyle1_Blue.tga" },
            { "hairstyle1_darkbrown",       "Hairstyle1_DarkBrown.tga" },
            { "hairstyle1_goldenblond",     "Hairstyle1_GoldenBlond.tga" },
            { "hairstyle1_lightblond",      "Hairstyle1_LightBlond.tga" },
            { "hairstyle1_lightbrown",      "Hairstyle1_LightBrown.tga" },
            { "hairstyle1_pink",            "Hairstyle1_Pink.tga" },
            { "hairstyle1_red",             "Hairstyle1_Red.tga" },
            { "hairstyle1_silver",          "Hairstyle1_Silver.tga" },
            { "hairstyle2_black",           "Hairstyle2_Black.tga" },
            { "hairstyle2_blue",            "Hairstyle2_Blue.tga" },
            { "hairstyle2_darkblond",       "Hairstyle2_DarkBlond.tga" },
            { "hairstyle2_darkbrown",       "Hairstyle2_DarkBrown.tga" },
            { "hairstyle2_lightblond",      "Hairstyle2_LightBlond.tga" },
            { "hairstyle2_lightbrown",      "Hairstyle2_LightBrown.tga" },
            { "hairstyle2_pink",            "Hairstyle2_Pink.tga" },
            { "hairstyle2_red",             "Hairstyle2_Red.tga" },
            { "hairstyle2_silver",          "Hairstyle2_Silver.tga" },
            { "hairstyle3_black",           "Hairstyle3_Black.tga" },
            { "hairstyle3_blue",            "Hairstyle3_Blue.tga" },
            { "hairstyle3_darkblond",       "Hairstyle3_DarkBlond.tga" },
            { "hairstyle3_darkbrown",       "Hairstyle3_DarkBrown.tga" },
            { "hairstyle3_lightblond",      "Hairstyle3_LightBlond.tga" },
            { "hairstyle3_lightbrown",      "Hairstyle3_LightBrown.tga" },
            { "hairstyle3_pink",            "Hairstyle3_Pink.tga" },
            { "hairstyle3_red",             "Hairstyle3_Red.tga" },
            { "hairstyle3_silver",          "Hairstyle3_Silver.tga" },
            { "hairstyle3_alpha_black",     "Hairstyle3_Black.tga" },
            { "hairstyle3_alpha_blue",      "Hairstyle3_Blue.tga" },
            { "hairstyle3_alpha_darkblond", "Hairstyle3_DarkBlond.tga" },
            { "hairstyle3_alpha_darkbrown", "Hairstyle3_DarkBrown.tga" },
            { "hairstyle3_alpha_lightblond","Hairstyle3_LightBlond.tga" },
            { "hairstyle3_alpha_lightbrown","Hairstyle3_LightBrown.tga" },
            { "hairstyle3_alpha_pink",      "Hairstyle3_Pink.tga" },
            { "hairstyle3_alpha_red",       "Hairstyle3_Red.tga" },
            { "hairstyle3_alpha_silver",    "Hairstyle3_Silver.tga" },
            { "eye_brown",                  "Eye_Brown.tga" },
            { "eye_darkblue",               "Eye_DarkBlue.tga" },
            { "eye_gold",                   "Eye_Gold.tga" },
            { "eye_graphite",               "Eye_Graphite.tga" },
            { "eye_green",                  "Eye_Green.tga" },
            { "eye_lightblue",              "Eye_LightBlue.tga" },
            { "eye_lightbrown",             "Eye_LightBrown.tga" },
            { "eye_silver",                 "Eye_Silver.tga" },
            { "eye_violet",                 "Eyes_Violet.tga" },
            { "communicator",               "Communicator_Diffuse.tga" },
            { "detector",                   "Detector_Diffuse.tga" },
        };

        [MenuItem("TreasureRoyale/Fix Scarlett Materials (Wire Textures)")]
        static void Run()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("[TR] URP Lit shader not found."); return; }

            var texByFile = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SCARLETT }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = Path.GetFileName(path).ToLower();
                if (!texByFile.ContainsKey(file)) texByFile[file] = path;
            }

            int shaderFixed = 0, texFixed = 0, skipped = 0;

            foreach (var matGuid in AssetDatabase.FindAssets("t:Material", new[] { SCARLETT }))
            {
                string matPath = AssetDatabase.GUIDToAssetPath(matGuid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null) continue;

                bool dirty = false;

                // Fix shader if not URP Lit
                if (mat.shader != urpLit)
                {
                    Texture existingTex = null;
                    if (mat.HasProperty("_BaseMap")) existingTex = mat.GetTexture("_BaseMap");
                    if (existingTex == null && mat.HasProperty("_MainTex")) existingTex = mat.GetTexture("_MainTex");
                    mat.shader = urpLit;
                    shaderFixed++;
                    dirty = true;
                    if (existingTex != null) { mat.SetTexture("_BaseMap", existingTex); mat.SetTexture("_MainTex", existingTex); }
                }

                // Only fix _BaseMap if it is strictly null — never overwrite existing textures
                if (!mat.HasProperty("_BaseMap") || mat.GetTexture("_BaseMap") != null)
                {
                    if (dirty) EditorUtility.SetDirty(mat);
                    continue;
                }

                string key = mat.name.ToLower();
                if (TexMap.TryGetValue(key, out string texFile) &&
                    texByFile.TryGetValue(texFile.ToLower(), out string texPath))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    if (tex != null)
                    {
                        mat.SetTexture("_BaseMap", tex);
                        mat.SetTexture("_MainTex", tex);
                        texFixed++;
                        dirty = true;
                        Debug.Log($"[TR] {mat.name} -> {tex.name}");
                    }
                }
                else skipped++;

                if (dirty) EditorUtility.SetDirty(mat);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[TR] Done. Shader fixes: {shaderFixed}, Texture fixes: {texFixed}, Skipped: {skipped}");
        }
    }
}
