using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace MarsFPSKit
{
    static class FixFBXMaterials
    {
        static readonly string[] ScanFolders = {
            "Assets/FencesPack",
            "Assets/AncientRuins",
            "Assets/Abandoned buildings",
            "Assets/EmbersStorm – Mediterranean Ruins Building Kit",
            "Assets/LowPolyDungeonsLite",
            "Assets/LowPolyMedievalStarterPack",
            "Assets/Medieval Bridge",
            "Assets/Mesh",
            "Assets/PolyRonin",
            "Assets/Symphonie",
            "Assets/A_piece_of_nature",
            "Assets/lowpoly-nature",
            "Assets/FPS Animated Weapons Pack",
            "Assets/Medieval Melee Weapon Pack",
            "Assets/Scarlett Riley",
            "Assets/314 Arts/MarsFPSKit/Models",
        };

        [MenuItem("TreasureRoyale/Fix FBX Embedded Materials (Extract + Upgrade)")]
        static void Run()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("[TR] URP Lit not found."); return; }

            int extracted = 0, upgraded = 0;

            foreach (var folder in ScanFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;

                foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
                {
                    string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                    if (importer == null) continue;

                    string dir = Path.GetDirectoryName(fbxPath) + "/Materials";
                    if (!AssetDatabase.IsValidFolder(dir))
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(fbxPath), "Materials");

                    importer.ExtractTextures(dir);

                    var objs = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                    foreach (var obj in objs)
                    {
                        if (obj is Material mat)
                        {
                            string matPath = dir + "/" + mat.name + ".mat";
                            if (!File.Exists(Path.GetFullPath(matPath)))
                            {
                                AssetDatabase.CreateAsset(new Material(mat), matPath);
                                extracted++;
                            }
                        }
                    }
                }
            }

            AssetDatabase.Refresh();

            foreach (var folder in ScanFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null || mat.shader == null) continue;
                    if (mat.shader.name.StartsWith("Universal Render Pipeline/")) continue;
                    if (mat.shader.name.StartsWith("TextMeshPro/")) continue;
                    if (mat.shader.name.StartsWith("GUI/")) continue;

                    Texture albedo = null;
                    if (mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");
                    if (albedo == null && mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
                    Color col = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                              : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                              : Color.white;

                    mat.shader = urpLit;
                    if (albedo != null) { mat.SetTexture("_BaseMap", albedo); mat.SetTexture("_MainTex", albedo); }
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                    EditorUtility.SetDirty(mat);
                    upgraded++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[TR] FBX materials: " + extracted + " extracted, " + upgraded + " upgraded to URP.");
        }
    }
}
