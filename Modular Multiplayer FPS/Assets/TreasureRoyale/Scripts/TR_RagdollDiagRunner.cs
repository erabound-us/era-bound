using System.Collections;
using UnityEngine;

namespace MarsFPSKit
{
    public class TR_RagdollDiagRunner : MonoBehaviour
    {
        public void Run() => StartCoroutine(DoRun());

        IEnumerator DoRun()
        {
            yield return new WaitForSeconds(0.15f);
            var sb = new System.Text.StringBuilder("[TR RagdollDiag] All enabled visible renderers 0.15s after death:\n");
            int count = 0;
            foreach (var r in FindObjectsOfType<Renderer>())
            {
                if (!r.enabled) continue;
                if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue;
                string mat = r.sharedMaterial?.name ?? "NULL";
                string shader = r.sharedMaterial?.shader?.name ?? "NULL";
                string path = GetPath(r.transform);
                sb.AppendLine($"  [{r.GetType().Name}] {r.gameObject.name} mat={mat} shader={shader} path={path}");
                count++;
            }
            sb.Insert(0, $"[TR RagdollDiag] count={count}\n");
            Debug.Log(sb.ToString());
        }

        string GetPath(Transform t)
        {
            string p = t.name;
            while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
            return p;
        }
    }
}
