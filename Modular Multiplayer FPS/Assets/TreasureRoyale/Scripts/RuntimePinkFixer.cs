using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MarsFPSKit
{
    public class RuntimePinkFixer : MonoBehaviour
    {
        static readonly string[] KeepPrefixes = {
            "Universal Render Pipeline/",
            "Skybox/", "Sprites/", "UI/", "TextMeshPro/", "GUI/", "Unlit/",
            "Shader Graphs/", "Suimono2/",
        };

        void Start() => StartCoroutine(FixAfterDelay());

        IEnumerator FixAfterDelay()
        {
            yield return null;
            yield return null;
            Fix("frame1");
            yield return null;
            Fix("frame2");
            yield return new WaitForSeconds(0.5f);
            Fix("frame3");
            yield return new WaitForSeconds(1.0f);
            Fix("frame4");
        }

        void LogRemaining()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[TR] Remaining non-URP renderers:");
            foreach (var r in FindObjectsOfType<Renderer>(true))
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null || mat.shader == null) continue;
                    string sn = mat.shader.name;
                    bool keep = sn.StartsWith("Universal Render Pipeline/")
                        || sn.StartsWith("Skybox/") || sn.StartsWith("Sprites/")
                        || sn.StartsWith("UI/") || sn.StartsWith("TextMeshPro/")
                        || sn.StartsWith("GUI/") || sn.StartsWith("Unlit/");
                    if (!keep)
                        sb.AppendLine("  " + r.gameObject.name + " -> " + sn);
                }
            }
            Debug.Log(sb.ToString());
        }

        void Fix(string tag)
        {
            var urpLit      = Shader.Find("Universal Render Pipeline/Lit");
            var urpUnlit    = Shader.Find("Universal Render Pipeline/Unlit");
            var urpParticle = Shader.Find("Universal Render Pipeline/Particles/Lit");
            if (urpLit == null) return;

            int fixed_ = 0;
            var seenMats = new System.Collections.Generic.HashSet<int>();
            var fixedMats = new System.Collections.Generic.Dictionary<int, Material>();

            foreach (var r in FindObjectsOfType<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null || mat.shader == null) continue;

                    int id = mat.GetInstanceID();

                    // If we already fixed this material, reuse the fixed version
                    if (fixedMats.TryGetValue(id, out Material already))
                    {
                        mats[i] = already;
                        changed = true;
                        continue;
                    }

                    if (seenMats.Contains(id)) continue;
                    seenMats.Add(id);

                    string sn = mat.shader.name;

                    // Re-resolve URP shaders that show as error/hidden due to stale GUID
                    if (sn.StartsWith("Universal Render Pipeline/") && mat.shader == Shader.Find(sn))
                        continue; // shader resolves fine, skip
                    if (sn.StartsWith("Universal Render Pipeline/"))
                    {
                        // Shader GUID stale — re-find by name
                        var resolved = Shader.Find(sn);
                        if (resolved != null)
                        {
                            mat.shader = resolved;
                            fixed_++;
                            continue;
                        }
                        // Can't resolve — fall through to replace with urpLit
                    }

                    bool keep = false;
                    foreach (var p in KeepPrefixes)
                        if (sn.StartsWith(p)) { keep = true; break; }
                    if (keep) continue;

                    Texture albedo = null;
                    if (mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");
                    if (albedo == null && mat.HasProperty("_MainTex")) albedo = mat.GetTexture("_MainTex");
                    Color col = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                              : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                              : Color.white;

                    string snl = sn.ToLower();
                    Shader target = urpLit;
                    if (snl.Contains("particle") || snl.Contains("alpha") || snl.Contains("smoke"))
                        target = urpParticle ?? urpLit;
                    else if (snl.Contains("unlit"))
                        target = urpUnlit ?? urpLit;

                    var inst = new Material(target);
                    inst.name = mat.name + "_URP";
                    if (albedo != null) { inst.SetTexture("_BaseMap", albedo); inst.SetTexture("_MainTex", albedo); }
                    if (inst.HasProperty("_BaseColor")) inst.SetColor("_BaseColor", col);

                    fixedMats[id] = inst;
                    mats[i] = inst;
                    changed = true;
                    fixed_++;
                    if (!tag.StartsWith("periodic"))
                        Debug.Log($"[TR] PinkFixer fixed: {mat.name} was={sn} on GO={r.gameObject.name}");
                }

                if (changed) r.sharedMaterials = mats;
            }

            if (fixed_ > 0)
                Debug.Log("[TR] RuntimePinkFixer (" + tag + "): fixed " + fixed_ + " materials.");
        }
    }
}
