using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Origin Timeline UI canvas root.
    // Displays active team timelines and lets the player warp into one.
    // Populated by TR_OriginTimelineManager.OnRoomListUpdate.
    public class TR_TeamWarpUI : MonoBehaviour
    {
        public static TR_TeamWarpUI Instance { get; private set; }

        [Header("Hub Status")]
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] TextMeshProUGUI playerCountText;

        [Header("Timeline List")]
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject timelineEntryPrefab;  // see TR_TimelineEntryUI

        [Header("AFK Notice")]
        [SerializeField] GameObject afkNoticeRoot;
        [SerializeField] TextMeshProUGUI afkNoticeText;

        readonly List<GameObject> _entries = new List<GameObject>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => OnEnteredOriginTimeline();

        // ── Called by TR_OriginTimelineManager ───────────────────────────

        bool _showAfkNotice;

        // Called by TR_OriginTimelineManager.OnDisconnected BEFORE flag is cleared
        public void ShowAfkNotice() => _showAfkNotice = true;

        public void OnEnteredOriginTimeline()
        {
            gameObject.SetActive(true);
            if (statusText) statusText.text = "Origin Timeline — Choose your next destination";
            if (afkNoticeRoot) afkNoticeRoot.SetActive(_showAfkNotice);
            if (_showAfkNotice)
            {
                if (afkNoticeText) afkNoticeText.text = "You were returned here due to inactivity. Your artifacts have been saved.";
                _showAfkNotice = false;
            }
            UpdatePlayerCount();
        }

        public void OnWarpedIntoTimeline()
        {
            gameObject.SetActive(false);
        }

        public void RefreshTimelineList(IReadOnlyList<TR_OriginTimelineManager.ActiveTimeline> timelines)
        {
            // Clear old entries
            foreach (var e in _entries) Destroy(e);
            _entries.Clear();

            if (timelines.Count == 0)
            {
                statusText.text = "No active timelines — start a new one!";
                return;
            }

            statusText.text = $"{timelines.Count} active timeline{(timelines.Count == 1 ? "" : "s")}";

            foreach (var tl in timelines)
            {
                var go = Instantiate(timelineEntryPrefab, listContainer, false);
                var entry = go.GetComponent<TR_TimelineEntryUI>();
                if (entry != null)
                    entry.Populate(tl, OnWarpButtonPressed);
                _entries.Add(go);
            }
        }

        public void OnPlayerCountChanged() => UpdatePlayerCount();

        // ── Button callbacks ──────────────────────────────────────────────

        void OnWarpButtonPressed(string roomName)
        {
            statusText.text = "Warping…";
            TR_OriginTimelineManager.Instance?.WarpIntoTimeline(roomName);
        }

        public void OnStartNewTimelinePressed()
        {
            // Delegates to MultiverseTransporterUI which handles era selection + room creation
            gameObject.SetActive(false);
            MultiverseTransporterUI.Instance?.Open();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        void UpdatePlayerCount()
        {
            if (playerCountText) playerCountText.text = "Local — not in a match";
            Debug.Log("[TR_TeamWarpUI] OnEnteredOriginTimeline fired");
        }
    }
}
