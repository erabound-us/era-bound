using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the timeline entry prefab used by TR_TeamWarpUI.
    // One instance per active era room shown in the Origin Timeline list.
    public class TR_TimelineEntryUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI eraLabel;
        [SerializeField] TextMeshProUGUI bucketLabel;
        [SerializeField] TextMeshProUGUI teamLabel;
        [SerializeField] TextMeshProUGUI playerCountLabel;
        [SerializeField] TextMeshProUGUI privacyBadge;  // "Open" or "Private"
        [SerializeField] Button warpButton;
        [SerializeField] TextMeshProUGUI warpButtonLabel;

        public void Populate(TR_OriginTimelineManager.ActiveTimeline tl, Action<string> onWarp)
        {
            eraLabel.text         = FormatEra(tl.era);
            bucketLabel.text      = tl.bucketId;
            teamLabel.text        = tl.teamId >= 0 ? $"Team {tl.teamId}" : "Solo";
            playerCountLabel.text = $"{tl.playerCount} / {tl.maxPlayers}";

            if (privacyBadge != null)
                privacyBadge.text = tl.openToPublic ? "Open" : "Private";

            bool full = tl.playerCount >= tl.maxPlayers;
            warpButton.interactable = !full;
            warpButtonLabel.text    = full ? "Full" : "Warp In";

            warpButton.onClick.RemoveAllListeners();
            if (!full)
                warpButton.onClick.AddListener(() => onWarp(tl.roomName));
        }

        static string FormatEra(string era)
        {
            // "deep_prehistoric" → "Deep Prehistoric"
            if (string.IsNullOrEmpty(era)) return "Unknown Era";
            return System.Globalization.CultureInfo.CurrentCulture
                .TextInfo.ToTitleCase(era.Replace('_', ' '));
        }
    }
}
