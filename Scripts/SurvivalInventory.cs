using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the Player prefab alongside Kit_PlayerBehaviour.
    // Manages food, medicine, repair kits, ammo, crafting materials, utility.
    // Heals route through Kit_PlayerBehaviour.vitalsManager.ApplyHeal.
    public class SurvivalInventory : MonoBehaviour
    {
        [System.Serializable]
        public class SurvivalItem
        {
            public string itemId;
            public SurvivalCategory category;
            public string displayName;
            public int quantity;
            public float healAmount;
            public float healDuration;
            public float armorRepairAmount;
            public float useTime;
            public bool hasSideEffect;
            public SideEffectType sideEffect;
            public float sideEffectChance;
            public float sideEffectDuration;
        }

        public enum SurvivalCategory { Food, Medicine, RepairKit, Ammo, CraftingMaterial, Utility }
        public enum SideEffectType { None, HealthLoss, MovementSlow, VisionBlur, Vulnerability }

        [Header("Slot Limits")]
        [SerializeField] int foodSlots = 4;
        [SerializeField] int medicineSlots = 3;
        [SerializeField] int repairSlots = 2;
        [SerializeField] int utilitySlots = 3;
        [SerializeField] int craftingSlots = 6;

        [Header("Armor")]
        [SerializeField] float maxArmorDurability = 100f;
        float _armorDurability;

        readonly Dictionary<SurvivalCategory, List<SurvivalItem>> _inventory =
            new Dictionary<SurvivalCategory, List<SurvivalItem>>();
        bool _isConsuming;

        Kit_PlayerBehaviour _pb;

        public float ArmorDurabilityPercent => _armorDurability / maxArmorDurability;

        void Awake()
        {
            _pb = GetComponent<Kit_PlayerBehaviour>();
            foreach (SurvivalCategory cat in System.Enum.GetValues(typeof(SurvivalCategory)))
                _inventory[cat] = new List<SurvivalItem>();
            _armorDurability = maxArmorDurability;
        }

        public bool TryPickup(SurvivalItem item)
        {
            int limit = GetSlotLimit(item.category);
            var slot = _inventory[item.category];
            var existing = slot.Find(i => i.itemId == item.itemId);
            if (existing != null) { existing.quantity += item.quantity; return true; }
            if (slot.Count >= limit) return false;
            slot.Add(item);
            return true;
        }

        public void UseFood(string itemId)
        {
            if (_isConsuming) return;
            var item = FindItem(SurvivalCategory.Food, itemId);
            if (item != null) StartCoroutine(ConsumeRoutine(item));
        }

        public void UseMedicine(string itemId)
        {
            var item = FindItem(SurvivalCategory.Medicine, itemId);
            if (item != null) StartCoroutine(ConsumeRoutine(item));
        }

        public void UseRepairKit(string itemId)
        {
            var item = FindItem(SurvivalCategory.RepairKit, itemId);
            if (item == null) return;
            ApplyArmorRepair(item.armorRepairAmount);
            ConsumeOne(item);
        }

        IEnumerator ConsumeRoutine(SurvivalItem item)
        {
            _isConsuming = true;
            SurvivalHUD.Instance?.ShowConsumeProgress(item.displayName, item.useTime);
            float elapsed = 0f;
            while (elapsed < item.useTime)
            {
                if (!_isConsuming) yield break;
                elapsed += Time.deltaTime;
                yield return null;
            }
            ApplyHeal(item);
            ConsumeOne(item);
            _isConsuming = false;
        }

        public void CancelConsume() => _isConsuming = false;

        void ApplyHeal(SurvivalItem item)
        {
            if (item.hasSideEffect && Random.value < item.sideEffectChance)
            {
                if (item.sideEffect == SideEffectType.HealthLoss)
                {
                    // Apply damage through kit vitals
                    _pb?.vitalsManager.ApplyEnvironmentalDamage(_pb, item.healAmount * 0.5f, 0);
                    return;
                }
                ApplySideEffect(item.sideEffect, item.sideEffectDuration);
            }

            if (_pb == null) return;

            if (item.healDuration <= 0)
                _pb.vitalsManager.ApplyHeal(_pb, item.healAmount);
            else
                StartCoroutine(HealOverTime(item.healAmount, item.healDuration));
        }

        IEnumerator HealOverTime(float total, float duration)
        {
            float healed = 0f;
            float rate = total / duration;
            while (healed < total)
            {
                float tick = rate * Time.deltaTime;
                _pb?.vitalsManager.ApplyHeal(_pb, tick);
                healed += tick;
                yield return null;
            }
        }

        void ApplySideEffect(SideEffectType effect, float duration)
        {
            var statusEffects = GetComponent<PlayerStatusEffects>();
            if (statusEffects == null) return;
            switch (effect)
            {
                case SideEffectType.MovementSlow: statusEffects.ApplySlow(0.5f, duration); break;
                case SideEffectType.VisionBlur:   statusEffects.ApplyBlur(duration); break;
                case SideEffectType.Vulnerability: statusEffects.ApplyVulnerability(duration); break;
            }
        }

        public void OnArmorHit(float damagePercent)
        {
            _armorDurability = Mathf.Max(0f, _armorDurability - maxArmorDurability * damagePercent);
            SurvivalHUD.Instance?.UpdateArmorDisplay(ArmorDurabilityPercent);
        }

        void ApplyArmorRepair(float repairPercent)
        {
            _armorDurability = Mathf.Min(maxArmorDurability,
                _armorDurability + maxArmorDurability * repairPercent);
            SurvivalHUD.Instance?.UpdateArmorDisplay(ArmorDurabilityPercent);
        }

        public float GetArmorDamageReduction() => ArmorDurabilityPercent * 0.30f;

        SurvivalItem FindItem(SurvivalCategory cat, string itemId) =>
            _inventory[cat].Find(i => i.itemId == itemId);

        void ConsumeOne(SurvivalItem item)
        {
            item.quantity--;
            if (item.quantity <= 0) _inventory[item.category].Remove(item);
            SurvivalHUD.Instance?.RefreshInventory();
        }

        int GetSlotLimit(SurvivalCategory cat) => cat switch
        {
            SurvivalCategory.Food             => foodSlots,
            SurvivalCategory.Medicine         => medicineSlots,
            SurvivalCategory.RepairKit        => repairSlots,
            SurvivalCategory.Utility          => utilitySlots,
            SurvivalCategory.CraftingMaterial => craftingSlots,
            _                                 => 4
        };
    }
}
