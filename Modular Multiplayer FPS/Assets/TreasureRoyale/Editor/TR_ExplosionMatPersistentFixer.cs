using UnityEditor;
using UnityEngine;

namespace MarsFPSKit
{
    // Runs every time Unity loads — fixes explosion particle materials that keep reverting
    [InitializeOnLoad]
    static class TR_ExplosionMatPersistentFixer
    {
        static TR_ExplosionMatPersistentFixer()
        {
            EditorApplication.delayCall += Fix;
        }

        static void Fix()
        {
            bool changed = false;

            // ShockWave and Embers need _Blend:2 (Additive in URP Particles/Unlit)
            // _Blend:2 = Additive: SrcBlend=One DstBlend=One
            changed |= FixMat(
                "Assets/314 Arts/MarsFPSKit/Unity Technologies/EffectExamples/FireExplosionEffects/Materials/ShockWaveParticle.mat",
                2, new Color(1f, 1f, 1f, 0.3f));
            changed |= FixMat(
                "Assets/314 Arts/MarsFPSKit/Unity Technologies/EffectExamples/FireExplosionEffects/Materials/EmbersParticle.mat",
                2, new Color(1f, 0.5f, 0.1f, 1f));

            // StoneSurface — opaque grey for debris mesh
            var stone = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/314 Arts/MarsFPSKit/Unity Technologies/EffectExamples/Shared/Materials/StoneSurface.mat");
            if (stone != null && stone.GetColor("_BaseColor") != new Color(0.55f, 0.52f, 0.48f, 1f))
            {
                stone.SetColor("_BaseColor", new Color(0.55f, 0.52f, 0.48f, 1f));
                stone.SetColor("_Color", new Color(0.55f, 0.52f, 0.48f, 1f));
                EditorUtility.SetDirty(stone);
                changed = true;
            }

            if (changed) AssetDatabase.SaveAssets();
        }

        static bool FixMat(string path, int blend, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) return false;
            float curBlend = mat.HasProperty("_Blend") ? mat.GetFloat("_Blend") : -1;
            if (Mathf.Approximately(curBlend, blend)) return false; // already correct

            mat.SetFloat("_Blend", blend);
            mat.SetFloat("_SrcBlend", 1f);   // One
            mat.SetFloat("_DstBlend", 1f);   // One
            mat.SetFloat("_SrcBlendAlpha", 1f);
            mat.SetFloat("_DstBlendAlpha", 1f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Surface", 1f);
            mat.SetColor("_BaseColor", color);
            mat.renderQueue = 3000;
            EditorUtility.SetDirty(mat);
            return true;
        }
    }
}
