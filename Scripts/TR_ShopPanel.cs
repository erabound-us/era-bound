using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Shop panel root.
    public class TR_ShopPanel : MonoBehaviour
    {
        [Header("Era Filter")]
        [SerializeField] TMP_Dropdown eraDropdown;

        [Header("Category Tabs")]
        [SerializeField] Button tabAll;
        [SerializeField] Button tabWeapons;
        [SerializeField] Button tabGear;
        [SerializeField] Button tabConsumables;
        [SerializeField] Button tabContainers;
        [SerializeField] Button tabPets;
        [SerializeField] Button tabVehicles;

        [Header("List")]
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject shopRowPrefab;   // TR_ShopRowUI

        [Header("Confirm Dialog")]
        [SerializeField] GameObject confirmRoot;
        [SerializeField] TextMeshProUGUI confirmText;
        [SerializeField] Button confirmBuyBtn;
        [SerializeField] Button confirmCancelBtn;

        CatalogItem[] _catalog;
        string _activeCategory = "all";
        CatalogItem _pendingItem;
        readonly List<GameObject> _rows = new List<GameObject>();

        static readonly string[] Eras = {
            "", "deep_prehistoric","prehistoric","ancient","medieval",
            "renaissance","industrial","modern","near_future","mid_future","far_future","deep_future"
        };

        void Awake()
        {
            if (eraDropdown != null)
            {
                eraDropdown.ClearOptions();
                var opts = new List<string> { "All Eras" };
                opts.AddRange(new[]{ "Deep Prehistoric","Prehistoric","Ancient","Medieval",
                    "Renaissance","Industrial","Modern","Near Future","Mid Future","Far Future","Deep Future" });
                eraDropdown.AddOptions(opts);
                eraDropdown.onValueChanged.AddListener(_ => Refresh());
            }
            if (tabAll)         tabAll.onClick.AddListener(()         => SetCategory("all"));
            if (tabWeapons)     tabWeapons.onClick.AddListener(()     => SetCategory("weapon"));
            if (tabGear)        tabGear.onClick.AddListener(()        => SetCategory("gear"));
            if (tabConsumables) tabConsumables.onClick.AddListener(() => SetCategory("consumable"));
            if (tabContainers)  tabContainers.onClick.AddListener(()  => SetCategory("container"));
            if (tabPets)        tabPets.onClick.AddListener(()        => SetCategory("pet"));
            if (tabVehicles)    tabVehicles.onClick.AddListener(()    => SetCategory("vehicle"));
            if (confirmBuyBtn)    confirmBuyBtn.onClick.AddListener(ConfirmBuy);
            if (confirmCancelBtn) confirmCancelBtn.onClick.AddListener(() => { if (confirmRoot) confirmRoot.SetActive(false); });
        }

        public void Refresh()
        {
            if (listContainer == null) return;
            if (!TR_HubApi.IsReady || shopRowPrefab == null) return;
            string era = eraDropdown != null ? Eras[eraDropdown.value] : "";
            TR_HubApi.Instance.GetCatalog(era, catalog =>
            {
                _catalog = catalog;
                RebuildList();
            });
        }

        void SetCategory(string cat)
        {
            _activeCategory = cat;
            RebuildList();
        }

        void RebuildList()
        {
            foreach (var r in _rows) Destroy(r);
            _rows.Clear();
            if (_catalog == null) return;

            foreach (var item in _catalog)
            {
                if (_activeCategory != "all" && !item.category.StartsWith(_activeCategory)) continue;
                var go  = Instantiate(shopRowPrefab, listContainer, false);
                var row = go.GetComponent<TR_ShopRowUI>();
                row?.Populate(item, OnBuyPressed);
                _rows.Add(go);
            }
        }

        void OnBuyPressed(CatalogItem item)
        {
            _pendingItem    = item;
            confirmText.text = $"Buy {item.name} for {item.shop_price:N0} CC?";
            confirmRoot.SetActive(true);
        }

        void ConfirmBuy()
        {
            confirmRoot.SetActive(false);
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/buy",
                new { item_id = _pendingItem.item_id, quantity = 1 },
                (json, err) =>
                {
                    if (err != null) return;
                    TR_HubManager.Instance.RefreshProfile();
                });
        }
    }
}
