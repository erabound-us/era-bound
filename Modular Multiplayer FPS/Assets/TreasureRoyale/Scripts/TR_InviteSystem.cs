using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // ── In-match side ─────────────────────────────────────────────────────
    // Attach to the pause menu or team panel in the ingame scene.
    // Lets any player in the room invite someone by Photon NickName.
    public class TR_InvitePanel : MonoBehaviourPun
    {
        [SerializeField] TMP_InputField inviteNameInput;
        [SerializeField] Button sendInviteButton;
        [SerializeField] TextMeshProUGUI inviteStatusText;

        // Photon custom event code for invite (reuse TREventIDs range)
        // We use a direct RPC via a well-known channel rather than a new event byte
        // because the recipient may be in a different room (Origin Timeline).
        // Instead we write to their Photon player properties via MasterClient.

        public void OnSendInvitePressed()
        {
            string targetName = inviteNameInput.text.Trim();
            if (string.IsNullOrEmpty(targetName)) return;

            sendInviteButton.interactable = false;
            inviteStatusText.text = $"Sending invite to {targetName}…";

            // Ask MasterClient to write the invite into the target player's custom properties
            // (target must be in the same Photon region — Origin Timeline room)
            photonView.RPC(nameof(RPC_SendInvite), RpcTarget.MasterClient,
                targetName, PhotonNetwork.CurrentRoom.Name);
        }

        [PunRPC]
        void RPC_SendInvite(string targetNickName, string roomName)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // Find the target player across all rooms in this region via Photon Friends
            // Since we can't directly reach players in other rooms, we use a room property
            // on the Origin Timeline room as a message bus.
            // Write: "invite_{targetNickName}" = roomName into the origin room's properties.
            // TR_InviteReceiver in the origin room polls for this key.

            // We raise a global event — origin room clients listen for it
            var data = new Hashtable
            {
                { (byte)0, targetNickName },
                { (byte)1, roomName },
                { (byte)2, PhotonNetwork.LocalPlayer.NickName } // sender
            };
            PhotonNetwork.RaiseEvent(TREventIDs.ArtifactScoreUpdate + 10, data,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);

            photonView.RPC(nameof(RPC_InviteSent), photonView.Owner);
        }

        [PunRPC]
        void RPC_InviteSent()
        {
            inviteStatusText.text = "Invite sent!";
            sendInviteButton.interactable = true;
        }
    }

    // ── Origin Timeline side ──────────────────────────────────────────────
    // Attach to a persistent object in the MainMenu scene.
    // Listens for invite events and shows the accept/decline prompt.
    public class TR_InviteReceiver : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static TR_InviteReceiver Instance { get; private set; }

        // Matches the code used in TR_InvitePanel
        const byte InviteEventCode = TREventIDs.ArtifactScoreUpdate + 10;

        [Header("Invite Prompt UI")]
        [SerializeField] GameObject promptRoot;
        [SerializeField] TextMeshProUGUI promptText;
        [SerializeField] Button acceptButton;
        [SerializeField] Button declineButton;

        string _pendingRoomName;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            promptRoot.SetActive(false);
        }

        public override void OnEnable()  { base.OnEnable();  PhotonNetwork.AddCallbackTarget(this); }
        public override void OnDisable() { base.OnDisable(); PhotonNetwork.RemoveCallbackTarget(this); }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != InviteEventCode) return;

            var data       = (Hashtable)photonEvent.CustomData;
            string target  = (string)data[(byte)0];
            string room    = (string)data[(byte)1];
            string sender  = (string)data[(byte)2];

            // Only show prompt if this event is addressed to us
            if (target != PhotonNetwork.LocalPlayer.NickName) return;

            _pendingRoomName = room;
            ShowPrompt(sender, room);
        }

        void ShowPrompt(string senderName, string roomName)
        {
            promptRoot.SetActive(true);
            promptText.text = $"{senderName} invited you to their timeline.\nWarp in?";

            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAccept);

            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(OnDecline);
        }

        void OnAccept()
        {
            promptRoot.SetActive(false);
            if (string.IsNullOrEmpty(_pendingRoomName)) return;

            // Mark ourselves as invited so TR_MidMatchWarpPlugin lets us through
            var props = new Hashtable { { "invited", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            TR_OriginTimelineManager.Instance?.WarpIntoTimeline(_pendingRoomName);
            _pendingRoomName = null;
        }

        void OnDecline()
        {
            promptRoot.SetActive(false);
            _pendingRoomName = null;
        }
    }
}
