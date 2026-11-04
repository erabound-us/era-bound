using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_CustomizePanel : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] Button tabOutfits;
        [SerializeField] Button tabWeaponSkins;
        [SerializeField] Button tabEmotes;

        [Header("List")]
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject cosmeticRowPrefab;   // TR_CosmeticRowUI

        [Header("Shop Link")]
        [SerializeField] Button browseShopBtn;

        readonly List<GameObject> _rows = new List<GameObject>();
        string _activeType = "outfit";

        void Awake()
        {
            if (tabOutfits)     tabOutfits.onClick.AddListener(()     => SetType("outfit"));
            if (tabWeaponSkins) tabWeaponSkins.onClick.AddListener(() => SetType("weapon_skin"));
            if (tabEmotes)      tabEmotes.onClick.AddListener(()      => SetType("emote"));
            if (browseShopBtn)  browseShopBtn.onClick.AddListener(()  => TR_HubManager.Instance?.ShowPanel(null));
        }

        public void Refresh() => LoadCosmetics();

        void SetType(string type) { _activeType = type; LoadCosmetics(); }

        void LoadCosmetics()
        {
            foreach (var r in _rows) Destroy(r);
            _rows.Clear();
            if (!TR_HubApi.IsReady || cosmeticRowPrefab == null || listContainer == null) return;
            TR_HubApi.Instance.Get($"/player/{TR_HubApi.PlayerId}/cosmetics", (json, err) =>
            {
                if (err != null) return;
                var all = JsonHelper.FromJsonArray<CosmeticEntry>(json);
                foreach (var c in all)
                {
                    if (c.cosmetic_type != _activeType) continue;
                    var go  = Instantiate(cosmeticRowPrefab, listContainer, false);
                    var row = go.GetComponent<TR_CosmeticRowUI>();
                    row?.Populate(c, OnEquipPressed);
                    _rows.Add(go);
                }
            });
        }

        void OnEquipPressed(string cosmeticId, string slot, bool equip)
        {
            TR_HubApi.Instance.Post(
                $"/player/{TR_HubApi.PlayerId}/equip-cosmetic",
                new { cosmetic_id = cosmeticId, slot, equip },
                (_, __) => LoadCosmetics());
        }

        [Serializable]
        public class CosmeticEntry
        {
            public int    id;
            public string cosmetic_id;
            public string cosmetic_type;
            public int    is_equipped;
            public string slot;
        }
    }
}
