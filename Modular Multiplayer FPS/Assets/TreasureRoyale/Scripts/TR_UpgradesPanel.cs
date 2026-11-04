using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_UpgradesPanel : MonoBehaviour
    {
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject upgradeRowPrefab;   // TR_UpgradeRowUI
        [SerializeField] TextMeshProUGUI statusText;

        readonly List<GameObject> _rows = new List<GameObject>();

        public void Refresh()
        {
            if (listContainer == null) return;
            if (!TR_HubApi.IsReady)
            {
                if (statusText) statusText.text = "Worker offline — run: wrangler dev";
                ShowOfflinePlaceholder("Permanent upgrades appear here.\n\nExamples: extra inventory slots, artifact carry capacity, extraction speed bonuses.");
                return;
            }
            if (upgradeRowPrefab == null) { if (statusText) statusText.text = "Rerun Setup Hub"; return; }
            foreach (var r in _rows) Destroy(r);
            _rows.Clear();

            TR_HubApi.Instance.GetCatalog("", catalog =>
            {
                TR_HubApi.Instance.GetInventory(inv =>
                {
                    var owned = new HashSet<string>();
                    foreach (var u in inv.upgrades) owned.Add(u.upgrade_id);

                    foreach (var item in catalog)
                    {
                        if (item.category != "upgrade") continue;
                        var go  = Instantiate(upgradeRowPrefab, listContainer, false);
                        var row = go.GetComponent<TR_UpgradeRowUI>();
                        row?.Populate(item, owned.Contains(item.item_id), OnBuyUpgrade);
                        _rows.Add(go);
                    }
                });
            });
        }

        void OnBuyUpgrade(string upgradeId)
        {
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/upgrade",
                new { upgrade_id = upgradeId },
                (json, err) =>
                {
                    statusText.text = err != null ? $"Error: {err}" : "Upgrade purchased!";
                    TR_HubManager.Instance.RefreshProfile();
                    Refresh();
                });
        }

        void ShowOfflinePlaceholder(string msg)
        {
            foreach (Transform c in listContainer) Destroy(c.gameObject);
            var go = new GameObject("Placeholder");
            go.transform.SetParent(listContainer, false);
            go.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 120;
            var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = msg; tmp.fontSize = 15; tmp.color = new Color(0.7f,0.7f,0.7f);
            tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = true;
        }
    }
}
