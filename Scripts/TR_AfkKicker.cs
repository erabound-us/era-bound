using Photon.Pun;
using TMPro;
using UnityEngine;

namespace MarsFPSKit
{
    // Replaces Kit_AfkLimitSimple for Treasure Royale.
    // After 5 minutes of no input, saves artifacts then fully disconnects
    // from Photon to free the connection slot. Player lands in local lobby.
    [CreateAssetMenu(menuName = "TreasureRoyale/AFK/TR AFK Kicker")]
    public class TR_AfkKicker : Kit_AfkLimitBase
    {
        [Tooltip("Seconds of inactivity before warning starts (default 240 = 4 min)")]
        public float warningAtSeconds = 240f;
        [Tooltip("Seconds after warning before returning to Origin Timeline (default 60 = 1 min)")]
        public float kickAfterWarningSeconds = 60f;

        // Runtime — stored here because ScriptableObject is a singleton per game session
        float _lastInputTime;
        bool _warningShown;
        float _warningShownAt;

        public override void StartRelay(Kit_IngameMain main, bool enabled, int afkLimit = 0)
        {
            _lastInputTime  = Time.time;
            _warningShown   = false;
            _warningShownAt = 0f;
        }

        public override void UpdateRelay(Kit_IngameMain main)
        {
            // Any key or mouse movement resets the timer
            if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                _lastInputTime = Time.time;
                _warningShown  = false;
                main.afkLimitUI?.DisplayWarning(0, kickAfterWarningSeconds, 0); // hide
                return;
            }

            float idle = Time.time - _lastInputTime;

            // Show warning at 4 minutes
            if (!_warningShown && idle >= warningAtSeconds)
            {
                _warningShown   = true;
                _warningShownAt = Time.time;
                float kickIn    = kickAfterWarningSeconds;
                main.afkLimitUI?.DisplayWarning(idle, kickIn, 1);
            }

            // Return to Origin Timeline at 5 minutes
            if (_warningShown && Time.time >= _warningShownAt + kickAfterWarningSeconds)
            {
                ReturnToOriginTimeline(main);
            }
        }

        void ReturnToOriginTimeline(Kit_IngameMain main)
        {
            if (main.myPlayer != null)
            {
                var inv = main.myPlayer.GetComponent<PlayerInventory>();
                inv?.SaveArtifactsOnAfkKick();
            }

            // Full disconnect — frees the Photon CCU slot entirely.
            // TR_OriginTimelineManager.OnDisconnected loads the local lobby scene.
            TR_OriginTimelineManager.ReturnFromAfk = true;
            PhotonNetwork.Disconnect();
        }
    }
}
