using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the Player prefab.
    // Manages 3 elite gear slots: Armor, Utility, Consumable.
    public class GearLoadout : MonoBehaviour
    {
        public static GearLoadout Instance { get; private set; }

        public enum GearSlot { Armor, Utility, Consumable }

        [System.Serializable]
        public class GearItem
        {
            public string itemId;
            public GearSlot slot;
            public bool hasRelicExtractionBonus;
            public bool damageSlowsChannel;
        }

        [SerializeField] GearItem armorSlot;
        [SerializeField] GearItem utilitySlot;
        [SerializeField] GearItem consumableSlot;

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        public bool HasRelicExtractionGear() =>
            (armorSlot     != null && armorSlot.hasRelicExtractionBonus)     ||
            (utilitySlot   != null && utilitySlot.hasRelicExtractionBonus)   ||
            (consumableSlot != null && consumableSlot.hasRelicExtractionBonus);

        public bool DamageSlowsChannel() =>
            (armorSlot     != null && armorSlot.damageSlowsChannel)     ||
            (utilitySlot   != null && utilitySlot.damageSlowsChannel)   ||
            (consumableSlot != null && consumableSlot.damageSlowsChannel);

        public void ConsumeOnDeath()
        {
            armorSlot      = null;
            utilitySlot    = null;
            consumableSlot = null;
        }
    }
}
