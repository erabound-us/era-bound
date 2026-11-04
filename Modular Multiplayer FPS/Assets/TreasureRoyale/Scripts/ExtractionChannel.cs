using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the ExtractionBeacon world object alongside a trigger collider.
    // Extends Kit_InteractableObject so the kit's weapon manager handles F-key and prompt display.
    public class ExtractionChannel : Kit_InteractableObject
    {
        [SerializeField] float beaconInteractRadius = 3f;

        bool _channeling;
        Vector3 _channelStartPos;
        Kit_PlayerBehaviour _channelingPlayer;

        void Awake()
        {
            interactionText = "Hold E to Extract";
        }

        public override bool CanInteract(Kit_PlayerBehaviour who)
        {
            if (ExtractionBeacon.Instance == null) return false;
            var state = ExtractionBeacon.Instance.State;
            bool beaconReady = state == ExtractionBeacon.BeaconState.Idle
                            || state == ExtractionBeacon.BeaconState.Contested;
            bool inRange = Vector3.Distance(who.transform.position,
                ExtractionBeacon.Instance.transform.position) <= beaconInteractRadius;
            bool hasArtifacts = PlayerInventory.LocalInstance != null
                             && PlayerInventory.LocalInstance.HasArtifacts();
            return beaconReady && inRange && hasArtifacts && !_channeling;
        }

        public override void Interact(Kit_PlayerBehaviour who)
        {
            if (!who.photonView.IsMine) return;
            _channeling = true;
            _channelStartPos = who.transform.position;
            _channelingPlayer = who;
            if (PhotonNetwork.InRoom) ExtractionBeacon.Instance.RequestBeginChannel(
                PhotonNetwork.LocalPlayer.ActorNumber, who.transform.position);
        }

        void Update()
        {
            if (!_channeling || _channelingPlayer == null) return;

            if (Vector3.Distance(_channelingPlayer.transform.position, _channelStartPos) > 1f)
                CancelChannel("Moved");
        }

        public void CancelChannel(string reason)
        {
            if (!_channeling) return;
            _channeling = false;
            if (PhotonNetwork.InRoom) ExtractionBeacon.Instance.RequestCancelChannel(PhotonNetwork.LocalPlayer.ActorNumber);
            ExtractionHUD.Instance?.OnChannelCancelled(reason);
            _channelingPlayer = null;
        }

        // Called by Kit_SimpleVitals damage path via TreasureRoyalePlugin
        public void OnDamageReceived() => CancelChannel("Took damage");

        public void OnExtractionSuccess() => _channeling = false;
    }
}
