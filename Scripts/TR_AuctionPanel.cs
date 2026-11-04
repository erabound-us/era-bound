using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Auction panel root.
    public class TR_AuctionPanel : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] Button tabBrowse;
        [SerializeField] Button tabSell;
        [SerializeField] Button tabMyListings;

        [Header("Browse")]
        [SerializeField] RectTransform browseContainer;
        [SerializeField] GameObject auctionRowPrefab;   // TR_AuctionRowUI

        [Header("Sell — Valuable")]
        [SerializeField] GameObject sellPanel;
        [SerializeField] RectTransform sellValuableContainer;
        [SerializeField] GameObject sellValuableRowPrefab;  // TR_SellValuableRowUI

        [Header("Create Listing")]
        [SerializeField] GameObject createListingPanel;
        [SerializeField] TMP_InputField askingPriceInput;
        [SerializeField] Toggle privateToggle;
        [SerializeField] TMP_InputField allowedBuyersInput;  // comma-separated player IDs
        [SerializeField] GameObject allowedBuyersRoot;
        [SerializeField] Button submitListingBtn;
        [SerializeField] TextMeshProUGUI listingFeePreview;

        [Header("Status")]
        [SerializeField] TextMeshProUGUI statusText;

        InventoryResponse _inventory;
        int _pendingSellValuableId;
        int _pendingListInventoryId;
        readonly List<GameObject> _rows = new List<GameObject>();

        void Awake()
        {
            if (tabBrowse)      tabBrowse.onClick.AddListener(ShowBrowse);
            if (tabSell)        tabSell.onClick.AddListener(ShowSell);
            if (tabMyListings)  tabMyListings.onClick.AddListener(ShowMyListings);
            if (privateToggle)  privateToggle.onValueChanged.AddListener(v => { if (allowedBuyersRoot) allowedBuyersRoot.SetActive(v); });
            if (askingPriceInput)  askingPriceInput.onValueChanged.AddListener(_ => UpdateFeePreview());
            if (submitListingBtn)  submitListingBtn.onClick.AddListener(SubmitListing);
        }

        public void Refresh() => ShowBrowse();

        void ShowBrowse()
        {
            if (sellPanel) sellPanel.SetActive(false);
            if (createListingPanel) createListingPanel.SetActive(false);
            ClearRows();
            if (statusText) statusText.text = TR_HubApi.IsReady ? "" : "Worker offline — run: wrangler dev";
            if (!TR_HubApi.IsReady || auctionRowPrefab == null || browseContainer == null) return;

            TR_HubApi.Instance.GetAuctionListings(listings =>
            {
                foreach (var l in listings)
                {
                    var go  = Instantiate(auctionRowPrefab, browseContainer, false);
                    var row = go.GetComponent<TR_AuctionRowUI>();
                    row?.Populate(l, OnBuyListingPressed, OnCancelListingPressed);
                    _rows.Add(go);
                }
                statusText.text = $"{listings.Length} listing{(listings.Length == 1 ? "" : "s")}";
            });
        }

        void OnBuyListingPressed(string listingId)
        {
            TR_HubApi.Instance.Post($"/auction/buy/{listingId}",
                new { buyer_id = TR_HubApi.PlayerId },
                (json, err) =>
                {
                    statusText.text = err != null ? $"Error: {err}" : "Purchase complete!";
                    TR_HubManager.Instance.RefreshProfile();
                    ShowBrowse();
                });
        }

        void OnCancelListingPressed(string listingId)
        {
            TR_HubApi.Instance.Post($"/auction/cancel/{listingId}",
                new { seller_id = TR_HubApi.PlayerId },
                (_, __) => ShowBrowse());
        }

        // ── Sell Valuables ────────────────────────────────────────────────────

        void ShowSell()
        {
            createListingPanel.SetActive(false);
            sellPanel.SetActive(true);
            ClearRows();

            TR_HubApi.Instance.GetInventory(inv =>
            {
                _inventory = inv;
                foreach (var v in inv.valuables)
                {
                    var go  = Instantiate(sellValuableRowPrefab, sellValuableContainer, false);
                    var row = go.GetComponent<TR_SellValuableRowUI>();
                    row?.Populate(v, OnSellValuablePressed, OnListValuablePressed);
                    _rows.Add(go);
                }
            });
        }

        void OnSellValuablePressed(int valuableId)
        {
            TR_HubApi.Instance.Post("/auction/sell-valuable",
                new { player_id = TR_HubApi.PlayerId, valuable_id = valuableId },
                (json, err) =>
                {
                    if (err != null) { statusText.text = $"Error: {err}"; return; }
                    var result = JsonUtility.FromJson<SellResult>(json);
                    statusText.text = $"Sold for {result.earned:N0} CC!";
                    TR_HubManager.Instance.RefreshProfile();
                    ShowSell();
                });
        }

        void OnListValuablePressed(int valuableId)
        {
            _pendingListInventoryId = -1;
            _pendingSellValuableId  = valuableId;
            createListingPanel.SetActive(true);
            askingPriceInput.text   = "";
            privateToggle.isOn      = false;
            allowedBuyersInput.text = "";
        }

        // ── Create Listing ────────────────────────────────────────────────────

        void ShowMyListings()
        {
            sellPanel.SetActive(false);
            createListingPanel.SetActive(false);
            // Reuse browse but filter to own listings
            ClearRows();
            TR_HubApi.Instance.GetAuctionListings(listings =>
            {
                foreach (var l in listings)
                {
                    if (l.seller_id != TR_HubApi.PlayerId) continue;
                    var go  = Instantiate(auctionRowPrefab, browseContainer, false);
                    var row = go.GetComponent<TR_AuctionRowUI>();
                    row?.Populate(l, OnBuyListingPressed, OnCancelListingPressed);
                    _rows.Add(go);
                }
            });
        }

        void UpdateFeePreview()
        {
            if (int.TryParse(askingPriceInput.text, out int price))
            {
                int fee = Mathf.Min(Mathf.CeilToInt(price * 0.03f), 25000);
                listingFeePreview.text = $"Listing fee: {fee:N0} CC (non-refundable)";
            }
            else listingFeePreview.text = "";
        }

        void SubmitListing()
        {
            if (!int.TryParse(askingPriceInput.text, out int price) || price <= 0)
            {
                statusText.text = "Enter a valid asking price.";
                return;
            }

            string[] buyers = null;
            if (privateToggle.isOn && !string.IsNullOrEmpty(allowedBuyersInput.text))
                buyers = allowedBuyersInput.text.Split(',');

            bool isValuable = _pendingSellValuableId > 0;
            TR_HubApi.Instance.Post("/auction/list", new
            {
                seller_id      = TR_HubApi.PlayerId,
                item_type      = isValuable ? "valuable" : "inventory",
                item_ref_id    = isValuable ? _pendingSellValuableId : _pendingListInventoryId,
                asking_price   = price,
                is_private     = privateToggle.isOn,
                allowed_buyers = buyers,
            }, (json, err) =>
            {
                if (err != null) { statusText.text = $"Error: {err}"; return; }
                statusText.text = "Listed!";
                createListingPanel.SetActive(false);
                TR_HubManager.Instance.RefreshProfile();
                ShowBrowse();
            });
        }

        void ClearRows()
        {
            foreach (var r in _rows) Destroy(r);
            _rows.Clear();
        }
        [System.Serializable] class SellResult { public int earned; }
    }
}
