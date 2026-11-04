using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "TreasureRoyale/Plugin/Core Plugin")]
    public class TreasureRoyalePlugin : Kit_Plugin
    {
        // Artifact scores tracked per actor number (MasterClient authoritative)
        readonly Dictionary<int, int> _artifactScores = new Dictionary<int, int>();

        public override void OnSetupDone(Kit_IngameMain main)
        {
            _artifactScores.Clear();
        }

        public override void PlayerLeftRoom(Kit_IngameMain main, Player player)
        {
            // ArtifactResetManager handles artifact drop — we just clean score
            _artifactScores.Remove(player.ActorNumber);
        }

        public override void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {
            if (eventCode == TREventIDs.ArtifactScoreUpdate && PhotonNetwork.IsMasterClient)
            {
                var data = (Hashtable)content;
                int actor = (int)data[(byte)0];
                int score = (int)data[(byte)1];
                _artifactScores[actor] = score;
                CheckForWinner(main);
            }
        }

        void CheckForWinner(Kit_IngameMain main)
        {
            // Called by MasterClient when timer runs out (via TimeRunOut) or on demand
            if (main.timer > 0) return;

            int bestActor = -1;
            int bestScore = -1;
            foreach (var kv in _artifactScores)
            {
                if (kv.Value > bestScore) { bestScore = kv.Value; bestActor = kv.Key; }
            }

            if (bestActor < 0) return;

            var winner = new Kit_Player { isBot = false, id = bestActor };
            main.EndGame(winner);
        }

        public void TriggerEndGame(Kit_IngameMain main) => CheckForWinner(main);
    }
}
