using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the ShopRow prefab.
    public class TR_ShopRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descText;
        [SerializeField] TextMeshProUGUI eraText;
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] TextMeshProUGUI durabilityText;
        [SerializeField] Button buyBtn;

        public void Populate(CatalogItem item, Action<CatalogItem> onBuy)
        {
            nameText.text  = item.name;
            descText.text  = item.description ?? "";
            priceText.text = $"{item.shop_price:N0} CC";

            eraText.text = string.IsNullOrEmpty(item.era_tags) || item.era_tags == "null"
                ? "All Eras"
                : item.era_tags.Replace("[","").Replace("]","").Replace("\"","").Replace(",", ", ");

            durabilityText.text = item.max_durability > 0
                ? $"Durability: {item.max_durability}"
                : item.is_consumable == 1 ? "One-use" : "Indestructible";

            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => onBuy(item));
        }
    }
}
