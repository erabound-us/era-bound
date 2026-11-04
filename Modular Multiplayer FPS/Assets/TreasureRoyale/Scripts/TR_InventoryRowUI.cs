using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the InventoryRow prefab.
    // Reused across gear, valuables, containers, and pets tabs.
    public class TR_InventoryRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI detailText;
        [SerializeField] TextMeshProUGUI durabilityText;
        [SerializeField] Slider durabilityBar;
        [SerializeField] Image legendaryGlow;       // enabled for legendary valuables
        [SerializeField] Button actionBtn;
        [SerializeField] TextMeshProUGUI actionBtnText;

        static readonly Color LegendaryColor = new Color(1f, 0.84f, 0f); // gold

        public void PopulateGear(InventoryItem item, Action<int> onRepair)
        {
            nameText.text   = item.name;
            detailText.text = $"{item.category} · ×{item.quantity}";
            legendaryGlow.gameObject.SetActive(false);

            bool hasDurability = item.max_durability > 0;
            durabilityBar.gameObject.SetActive(hasDurability);
            durabilityText.gameObject.SetActive(hasDurability);

            if (hasDurability)
            {
                float pct = (float)item.durability / item.max_durability;
                durabilityBar.value  = pct;
                durabilityText.text  = $"{item.durability}/{item.max_durability}";
                durabilityBar.fillRect.GetComponent<Image>().color = DurabilityColor(pct);
            }

            actionBtnText.text = "Repair";
            actionBtn.gameObject.SetActive(hasDurability && item.durability < item.max_durability);
            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() => onRepair(item.id));
        }

        public void PopulateValuable(ValuableItem v)
        {
            nameText.text   = v.name;
            detailText.text = $"{v.era} · {v.rarity}";
            if (!string.IsNullOrEmpty(v.historical_figure))
                detailText.text += $" · {v.historical_figure}";

            bool legendary = v.is_legendary == 1;
            legendaryGlow.gameObject.SetActive(legendary);
            if (legendary) legendaryGlow.color = LegendaryColor;

            durabilityBar.gameObject.SetActive(false);
            durabilityText.gameObject.SetActive(false);
            actionBtn.gameObject.SetActive(false);
        }

        public void PopulateContainer(ContainerItem c, Action<int> onEquip)
        {
            nameText.text   = c.name;
            detailText.text = $"+{c.extra_slots} slots";
            legendaryGlow.gameObject.SetActive(false);
            durabilityBar.gameObject.SetActive(false);
            durabilityText.gameObject.SetActive(false);

            actionBtnText.text = c.is_equipped == 1 ? "Equipped ✓" : "Equip";
            actionBtn.gameObject.SetActive(true);
            actionBtn.interactable = c.is_equipped == 0;
            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() => onEquip(c.id));
        }

        public void PopulatePet(PetItem p, Action<int> onEquip)
        {
            nameText.text   = $"{p.name} ({p.pet_type})";
            detailText.text = $"+{p.extra_slots} carry slots";
            legendaryGlow.gameObject.SetActive(false);
            durabilityBar.gameObject.SetActive(false);
            durabilityText.gameObject.SetActive(false);

            actionBtnText.text = p.is_active == 1 ? "Active ✓" : "Activate";
            actionBtn.gameObject.SetActive(true);
            actionBtn.interactable = p.is_active == 0;
            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() => onEquip(p.id));
        }

        static Color DurabilityColor(float pct) => pct > 0.6f
            ? Color.green
            : pct > 0.3f ? Color.yellow : Color.red;
    }
}
