using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the browse entry prefab used by TR_MatchmakingUI's Browse tab.
    public class TR_BrowseEntryUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI eraLabel;
        [SerializeField] TextMeshProUGUI modeLabel;
        [SerializeField] TextMeshProUGUI teamLabel;
        [SerializeField] TextMeshProUGUI playerCountLabel;
        [SerializeField] TextMeshProUGUI privacyBadge;
        [SerializeField] Button warpButton;
        [SerializeField] TextMeshProUGUI warpButtonLabel;

        public void Populate(TR_OriginTimelineManager.ActiveTimeline tl, Action<string> onWarp)
        {
            eraLabel.text         = FormatEra(tl.era);
            modeLabel.text        = FormatMode(tl.playMode);
            teamLabel.text        = tl.teamId >= 0 ? $"Team {tl.teamId}" : "Solo";
            playerCountLabel.text = $"{tl.playerCount} / {tl.maxPlayers}";
            privacyBadge.text     = tl.openToPublic ? "Open" : "Private";

            bool full = tl.playerCount >= tl.maxPlayers;
            warpButton.interactable = !full;
            warpButtonLabel.text    = full ? "Full" : "Warp In";

            warpButton.onClick.RemoveAllListeners();
            if (!full) warpButton.onClick.AddListener(() => onWarp(tl.roomName));
        }

        static string FormatEra(string era) =>
            System.Globalization.CultureInfo.CurrentCulture
                .TextInfo.ToTitleCase((era ?? "unknown").Replace('_', ' '));

        static string FormatMode(TR_Matchmaker.PlayMode mode) => mode switch
        {
            TR_Matchmaker.PlayMode.Solo     => "Solo",
            TR_Matchmaker.PlayMode.Team     => "Team",
            TR_Matchmaker.PlayMode.Timeline => "Timeline",
            _                               => "Open"
        };
    }
}
