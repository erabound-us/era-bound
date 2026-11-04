using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Inventory panel root.
    public class TR_InventoryPanel : MonoBehaviour
    {
        [Header("Slot Counter")]
        [SerializeField] TextMeshProUGUI slotCountText;

        [Header("Tabs")]
        [SerializeField] Button tabGear;
        [SerializeField] Button tabValuables;
        [SerializeField] Button tabContainers;
        [SerializeField] Button tabPets;

        [Header("List")]
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject inventoryRowPrefab;   // TR_InventoryRowUI

        [Header("Repair Panel")]
        [SerializeField] GameObject repairConfirmRoot;
        [SerializeField] TextMeshProUGUI repairCostText;
        [SerializeField] Button repairConfirmBtn;
        [SerializeField] Button repairCancelBtn;

        InventoryResponse _data;
        int _pendingRepairId;
        readonly List<GameObject> _rows = new List<GameObject>();

        void Awake()
        {
            if (tabGear)         tabGear.onClick.AddListener(ShowGear);
            if (tabValuables)    tabValuables.onClick.AddListener(ShowValuables);
            if (tabContainers)   tabContainers.onClick.AddListener(ShowContainers);
            if (tabPets)         tabPets.onClick.AddListener(ShowPets);
            if (repairConfirmBtn) repairConfirmBtn.onClick.AddListener(ConfirmRepair);
            if (repairCancelBtn)  repairCancelBtn.onClick.AddListener(() => { if (repairConfirmRoot) repairConfirmRoot.SetActive(false); });
        }

        public void Refresh()
        {
            if (listContainer == null) return;
            if (!TR_HubApi.IsReady)
            {
                if (slotCountText) slotCountText.text = "Offline";
                ShowOfflinePlaceholder(listContainer, "Worker offline — run: wrangler dev\n\nYour artifact gear, valuables, containers and pets appear here.");
                return;
            }
            if (inventoryRowPrefab == null) { if (slotCountText) slotCountText.text = "Rerun Tools > Treasure Royale > Setup Hub"; return; }
            TR_HubApi.Instance.GetInventory(data =>
            {
                _data = data;
                if (slotCountText) slotCountText.text = $"Slots: {data.usedSlots} / {data.totalSlots}";
                ShowGear();
            });
        }

        void ShowGear()
        {
            ClearRows();
            if (_data?.gear == null) return;
            foreach (var item in _data.gear)
            {
                var go  = Instantiate(inventoryRowPrefab, listContainer, false);
                var row = go.GetComponent<TR_InventoryRowUI>();
                row?.PopulateGear(item, OnRepairPressed);
                _rows.Add(go);
            }
        }

        void ShowValuables()
        {
            ClearRows();
            if (_data?.valuables == null) return;
            foreach (var v in _data.valuables)
            {
                var go  = Instantiate(inventoryRowPrefab, listContainer, false);
                var row = go.GetComponent<TR_InventoryRowUI>();
                row?.PopulateValuable(v);
                _rows.Add(go);
            }
        }

        void ShowContainers()
        {
            ClearRows();
            if (_data?.containers == null) return;
            foreach (var c in _data.containers)
            {
                var go  = Instantiate(inventoryRowPrefab, listContainer, false);
                var row = go.GetComponent<TR_InventoryRowUI>();
                row?.PopulateContainer(c, OnEquipContainerPressed);
                _rows.Add(go);
            }
        }

        void ShowPets()
        {
            ClearRows();
            if (_data?.pets == null) return;
            foreach (var p in _data.pets)
            {
                var go  = Instantiate(inventoryRowPrefab, listContainer, false);
                var row = go.GetComponent<TR_InventoryRowUI>();
                row?.PopulatePet(p, OnEquipPetPressed);
                _rows.Add(go);
            }
        }

        void OnRepairPressed(int inventoryId)
        {
            _pendingRepairId = inventoryId;
            TR_HubApi.Instance.Get(
                $"/player/{TR_HubApi.PlayerId}/repair-estimate?inventory_id={inventoryId}",
                (json, err) =>
                {
                    if (err != null) return;
                    var est = JsonUtility.FromJson<RepairEstimate>(json);
                    repairCostText.text = est != null && est.note.Length > 0
                        ? est.note
                        : $"Repair cost: {est.cost:N0} CC";
                    repairConfirmRoot.SetActive(true);
                });
        }

        void ConfirmRepair()
        {
            repairConfirmRoot.SetActive(false);
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/repair",
                new { inventory_id = _pendingRepairId },
                (json, err) =>
                {
                    TR_HubManager.Instance.RefreshProfile();
                    Refresh();
                });
        }

        void OnEquipContainerPressed(int dbId)
        {
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/equip-container",
                new { container_db_id = dbId },
                (_, __) => Refresh());
        }

        void OnEquipPetPressed(int dbId)
        {
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/equip-pet",
                new { pet_db_id = dbId },
                (_, __) => Refresh());
        }

        void ClearRows()
        {
            foreach (var r in _rows) Destroy(r);
            _rows.Clear();
        }

        static void ShowOfflinePlaceholder(RectTransform container, string msg)
        {
            foreach (Transform c in container) Destroy(c.gameObject);
            var go = new GameObject("Placeholder");
            go.transform.SetParent(container, false);
            go.AddComponent<LayoutElement>().preferredHeight = 120;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = msg; tmp.fontSize = 15; tmp.color = new Color(0.7f,0.7f,0.7f);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = true;
        }
        [System.Serializable] class RepairEstimate { public int cost; public string note; }
    }
}
