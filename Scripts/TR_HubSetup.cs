#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarsFPSKit.UI;

namespace MarsFPSKit
{
    public static class TR_HubSetup
    {
        const string MAIN_MENU_SCENE = "Assets/314 Arts/MarsFPSKit/Scenes/MainMenu.unity";
        const string PREFABS_PATH    = "Assets/TreasureRoyale/Prefabs";

        const float TAB_BAR_HEIGHT  = 60f;
        const float COIN_BAR_HEIGHT = 40f;
        const int   TAB_COUNT       = 7;

        [MenuItem("Tools/Treasure Royale/Open Hub Scene")]
        static void OpenHubScene()
        {
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);
            Debug.Log("[TR] MainMenu.unity opened and set as active scene.");
        }

        [MenuItem("Tools/Treasure Royale/Setup Hub (Run Once)")]
        static void SetupHub()
        {
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            var menuManager = Object.FindObjectOfType<Kit_MenuManager>();
            if (menuManager == null)
            {
                Debug.LogError("[TR Setup] Kit_MenuManager not found in MainMenu scene.");
                return;
            }

            // ── Destroy existing TR objects so we can rebuild cleanly ─────
            DestroyIfExists("TR_HubCanvas");
            DestroyIfExists("TR_PersistentManager");

            // ── Persistent manager ────────────────────────────────────────
            var persistent = new GameObject("TR_PersistentManager");
            AddComp(persistent, "MarsFPSKit.TR_HubApi");
            AddComp(persistent, "MarsFPSKit.TR_OriginTimelineManager");
            AddComp(persistent, "MarsFPSKit.TR_PlatformQualityBootstrap");
            AddComp(persistent, "MarsFPSKit.TR_HubBootstrap");

            // ── Hub Canvas ────────────────────────────────────────────────
            var hubGO  = new GameObject("TR_HubCanvas");
            var canvas = Undo.AddComponent<Canvas>(hubGO);
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = Undo.AddComponent<CanvasScaler>(hubGO);
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            Undo.AddComponent<GraphicRaycaster>(hubGO);
            AddComp(hubGO, "MarsFPSKit.TR_HubManager");

            // ── Panels FIRST (renders behind tab bar) ─────────────────────
            var panelTimelines = MakePanel(hubGO, "Panel_Timelines");
            var panelInventory = MakePanel(hubGO, "Panel_Inventory");
            var panelShop      = MakePanel(hubGO, "Panel_Shop");
            var panelAuction   = MakePanel(hubGO, "Panel_Auction");
            var panelCustomize = MakePanel(hubGO, "Panel_Customize");
            var panelUpgrades  = MakePanel(hubGO, "Panel_Upgrades");
            var panelChatbot   = MakePanel(hubGO, "Panel_Chatbot");

            AddComp(panelTimelines, "MarsFPSKit.TR_TeamWarpUI");
            AddComp(panelInventory, "MarsFPSKit.TR_InventoryPanel");
            AddComp(panelShop,      "MarsFPSKit.TR_ShopPanel");
            AddComp(panelAuction,   "MarsFPSKit.TR_AuctionPanel");
            AddComp(panelCustomize, "MarsFPSKit.TR_CustomizePanel");
            AddComp(panelUpgrades,  "MarsFPSKit.TR_UpgradesPanel");
            AddComp(panelChatbot,   "MarsFPSKit.TR_EraChatbotPanel");

            // ── Coin bar (top) — renders above panels ─────────────────────
            var coinBar = MakeRect(hubGO, "CoinBar");
            SetAnchors(coinBar, new Vector2(0, 1), new Vector2(1, 1),
                       new Vector2(0, -COIN_BAR_HEIGHT), new Vector2(0, 0));
            coinBar.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var coinGO   = MakeRect(coinBar, "CoinDisplay");
            SetAnchors(coinGO, new Vector2(1, 0), new Vector2(1, 1),
                       new Vector2(-200, 0), new Vector2(-10, 0));
            var coinText = coinGO.AddComponent<TextMeshProUGUI>();
            coinText.text      = "⏱ 10,000 CC";
            coinText.fontSize  = 22;
            coinText.alignment = TextAlignmentOptions.MidlineRight;
            coinText.color     = new Color(1f, 0.85f, 0.2f);

            // ── Tab bar (bottom) — renders above panels ───────────────────
            var tabBar = MakeRect(hubGO, "TabBar");
            SetAnchors(tabBar, new Vector2(0, 0), new Vector2(1, 0),
                       new Vector2(0, 0), new Vector2(0, TAB_BAR_HEIGHT));
            tabBar.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            // ── Tab buttons ───────────────────────────────────────────────
            string[] labels = { "Timelines","Inventory","Shop","Auction","Customize","Upgrades","Era Guide" };
            var panels = new GameObject[]
                { panelTimelines, panelInventory, panelShop, panelAuction, panelCustomize, panelUpgrades, panelChatbot };

            var buttons = new Button[TAB_COUNT];
            for (int i = 0; i < TAB_COUNT; i++)
            {
                float xMin = (float)i / TAB_COUNT;
                float xMax = (float)(i + 1) / TAB_COUNT;

                var btnGO = MakeRect(tabBar, "Btn_" + labels[i].Replace(" ", ""));
                SetAnchors(btnGO, new Vector2(xMin, 0), new Vector2(xMax, 1),
                           new Vector2(2, 2), new Vector2(-2, -2));

                var btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

                var btn = btnGO.AddComponent<Button>();
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.3f, 0.3f, 0.45f);
                colors.pressedColor     = new Color(0.5f, 0.4f, 0.1f);
                btn.colors = colors;

                var lblGO = MakeRect(btnGO, "Label");
                SetAnchors(lblGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var tmp = lblGO.AddComponent<TextMeshProUGUI>();

                tmp.text      = labels[i];
                tmp.fontSize  = 16;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color     = Color.white;

                buttons[i] = btn;
            }

            // ── AFK notice ────────────────────────────────────────────────
            var afkRoot = MakeRect(hubGO, "AfkNotice");
            SetAnchors(afkRoot, new Vector2(0.25f, 0.8f), new Vector2(0.75f, 0.95f),
                       Vector2.zero, Vector2.zero);
            afkRoot.AddComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 0.9f);
            afkRoot.SetActive(false);

