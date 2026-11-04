using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to the MarsFPSKit_IngamePrefab alongside Kit_IngameMain.
    // Handles a player who warps into an active era room mid-match.
    // Works as a Kit_Plugin so it receives OnSetupDone and PlayerJoinedRoom hooks.
    [CreateAssetMenu(menuName = "TreasureRoyale/Plugin/Mid-Match Warp Plugin")]
    public class TR_MidMatchWarpPlugin : Kit_Plugin
    {
        // Room property key written by MultiverseTransporterUI when creating the room
        const string TeamIdKey = "teamId";

        public override void PlayerJoinedRoom(Kit_IngameMain main, Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var props = PhotonNetwork.CurrentRoom.CustomProperties;

            // If the room is private, kick the joining player back to origin
            bool openToPublic = !props.ContainsKey("openToPublic") || (bool)props["openToPublic"];
            if (!openToPublic)
            {
                // Check if this player was explicitly invited (has "invited" flag set by host)
                bool invited = newPlayer.CustomProperties.ContainsKey("invited")
                            && (bool)newPlayer.CustomProperties["invited"];
                if (!invited)
                {
                    // Kick back — send RPC to that player only
                    var kickView = GetPhotonViewForPlayer(newPlayer);
                    kickView?.RPC(nameof(RPC_KickedPrivateRoom), newPlayer);
                    return;
                }
            }

            int teamId = props.ContainsKey(TeamIdKey) ? (int)props[TeamIdKey] : -1;

            var view = GetPhotonViewForPlayer(newPlayer);
            if (view == null) return;

            view.RPC(nameof(RPC_AssignTeamAndSpawn), newPlayer, teamId);

            // Broadcast warp-in notification to existing players
            var data = new Hashtable
            {
                { (byte)0, newPlayer.NickName },
                { (byte)1, teamId }
            };
            PhotonNetwork.RaiseEvent(TREventIDs.BeaconStateChanged, data,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                ExitGames.Client.Photon.SendOptions.SendReliable);
        }

        // Runs on the joining client — room was private and they weren't invited
        [PunRPC]
        void RPC_KickedPrivateRoom()
        {
            TR_OriginTimelineManager.Instance?.EnterOriginTimeline();
            ExtractionHUD.Instance?.ShowMessage("That timeline is private.");
        }

        // Runs on the joining client
        [PunRPC]
        void RPC_AssignTeamAndSpawn(int teamId)
        {
            var main = FindObjectOfType<Kit_IngameMain>();
            if (main == null) return;

            // Set team in Photon player properties (kit convention)
            var table = PhotonNetwork.LocalPlayer.CustomProperties;
            table["team"] = teamId;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            main.assignedTeamID = teamId;

            // Request a safe drop zone — PlayerSpawnManager handles the rest
            PlayerSpawnManager.Instance?.photonView.RPC(
                "RPC_RequestDropZone", RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber);
        }

        PhotonView GetPhotonViewForPlayer(Player player)
        {
            foreach (var pv in FindObjectsOfType<PhotonView>())
                if (pv.Owner == player) return pv;
            return null;
        }
    }
}
