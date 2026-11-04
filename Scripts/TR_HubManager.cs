using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Root controller for the local Origin Timeline hub scene.
    // Attach to the HubCanvas root. Manages tab switching and coin display.
    public class TR_HubManager : MonoBehaviour
    {
        public static TR_HubManager Instance { get; private set; }

        [Header("Coin Display")]
        [SerializeField] TextMeshProUGUI coinText;

        [Header("Panels")]
        [SerializeField] GameObject panelInventory;
        [SerializeField] GameObject panelShop;
        [SerializeField] GameObject panelAuction;
        [SerializeField] GameObject panelCustomize;
        [SerializeField] GameObject panelUpgrades;
        [SerializeField] GameObject panelTimelines;   // TR_TeamWarpUI lives here
        [SerializeField] GameObject panelChatbot;

        [Header("Tab Buttons")]
        [SerializeField] Button btnInventory;
        [SerializeField] Button btnShop;
        [SerializeField] Button btnAuction;
        [SerializeField] Button btnCustomize;
        [SerializeField] Button btnUpgrades;
        [SerializeField] Button btnTimelines;
        [SerializeField] Button btnChatbot;

        [Header("AFK Notice")]
        [SerializeField] GameObject afkNoticeRoot;
        [SerializeField] TextMeshProUGUI afkNoticeText;

        PlayerProfile _profile;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Debug.Log("[TR_HubManager] Awake");

            // Assign TMP font at runtime (editor scripting doesn't persist font refs)
            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
                foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
                    tmp.font = font;

            // Disable Kit_MenuManager so its login UI doesn't block clicks.
            // Only disable canvases that are NOT TR_HubCanvas.
            var menuManager = FindObjectOfType<MarsFPSKit.UI.Kit_MenuManager>();
            if (menuManager != null)
            {
                Debug.Log($"[TR_HubManager] Disabling Kit_MenuManager and {menuManager.menuScreens.Length} kit screens");
                foreach (var screen in menuManager.menuScreens)
                    if (screen.root) screen.root.SetActive(false);
                menuManager.enabled = false;
                foreach (var c in menuManager.GetComponentsInChildren<GraphicRaycaster>(true))
                    c.enabled = false;
            }
            else Debug.LogWarning("[TR_HubManager] Kit_MenuManager not found");

            // Set camera to solid dark color so skybox doesn't show through panels
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.16f);
                mainCam.cullingMask = 0;
            }

            // Disable ALL kit canvas GameObjects — they render over TR_HubCanvas
            // Keep EventSystem alive — buttons need it
            foreach (var kitCanvas in FindObjectsOfType<Canvas>(true))
            {
                if (kitCanvas.gameObject == gameObject) continue;
                if (kitCanvas.transform.IsChildOf(transform)) continue;
                // Only disable root canvases with a GraphicRaycaster (visible UI)
                if (kitCanvas.GetComponent<GraphicRaycaster>() == null) continue;
                kitCanvas.gameObject.SetActive(false);
                Debug.Log($"[TR_HubManager] Disabled canvas: {kitCanvas.gameObject.name}");
            }

            Debug.Log($"[TR_HubManager] Canvas enabled={GetComponent<Canvas>()?.enabled} font={font != null}");
        }

        void Start()
        {
            foreach (var c in FindObjectsOfType<Canvas>())
                Debug.Log($"[TR] Canvas: {c.gameObject.name} sortOrder={c.sortingOrder} enabled={c.enabled} raycast={c.GetComponent<GraphicRaycaster>() != null}");

            if (btnInventory) { btnInventory.onClick.AddListener(() => { Debug.Log("[TR] Inventory clicked"); ShowPanel(panelInventory); }); }
            if (btnShop)      { btnShop.onClick.AddListener(()      => { Debug.Log("[TR] Shop clicked");      ShowPanel(panelShop); }); }
            if (btnAuction)   { btnAuction.onClick.AddListener(()   => { Debug.Log("[TR] Auction clicked");   ShowPanel(panelAuction); }); }
            if (btnCustomize) { btnCustomize.onClick.AddListener(() => { Debug.Log("[TR] Customize clicked"); ShowPanel(panelCustomize); }); }
            if (btnUpgrades)  { btnUpgrades.onClick.AddListener(()  => { Debug.Log("[TR] Upgrades clicked");  ShowPanel(panelUpgrades); }); }
            if (btnTimelines) { btnTimelines.onClick.AddListener(() => { Debug.Log("[TR] Timelines clicked"); ShowPanel(panelTimelines); }); }
            if (btnChatbot)   { btnChatbot.onClick.AddListener(()   => { Debug.Log("[TR] Chatbot clicked");   ShowPanel(panelChatbot); }); }

            Debug.Log($"[TR] Buttons wired: inv={btnInventory != null} shop={btnShop != null} timelines={btnTimelines != null}");

            if (panelTimelines) ShowPanel(panelTimelines);
            StartCoroutine(RefreshWhenReady());
        }

        IEnumerator RefreshWhenReady()
        {
            yield return new WaitUntil(() => TR_HubApi.IsReady);
            RefreshProfile();
        }

        public void RefreshProfile()
        {
            if (TR_HubApi.Instance == null) return;
            TR_HubApi.Instance.GetPlayer(p =>
            {
                _profile = p;
                if (coinText) coinText.text = $"⏱ {p.chrono_coins:N0} CC";
            });
        }

        public void ShowAfkNotice()
        {
            afkNoticeRoot.SetActive(true);
            afkNoticeText.text = "You were returned here due to inactivity. Your artifacts have been saved.";
        }

        public void ShowPanel(GameObject target)
        {
            panelInventory.SetActive(panelInventory == target);
            panelShop.SetActive(panelShop == target);
            panelAuction.SetActive(panelAuction == target);
            panelCustomize.SetActive(panelCustomize == target);
            panelUpgrades.SetActive(panelUpgrades == target);
            panelTimelines.SetActive(panelTimelines == target);
            panelChatbot.SetActive(panelChatbot == target);

            if (target == panelTimelines) panelTimelines.GetComponent<TR_TeamWarpUI>()?.OnEnteredOriginTimeline();
            if (target == panelInventory) panelInventory.GetComponent<TR_InventoryPanel>()?.Refresh();
            if (target == panelShop)      panelShop.GetComponent<TR_ShopPanel>()?.Refresh();
            if (target == panelAuction)   panelAuction.GetComponent<TR_AuctionPanel>()?.Refresh();
            if (target == panelCustomize) panelCustomize.GetComponent<TR_CustomizePanel>()?.Refresh();
            if (target == panelUpgrades)  panelUpgrades.GetComponent<TR_UpgradesPanel>()?.Refresh();
        }
    }
}
