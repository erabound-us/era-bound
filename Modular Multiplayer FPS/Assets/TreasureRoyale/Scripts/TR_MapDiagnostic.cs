using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace MarsFPSKit
{
    public class TR_MapDiagnostic : MonoBehaviour
    {
        // ── static analysis runs 2s after spawn ──────────────────────────
        void Start() => Invoke(nameof(RunStatic), 2f);

        void RunStatic()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TR MAP DIAGNOSTIC ===");

            int water = 0, solidRocks = 0, noColRocks = 0, bridges = 0;

            foreach (var go in Object.FindObjectsOfType<GameObject>())
            {
                if (!go.name.StartsWith("ERA_")) continue;

                if (go.name.StartsWith("ERA_Water"))
                {
                    water++;
                    var rend = go.GetComponentInChildren<Renderer>();
                    bool vis = rend != null && rend.enabled;
                    string shader = rend?.sharedMaterial?.shader.name ?? "NULL";
                    sb.AppendLine($"  WATER pos={V(go.transform.position)} scale={V(go.transform.localScale)} visible={vis} shader={shader}");
                }
                else if (go.name.StartsWith("ERA_Rock"))
                {
                    var rend = go.GetComponentInChildren<Renderer>();
                    bool solid = rend?.sharedMaterial?.shader.name == "Unlit/Color";
                    if (solid) solidRocks++;

                    bool hasCol = false;
                    foreach (var c in go.GetComponentsInChildren<Collider>())
                        if (c.enabled && !c.isTrigger) { hasCol = true; break; }
                    if (!hasCol) noColRocks++;

                    if (solid || !hasCol)
                        sb.AppendLine($"  ROCK {go.name} pos={V(go.transform.position)} solidColor={solid} hasCollider={hasCol}");
                }
                else if (go.name.StartsWith("ERA_") && go.GetComponent<SphereCollider>() != null)
                {
                    var sc = go.GetComponent<SphereCollider>();
                    var rend = go.GetComponentInChildren<Renderer>();
                    string shader = rend?.sharedMaterial?.shader.name ?? "NULL";
                    sb.AppendLine($"  SPHERE_COL {go.name} pos={V(go.transform.position)} shader={shader} r={sc.radius:F2}");
                }
                else if (go.name.StartsWith("ERA_Tree"))
                {
                    var rend = go.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        string shader = rend.sharedMaterial?.shader.name ?? "NULL";
                        bool dark = rend.sharedMaterial?.shader.name == "Unlit/Color" &&
                                    rend.sharedMaterial.color.r < 0.1f &&
                                    rend.sharedMaterial.color.g < 0.1f &&
                                    rend.sharedMaterial.color.b < 0.1f;
                        if (dark) sb.AppendLine($"  DARK_TREE {go.name} pos={V(go.transform.position)} shader={shader} color={rend.sharedMaterial.color}");
                    }
                }
                else if (go.name.StartsWith("ERA_Bridge"))
                {
                    bridges++;
                    // Find nearest water
                    float nearest = float.MaxValue;
                    foreach (var w in Object.FindObjectsOfType<GameObject>())
                        if (w.name.StartsWith("ERA_Water"))
                            nearest = Mathf.Min(nearest, Vector3.Distance(go.transform.position, w.transform.position));
                    sb.AppendLine($"  BRIDGE pos={V(go.transform.position)} rot={go.transform.eulerAngles.y:F0}deg nearestWater={nearest:F1}m");
                }
            }

            // Hidden renderers under ERA_ roots
            int hiddenEra = 0;
            foreach (var r in Object.FindObjectsOfType<Renderer>())
            {
                if (r.enabled) continue;
                Transform p = r.transform;
                while (p != null) { if (p.name.StartsWith("ERA_")) { hiddenEra++; break; } p = p.parent; }
            }

            sb.AppendLine($"SUMMARY: water={water} solidRocks={solidRocks} noColRocks={noColRocks} bridges={bridges} hiddenEraRenderers={hiddenEra}");
            sb.AppendLine("=== END STATIC ===");
            Debug.Log(sb.ToString());
        }

        // ── live collision monitor ────────────────────────────────────────
        Transform _player;
        Vector3 _intervalStartPos;
        float _stuckTimer;
        const float STUCK_THRESHOLD = 1.5f;  // less than 1.5m over the interval = stuck
        const float STUCK_REPORT_INTERVAL = 1.5f;
        readonly HashSet<string> _reported = new HashSet<string>();

        void Update()
        {
            if (_player == null)
            {
                var pb = Object.FindObjectOfType<Kit_PlayerBehaviour>();
                if (pb == null) return;
                _player = pb.transform;
                _intervalStartPos = _player.position;
            }

            bool playerMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

            if (playerMoving)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= STUCK_REPORT_INTERVAL)
                {
                    float moved = Vector3.Distance(_player.position, _intervalStartPos);
                    if (moved < STUCK_THRESHOLD) ReportInvisibleBlocker();
                    _stuckTimer = 0;
                    _intervalStartPos = _player.position;
                }
            }
            else
            {
                _stuckTimer = 0;
                _intervalStartPos = _player.position;
            }
        }

        void ReportInvisibleBlocker()
        {
            // Cast rays in movement directions to find what's blocking
            Vector3 pos = _player.position + Vector3.up * 1f;
            string[] dirs = { "forward", "back", "left", "right" };
            Vector3[] vecs = { _player.forward, -_player.forward, -_player.right, _player.right };

            var sb = new StringBuilder();
            sb.AppendLine($"[TR DIAG] Player STUCK at {V(_player.position)} — scanning blockers:");

            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(pos, vecs[i], out RaycastHit hit, 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    // Ignore player's own body parts
                    if (hit.collider.transform.IsChildOf(_player)) continue;
                    var r = hit.collider.GetComponentInChildren<Renderer>() ?? hit.collider.GetComponentInParent<Renderer>();
                    bool visible = r != null && r.enabled;
                    string goName = hit.collider.gameObject.name;
                    string key = goName + dirs[i];
                    if (!_reported.Contains(key))
                    {
                        _reported.Add(key);
                        sb.AppendLine($"  {dirs[i]}: HIT '{goName}' (parent='{hit.collider.transform.parent?.name}') dist={hit.distance:F2}m rendererVisible={visible}");
                    }
                }
            }
            Debug.LogWarning(sb.ToString());
        }

        // ── trigger/collision passthrough detector ────────────────────────
        // Attach a child trigger sphere to the player to detect when they
        // pass through solid-looking objects
        void LateUpdate()
        {
            if (_player == null) return;

            // Sphere overlap at player position — find solid ERA_ objects the player is inside
            var cols = Physics.OverlapSphere(_player.position + Vector3.up, 0.6f);
            foreach (var c in cols)
            {
                if (c.isTrigger) continue;
                Transform p = c.transform;
                bool isEra = false;
                while (p != null) { if (p.name.StartsWith("ERA_Rock") || p.name.StartsWith("ERA_Structure") || p.name.StartsWith("ERA_Tree")) { isEra = true; break; } p = p.parent; }
                if (!isEra) continue;
                // Skip ERA_Ground — it's a flat plane, player is always geometrically "inside" it
                bool isGround = false; var pg = c.transform; while(pg!=null){if(pg.name=="ERA_Ground"||pg.name=="ERA_GroundExt"){isGround=true;break;}pg=pg.parent;}
                if(isGround) continue;

                string key = "inside_" + c.gameObject.name;
                if (_reported.Contains(key)) continue;
                _reported.Add(key);

                var rend = c.GetComponentInChildren<Renderer>() ?? c.GetComponentInParent<Renderer>();
                bool visible = rend != null && rend.enabled;
                Debug.LogWarning($"[TR DIAG] Player INSIDE solid ERA object '{c.gameObject.name}' (parent='{c.transform.parent?.name}') rendererVisible={visible} — this is a walk-through collision bug");
            }
        }

        static string V(Vector3 v) => $"({v.x:F1},{v.y:F1},{v.z:F1})";
    }
}
