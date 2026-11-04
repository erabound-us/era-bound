using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the main matchmaking canvas in the MainMenu scene.
    // Three tabs:
    //   Quick Play — auto-match by mode (Solo / Team / Timeline)
    //   Browse     — manual room list with era/mode filters
    //   Custom     — full era picker + team/privacy options
    public class TR_MatchmakingUI : MonoBehaviour
    {
        public static TR_MatchmakingUI Instance { get; private set; }

        // ── Tab roots ─────────────────────────────────────────────────────
        [Header("Tabs")]
        [SerializeField] GameObject quickPlayTab;
        [SerializeField] GameObject browseTab;
        [SerializeField] GameObject customTab;

        [Header("Tab Buttons")]
        [SerializeField] Button quickPlayTabBtn;
        [SerializeField] Button browseTabBtn;
        [SerializeField] Button customTabBtn;

        // ── Quick Play tab ────────────────────────────────────────────────
        [Header("Quick Play")]
        [SerializeField] Button soloBtn;
        [SerializeField] Button teamBtn;
        [SerializeField] Button timelineBtn;
        [SerializeField] TextMeshProUGUI quickPlayStatusText;
        [SerializeField] Button cancelSearchBtn;

        // Highlight the selected mode button
        [SerializeField] Color selectedModeColor   = new Color(0.2f, 0.7f, 1f);
        [SerializeField] Color unselectedModeColor = new Color(0.15f, 0.15f, 0.2f);

        TR_Matchmaker.PlayMode _selectedMode = TR_Matchmaker.PlayMode.Solo;
        bool _searching;

        // ── Browse tab ────────────────────────────────────────────────────
        [Header("Browse")]
        [SerializeField] RectTransform browseListContainer;
        [SerializeField] GameObject browseEntryPrefab;       // TR_BrowseEntryUI
        [SerializeField] TMP_Dropdown eraFilterDropdown;
        [SerializeField] TMP_Dropdown modeFilterDropdown;
        [SerializeField] Button refreshBrowseBtn;

        readonly List<GameObject> _browseEntries = new List<GameObject>();

        // ── Custom tab ────────────────────────────────────────────────────
        [Header("Custom")]
        // Custom tab delegates entirely to MultiverseTransporterUI
        // We just show/hide it and pass the open signal
        [SerializeField] Button openCustomBtn;

        // ── Shared status ─────────────────────────────────────────────────
        [Header("Shared")]
        [SerializeField] TextMeshProUGUI sharedStatusText;

        // ─────────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Tab buttons
            quickPlayTabBtn.onClick.AddListener(() => ShowTab(0));
            browseTabBtn.onClick.AddListener(()    => ShowTab(1));
            customTabBtn.onClick.AddListener(()    => ShowTab(2));

            // Quick Play mode buttons
            soloBtn.onClick.AddListener(()     => SelectMode(TR_Matchmaker.PlayMode.Solo));
            teamBtn.onClick.AddListener(()     => SelectMode(TR_Matchmaker.PlayMode.Team));
            timelineBtn.onClick.AddListener(() => SelectMode(TR_Matchmaker.PlayMode.Timeline));

            cancelSearchBtn.onClick.AddListener(OnCancelSearch);
            cancelSearchBtn.gameObject.SetActive(false);

            // Browse
            refreshBrowseBtn.onClick.AddListener(RefreshBrowse);
            eraFilterDropdown.onValueChanged.AddListener(_ => RefreshBrowse());
            modeFilterDropdown.onValueChanged.AddListener(_ => RefreshBrowse());

            // Custom
            openCustomBtn.onClick.AddListener(OnOpenCustom);

            // Wire matchmaker callbacks
            if (TR_Matchmaker.Instance != null)
            {
                TR_Matchmaker.Instance.OnStatusChanged = s =>
                {
                    quickPlayStatusText.text = s;
                    sharedStatusText.text    = s;
                };
                TR_Matchmaker.Instance.OnSearchFailed = () =>
                {
                    _searching = false;
                    cancelSearchBtn.gameObject.SetActive(false);
                };
            }

            SelectMode(TR_Matchmaker.PlayMode.Solo);
            ShowTab(0);
            PopulateFilterDropdowns();
        }

        // ── Tab switching ─────────────────────────────────────────────────

        void ShowTab(int index)
        {
            quickPlayTab.SetActive(index == 0);
            browseTab.SetActive(index == 1);
            customTab.SetActive(index == 2);

            if (index == 1) RefreshBrowse();
        }

        // ── Quick Play ────────────────────────────────────────────────────

        void SelectMode(TR_Matchmaker.PlayMode mode)
        {
            _selectedMode = mode;
            SetButtonColor(soloBtn,     mode == TR_Matchmaker.PlayMode.Solo);
            SetButtonColor(teamBtn,     mode == TR_Matchmaker.PlayMode.Team);
            SetButtonColor(timelineBtn, mode == TR_Matchmaker.PlayMode.Timeline);

            quickPlayStatusText.text = mode switch
            {
                TR_Matchmaker.PlayMode.Solo     => "Solo — you'll be placed in any open timeline",
                TR_Matchmaker.PlayMode.Team     => "Team — you'll join or start a team timeline",
                TR_Matchmaker.PlayMode.Timeline => "Timeline — pick an era, we'll find the best match",
                _                               => ""
            };

            // Timeline mode opens the era picker first
            if (mode == TR_Matchmaker.PlayMode.Timeline)
                OpenEraPickerForQuickPlay();
        }

        void OpenEraPickerForQuickPlay()
        {
            // Reuse MultiverseTransporterUI in quick-play mode:
            // it resolves the bucket/seed, then calls back here instead of creating a room
            MultiverseTransporterUI.Instance?.OpenForQuickPlay(OnEraResolvedForQuickPlay);
        }

        void OnEraResolvedForQuickPlay(MultiverseTransporterUI.ResolvedTimeline resolved)
        {
            quickPlayStatusText.text = $"Timeline: {resolved.eraLabel} — press Play to search";
            _pendingResolved = resolved;
        }

        MultiverseTransporterUI.ResolvedTimeline? _pendingResolved;

        // Called by the Play button on the Quick Play tab
        public void OnQuickPlayPressed()
        {
            if (_searching) return;
            if (TR_Matchmaker.Instance == null) return;

            _searching = true;
            cancelSearchBtn.gameObject.SetActive(true);

            // For Timeline mode we need a resolved era; for Solo/Team we use empty (any era)
            var resolved = _pendingResolved ?? default;

            TR_Matchmaker.Instance.StartSearch(
                mode:           _selectedMode,
                bucketId:       resolved.bucketId ?? "",
                era:            resolved.era ?? "",
                teamId:         -1,
                seed:           resolved.universeSeed,
                anchorYear:     resolved.anchorYear,
                gameModeIndex:  0,
                isPublic:       true
            );
        }

        void OnCancelSearch()
        {
            TR_Matchmaker.Instance?.CancelSearch();
            _searching = false;
            cancelSearchBtn.gameObject.SetActive(false);
        }

        // ── Browse tab ────────────────────────────────────────────────────

        void PopulateFilterDropdowns()
        {
            eraFilterDropdown.ClearOptions();
            eraFilterDropdown.AddOptions(new List<string>
            {
                "All Eras",
                "Deep Prehistoric", "Prehistoric", "Ancient", "Medieval",
                "Renaissance", "Industrial", "Modern",
                "Near Future", "Mid Future", "Far Future", "Deep Future"
            });

            modeFilterDropdown.ClearOptions();
            modeFilterDropdown.AddOptions(new List<string> { "All Modes", "Solo", "Team", "Timeline" });
        }

        void RefreshBrowse()
        {
            foreach (var e in _browseEntries) Destroy(e);
            _browseEntries.Clear();

            var timelines = TR_OriginTimelineManager.Instance?.ActiveTimelines;
            if (timelines == null || timelines.Count == 0)
            {
                sharedStatusText.text = "No active timelines found.";
                return;
            }

            string eraFilter  = EraFilterValue();
            int    modeFilter = modeFilterDropdown.value; // 0=all,1=solo,2=team,3=timeline

            int shown = 0;
            foreach (var tl in timelines)
            {
                if (!string.IsNullOrEmpty(eraFilter) && tl.era != eraFilter) continue;
                if (modeFilter > 0 && (int)tl.playMode != modeFilter - 1) continue;

                var go    = Instantiate(browseEntryPrefab, browseListContainer, false);
                var entry = go.GetComponent<TR_BrowseEntryUI>();
                entry?.Populate(tl, OnBrowseWarpPressed);
                _browseEntries.Add(go);
                shown++;
            }

            sharedStatusText.text = shown == 0 ? "No timelines match your filters." : $"{shown} timeline{(shown == 1 ? "" : "s")} found.";
        }

        string EraFilterValue()
        {
            int idx = eraFilterDropdown.value;
            if (idx == 0) return "";
            string[] eras = { "", "deep_prehistoric", "prehistoric", "ancient", "medieval",
                               "renaissance", "industrial", "modern",
                               "near_future", "mid_future", "far_future", "deep_future" };
            return idx < eras.Length ? eras[idx] : "";
        }

        void OnBrowseWarpPressed(string roomName)
        {
            sharedStatusText.text = "Warping…";
            TR_OriginTimelineManager.Instance?.WarpIntoTimeline(roomName);
        }

        // ── Custom tab ────────────────────────────────────────────────────

        void OnOpenCustom()
        {
            MultiverseTransporterUI.Instance?.Open();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        void SetButtonColor(Button btn, bool selected)
        {
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = selected ? selectedModeColor : unselectedModeColor;
        }
    }
}
