using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Multiverse Transporter UI canvas root in the MainMenu scene.
    // Handles era selection, calls the seed Lambda, and creates/joins the era room.
    // Room properties written here are read by:
    //   - Kit_PvE_TreasureRoyale (gameModeType, gameMode)
    //   - MapVariantManager (universeSeed)
    //   - TR_OriginTimelineManager (era, bucketId, teamId)
    //   - TR_MidMatchWarpPlugin (teamId)
    //   - TR_InterestManager (era)
    public class MultiverseTransporterUI : MonoBehaviourPunCallbacks
    {
        public static MultiverseTransporterUI Instance { get; private set; }

        // Returned to TR_MatchmakingUI after era resolution in quick-play mode
        public struct ResolvedTimeline
        {
            public string bucketId;
            public string era;
            public string eraLabel;
            public int    anchorYear;
            public int    universeSeed;
        }

        // Set by OpenForQuickPlay — if non-null, skip room creation and call back instead
        System.Action<ResolvedTimeline> _quickPlayCallback;

        [Header("UI")]
        [SerializeField] TMP_InputField yearInput;
        [SerializeField] TextMeshProUGUI resolvedEraLabel;
        [SerializeField] TextMeshProUGUI resolvedAnchorLabel;
        [SerializeField] Button confirmButton;
        [SerializeField] TextMeshProUGUI statusText;

        [Header("Team")]
        [SerializeField] TMP_InputField teamIdInput; // optional — blank = solo

        [Header("Privacy")]
        [SerializeField] UnityEngine.UI.Toggle openToPublicToggle;
        [SerializeField] TextMeshProUGUI privacyLabel;

        [Header("Game Mode Index")]
        [Tooltip("Index of Kit_PvE_TreasureRoyale in Kit_GameInformation.allCoopGameModes")]
        [SerializeField] int treasureRoyaleGameModeIndex = 0;

        // Resolved from Lambda
        string _resolvedBucketId;
        string _resolvedEra;
        int    _resolvedAnchorYear;
        int    _resolvedSeed;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            gameObject.SetActive(false);
        }

        public void Open()
        {
            _quickPlayCallback = null;
            gameObject.SetActive(true);
            confirmButton.interactable = false;
            statusText.text = "Enter a year to begin";
            if (openToPublicToggle != null)
            {
                openToPublicToggle.isOn = true;
                openToPublicToggle.onValueChanged.AddListener(OnPrivacyToggleChanged);
            }
            UpdatePrivacyLabel(true);
        }

        // Called by TR_MatchmakingUI for Timeline quick-play:
        // resolves era/seed but hands control back via callback instead of creating a room
        public void OpenForQuickPlay(System.Action<ResolvedTimeline> callback)
        {
            _quickPlayCallback = callback;
            gameObject.SetActive(true);
            confirmButton.interactable = false;
            statusText.text = "Pick a year for your timeline";
            if (openToPublicToggle != null) openToPublicToggle.gameObject.SetActive(false);
        }

        void OnPrivacyToggleChanged(bool isOpen) => UpdatePrivacyLabel(isOpen);

        void UpdatePrivacyLabel(bool isOpen)
        {
            if (privacyLabel != null)
                privacyLabel.text = isOpen ? "Open — anyone can warp in" : "Private — invite only";
        }

        // Called by the year input field's OnEndEdit event
        public void OnYearSubmitted(string yearStr)
        {
            if (!int.TryParse(yearStr, out int year)) return;
            statusText.text = "Resolving timeline…";
            confirmButton.interactable = false;
            StartCoroutine(ResolveSeedFromLambda(year));
        }

        System.Collections.IEnumerator ResolveSeedFromLambda(int year)
        {
            // POST to API Gateway → generate_universe_seed Lambda
            string url = $"{TRConfig.ApiBaseUrl}/seed?year={year}";
            using var req = UnityEngine.Networking.UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", "Bearer " + TRConfig.CognitoToken);
            yield return req.SendWebRequest();

            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                statusText.text = "Could not resolve timeline. Try again.";
                yield break;
            }

            var response = JsonUtility.FromJson<SeedResponse>(req.downloadHandler.text);
            _resolvedBucketId    = response.bucketId;
            _resolvedEra         = response.era;
            _resolvedAnchorYear  = response.anchorYear;
            _resolvedSeed        = response.universeSeed;

            resolvedEraLabel.text    = response.eraLabel;
            resolvedAnchorLabel.text = $"{response.eventName} ({response.anchorYear} CE/BCE)";
            statusText.text          = "Timeline resolved — confirm to warp";
            confirmButton.interactable = true;
        }

        // Called by the Confirm button
        public void OnConfirmPressed()
        {
            if (string.IsNullOrEmpty(_resolvedBucketId)) return;

            // Quick-play mode: hand resolved data back to TR_MatchmakingUI
            if (_quickPlayCallback != null)
            {
                _quickPlayCallback(new ResolvedTimeline
                {
                    bucketId     = _resolvedBucketId,
                    era          = _resolvedEra,
                    eraLabel     = resolvedEraLabel.text,
                    anchorYear   = _resolvedAnchorYear,
                    universeSeed = _resolvedSeed
                });
                _quickPlayCallback = null;
                gameObject.SetActive(false);
                return;
            }

            int teamId = -1;
            if (!string.IsNullOrEmpty(teamIdInput?.text))
                int.TryParse(teamIdInput.text, out teamId);

            CreateOrJoinEraRoom(teamId);
        }

        void CreateOrJoinEraRoom(int teamId)
        {
            statusText.text = "Warping…";
            confirmButton.interactable = false;

            bool isPublic = openToPublicToggle == null || openToPublicToggle.isOn;

            // Only try to join an existing public room — never drop into a private one
            if (isPublic)
            {
                var filter = new Hashtable { { "bucketId", _resolvedBucketId }, { "openToPublic", true } };
                PhotonNetwork.JoinRandomRoom(filter, 0);
            }
            else
            {
                // Private — skip matchmaking, go straight to create
                OnJoinRandomFailed(0, "private room requested");
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            int teamId = -1;
            if (!string.IsNullOrEmpty(teamIdInput?.text))
                int.TryParse(teamIdInput.text, out teamId);

            bool isPublic = openToPublicToggle == null || openToPublicToggle.isOn;

            string roomName = $"{_resolvedBucketId}_{System.Guid.NewGuid().ToString("N")[..6]}";

            var options = new RoomOptions
            {
                IsOpen    = true,
                IsVisible = isPublic,   // private rooms hidden from browser
                CustomRoomProperties = new Hashtable
                {
                    { "gameModeType",  1 },
                    { "gameMode",      treasureRoyaleGameModeIndex },
                    { "era",           _resolvedEra },
                    { "bucketId",      _resolvedBucketId },
                    { "universeSeed",  _resolvedSeed },
                    { "anchorYear",    _resolvedAnchorYear },
                    { "teamId",        teamId },
                    { "playMode",      (int)TR_Matchmaker.PlayMode.Timeline },
                    { "type",          "era" },
                    { "openToPublic",  isPublic }
                },
                CustomRoomPropertiesForLobby = new[]
                    { "era", "bucketId", "teamId", "type", "openToPublic" }
            };

            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        [System.Serializable]
        class SeedResponse
        {
            public string bucketId;
            public string era;
            public string eraLabel;
            public string eventName;
            public int    anchorYear;
            public int    universeSeed;
        }
    }

    // Centralised config — set at runtime from Cognito auth flow
    public static class TRConfig
    {
        public static string ApiBaseUrl   = "https://your-api-id.execute-api.region.amazonaws.com/prod";
        public static string CognitoToken = "";
    }
}
