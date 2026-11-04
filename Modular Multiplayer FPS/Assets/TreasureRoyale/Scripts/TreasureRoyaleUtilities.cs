using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    // Lightweight scene marker — place in scene to define valid drop zone positions.
    public class DropZoneMarker : MonoBehaviour
    {
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
            Gizmos.DrawSphere(transform.position, 2f);
        }
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────

    // Attach to a persistent scene object.
    // Plays era-keyed particle + audio on player arrival.
    public class ArrivalEffect : MonoBehaviour
    {
        [System.Serializable]
        public struct EraEffect
        {
            public string eraId;
            public ParticleSystem particles;
            public AudioClip sound;
        }

        [SerializeField] EraEffect[] eraEffects;
        [SerializeField] AudioSource audioSource;

        string _currentEraId;

        public void SetEra(string eraId) => _currentEraId = eraId;

        public void Play(Vector3 position)
        {
            foreach (var e in eraEffects)
            {
                if (e.eraId != _currentEraId) continue;
                if (e.particles)
                {
                    e.particles.transform.position = position;
                    e.particles.Play();
                }
                if (e.sound && audioSource)
                    audioSource.PlayOneShot(e.sound);
                return;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    // Pure static calculator — no MonoBehaviour needed.
    public static class CrossEraPowerCalculator
    {
        const float DegradationPerTier = 0.12f;
        const float BonusPerTier       = 0.15f;
        const float AffinityFloor      = 0.40f;
        const float AffinityCap        = 1.60f;

        public static float GetAffinityMultiplier(int nativeEraTier, int currentEraTier, float upgradeFloor = AffinityFloor)
        {
            int delta = currentEraTier - nativeEraTier;
            if (delta == 0) return 1f;
            float multiplier = delta > 0
                ? 1f - delta * DegradationPerTier
                : 1f + Mathf.Abs(delta) * BonusPerTier;
            return Mathf.Clamp(multiplier, upgradeFloor, AffinityCap);
        }

        public static int GetThreatLevel(int nativeEraTier, int currentEraTier, int guardianWardLevel = 0) =>
            Mathf.Max(0, Mathf.Max(0, nativeEraTier - currentEraTier) - guardianWardLevel);

        public static float ApplyStat(float baseStat, int nativeEraTier, int currentEraTier, float upgradeFloor = AffinityFloor) =>
            baseStat * GetAffinityMultiplier(nativeEraTier, currentEraTier, upgradeFloor);

        public static float UpgradeTierToFloor(int upgradeTier) => upgradeTier switch
        {
            1 => 0.55f,
            2 => 0.70f,
            3 => 0.90f,
            _ => 0.40f
        };
    }

    // ─────────────────────────────────────────────────────────────────────────

    // Static weighted random roll utility for survival loot.
    public static class SurvivalLootTable
    {
        [System.Serializable]
        public class WeightedItem
        {
            public SurvivalInventory.SurvivalItem item;
            public float weight;
        }

        public static SurvivalInventory.SurvivalItem Roll(List<WeightedItem> table)
        {
            if (table == null || table.Count == 0) return null;
            float total = 0f;
            foreach (var entry in table) total += entry.weight;
            float roll = Random.value * total;
            float cumulative = 0f;
            foreach (var entry in table)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                    return Clone(entry.item);
            }
            return Clone(table[table.Count - 1].item);
        }

        static SurvivalInventory.SurvivalItem Clone(SurvivalInventory.SurvivalItem src)
        {
            return new SurvivalInventory.SurvivalItem
            {
                itemId           = src.itemId,
                category         = src.category,
                displayName      = src.displayName,
                quantity         = src.quantity,
                healAmount       = src.healAmount,
                healDuration     = src.healDuration,
                armorRepairAmount = src.armorRepairAmount,
                useTime          = src.useTime,
                hasSideEffect    = src.hasSideEffect,
                sideEffect       = src.sideEffect,
                sideEffectChance = src.sideEffectChance,
                sideEffectDuration = src.sideEffectDuration
            };
        }
    }
}
