using UnityEngine;
using System.Collections.Generic;

namespace MarsFPSKit
{
    public static class TR_MapValidator
    {
        public class ValidationResult
        {
            public bool passed;
            public List<string> errors = new List<string>();
            public List<string> warnings = new List<string>();
        }

        // Names that are NOT player-entry swim triggers
        static bool IsNonEntryTrigger(string name) =>
            name.Contains("Drown") || name == "ERA_OceanTrigger" || name == "ERA_OceanDrown";

        public static ValidationResult Validate()
        {
            var result = new ValidationResult();
            CheckWorldFloor(result);
            CheckERAGround(result);
            CheckWaterTriggers(result);
            CheckSpawnSafety(result);
            CheckOceanBoundary(result);
            result.passed = result.errors.Count == 0;
            return result;
        }

        public static void AutoFix()
        {
            // Ensure world floor exists
            if (GameObject.Find("ERA_WorldFloor") == null)
            {
                var floor = new GameObject("ERA_WorldFloor");
                floor.transform.position = new Vector3(0, -6f, 0);
                floor.AddComponent<BoxCollider>().size = new Vector3(16000f, 0.5f, 16000f);
                Debug.Log("[TR AutoFix] Created missing ERA_WorldFloor.");
            }

            // Move spawn points that are inside swim triggers to safe positions
            var spawns = Object.FindObjectsOfType<Kit_PlayerSpawn>();
            var triggers = Object.FindObjectsOfType<Kit_SwimmingTrigger>();

            // Build list of safe candidate positions (fixed grid away from water)
            var safeCandidates = new Vector3[] {
                new Vector3( 50f, 1.5f,  50f),
                new Vector3(-50f, 1.5f,  50f),
                new Vector3( 50f, 1.5f, -50f),
                new Vector3(-50f, 1.5f, -50f),
                new Vector3(100f, 1.5f,   0f),
                new Vector3(-100f,1.5f,   0f),
                new Vector3(  0f, 1.5f, 100f),
                new Vector3(  0f, 1.5f,-100f),
                new Vector3( 80f, 1.5f,  80f),
                new Vector3(-80f, 1.5f, -80f),
            };

            foreach (var spawn in spawns)
            {
                bool insideWater = false;
                foreach (var st in triggers)
                {
                    if (IsNonEntryTrigger(st.gameObject.name)) continue;
                    if (st.col != null && st.col.bounds.Contains(spawn.transform.position))
                    { insideWater = true; break; }
                }
                if (!insideWater) continue;

                // Find first safe candidate not inside any water trigger
                foreach (var candidate in safeCandidates)
                {
                    bool safe = true;
                    foreach (var st in triggers)
                    {
                        if (IsNonEntryTrigger(st.gameObject.name)) continue;
                        if (st.col != null && st.col.bounds.Contains(candidate)) { safe = false; break; }
                    }
                    if (safe)
                    {
                        Debug.Log($"[TR AutoFix] Moved spawn '{spawn.name}' to safe position {candidate}");
                        spawn.transform.position = candidate;
                        break;
                    }
                }
            }
        }

        static void CheckWorldFloor(ValidationResult r)
        {
            var floor = GameObject.Find("ERA_WorldFloor");
            if (floor == null)
            { r.errors.Add("ERA_WorldFloor missing — players can fall through the world."); return; }
            var bc = floor.GetComponent<BoxCollider>();
            if (bc == null)
                r.errors.Add("ERA_WorldFloor has no BoxCollider.");
            else if (bc.size.x < 1000f)
                r.warnings.Add($"ERA_WorldFloor may be too small ({bc.size.x}x{bc.size.z}).");
            if (floor.transform.position.y > -4f)
                r.errors.Add($"ERA_WorldFloor at y={floor.transform.position.y} is too shallow — swim depth clamp will hit it.");
            if (floor.transform.position.y < -10f)
                r.warnings.Add($"ERA_WorldFloor at y={floor.transform.position.y} is very deep.");
        }

        static void CheckERAGround(ValidationResult r)
        {
            var ground = GameObject.Find("ERA_Ground");
            if (ground == null)
                r.errors.Add("ERA_Ground missing — no walkable surface.");
        }

        static void CheckWaterTriggers(ValidationResult r)
        {
            var triggers = Object.FindObjectsOfType<Kit_SwimmingTrigger>();
            if (triggers.Length == 0)
            { r.warnings.Add("No swim triggers found — water areas won't be swimmable."); return; }

            int entryCount = 0;
            foreach (var st in triggers)
            {
                if (IsNonEntryTrigger(st.gameObject.name)) continue;
                entryCount++;
                if (st.col == null)
                    r.errors.Add($"Swim trigger '{st.gameObject.name}' has null col reference.");
                else if (st.col.bounds.max.y < 0f)
                    r.errors.Add($"Swim trigger '{st.gameObject.name}' top={st.col.bounds.max.y:F2} is below ground — players can never enter.");
            }
            if (entryCount == 0)
                r.warnings.Add("No player-entry swim triggers found.");
        }

        static void CheckSpawnSafety(ValidationResult r)
        {
            var spawns = Object.FindObjectsOfType<Kit_PlayerSpawn>();
            var triggers = Object.FindObjectsOfType<Kit_SwimmingTrigger>();

            if (spawns.Length == 0)
            { r.warnings.Add("No Kit_PlayerSpawn found — default spawn will be used."); return; }

            foreach (var spawn in spawns)
            {
                Vector3 pos = spawn.transform.position;
                foreach (var st in triggers)
                {
                    if (IsNonEntryTrigger(st.gameObject.name)) continue;
                    if (st.col != null && st.col.bounds.Contains(pos))
                        r.errors.Add($"spawn '{spawn.name}' at {pos} is INSIDE swim trigger '{st.gameObject.name}' — player will spawn underwater.");
                }
            }
        }

        static void CheckOceanBoundary(ValidationResult r)
        {
            if (GameObject.Find("ERA_OceanN") == null || GameObject.Find("ERA_OceanS") == null)
                r.warnings.Add("Ocean boundary panels missing — no visual ocean boundary.");
        }
    }
}
