using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class TR_TeamWarpUI : MonoBehaviour
    {
        public static TR_TeamWarpUI Instance { get; private set; }

        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] TextMeshProUGUI playerCountText;
        [SerializeField] RectTransform listContainer;
        [SerializeField] GameObject scrollListGO;
        [SerializeField] GameObject offlinePlaceholderGO;
        [SerializeField] Button offlinePlayBtn;
        [SerializeField] GameObject timelineEntryPrefab;
        [SerializeField] GameObject afkNoticeRoot;
        [SerializeField] TextMeshProUGUI afkNoticeText;

        readonly List<GameObject> _entries = new List<GameObject>();
        bool _showAfkNotice;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Wire offline button permanently — always visible
            if (offlinePlayBtn)
            {
                offlinePlayBtn.onClick.RemoveAllListeners();
                offlinePlayBtn.onClick.AddListener(() => TR_OriginTimelineManager.Instance?.LaunchOfflineMatch());
                offlinePlayBtn.gameObject.SetActive(true);
            }

            StartCoroutine(CheckInternetThenEnter());
        }

        IEnumerator CheckInternetThenEnter()
        {
            if (statusText) statusText.text = "Checking connection\u2026";
            var req = UnityWebRequest.Head("https://www.google.com");
            req.timeout = 5;
            yield return req.SendWebRequest();
            bool online = req.result == UnityWebRequest.Result.Success;
            req.Dispose();

            if (online)
            {
                TR_OriginTimelineManager.Instance?.EnterOriginTimeline();
            }
            else
            {
                if (scrollListGO) scrollListGO.SetActive(false);
                if (offlinePlaceholderGO) offlinePlaceholderGO.SetActive(true);
                if (statusText) statusText.text = "No internet connection";
                if (playerCountText) playerCountText.text = "Practice mode only";
            }
        }

        public void ShowAfkNotice() => _showAfkNotice = true;

        public void OnEnteredOriginTimeline()
        {
            gameObject.SetActive(true);
            if (offlinePlaceholderGO) offlinePlaceholderGO.SetActive(false);
            if (scrollListGO) scrollListGO.SetActive(true);
            if (statusText) statusText.text = "Origin Timeline \u2014 Choose your next destination";
            if (afkNoticeRoot) afkNoticeRoot.SetActive(_showAfkNotice);
            if (_showAfkNotice)
            {
                if (afkNoticeText) afkNoticeText.text = "You were returned here due to inactivity. Your artifacts have been saved.";
                _showAfkNotice = false;
            }
            if (playerCountText) playerCountText.text = "Local \u2014 not in a match";
        }

        public void Reshow()
        {
            // Just ensure offline button is always on
            if (offlinePlayBtn) offlinePlayBtn.gameObject.SetActive(true);
        }

        public void OnWarpedIntoTimeline() => gameObject.SetActive(false);

        public void RefreshTimelineList(IReadOnlyList<TR_OriginTimelineManager.ActiveTimeline> timelines)
        {
            foreach (var e in _entries) Destroy(e);
            _entries.Clear();
            if (offlinePlaceholderGO) offlinePlaceholderGO.SetActive(false);
            if (scrollListGO) scrollListGO.SetActive(true);

            if (timelines.Count == 0)
            {
                if (statusText) statusText.text = "No active timelines \u2014 start a new one!";
                return;
            }

            if (statusText) statusText.text = timelines.Count + " active timeline" + (timelines.Count == 1 ? "" : "s");

            foreach (var tl in timelines)
            {
                var go = Instantiate(timelineEntryPrefab, listContainer, false);
                var entry = go.GetComponent<TR_TimelineEntryUI>();
                if (entry != null) entry.Populate(tl, OnWarpButtonPressed);
                _entries.Add(go);
            }
        }

        public void OnPlayerCountChanged()
        {
            if (playerCountText) playerCountText.text = "Local \u2014 not in a match";
        }

        void OnWarpButtonPressed(string roomName)
        {
            if (statusText) statusText.text = "Warping...";
            TR_OriginTimelineManager.Instance?.WarpIntoTimeline(roomName);
        }

        public void OnStartNewTimelinePressed()
        {
            gameObject.SetActive(false);
            MultiverseTransporterUI.Instance?.Open();
        }
    }
}
