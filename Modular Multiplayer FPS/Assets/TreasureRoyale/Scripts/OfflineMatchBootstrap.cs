using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

namespace MarsFPSKit
{
    /// <summary>
    /// Attach to any scene GameObject. When playing Showcase_Map directly in the editor
    /// (without going through MainMenu), this auto-starts an offline medieval match.
    /// </summary>
    public class OfflineMatchBootstrap : MonoBehaviour
    {
        [Tooltip("Era to use when auto-launching. Leave blank for random.")]
        public string era = "medieval";

        void Awake()
        {
            // Only bootstrap if we are in a Photon room (real match load)
            if (!PhotonNetwork.InRoom) return;

            // Only bootstrap if no match is already set up
            if (TR_OriginTimelineManager.IsOfflineMatch) return;
            if (TR_OriginTimelineManager.OfflineMatchProps != null) return;

            string chosenEra = string.IsNullOrEmpty(era) ? "medieval" : era;

            TR_OriginTimelineManager.OfflineMatchProps = new Hashtable
            {
                { "gameModeType",  1 },
                { "gameMode",      0 },
                { "era",           chosenEra },
                { "bucketId",      chosenEra + "_offline" },
                { "universeSeed",  Random.Range(1, 999999) },
                { "anchorYear",    0 },
                { "teamId",        -1 },
                { "type",          "era" },
                { "openToPublic",  false }
            };

            TR_OriginTimelineManager.IsOfflineMatch = true;
            Debug.Log("[TR] OfflineMatchBootstrap: started offline " + chosenEra + " match.");
        }
    }
}
