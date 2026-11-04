using UnityEngine;

namespace MarsFPSKit
{
    public class TR_ParticleDiagnostic : MonoBehaviour
    {
        void Start()
        {
            Invoke(nameof(Run), 10f);
        }

        void Run()
        {
            // Log all particle renderers in the entire scene (catches impact effects)
            var renderers = FindObjectsOfType<ParticleSystemRenderer>(true);
            Debug.Log($"[TR ParticleDiag] Found {renderers.Length} ParticleSystemRenderers in scene:");
            foreach (var r in renderers)
            {
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var mat = r.sharedMaterials[i];
                    if (mat == null) continue;
                    // Only log broken ones: opaque (dstBlend=0) or missing texture
                    float dst = 0;
                    if (mat.HasProperty("_DstBlend")) try { dst = mat.GetFloat("_DstBlend"); } catch { }
                    float src = 1;
                    if (mat.HasProperty("_SrcBlend")) try { src = mat.GetFloat("_SrcBlend"); } catch { }
                    bool opaque = dst == 0 && src == 1;
                    bool noTex = mat.mainTexture == null;
                    // src=2 dst=1 is additive, src=5 dst=1 is soft-additive — both valid transparent blends
                    bool isValidTransparent = (src == 2 && dst == 1) || (src == 5 && dst == 1) || (src == 1 && dst == 1);
                    // Only flag particle shader materials — StoneSurface and other surface mats are intentionally opaque
                    bool isParticleShader = mat.shader.name.Contains("Particles");
                    if (isParticleShader && opaque && noTex && !isValidTransparent)
                        Debug.LogWarning($"[TR ParticleDiag] BROKEN: go={r.gameObject.name} mat={mat.name} shader={mat.shader.name} src={src} dst={dst} tex={mat.mainTexture?.name ?? "NONE"}");
                }
            }
            Debug.Log("[TR ParticleDiag] Done.");
        }

        static string GetPath(Transform t, Transform root)
        {
            if (t == root || t == null) return "";
            string parent = GetPath(t.parent, root);
            return string.IsNullOrEmpty(parent) ? t.name : parent + "/" + t.name;
        }
    }
}
