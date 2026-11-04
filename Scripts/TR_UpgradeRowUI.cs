using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_UpgradeRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descText;
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] Button buyBtn;
        [SerializeField] GameObject ownedBadge;

        public void Populate(CatalogItem item, bool owned, Action<string> onBuy)
        {
            nameText.text  = item.name;
            descText.text  = item.description ?? "";
            priceText.text = $"{item.shop_price:N0} CC";
            ownedBadge.SetActive(owned);
            buyBtn.gameObject.SetActive(!owned);
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => onBuy(item.item_id));
        }
    }
}
