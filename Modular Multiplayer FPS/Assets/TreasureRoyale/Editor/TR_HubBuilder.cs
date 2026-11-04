#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MarsFPSKit
{
    public static class TR_HubBuilder
    {
        const string PREFAB_ROOT = "Assets/TreasureRoyale/Prefabs";
        const string THEME_PATH  = "Assets/TreasureRoyale/Themes/TR_HubTheme_Default.asset";

        static TR_HubTheme T;

        [MenuItem("Tools/Treasure Royale/1 - Build Hub Prefabs")]
        static void Build()
        {
            T = AssetDatabase.LoadAssetAtPath<TR_HubTheme>(THEME_PATH);
            if (T == null) { Debug.LogError("[TR Builder] Theme not found. Run step 0 first."); return; }

            BuildRowPrefabs();
            BuildPanelTimelines();
            BuildPanelInventory();
            BuildPanelShop();
            BuildPanelUpgrades();
            BuildSimplePanel("TR_Panel_Auction",   "Auction House",  "Browse, sell, and list valuables",      "MarsFPSKit.TR_AuctionPanel");
            BuildSimplePanel("TR_Panel_Customize", "Customize",      "Outfits, weapon skins, emotes",         "MarsFPSKit.TR_CustomizePanel");
            BuildSimplePanel("TR_Panel_Chatbot",   "Era Guide",      "Ask the multiverse historian anything", "MarsFPSKit.TR_EraChatbotPanel");
            BuildHubCanvas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TR Builder] Done.");
        }

        // ── Canvas ────────────────────────────────────────────────────────

        static void BuildHubCanvas()
        {
            var root = new GameObject("TR_HubCanvas");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<TR_HubManager>();

            // Tab bar — pinned to bottom
            var tabBar = MakeImage(root, "TabBar", T.tabBarBg);
            Stretch(tabBar, new Vector2(0,0), new Vector2(1,0), new Vector2(0,0), new Vector2(0, T.tabBarHeight));

            // Coin bar — pinned to top
            var coinBar = MakeImage(root, "CoinBar", T.coinBarBg);
            Stretch(coinBar, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-T.coinBarHeight), new Vector2(0,0));
            var coinTmp = MakeTMP(coinBar, "CoinDisplay", "⏱ 10,000 CC", T.fontBody, T.coinTextColor);
            Stretch(coinTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(0,0), new Vector2(-10,0));
            coinTmp.alignment = TextAlignmentOptions.MidlineRight;

            // Panels — stretch between coin bar and tab bar
            string[] panelNames = {
                "TR_Panel_Timelines","TR_Panel_Inventory","TR_Panel_Shop",
                "TR_Panel_Auction","TR_Panel_Customize","TR_Panel_Upgrades","TR_Panel_Chatbot"
            };
            var panels = new GameObject[panelNames.Length];
            for (int i = 0; i < panelNames.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ROOT + "/" + panelNames[i] + ".prefab");
                if (prefab == null) { Debug.LogError("[TR Builder] Missing panel prefab: " + panelNames[i]); continue; }
                // Instantiate then immediately unpack so RectTransform stretch is baked into canvas prefab
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                panels[i] = inst;
                // Stretch to fill between coin bar top and tab bar bottom
                Stretch(panels[i], new Vector2(0,0), new Vector2(1,1),
                    new Vector2(0, T.tabBarHeight), new Vector2(0, -T.coinBarHeight));
                panels[i].SetActive(i == 0);
            }

            // Tab buttons
            string[] labels = { "Timelines","Inventory","Shop","Auction","Customize","Upgrades","Era Guide" };
            var buttons = new Button[7];
            for (int i = 0; i < 7; i++)
            {
                float x0 = (float)i/7, x1 = (float)(i+1)/7;
                var btnGO = MakeImage(tabBar, "Btn_" + labels[i].Replace(" ",""), T.tabBtnNormal);
                Stretch(btnGO, new Vector2(x0,0), new Vector2(x1,1), new Vector2(2,2), new Vector2(-2,-2));
                var btn = btnGO.AddComponent<Button>();
                var bc = btn.colors;
                bc.highlightedColor = T.tabBtnHover;
                bc.pressedColor     = T.tabBtnPressed;
                btn.colors = bc;
                var lbl = MakeTMP(btnGO, "Label", labels[i], T.fontBody, T.textPrimary);
                Stretch(lbl.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                lbl.alignment = TextAlignmentOptions.Center;
                buttons[i] = btn;
            }

            // AFK notice
            var afkRoot = MakeImage(root, "AfkNotice", new Color(0.6f,0.1f,0.1f,0.9f));
            Stretch(afkRoot, new Vector2(0.25f,0.88f), new Vector2(0.75f,0.97f), Vector2.zero, Vector2.zero);
            afkRoot.SetActive(false);
            var afkTmp = MakeTMP(afkRoot, "AfkText", "You were returned here due to inactivity.", T.fontBody, T.textPrimary);
            Stretch(afkTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(10,5), new Vector2(-10,-5));
            afkTmp.alignment = TextAlignmentOptions.Center;

            // Wire TR_HubManager
            var mgr = root.GetComponent<TR_HubManager>();
            var so = new SerializedObject(mgr);
            so.FindProperty("coinText").objectReferenceValue       = coinTmp;
            so.FindProperty("panelTimelines").objectReferenceValue = panels[0];
            so.FindProperty("panelInventory").objectReferenceValue = panels[1];
            so.FindProperty("panelShop").objectReferenceValue      = panels[2];
            so.FindProperty("panelAuction").objectReferenceValue   = panels[3];
            so.FindProperty("panelCustomize").objectReferenceValue = panels[4];
            so.FindProperty("panelUpgrades").objectReferenceValue  = panels[5];
            so.FindProperty("panelChatbot").objectReferenceValue   = panels[6];
            so.FindProperty("btnTimelines").objectReferenceValue   = buttons[0];
            so.FindProperty("btnInventory").objectReferenceValue   = buttons[1];
            so.FindProperty("btnShop").objectReferenceValue        = buttons[2];
            so.FindProperty("btnAuction").objectReferenceValue     = buttons[3];
            so.FindProperty("btnCustomize").objectReferenceValue   = buttons[4];
            so.FindProperty("btnUpgrades").objectReferenceValue    = buttons[5];
            so.FindProperty("btnChatbot").objectReferenceValue     = buttons[6];
            so.FindProperty("afkNoticeRoot").objectReferenceValue  = afkRoot;
            so.FindProperty("afkNoticeText").objectReferenceValue  = afkTmp;
            so.ApplyModifiedProperties();

            SavePrefab(root, PREFAB_ROOT + "/TR_HubCanvas.prefab");
        }

        // ── Panel builders ────────────────────────────────────────────────

        static void BuildPanelTimelines()
        {
            var root = MakePanel("TR_Panel_Timelines", "MarsFPSKit.TR_TeamWarpUI");

            var statusTmp = MakeTMP(root, "StatusText",
                "Origin Timeline — Choose your destination", T.fontTitle, T.textPrimary);
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontTitle * 1.5f;

            var pcTmp = MakeTMP(root, "PlayerCountText", "", T.fontBody, T.textSecondary);
            pcTmp.alignment = TextAlignmentOptions.Center;
            pcTmp.gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontBody * 1.5f;

            var listGO = MakeScrollList(root, "TimelineList");
            listGO.AddComponent<LayoutElement>().flexibleHeight = 1;

            var afkRoot = MakeImage(root, "AfkNotice", new Color(0.6f,0.1f,0.1f,0.9f));
            afkRoot.AddComponent<LayoutElement>().preferredHeight = 40;
            afkRoot.SetActive(false);
            var afkTmp = MakeTMP(afkRoot, "Text", "Returned due to inactivity.", T.fontSmall, T.textPrimary);
            Stretch(afkTmp.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            afkTmp.alignment = TextAlignmentOptions.Center;

            var warpUI = root.GetComponent<TR_TeamWarpUI>();
            if (warpUI != null)
            {
                var so = new SerializedObject(warpUI);
                so.FindProperty("statusText").objectReferenceValue      = statusTmp;
                so.FindProperty("playerCountText").objectReferenceValue = pcTmp;
                so.FindProperty("listContainer").objectReferenceValue   = listGO.GetComponent<RectTransform>();
                so.FindProperty("afkNoticeRoot").objectReferenceValue   = afkRoot;
                so.FindProperty("afkNoticeText").objectReferenceValue   = afkTmp;
                so.ApplyModifiedProperties();
            }
            WirePrefabRef(root, "MarsFPSKit.TR_TeamWarpUI", "timelineEntryPrefab", "TR_TimelineEntryPrefab");
            SavePrefab(root, PREFAB_ROOT + "/TR_Panel_Timelines.prefab");
        }

        static void BuildPanelInventory()
        {
            var root = MakePanel("TR_Panel_Inventory", "MarsFPSKit.TR_InventoryPanel");

            string[] tabs = { "Gear","Valuables","Containers","Pets" };
            var tabRow = MakeImage(root, "TabRow", new Color(0.1f,0.1f,0.15f));
            tabRow.AddComponent<LayoutElement>().preferredHeight = 44;
            var tabBtns = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float x0 = (float)i/4, x1 = (float)(i+1)/4;
                var b = MakeImage(tabRow, "Tab_"+tabs[i], new Color(0.25f,0.25f,0.38f));
                Stretch(b, new Vector2(x0,0), new Vector2(x1,1), new Vector2(2,2), new Vector2(-2,-2));
                tabBtns[i] = b.AddComponent<Button>();
                var lbl = MakeTMP(b, "Label", tabs[i], T.fontSmall, T.textPrimary);
                Stretch(lbl.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                lbl.alignment = TextAlignmentOptions.Center;
            }

            var slotTmp = MakeTMP(root, "SlotCount", "Slots: 0 / 16", T.fontSmall, T.textSecondary);
            slotTmp.alignment = TextAlignmentOptions.MidlineRight;
            slotTmp.gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontSmall * 1.5f;

            var (listGO, content) = MakeScrollListWithContent(root, "ItemList");
            listGO.AddComponent<LayoutElement>().flexibleHeight = 1;

            var inv = root.GetComponent<TR_InventoryPanel>();
            if (inv != null)
            {
                var so = new SerializedObject(inv);
                so.FindProperty("tabGear").objectReferenceValue       = tabBtns[0];
                so.FindProperty("tabValuables").objectReferenceValue  = tabBtns[1];
                so.FindProperty("tabContainers").objectReferenceValue = tabBtns[2];
                so.FindProperty("tabPets").objectReferenceValue       = tabBtns[3];
                so.FindProperty("slotCountText").objectReferenceValue = slotTmp;
                so.FindProperty("listContainer").objectReferenceValue = content;
                so.ApplyModifiedProperties();
            }
            WirePrefabRef(root, "MarsFPSKit.TR_InventoryPanel", "inventoryRowPrefab", "TR_InventoryRowPrefab");
            SavePrefab(root, PREFAB_ROOT + "/TR_Panel_Inventory.prefab");
        }

        static void BuildPanelShop()
        {
            var root = MakePanel("TR_Panel_Shop", "MarsFPSKit.TR_ShopPanel");
            MakeTMP(root, "Title", "NPC Shop", T.fontTitle, T.textPrimary)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontTitle * 1.5f;
            var (listGO, content) = MakeScrollListWithContent(root, "ItemList");
            listGO.AddComponent<LayoutElement>().flexibleHeight = 1;
            var shop = root.GetComponent<TR_ShopPanel>();
            if (shop != null)
            {
                var so = new SerializedObject(shop);
                so.FindProperty("listContainer").objectReferenceValue = content;
                so.ApplyModifiedProperties();
            }
            WirePrefabRef(root, "MarsFPSKit.TR_ShopPanel", "shopRowPrefab", "TR_ShopRowPrefab");
            SavePrefab(root, PREFAB_ROOT + "/TR_Panel_Shop.prefab");
        }

        static void BuildPanelUpgrades()
        {
            var root = MakePanel("TR_Panel_Upgrades", "MarsFPSKit.TR_UpgradesPanel");
            MakeTMP(root, "Title", "Permanent Upgrades", T.fontTitle, T.textPrimary)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontTitle * 1.5f;
            var sTmp = MakeTMP(root, "StatusText", "", T.fontBody, T.textAccent);
            sTmp.gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontBody * 1.5f;
            var (listGO, content) = MakeScrollListWithContent(root, "ItemList");
            listGO.AddComponent<LayoutElement>().flexibleHeight = 1;
            var up = root.GetComponent<TR_UpgradesPanel>();
            if (up != null)
            {
                var so = new SerializedObject(up);
                so.FindProperty("listContainer").objectReferenceValue = content;
                so.FindProperty("statusText").objectReferenceValue    = sTmp;
                so.ApplyModifiedProperties();
            }
            WirePrefabRef(root, "MarsFPSKit.TR_UpgradesPanel", "upgradeRowPrefab", "TR_UpgradeRowPrefab");
            SavePrefab(root, PREFAB_ROOT + "/TR_Panel_Upgrades.prefab");
        }

        static void BuildSimplePanel(string prefabName, string title, string subtitle, string compType)
        {
            var root = MakePanel(prefabName, compType);
            MakeTMP(root, "Title", title, T.fontTitle, T.textPrimary)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontTitle * 1.5f;
            MakeTMP(root, "Subtitle", subtitle, T.fontSubtitle, T.textSecondary)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = T.fontSubtitle * 1.5f;
            SavePrefab(root, PREFAB_ROOT + "/" + prefabName + ".prefab");
        }

        // ── Row prefabs ───────────────────────────────────────────────────

        static void BuildRowPrefabs()
        {
            string[] names = {
                "TR_InventoryRowPrefab","TR_ShopRowPrefab","TR_AuctionRowPrefab",
                "TR_SellValuableRowPrefab","TR_TimelineEntryPrefab",
                "TR_UpgradeRowPrefab","TR_CosmeticRowPrefab"
            };
            string[] types = {
                "MarsFPSKit.TR_InventoryRowUI","MarsFPSKit.TR_ShopRowUI","MarsFPSKit.TR_AuctionRowUI",
                "MarsFPSKit.TR_SellValuableRowUI","MarsFPSKit.TR_TimelineEntryUI",
                "MarsFPSKit.TR_UpgradeRowUI","MarsFPSKit.TR_CosmeticRowUI"
            };
            for (int i = 0; i < names.Length; i++)
                BuildRowPrefab(names[i], types[i]);
        }

        static void BuildRowPrefab(string prefabName, string typeName)
        {
            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            var go = new GameObject(prefabName);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, T.rowHeight);
            go.AddComponent<Image>().color = T.rowBg;
            if (type != null) go.AddComponent(type);

            var nameTmp = MakeTMP(go, "NameText", "Item Name", T.fontBody, T.textPrimary);
            var nrt = nameTmp.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0,0.5f); nrt.anchorMax = new Vector2(0.6f,1f);
            nrt.offsetMin = new Vector2(10,0);   nrt.offsetMax = Vector2.zero;

            var detailTmp = MakeTMP(go, "DetailText", "Detail", T.fontSmall, T.textSecondary);
            var drt = detailTmp.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0,0);    drt.anchorMax = new Vector2(0.6f,0.5f);
            drt.offsetMin = new Vector2(10,0);   drt.offsetMax = Vector2.zero;

            var btnGO = new GameObject("ActionButton");
            btnGO.transform.SetParent(go.transform, false);
            var brt = btnGO.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.72f,0.1f); brt.anchorMax = new Vector2(0.98f,0.9f);
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            btnGO.AddComponent<Image>().color = T.actionBtnColor;
            btnGO.AddComponent<Button>();
            var btnLbl = MakeTMP(btnGO, "Label", "Action", T.fontSmall, T.textPrimary);
            Stretch(btnLbl.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            btnLbl.alignment = TextAlignmentOptions.Center;

            SavePrefab(go, PREFAB_ROOT + "/" + prefabName + ".prefab");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        static GameObject MakePanel(string name, string compTypeName)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = T.panelBg;
            img.raycastTarget = false;
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.spacing = 8;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            var type = System.Type.GetType(compTypeName + ", Assembly-CSharp");
            if (type != null) go.AddComponent(type);
            return go;
        }

        static GameObject MakeImage(GameObject parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>().color = color;
            return go;
        }

        static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (T.font != null) tmp.font = T.font;
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        static GameObject MakeScrollList(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>().color = T.listBg;
            var sv = go.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;
            var content = new GameObject("Content");
            content.transform.SetParent(go.transform, false);
            var crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0,1); crt.anchorMax = new Vector2(1,1);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            sv.content = crt;
            return go;
        }

        static (GameObject list, RectTransform content) MakeScrollListWithContent(GameObject parent, string name)
        {
            var go = MakeScrollList(parent, name);
            return (go, go.transform.Find("Content").GetComponent<RectTransform>());
        }

        static void Stretch(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        }

        static void WirePrefabRef(GameObject panelGO, string compTypeName, string fieldName, string rowPrefabName)
        {
            var type = System.Type.GetType(compTypeName + ", Assembly-CSharp");
            if (type == null) return;
            var comp = panelGO.GetComponent(type) as MonoBehaviour;
            if (comp == null) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ROOT + "/" + rowPrefabName + ".prefab");
            if (prefab == null) return;
            var so = new SerializedObject(comp);
            so.FindProperty(fieldName).objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
        }

        static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
#endif
