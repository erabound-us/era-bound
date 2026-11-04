using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    public class TR_OriginTimelineManager : MonoBehaviourPunCallbacks
    {
        public static TR_OriginTimelineManager Instance { get; private set; }

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
            // Reset match state when returning to MainMenu
            IsOfflineMatch = false;
            OfflineMatchProps = null;
        }

        void Start() { }

        public void EnterOriginTimeline()
        {
            if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
            {
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

        public void LaunchOfflineMatch(string era = "") => StartCoroutine(LaunchOfflineRoutine(era));

        IEnumerator LaunchOfflineRoutine(string era)
        {
            if (string.IsNullOrEmpty(era))
            {
                string[] eras = { "deep_prehistoric", "prehistoric", "ancient", "medieval",
                                  "renaissance", "industrial", "modern" };
                era = eras[Random.Range(0, eras.Length)];
            }

            OfflineMatchProps = new ExitGames.Client.Photon.Hashtable
            {
                { "gameModeType",  1 },
                { "gameMode",      0 },
                { "era",           era },
                { "bucketId",      era + "_offline" },
                { "universeSeed",  Random.Range(1, 999999) },
                { "anchorYear",    0 },
                { "teamId",        -1 },
                { "type",          "era" },
                { "openToPublic",  false }
            };

            IsOfflineMatch = true;
            Kit_GameSettings.currentNetworkingMode = KitNetworkingMode.Traditional;
            Kit_SceneSyncer.instance.LoadScene("Showcase_Map");
            yield break;
        }

        public static ExitGames.Client.Photon.Hashtable OfflineMatchProps;
        public static bool IsOfflineMatch;

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
            if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
                PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        public override void OnJoinedRoom()
        {
            _pendingWarpRoom = null;
            TR_TeamWarpUI.Instance?.OnWarpedIntoTimeline();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning("[TR] Failed to join era room: " + message + ". Returning to origin.");
            _pendingWarpRoom = null;
            PhotonNetwork.JoinLobby();
        }

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
