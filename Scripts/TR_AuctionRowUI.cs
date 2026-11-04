using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_AuctionRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI sellerText;
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] TextMeshProUGUI expiryText;
        [SerializeField] GameObject privateBadge;
        [SerializeField] Button buyBtn;
        [SerializeField] Button cancelBtn;

        public void Populate(AuctionListing l, Action<string> onBuy, Action<string> onCancel)
        {
            var snap = JsonUtility.FromJson<AuctionSnap>(l.item_snapshot);
            itemNameText.text = snap?.name ?? "Unknown Item";
            sellerText.text   = $"Seller: {l.seller_name}";
            priceText.text    = $"{l.asking_price:N0} CC";
            privateBadge.SetActive(l.is_private == 1);

            long secsLeft = l.expires_at - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            expiryText.text = secsLeft > 86400
                ? $"Expires in {secsLeft / 86400}d"
                : $"Expires in {secsLeft / 3600}h";

            bool isMine = l.seller_id == TR_HubApi.PlayerId;
            buyBtn.gameObject.SetActive(!isMine);
            cancelBtn.gameObject.SetActive(isMine);

            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => onBuy(l.listing_id));
            cancelBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.AddListener(() => onCancel(l.listing_id));
        }
        [System.Serializable] class AuctionSnap { public string name; public string rarity; public string era; }
    }
}
