using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_SellValuableRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI detailText;
        [SerializeField] TextMeshProUGUI estimateText;
        [SerializeField] Image legendaryGlow;
        [SerializeField] Button sellBtn;      // instant sell to fence
        [SerializeField] Button listBtn;      // list at auction

        public void Populate(ValuableItem v, Action<int> onSell, Action<int> onList)
        {
            nameText.text   = v.name;
            detailText.text = $"{v.era} · {v.rarity}";
            if (!string.IsNullOrEmpty(v.historical_figure))
                detailText.text += $" · {v.historical_figure}";

            bool legendary = v.is_legendary == 1;
            legendaryGlow.gameObject.SetActive(legendary);

            // Fetch estimate from worker
            string q = $"/valuables/estimate?era={v.era}&rarity={v.rarity}" +
                       $"&is_legendary={(legendary ? 1 : 0)}" +
                       $"&historical_figure={Uri.EscapeDataString(v.historical_figure ?? "")}" +
                       $"&base_value={v.base_value}";

            TR_HubApi.Instance.Get(q, (json, err) =>
            {
                if (err != null) return;
                var result = JsonUtility.FromJson<EstimateResult>(json);
                estimateText.text = $"~{result.estimated_price:N0} CC";
            });

            sellBtn.onClick.RemoveAllListeners();
            sellBtn.onClick.AddListener(() => onSell(v.id));
            listBtn.onClick.RemoveAllListeners();
            listBtn.onClick.AddListener(() => onList(v.id));
        }
        [Serializable] class EstimateResult { public int estimated_price; }
    }
}