            var afkTextGO = MakeRect(afkRoot, "AfkText");
            SetAnchors(afkTextGO, Vector2.zero, Vector2.one, new Vector2(10,5), new Vector2(-10,-5));
            var afkText = afkTextGO.AddComponent<TextMeshProUGUI>();
            afkText.text      = "You were returned here due to inactivity.";
            afkText.fontSize  = 18;
            afkText.alignment = TextAlignmentOptions.Center;

            // ── Wire TR_HubManager ────────────────────────────────────────
            var hubManager = hubGO.GetComponent("TR_HubManager") as MonoBehaviour;
            if (hubManager != null)
            {
                var so = new SerializedObject(hubManager);
                so.FindProperty("coinText").objectReferenceValue        = coinText;
                so.FindProperty("panelTimelines").objectReferenceValue  = panelTimelines;
                so.FindProperty("panelInventory").objectReferenceValue  = panelInventory;
                so.FindProperty("panelShop").objectReferenceValue       = panelShop;
                so.FindProperty("panelAuction").objectReferenceValue    = panelAuction;
                so.FindProperty("panelCustomize").objectReferenceValue  = panelCustomize;
                so.FindProperty("panelUpgrades").objectReferenceValue   = panelUpgrades;
                so.FindProperty("panelChatbot").objectReferenceValue    = panelChatbot;
                so.FindProperty("btnTimelines").objectReferenceValue    = buttons[0];
                so.FindProperty("btnInventory").objectReferenceValue    = buttons[1];
                so.FindProperty("btnShop").objectReferenceValue         = buttons[2];
                so.FindProperty("btnAuction").objectReferenceValue      = buttons[3];
                so.FindProperty("btnCustomize").objectReferenceValue    = buttons[4];
                so.FindProperty("btnUpgrades").objectReferenceValue     = buttons[5];
                so.FindProperty("btnChatbot").objectReferenceValue      = buttons[6];
                so.FindProperty("afkNoticeRoot").objectReferenceValue   = afkRoot;
                so.FindProperty("afkNoticeText").objectReferenceValue   = afkText;
                so.ApplyModifiedProperties();
            }

