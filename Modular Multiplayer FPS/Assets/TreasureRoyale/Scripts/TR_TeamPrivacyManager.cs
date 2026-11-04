using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the in-match pause menu or HUD alongside a PhotonView.
    // Lets the team toggle their timeline between Open (anyone can warp in)
    // and Private (invite only — room is closed to random joins).
    //
    // Any player can request the toggle; MasterClient is the authority.
    // State is stored in room custom properties so it survives MasterClient migration.
    public class TR_TeamPrivacyManager : MonoBehaviourPun, IInRoomCallbacks
    {
        public static TR_TeamPrivacyManager Instance { get; private set; }

        const string PropKey = "openToPublic";

        [Header("UI")]
        [SerializeField] Button toggleButton;
        [SerializeField] TextMeshProUGUI toggleButtonLabel;
        [SerializeField] TextMeshProUGUI statusBadge;   // shown on HUD at all times

        bool _isPublic = true;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
        void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

        void Start()
        {
            // Read initial state from room properties
            if (PhotonNetwork.InRoom)
            {
                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                _isPublic = !props.ContainsKey(PropKey) || (bool)props[PropKey];
            }
            RefreshUI();
        }

        // ── Called by the toggle button ───────────────────────────────────

        public void RequestToggle()
        {
            // Any player can request — MasterClient applies
            photonView.RPC(nameof(RPC_RequestToggle), RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [PunRPC]
        void RPC_RequestToggle(int requestingActor)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            bool newState = !_isPublic;

            // Flip Photon room visibility and open state
            PhotonNetwork.CurrentRoom.IsOpen    = newState;
            PhotonNetwork.CurrentRoom.IsVisible = newState;

            // Write to room properties so it persists and syncs via OnRoomPropertiesUpdate
            var props = new Hashtable { { PropKey, newState } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        // ── IInRoomCallbacks ──────────────────────────────────────────────

        public void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (!changedProps.ContainsKey(PropKey)) return;
            _isPublic = (bool)changedProps[PropKey];
            RefreshUI();
        }

        // Unused callbacks
        public void OnPlayerEnteredRoom(Player p) { }
        public void OnPlayerLeftRoom(Player p) { }
        public void OnPlayerPropertiesUpdate(Player p, Hashtable h) { }
        public void OnMasterClientSwitched(Player p) { }

        // ── UI ────────────────────────────────────────────────────────────

        void RefreshUI()
        {
            if (toggleButtonLabel != null)
                toggleButtonLabel.text = _isPublic
                    ? "Make Private"
                    : "Make Open";

            if (statusBadge != null)
            {
                statusBadge.text  = _isPublic ? "Open" : "Private";
                statusBadge.color = _isPublic
                    ? new Color(0.2f, 0.8f, 0.3f)   // green
                    : new Color(0.9f, 0.4f, 0.1f);  // amber
            }
        }
    }
}
