using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to a persistent MainMenu scene object.
    // Called by TR_MatchmakingUI when the player hits Quick Play.
    // Scores all visible era rooms and joins the best fit, or creates a new one.
    public class TR_Matchmaker : MonoBehaviourPunCallbacks
    {
        public static TR_Matchmaker Instance { get; private set; }

        public enum PlayMode { Solo, Team, Timeline }

        // Minimum score (0–1) a room must reach before we join it.
        // Below this we create a fresh room instead.
        [SerializeField] float joinScoreThreshold = 0.3f;

        // How long to wait for the room list before giving up and creating
        [SerializeField] float roomListTimeoutSeconds = 5f;

        bool _searching;
        PlayMode _pendingMode;
        string _pendingBucketId;   // set for Timeline mode; empty = any era
        string _pendingEra;
        int    _pendingTeamId;
        int    _pendingSeed;
        int    _pendingAnchorYear;
        int    _pendingGameModeIndex;
        bool   _pendingIsPublic;

        // Injected by TR_MatchmakingUI before calling StartSearch
        public System.Action<string> OnStatusChanged;
        public System.Action         OnSearchFailed;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ────────────────────────────────────────────────────

        public void StartSearch(
            PlayMode mode,
            string bucketId,
            string era,
            int teamId,
            int seed,
            int anchorYear,
            int gameModeIndex,
            bool isPublic)
        {
            if (_searching) return;
            _searching        = true;
            _pendingMode      = mode;
            _pendingBucketId  = bucketId;
            _pendingEra       = era;
            _pendingTeamId    = teamId;
            _pendingSeed      = seed;
            _pendingAnchorYear = anchorYear;
            _pendingGameModeIndex = gameModeIndex;
            _pendingIsPublic  = isPublic;

            OnStatusChanged?.Invoke(ModeLabel(mode) + " — searching…");
            StartCoroutine(SearchRoutine());
        }

        public void CancelSearch()
        {
            StopAllCoroutines();
            _searching = false;
            OnStatusChanged?.Invoke("Search cancelled.");
        }

        // ── Search logic ──────────────────────────────────────────────────

        IEnumerator SearchRoutine()
        {
            // Give the room list a moment to populate if we just entered the lobby
            float waited = 0f;
            while (TR_OriginTimelineManager.Instance == null ||
                   TR_OriginTimelineManager.Instance.ActiveTimelines.Count == 0)
            {
                waited += Time.deltaTime;
                if (waited >= roomListTimeoutSeconds) break;
                yield return null;
            }

            var best = FindBestRoom(TR_OriginTimelineManager.Instance?.ActiveTimelines);

            if (best.HasValue && best.Value.score >= joinScoreThreshold)
            {
                OnStatusChanged?.Invoke($"Found a match — warping to {FormatEra(best.Value.timeline.era)}…");
                TR_OriginTimelineManager.Instance.WarpIntoTimeline(best.Value.timeline.roomName);
            }
            else
            {
                OnStatusChanged?.Invoke("No suitable match found — creating new timeline…");
                CreateRoom();
            }

            _searching = false;
        }

        struct ScoredRoom
        {
            public TR_OriginTimelineManager.ActiveTimeline timeline;
            public float score;
        }

        ScoredRoom? FindBestRoom(IReadOnlyList<TR_OriginTimelineManager.ActiveTimeline> rooms)
        {
            if (rooms == null || rooms.Count == 0) return null;

            ScoredRoom? best = null;

            foreach (var tl in rooms)
            {
                if (!tl.isOpen || !tl.openToPublic) continue;
                if (tl.playerCount >= tl.maxPlayers) continue;

                float score = ScoreRoom(tl);
                if (best == null || score > best.Value.score)
                    best = new ScoredRoom { timeline = tl, score = score };
            }

            return best;
        }

        float ScoreRoom(TR_OriginTimelineManager.ActiveTimeline tl)
        {
            float score = 0f;

            // Fill rate: prefer rooms that are half-full (social, not empty, not full)
            float fill = tl.maxPlayers > 0 ? (float)tl.playerCount / tl.maxPlayers : 0f;
            score += 0.4f * Mathf.Sin(fill * Mathf.PI); // peaks at 50% fill

            // Era match: Timeline mode requires exact bucket; Team/Solo prefer same era
            if (_pendingMode == PlayMode.Timeline)
            {
                if (!string.IsNullOrEmpty(_pendingBucketId) && tl.bucketId == _pendingBucketId)
                    score += 0.5f;
                else
                    score -= 1f; // wrong timeline — disqualify
            }
            else if (!string.IsNullOrEmpty(_pendingEra) && tl.era == _pendingEra)
            {
                score += 0.2f;
            }

            // Team mode: prefer rooms that already have a team (teamId >= 0)
            if (_pendingMode == PlayMode.Team && tl.teamId >= 0)
                score += 0.2f;

            // Solo mode: prefer rooms with no team (solo-friendly)
            if (_pendingMode == PlayMode.Solo && tl.teamId < 0)
                score += 0.1f;

            return score;
        }

        void CreateRoom()
        {
            string roomName = string.IsNullOrEmpty(_pendingBucketId)
                ? $"auto_{_pendingEra}_{System.Guid.NewGuid().ToString("N")[..6]}"
                : $"{_pendingBucketId}_{System.Guid.NewGuid().ToString("N")[..6]}";

            int maxPlayers = EraMaxPlayers(_pendingEra);

            var options = new RoomOptions
            {
                IsOpen    = true,
                IsVisible = _pendingIsPublic,
                MaxPlayers = (byte)maxPlayers,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "gameModeType",  1 },
                    { "gameMode",      _pendingGameModeIndex },
                    { "era",           _pendingEra },
                    { "bucketId",      _pendingBucketId },
                    { "universeSeed",  _pendingSeed },
                    { "anchorYear",    _pendingAnchorYear },
                    { "teamId",        _pendingTeamId },
                    { "playMode",      (int)_pendingMode },
                    { "type",          "era" },
                    { "openToPublic",  _pendingIsPublic }
                },
                CustomRoomPropertiesForLobby = new[]
                    { "era", "bucketId", "teamId", "type", "openToPublic", "playMode" }
            };

            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        // ── Photon callbacks ──────────────────────────────────────────────

        public override void OnCreatedRoom()
        {
            OnStatusChanged?.Invoke("Timeline created — warping in…");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            OnStatusChanged?.Invoke("Could not create timeline. Try again.");
            OnSearchFailed?.Invoke();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        // Era player caps from steering rules
        static int EraMaxPlayers(string era) => era switch
        {
            "deep_prehistoric" => 8,
            "prehistoric"      => 12,
            "ancient"          => 12,
            "medieval"         => 14,
            "renaissance"      => 16,
            "industrial"       => 16,
            "modern"           => 16,
            "near_future"      => 16,
            "mid_future"       => 16,
            "far_future"       => 16,
            "deep_future"      => 16,
            _                  => 12
        };

        static string ModeLabel(PlayMode m) => m switch
        {
            PlayMode.Solo     => "Solo",
            PlayMode.Team     => "Team",
            PlayMode.Timeline => "Timeline",
            _                 => "Quick Play"
        };

        static string FormatEra(string era) =>
            System.Globalization.CultureInfo.CurrentCulture
                .TextInfo.ToTitleCase(era.Replace('_', ' '));
    }
}