            // ── Panel interiors ───────────────────────────────────────────
            WireInventoryPanel(panelInventory);
            WireShopPanel(panelShop);
            WireUpgradesPanel(panelUpgrades);
            WireSimpleLabel(panelAuction,   "Auction House",  "Browse, sell, and list valuables");
            WireSimpleLabel(panelCustomize, "Customize",      "Outfits, weapon skins, emotes");
            WireSimpleLabel(panelChatbot,   "Era Guide",      "Ask the multiverse historian anything");

            // ── Wire TR_TeamWarpUI ────────────────────────────────────────
            var statusGO = MakeRect(panelTimelines, "StatusText");
            SetAnchors(statusGO, new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
            var statusTmp = statusGO.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "Origin Timeline — Choose your destination";
            statusTmp.fontSize = 24;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = Color.white;

            var pcGO = MakeRect(panelTimelines, "PlayerCountText");
            SetAnchors(pcGO, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.86f), Vector2.zero, Vector2.zero);
            var pcTmp = pcGO.AddComponent<TextMeshProUGUI>();
            pcTmp.text = ""; pcTmp.fontSize = 16; pcTmp.alignment = TextAlignmentOptions.Center;
            pcTmp.color = new Color(0.7f, 0.7f, 0.7f);

            var listGO = MakeRect(panelTimelines, "TimelineList");
            SetAnchors(listGO, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.76f), Vector2.zero, Vector2.zero);
            var sv = listGO.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;

            var tlAfkRoot = MakeRect(panelTimelines, "AfkNotice");
            SetAnchors(tlAfkRoot, new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.96f), Vector2.zero, Vector2.zero);
            tlAfkRoot.AddComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 0.9f);
            tlAfkRoot.SetActive(false);
            var tlAfkTextGO = MakeRect(tlAfkRoot, "Text");
            SetAnchors(tlAfkTextGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tlAfkText = tlAfkTextGO.AddComponent<TextMeshProUGUI>();
            tlAfkText.text = "Returned due to inactivity."; tlAfkText.fontSize = 16;
            tlAfkText.alignment = TextAlignmentOptions.Center;

            var teamWarpUI = panelTimelines.GetComponent("TR_TeamWarpUI") as MonoBehaviour;
            if (teamWarpUI != null)
            {
                var so = new SerializedObject(teamWarpUI);
                so.FindProperty("statusText").objectReferenceValue      = statusTmp;
                so.FindProperty("playerCountText").objectReferenceValue = pcTmp;
                so.FindProperty("listContainer").objectReferenceValue   = listGO.GetComponent<RectTransform>() ?? listGO.AddComponent<RectTransform>();
                so.FindProperty("afkNoticeRoot").objectReferenceValue   = tlAfkRoot;
                so.FindProperty("afkNoticeText").objectReferenceValue   = tlAfkText;
                so.ApplyModifiedProperties();
            }

            // ── Hook onLogin → TR_HubBootstrap.OnLogin ────────────────────
            var bootstrap = persistent.GetComponent("TR_HubBootstrap") as MonoBehaviour;
            if (bootstrap != null)
            {
                var soMM  = new SerializedObject(menuManager);
                var calls = soMM.FindProperty("onLogin.m_PersistentCalls.m_Calls");
                if (calls != null && calls.arraySize == 0)
                {
                    UnityEventTools.AddVoidPersistentListener(
                        menuManager.onLogin,
                        System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction),
                            bootstrap, "OnLogin"
                        ) as UnityEngine.Events.UnityAction);
                }
                soMM.ApplyModifiedProperties();
            }

            // ── Row prefabs ───────────────────────────────────────────────
            CreateRowPrefab("TR_InventoryRowPrefab",    "MarsFPSKit.TR_InventoryRowUI");
            CreateRowPrefab("TR_ShopRowPrefab",         "MarsFPSKit.TR_ShopRowUI");
            CreateRowPrefab("TR_AuctionRowPrefab",      "MarsFPSKit.TR_AuctionRowUI");
            CreateRowPrefab("TR_SellValuableRowPrefab", "MarsFPSKit.TR_SellValuableRowUI");
            CreateRowPrefab("TR_TimelineEntryPrefab",   "MarsFPSKit.TR_TimelineEntryUI");
            CreateRowPrefab("TR_UpgradeRowPrefab",      "MarsFPSKit.TR_UpgradeRowUI");
            CreateRowPrefab("TR_CosmeticRowPrefab",     "MarsFPSKit.TR_CosmeticRowUI");

            // ── Save ──────────────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TR Setup] Done. Saved={saved}. Hub wired into MainMenu.unity.");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        static void DestroyIfExists(string name)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go == null) continue;
                try { if (go.name == name && go.scene.isLoaded) Object.DestroyImmediate(go); }
                catch { }
            }
        }

        static void AddComp(GameObject go, string typeName)
        {
            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) { Debug.LogWarning($"[TR Setup] Type not found: {typeName}"); return; }
            if (go.GetComponent(type) == null) go.AddComponent(type);
        }

        static GameObject MakeRect(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        // offsetMin = bottom-left offset from anchor, offsetMax = top-right offset from anchor
        static void SetAnchors(GameObject go,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        static GameObject MakePanel(GameObject parent, string name)
        {
            var go = MakeRect(parent, name);
            SetAnchors(go, Vector2.zero, Vector2.one,
                       new Vector2(0, TAB_BAR_HEIGHT), new Vector2(0, -COIN_BAR_HEIGHT));
            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.18f, 1f);
            img.raycastTarget = false;
            go.SetActive(false);
            return go;
        }

        static void WireSimpleLabel(GameObject panel, string title, string subtitle)
        {
            var titleGO = MakeRect(panel, "Title");
            SetAnchors(titleGO, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.85f), Vector2.zero, Vector2.zero);
            var t = titleGO.AddComponent<TextMeshProUGUI>();
            t.text = title; t.fontSize = 28; t.alignment = TextAlignmentOptions.Center; t.color = Color.white;

            var subGO = MakeRect(panel, "Subtitle");
            SetAnchors(subGO, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.72f), Vector2.zero, Vector2.zero);
            var s = subGO.AddComponent<TextMeshProUGUI>();
            s.text = subtitle; s.fontSize = 18; s.alignment = TextAlignmentOptions.Center;
            s.color = new Color(0.7f, 0.7f, 0.7f);
        }

        static void WireInventoryPanel(GameObject panel)
        {
            // Tab buttons row
            string[] tabs = { "Gear", "Valuables", "Containers", "Pets" };
            var tabBar = MakeRect(panel, "TabRow");
            SetAnchors(tabBar, new Vector2(0,0.88f), new Vector2(1,0.97f), new Vector2(10,0), new Vector2(-10,0));
            tabBar.AddComponent<Image>().color = new Color(0.1f,0.1f,0.15f);
            var tabBtns = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float x0 = (float)i/4, x1 = (float)(i+1)/4;
                var b = MakeRect(tabBar, "Tab_"+tabs[i]);
                SetAnchors(b, new Vector2(x0,0), new Vector2(x1,1), new Vector2(2,2), new Vector2(-2,-2));
                b.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.38f);
                tabBtns[i] = b.AddComponent<Button>();
                var lbl = MakeRect(b, "Label");
                SetAnchors(lbl, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var tmp = lbl.AddComponent<TextMeshProUGUI>();
                tmp.text = tabs[i]; tmp.fontSize = 15; tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            // Slot counter
            var slotGO = MakeRect(panel, "SlotCount");
            SetAnchors(slotGO, new Vector2(0,0.82f), new Vector2(1,0.89f), new Vector2(10,0), new Vector2(-10,0));
            var slotTmp = slotGO.AddComponent<TextMeshProUGUI>();
            slotTmp.text = "Slots: 0 / 16"; slotTmp.fontSize = 15;
            slotTmp.alignment = TextAlignmentOptions.MidlineRight; slotTmp.color = new Color(0.7f,0.7f,0.7f);
            // Scroll list
            var listGO = MakeRect(panel, "ItemList");
            SetAnchors(listGO, new Vector2(0.02f,0.05f), new Vector2(0.98f,0.82f), Vector2.zero, Vector2.zero);
            listGO.AddComponent<Image>().color = new Color(0,0,0,0.3f);
            var sv = listGO.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;
            var content = MakeRect(listGO, "Content");
            SetAnchors(content, new Vector2(0,1), new Vector2(1,1), Vector2.zero, Vector2.zero);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            sv.content = content.GetComponent<RectTransform>();

            var inv = panel.GetComponent("TR_InventoryPanel") as MonoBehaviour;
            if (inv != null)
            {
                var so = new SerializedObject(inv);
                so.FindProperty("tabGear").objectReferenceValue       = tabBtns[0];
                so.FindProperty("tabValuables").objectReferenceValue  = tabBtns[1];
                so.FindProperty("tabContainers").objectReferenceValue = tabBtns[2];
                so.FindProperty("tabPets").objectReferenceValue       = tabBtns[3];
                so.FindProperty("slotCountText").objectReferenceValue = slotTmp;
                so.FindProperty("listContainer").objectReferenceValue = content.GetComponent<RectTransform>();
                so.ApplyModifiedProperties();
            }
        }

        static void WireShopPanel(GameObject panel)
        {
            var titleGO = MakeRect(panel, "Title");
            SetAnchors(titleGO, new Vector2(0.02f,0.9f), new Vector2(0.5f,0.97f), Vector2.zero, Vector2.zero);
            var t = titleGO.AddComponent<TextMeshProUGUI>();
            t.text = "NPC Shop"; t.fontSize = 24; t.color = Color.white;

            var listGO = MakeRect(panel, "ItemList");
            SetAnchors(listGO, new Vector2(0.02f,0.05f), new Vector2(0.98f,0.88f), Vector2.zero, Vector2.zero);
            listGO.AddComponent<Image>().color = new Color(0,0,0,0.3f);
            var sv = listGO.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;
            var content = MakeRect(listGO, "Content");
            SetAnchors(content, new Vector2(0,1), new Vector2(1,1), Vector2.zero, Vector2.zero);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            sv.content = content.GetComponent<RectTransform>();

            var shop = panel.GetComponent("TR_ShopPanel") as MonoBehaviour;
            if (shop != null)
            {
                var so = new SerializedObject(shop);
                so.FindProperty("listContainer").objectReferenceValue = content.GetComponent<RectTransform>();
                so.ApplyModifiedProperties();
            }
        }

        static void WireUpgradesPanel(GameObject panel)
        {
            var titleGO = MakeRect(panel, "Title");
            SetAnchors(titleGO, new Vector2(0.02f,0.9f), new Vector2(0.7f,0.97f), Vector2.zero, Vector2.zero);
            var t = titleGO.AddComponent<TextMeshProUGUI>();
            t.text = "Permanent Upgrades"; t.fontSize = 24; t.color = Color.white;

            var statusGO = MakeRect(panel, "StatusText");
            SetAnchors(statusGO, new Vector2(0.02f,0.83f), new Vector2(0.98f,0.9f), Vector2.zero, Vector2.zero);
            var sTmp = statusGO.AddComponent<TextMeshProUGUI>();
            sTmp.text = ""; sTmp.fontSize = 15; sTmp.color = new Color(0.4f,0.9f,0.4f);

            var listGO = MakeRect(panel, "ItemList");
            SetAnchors(listGO, new Vector2(0.02f,0.05f), new Vector2(0.98f,0.83f), Vector2.zero, Vector2.zero);
            listGO.AddComponent<Image>().color = new Color(0,0,0,0.3f);
            var sv = listGO.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;
            var content = MakeRect(listGO, "Content");
            SetAnchors(content, new Vector2(0,1), new Vector2(1,1), Vector2.zero, Vector2.zero);
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            sv.content = content.GetComponent<RectTransform>();

            var up = panel.GetComponent("TR_UpgradesPanel") as MonoBehaviour;
            if (up != null)
            {
                var so = new SerializedObject(up);
                so.FindProperty("listContainer").objectReferenceValue = content.GetComponent<RectTransform>();
                so.FindProperty("statusText").objectReferenceValue    = sTmp;
                so.ApplyModifiedProperties();
            }
        }

        static Button MakeTabButton(GameObject parent, string name, string label)
        {
            var rt  = MakeRect(parent, name);
            rt.gameObject.AddComponent<Image>();
            var btn = rt.gameObject.AddComponent<Button>();
            var lbl = MakeRect(rt.gameObject, "Label");
            SetAnchors(lbl, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tmp = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 16; tmp.alignment = TextAlignmentOptions.Center;
            return btn;
        }

        static void CreateRowPrefab(string prefabName, string typeName)
        {
            string path = PREFABS_PATH + "/" + prefabName + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) { Debug.LogWarning($"[TR Setup] Type not found: {typeName}"); return; }

            var go = new GameObject(prefabName);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);
            go.AddComponent(type);

            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(go.transform, false);
            var nrt = nameGO.AddComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 0.5f); nrt.anchorMax = new Vector2(0.5f, 1f);
            nrt.offsetMin = new Vector2(10, 0);   nrt.offsetMax = Vector2.zero;
            var ntmp = nameGO.AddComponent<TextMeshProUGUI>();
            ntmp.text = "Item Name"; ntmp.fontSize = 16;

            var detailGO = new GameObject("DetailText");
            detailGO.transform.SetParent(go.transform, false);
            var drt = detailGO.AddComponent<RectTransform>();
            drt.anchorMin = new Vector2(0, 0);    drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.offsetMin = new Vector2(10, 0);   drt.offsetMax = Vector2.zero;
            var dtmp = detailGO.AddComponent<TextMeshProUGUI>();
            dtmp.text = "Detail"; dtmp.fontSize = 13; dtmp.color = new Color(0.7f, 0.7f, 0.7f);

            var btnGO = new GameObject("ActionButton");
            btnGO.transform.SetParent(go.transform, false);
            var brt = btnGO.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.75f, 0.1f); brt.anchorMax = new Vector2(0.98f, 0.9f);
            brt.offsetMin = Vector2.zero;              brt.offsetMax = Vector2.zero;
            btnGO.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.8f);
            btnGO.AddComponent<Button>();
            var btnLbl = new GameObject("Label");
            btnLbl.transform.SetParent(btnGO.transform, false);
            var blrt = btnLbl.AddComponent<RectTransform>();
            blrt.anchorMin = Vector2.zero; blrt.anchorMax = Vector2.one;
            blrt.offsetMin = Vector2.zero; blrt.offsetMax = Vector2.zero;
            var btmp = btnLbl.AddComponent<TextMeshProUGUI>();
            btmp.text = "Action"; btmp.fontSize = 14; btmp.alignment = TextAlignmentOptions.Center;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
#endif
