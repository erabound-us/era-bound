using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_HubManager : MonoBehaviour
    {
        public static TR_HubManager Instance { get; private set; }

        TextMeshProUGUI _coinText;
        GameObject _panelTimelines, _panelInventory, _panelShop,
                   _panelAuction, _panelCustomize, _panelUpgrades, _panelChatbot;
        TMP_FontAsset _font;

        static readonly Color BgDark      = new Color(0.08f, 0.08f, 0.13f);
        static readonly Color PanelBg     = new Color(0.18f, 0.12f, 0.35f);
        static readonly Color TabBarBg    = new Color(0.10f, 0.08f, 0.22f);
        static readonly Color TabNormal   = new Color(0.28f, 0.20f, 0.50f);
        static readonly Color TabHover    = new Color(0.45f, 0.35f, 0.70f);
        static readonly Color TabPressed  = new Color(0.70f, 0.55f, 0.10f);
        static readonly Color CoinBarBg   = new Color(0.10f, 0.06f, 0.25f, 1f);
        static readonly Color Gold        = new Color(1.00f, 0.85f, 0.20f);
        static readonly Color White       = Color.white;
        static readonly Color Grey        = new Color(0.85f, 0.85f, 0.85f);
        static readonly Color ListBg      = new Color(0.10f, 0.07f, 0.20f, 1f);
        static readonly Color AccentGreen  = new Color(0.40f, 0.90f, 0.40f);
        static readonly Color OfflineBanner = new Color(0.55f, 0.20f, 0.05f, 1f);

        const float TAB_H      = 60f;
        const float COIN_H     = 40f;
        const float BANNER_H   = 28f;
        const float BOTTOM_H   = TAB_H + BANNER_H;
        const float F_TITLE = 28f;
        const float F_BODY  = 18f;
        const float F_SMALL = 14f;

        PlayerProfile _profile;
        GameObject _offlineBannerGO;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            Build();
            DisableKitUI();
            SetupCamera();
        }

        void Start()
        {
            StartCoroutine(InitialShow());
            StartCoroutine(RefreshWhenReady());
            StartCoroutine(PollConnectionBanner());
        }

        IEnumerator PollConnectionBanner()
        {
            var wait = new WaitForSeconds(3f);
            while (true)
            {
                bool offline = !Photon.Pun.PhotonNetwork.IsConnected;
                if (_offlineBannerGO) _offlineBannerGO.SetActive(offline);
                yield return wait;
            }
        }

        void DisableKitUI()
        {
            var mm = FindObjectOfType<MarsFPSKit.UI.Kit_MenuManager>();
            if (mm != null)
            {
                foreach (var s in mm.menuScreens)
                    if (s.root) s.root.SetActive(false);
                mm.enabled = false;
            }
            foreach (var c in FindObjectsOfType<Canvas>(true))
            {
                if (c.gameObject.name == "TR_HubCanvas") continue;
                if (c.transform.IsChildOf(transform)) continue;
                if (c.GetComponent<GraphicRaycaster>() != null)
                    c.gameObject.SetActive(false);
            }
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = BgDark;
            cam.cullingMask     = 0;
        }

        void Build()
        {
            var canvasGO = new GameObject("TR_HubCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            var ui = canvasGO.transform;

            var coinBar = MkRect("CoinBar", ui, CoinBarBg);
            Fill(coinBar, 0, 1, 1, 1, 0, -COIN_H, 0, 0);
            _coinText = Txt(coinBar, "CoinText", "10,000 CC", F_BODY, Gold);
            Fill(_coinText.gameObject, 0, 1, 0, 1, 8, 0, -8, 0);
            _coinText.alignment = TextAlignmentOptions.MidlineRight;

            var tabBar = MkRect("TabBar", ui, TabBarBg);
            Fill(tabBar, 0, 1, 0, 0, 0, BANNER_H, 0, BANNER_H + TAB_H);

            _offlineBannerGO = MkRect("OfflineBanner", ui, OfflineBanner);
            Fill(_offlineBannerGO, 0, 1, 0, 0, 0, 0, 0, BANNER_H);
            _offlineBannerGO.GetComponent<Image>().raycastTarget = false;
            var bannerTxt = Txt(_offlineBannerGO, "Txt",
                "⚠  No internet connection — timeline traversal and artifact trading unavailable. Practice mode only.",
                F_SMALL, White);
            Fill(bannerTxt.gameObject, 0, 1, 0, 1, 12, 0, -12, 0);
            bannerTxt.alignment = TextAlignmentOptions.Midline;
            bannerTxt.enableWordWrapping = true;
            _offlineBannerGO.SetActive(!Photon.Pun.PhotonNetwork.IsConnected);

            _panelTimelines = PanelTimelines(ui);
            _panelInventory = PanelInventory(ui);
            _panelShop      = PanelShop(ui);
            _panelAuction   = PanelSimple(ui, "TR_Panel_Auction",   "Auction House",  "Browse, sell, and list valuables",      typeof(TR_AuctionPanel));
            _panelCustomize = PanelSimple(ui, "TR_Panel_Customize", "Customize",      "Outfits, weapon skins, emotes",         typeof(TR_CustomizePanel));
            _panelUpgrades  = PanelUpgrades(ui);
            _panelChatbot   = PanelSimple(ui, "TR_Panel_Chatbot",   "Era Guide",      "Ask the multiverse historian anything", typeof(TR_EraChatbotPanel));

            string[] labels = { "Timelines","Inventory","Shop","Auction","Customize","Upgrades","Era Guide" };
            GameObject[] panels = { _panelTimelines,_panelInventory,_panelShop,_panelAuction,_panelCustomize,_panelUpgrades,_panelChatbot };
            for (int i = 0; i < 7; i++)
            {
                int idx = i;
                var btnGO = MkRect("Btn_"+labels[i], tabBar.transform, TabNormal);
                Fill(btnGO, (float)i/7, (float)(i+1)/7, 0, 1, 2, 2, -2, -2);
                var btn = btnGO.AddComponent<Button>();
                var bc = btn.colors; bc.highlightedColor = TabHover; bc.pressedColor = TabPressed; btn.colors = bc;
                var lbl = Txt(btnGO, "Lbl", labels[i], F_SMALL, White);
                Fill(lbl.gameObject, 0, 1, 0, 1, 4, 2, -4, -2);
                lbl.alignment = TextAlignmentOptions.Center;
                btn.onClick.AddListener(() => ShowPanel(panels[idx]));
            }
        }

        IEnumerator InitialShow()
        {
            yield return null;
            ShowPanel(_panelTimelines);
        }

        // ── Panel builders ────────────────────────────────────────────────

        GameObject PanelTimelines(Transform ui)
        {
            var root   = MkPanel("TR_Panel_Timelines", ui);
            root.SetActive(true);
            var warpUI = root.AddComponent<TR_TeamWarpUI>();
            root.SetActive(false);
            var body   = MkBody(root);

            var st = Txt(body, "StatusText", "Origin Timeline", F_TITLE, White);
            st.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            st.alignment = TextAlignmentOptions.Center;

            var pc = Txt(body, "PlayerCountText", "", F_BODY, Grey);
            pc.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
            pc.alignment = TextAlignmentOptions.Center;

            var (list, content) = ScrollList(body, "TimelineList");
            list.AddComponent<LayoutElement>().flexibleHeight = 1;

            var phGO = MkRect("OfflinePlaceholder", root.transform, new Color(0.08f, 0.05f, 0.18f, 1f));
            var phRT = phGO.GetComponent<RectTransform>();
            phRT.anchorMin = new Vector2(0f, 0.05f);
            phRT.anchorMax = new Vector2(1f, 0.85f);
            phRT.offsetMin = new Vector2(20f, 0f);
            phRT.offsetMax = new Vector2(-20f, 0f);
            var phText = new GameObject("PlaceholderText");
            phText.transform.SetParent(phGO.transform, false);
            var phTextRT = phText.AddComponent<RectTransform>();
            phTextRT.anchorMin = Vector2.zero; phTextRT.anchorMax = Vector2.one;
            phTextRT.offsetMin = new Vector2(12, 12); phTextRT.offsetMax = new Vector2(-12, -12);
            var phTMP = phText.AddComponent<TextMeshProUGUI>();
            if (_font) phTMP.font = _font;
            phTMP.text = "No internet connection detected.\n\nTo traverse timelines and collect real artifacts from across history, you'll need an active connection.\n\nYou can still explore and practice in offline mode using the button below.";
            phTMP.fontSize = F_BODY;
            phTMP.color = Color.white;
            phTMP.alignment = TextAlignmentOptions.TopLeft;
            phTMP.enableWordWrapping = true;
            phGO.SetActive(false);

            // Offline practice button — sits at the bottom of the panel, inside panel bounds
            var offlineBtnGO = MkRect("OfflinePlayBtn", root.transform, new Color(0.55f, 0.35f, 0.10f));
            var offlineBtnRT = offlineBtnGO.GetComponent<RectTransform>();
            offlineBtnRT.anchorMin = new Vector2(0.2f, 0f);
            offlineBtnRT.anchorMax = new Vector2(0.8f, 0f);
            offlineBtnRT.offsetMin = new Vector2(0f,  8f);
            offlineBtnRT.offsetMax = new Vector2(0f, 52f);
            var offlineBtn = offlineBtnGO.AddComponent<Button>();
            var offlineLbl = Txt(offlineBtnGO, "Lbl", "Play Practice Match (Offline)", F_BODY, White);
            Fill(offlineLbl.gameObject, 0, 1, 0, 1, 4, 2, -4, -2);
            offlineLbl.alignment = TextAlignmentOptions.Center;
            offlineBtnGO.SetActive(true); // always visible

            Inject(warpUI, "statusText",           st);
            Inject(warpUI, "playerCountText",      pc);
            Inject(warpUI, "scrollListGO",         list);
            Inject(warpUI, "listContainer",        content);
            Inject(warpUI, "offlinePlaceholderGO", phGO);
            Inject(warpUI, "offlinePlayBtn",       offlineBtn);
            return root;
        }

        GameObject PanelInventory(Transform ui)
        {
            var root = MkPanel("TR_Panel_Inventory", ui);
            var inv  = root.AddComponent<TR_InventoryPanel>();
            var body = MkBody(root);

            var tabRow = MkRect("TabRow", body.transform, new Color(0.08f,0.08f,0.14f));
            tabRow.AddComponent<LayoutElement>().preferredHeight = 44;
            string[] tabs = { "Gear","Valuables","Containers","Pets" };
            var tabBtns = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var b = MkRect("Tab_"+tabs[i], tabRow.transform, new Color(0.22f,0.22f,0.36f));
                Fill(b, (float)i/4, (float)(i+1)/4, 0, 1, 2, 2, -2, -2);
                tabBtns[i] = b.AddComponent<Button>();
                var lbl = Txt(b, "Lbl", tabs[i], F_SMALL, White);
                Fill(lbl.gameObject, 0, 1, 0, 1, 2, 2, -2, -2);
                lbl.alignment = TextAlignmentOptions.Center;
            }

            var slot = Txt(body, "SlotCount", "Slots: 0 / 16", F_SMALL, Grey);
            slot.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            slot.alignment = TextAlignmentOptions.MidlineRight;

            var (list, content) = ScrollList(body, "ItemList");
            list.AddComponent<LayoutElement>().flexibleHeight = 1;

            Inject(inv, "tabGear",       tabBtns[0]);
            Inject(inv, "tabValuables",  tabBtns[1]);
            Inject(inv, "tabContainers", tabBtns[2]);
            Inject(inv, "tabPets",       tabBtns[3]);
            Inject(inv, "slotCountText", slot);
            Inject(inv, "listContainer", content);
            return root;
        }

        GameObject PanelShop(Transform ui)
        {
            var root = MkPanel("TR_Panel_Shop", ui);
            var shop = root.AddComponent<TR_ShopPanel>();
            var body = MkBody(root);

            var title = Txt(body, "Title", "NPC Shop", F_TITLE, White);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            title.alignment = TextAlignmentOptions.Center;

            var (list, content) = ScrollList(body, "ItemList");
            list.AddComponent<LayoutElement>().flexibleHeight = 1;

            Inject(shop, "listContainer", content);
            return root;
        }

        GameObject PanelUpgrades(Transform ui)
        {
            var root = MkPanel("TR_Panel_Upgrades", ui);
            var up   = root.AddComponent<TR_UpgradesPanel>();
            var body = MkBody(root);

            var title = Txt(body, "Title", "Permanent Upgrades", F_TITLE, White);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            title.alignment = TextAlignmentOptions.Center;

            var st = Txt(body, "StatusText", "", F_BODY, AccentGreen);
            st.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

            var (list, content) = ScrollList(body, "ItemList");
            list.AddComponent<LayoutElement>().flexibleHeight = 1;

            Inject(up, "listContainer", content);
            Inject(up, "statusText",    st);
            return root;
        }

        GameObject PanelSimple(Transform ui, string name, string title, string subtitle, System.Type comp)
        {
            var root = MkPanel(name, ui);
            var c    = (MonoBehaviour)root.AddComponent(comp);
            var body = MkBody(root);

            var t = Txt(body, "Title", title, F_TITLE, White);
            t.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            t.alignment = TextAlignmentOptions.Center;

            var s = Txt(body, "Subtitle", subtitle, F_BODY, Grey);
            s.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
            s.alignment = TextAlignmentOptions.Center;

            var (list, content) = ScrollList(body, "ItemList");
            list.AddComponent<LayoutElement>().flexibleHeight = 1;

            Inject(c, "listContainer",   content);
            Inject(c, "browseContainer", content);
            Inject(c, "responseText",    s);     // TR_EraChatbotPanel
            Inject(c, "statusText",      s);     // TR_AuctionPanel
            return root;
        }

        // ── Panel management ──────────────────────────────────────────────

        public void ShowPanel(GameObject target)
        {
            _panelTimelines.SetActive(_panelTimelines == target);
            _panelInventory.SetActive(_panelInventory == target);
            _panelShop.SetActive(_panelShop == target);
            _panelAuction.SetActive(_panelAuction == target);
            _panelCustomize.SetActive(_panelCustomize == target);
            _panelUpgrades.SetActive(_panelUpgrades == target);
            _panelChatbot.SetActive(_panelChatbot == target);

            if (target == _panelTimelines) _panelTimelines.GetComponent<TR_TeamWarpUI>()?.Reshow();
            if (target == _panelInventory) _panelInventory.GetComponent<TR_InventoryPanel>()?.Refresh();
            if (target == _panelShop)      _panelShop.GetComponent<TR_ShopPanel>()?.Refresh();
            if (target == _panelUpgrades)  _panelUpgrades.GetComponent<TR_UpgradesPanel>()?.Refresh();
            if (target == _panelAuction)   _panelAuction.GetComponent<TR_AuctionPanel>()?.Refresh();
            if (target == _panelCustomize) _panelCustomize.GetComponent<TR_CustomizePanel>()?.Refresh();
            if (target == _panelChatbot)   _panelChatbot.GetComponent<TR_EraChatbotPanel>()?.Refresh();
        }

        IEnumerator RefreshWhenReady()
        {
            yield return new WaitUntil(() => TR_HubApi.IsReady);
            RefreshProfile();
        }

        public void RefreshProfile()
        {
            TR_HubApi.Instance?.GetPlayer(p =>
            {
                _profile = p;
                if (_coinText) _coinText.text = p.chrono_coins.ToString("N0") + " CC";
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────

        GameObject MkPanel(string name, Transform parent)
        {
            var go = MkRect(name, parent, PanelBg);
            Fill(go, 0, 1, 0, 1, 0, BOTTOM_H, 0, -COIN_H);
            go.GetComponent<Image>().raycastTarget = false;
            go.SetActive(false);
            return go;
        }

        GameObject MkBody(GameObject panel)
        {
            var go = new GameObject("Body");
            go.transform.SetParent(panel.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 12); rt.offsetMax = new Vector2(-12, -12);
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight     = true;
            vlg.childControlWidth      = true;
            vlg.childAlignment = TextAnchor.UpperLeft;
            return go;
        }

        GameObject MkRect(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            return go;
        }

        void Fill(GameObject go, float ax0, float ax1, float ay0, float ay1,
                  float ox0, float oy0, float ox1, float oy1)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(ax0, ay0); rt.anchorMax = new Vector2(ax1, ay1);
            rt.offsetMin = new Vector2(ox0, oy0); rt.offsetMax = new Vector2(ox1, oy1);
        }

        TextMeshProUGUI Txt(GameObject parent, string name, string text, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (_font) tmp.font = _font;
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        (GameObject go, RectTransform content) ScrollList(GameObject parent, string name)
        {
            var go = MkRect(name, parent.transform, ListBg);
            var sv = go.AddComponent<ScrollRect>();
            sv.vertical = true; sv.horizontal = false;

            var vp = new GameObject("Viewport");
            vp.transform.SetParent(go.transform, false);
            var vprt = vp.AddComponent<RectTransform>();
            vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
            vprt.offsetMin = vprt.offsetMax = Vector2.zero;
            vp.AddComponent<Image>().color = Color.clear;
            vp.AddComponent<Mask>().showMaskGraphic = false;
            sv.viewport = vprt;

            var content = new GameObject("Content");
            content.transform.SetParent(vp.transform, false);
            var crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0,1); crt.anchorMax = new Vector2(1,1);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4; vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            sv.content = crt;
            return (go, crt);
        }

        void Inject(MonoBehaviour target, string field, object value)
        {
            var f = target.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            f?.SetValue(target, value);
        }
    }
}
