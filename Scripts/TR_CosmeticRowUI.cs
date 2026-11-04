using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_CosmeticRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI typeText;
        [SerializeField] GameObject equippedBadge;
        [SerializeField] Button equipBtn;
        [SerializeField] TextMeshProUGUI equipBtnText;

        public void Populate(TR_CustomizePanel.CosmeticEntry c, Action<string, string, bool> onEquip)
        {
            nameText.text  = c.cosmetic_id;
            typeText.text  = c.cosmetic_type.Replace("_", " ");
            bool equipped  = c.is_equipped == 1;
            equippedBadge.SetActive(equipped);
            equipBtnText.text = equipped ? "Unequip" : "Equip";

            equipBtn.onClick.RemoveAllListeners();
            equipBtn.onClick.AddListener(() => onEquip(c.cosmetic_id, c.slot, !equipped));
        }
    }
}
