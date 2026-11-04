using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to a persistent object in the MainMenu scene alongside Kit_MenuManager.
    // Origin Timeline is now a LOCAL offline scene — no Photon room is held.
    // Photon is reconnected only when the player actively joins a timeline.
    //
    // Players land here after:
    //   - AFK kick (full Photon disconnect)
    //   - Voluntary extraction / match end (LeaveRoom → disconnect)
    //   - First launch
    public class TR_OriginTimelineManager : MonoBehaviourPunCallbacks
    {
        public static TR_OriginTimelineManager Instance { get; private set; }

        // Set by TR_AfkKicker before Disconnect so OnDisconnected shows the AFK notice
        public static bool ReturnFromAfk;

        readonly List<ActiveTimeline> _activeTimelines = new List<ActiveTimeline>();
        public IReadOnlyList<ActiveTimeline> ActiveTimelines => _activeTimelines;

        string _pendingWarpRoom;

        public struct ActiveTimeline
        {
            public string roomName;
            public string era;
            public string bucketId;
            public int    teamId;
            public int    playerCount;
            public int    maxPlayers;
            public bool   isOpen;
            public bool   openToPublic;
            public TR_Matchmaker.PlayMode playMode;
        }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => EnterOriginTimeline();

        // ── Entry point ───────────────────────────────────────────────────

        // Called on first launch or after returning from a match.
        // Connects to Photon (lobby only) so room list populates, but joins no room.
        public void EnterOriginTimeline()
        {
            if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
            {
                Debug.LogWarning("[TR] No Photon App ID set — skipping Photon connect. Set it in PhotonServerSettings.");
                TR_TeamWarpUI.Instance?.OnEnteredOriginTimeline();
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
                return;
            }

            PhotonNetwork.ConnectUsingSettings();
        }

        // ── Warp into an active team timeline ────────────────────────────

        public void WarpIntoTimeline(string roomName)
        {
            _pendingWarpRoom = roomName;

            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings();
            else if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby();
            else
                PhotonNetwork.JoinRoom(_pendingWarpRoom);
        }

        // ── Photon callbacks ──────────────────────────────────────────────

        public override void OnConnectedToMaster()
        {
            if (!string.IsNullOrEmpty(_pendingWarpRoom))
            {
                PhotonNetwork.JoinRoom(_pendingWarpRoom);
                return;
            }

            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            // Room list will start arriving via OnRoomListUpdate.
            // Notify hub UI we're ready.
            TR_TeamWarpUI.Instance?.OnEnteredOriginTimeline();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (ReturnFromAfk)
            {
                ReturnFromAfk = false;
                TR_TeamWarpUI.Instance?.ShowAfkNotice();
                EnterOriginTimeline();
                return;
            }
            // Only reconnect if we have a valid App ID
            if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
                PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnLeftRoom()
        {
            // After leaving an era room, disconnect fully to free the slot,
            // then reconnect to lobby-only mode.
            PhotonNetwork.Disconnect();
        }

        public override void OnJoinedRoom()
        {
            _pendingWarpRoom = null;
            TR_TeamWarpUI.Instance?.OnWarpedIntoTimeline();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"[TR] Failed to join era room: {message}. Returning to origin.");
            _pendingWarpRoom = null;
            PhotonNetwork.JoinLobby();
        }

        // ── Room list ─────────────────────────────────────────────────────

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _activeTimelines.Clear();

            foreach (var info in roomList)
            {
                if (!info.IsOpen || info.RemovedFromList) continue;
                var p = info.CustomProperties;
                if (p == null) continue;
                if (p.ContainsKey("type") && (string)p["type"] == "origin") continue;

                bool openToPublic = !p.ContainsKey("openToPublic") || (bool)p["openToPublic"];
                if (!openToPublic) continue;

                var rawMode = p.ContainsKey("playMode") ? (int)p["playMode"] : 0;
                _activeTimelines.Add(new ActiveTimeline
                {
                    roomName     = info.Name,
                    era          = p.ContainsKey("era")      ? (string)p["era"]      : "unknown",
                    bucketId     = p.ContainsKey("bucketId") ? (string)p["bucketId"] : "",
                    teamId       = p.ContainsKey("teamId")   ? (int)p["teamId"]      : -1,
                    playerCount  = info.PlayerCount,
                    maxPlayers   = info.MaxPlayers,
                    isOpen       = info.IsOpen,
                    openToPublic = openToPublic,
                    playMode     = (TR_Matchmaker.PlayMode)rawMode
                });
            }

            TR_TeamWarpUI.Instance?.RefreshTimelineList(_activeTimelines);
        }
    }
}
