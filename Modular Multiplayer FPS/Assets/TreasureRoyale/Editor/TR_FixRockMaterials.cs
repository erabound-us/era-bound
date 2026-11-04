using UnityEngine;
using UnityEditor;

namespace MarsFPSKit
{
    public class TR_FixRockMaterials
    {
        [MenuItem("TreasureRoyale/Fix Rock Materials (URP)")]
        static void Fix()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) { Debug.LogError("URP/Lit shader not found"); return; }

            var searchFolders = new[] {
                "Assets/Rocks and Boulders 2",
                "Assets/AncientRuins",
                "Assets/LowPolyMedievalStarterPack",
                "Assets/Symphonie",
                "Assets/EmbersStorm - Mediterranean Ruins Building Kit",
                "Assets/A_piece_of_nature",
                "Assets/lowpoly-nature",
                "Assets/Medieval Bridge",
                "Assets/LowPolyDungeonsLite",
                "Assets/FencesPack",
                "Assets/PolyRonin",
                "Assets/Mesh",
            };

            int fixed_ = 0;
            foreach (var folder in searchFolders)
            {
                string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null) continue;

                    bool dirty = false;

                    // Switch non-URP shaders to URP/Lit
                    if (mat.shader != urpLit && !mat.shader.name.StartsWith("Universal Render Pipeline"))
                    {
                        mat.shader = urpLit;
                        dirty = true;
                    }

                    // After shader switch (or if already URP/Lit), migrate _MainTex -> _BaseMap if _BaseMap is empty
                    if (mat.shader == urpLit)
                    {
                        Texture baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
                        if (baseMap == null)
                        {
                            // Try to recover texture from other known slots
                            Texture tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                            if (tex == null) tex = mat.HasProperty("_MainTex2") ? mat.GetTexture("_MainTex2") : null;
                            if (tex == null) tex = mat.HasProperty("_DetailAlbedoMap") ? mat.GetTexture("_DetailAlbedoMap") : null;
                            if (tex != null)
                            {
                                mat.SetTexture("_BaseMap", tex);
                                dirty = true;
                            }
                        }

                        // Ensure non-shiny rock appearance
                        if (mat.HasProperty("_Smoothness") && mat.GetFloat("_Smoothness") > 0.15f)
                        {
                            mat.SetFloat("_Smoothness", 0.1f);
                            dirty = true;
                        }
                        if (mat.HasProperty("_Metallic") && mat.GetFloat("_Metallic") > 0f)
                        {
                            mat.SetFloat("_Metallic", 0f);
                            dirty = true;
                        }
                    }

                    if (dirty)
                    {
                        EditorUtility.SetDirty(mat);
                        fixed_++;
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[TR] Fixed {fixed_} rock/ruin materials to URP/Lit with correct textures");
        }
    }
}
