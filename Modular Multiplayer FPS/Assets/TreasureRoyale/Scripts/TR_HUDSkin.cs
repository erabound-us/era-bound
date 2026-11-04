using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MarsFPSKit
{
    [DefaultExecutionOrder(100)]
    public class TR_HUDSkin : MonoBehaviour
    {
        static readonly Color BG        = new Color(0.04f, 0.04f, 0.06f, 0.72f);
        static readonly Color GOLD      = new Color(0.85f, 0.68f, 0.20f, 1f);
        static readonly Color TEXT      = new Color(0.92f, 0.88f, 0.78f, 1f);
        static readonly Color CROSSHAIR = new Color(0.90f, 0.82f, 0.40f, 0.90f);
        static readonly Color HITMARK   = new Color(1.00f, 0.30f, 0.10f, 1f);
        static readonly Color BLOOD     = new Color(0.55f, 0.05f, 0.05f, 0f);

        // Compass: 8 directions × 160px per 90° = 160/2 = 80px per 45°
        // movePerDirection=160 means 90°→160px, so 45°→80px
        static readonly string[] DIRS = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

        void Start()
        {
            var hud = FindObjectOfType<Kit_PlayerHUD>();
            if (hud == null) return;
            ApplyHUD(hud);
            RebuildCompass();
        }

        void ApplyHUD(Kit_PlayerHUD hud)
        {
            SetImageColor(hud.crosshairLeft,  CROSSHAIR);
            SetImageColor(hud.crosshairRight, CROSSHAIR);
            SetImageColor(hud.crosshairUp,    CROSSHAIR);
            SetImageColor(hud.crosshairDown,  CROSSHAIR);
            SetImageColor(hud.hitmarkerImage, HITMARK);
            SetImageColor(hud.bloodyScreen, BLOOD);

            foreach (var t in hud.GetComponentsInChildren<TextMeshProUGUI>(true))
                t.color = TEXT;

            foreach (var img in hud.GetComponentsInChildren<Image>(true))
            {
                if (img == hud.crosshairLeft || img == hud.crosshairRight ||
                    img == hud.crosshairUp   || img == hud.crosshairDown  ||
                    img == hud.hitmarkerImage || img == hud.bloodyScreen) continue;
                if (img.sprite == null)
                    img.color = BG;
            }

            SetImageColor(hud.indicatorImage, GOLD);
            hud.weaponDisplaySelectedColor   = GOLD;
            hud.weaponDisplayUnselectedColor = new Color(0.4f, 0.35f, 0.15f, 0.6f);

            foreach (var kf in FindObjectsOfType<TextMeshProUGUI>())
                if (kf.transform.parent != null && kf.transform.parent.name.Contains("Kill"))
                    kf.color = GOLD;
        }

        void RebuildCompass()
        {
            var minimap = FindObjectOfType<Kit_MinimapDefault>();
            if (minimap == null || minimap.minimapCompass == null) return;

            var compass = minimap.minimapCompass;

            // Remove existing sprite-based children (wrong labels)
            for (int i = compass.childCount - 1; i >= 0; i--)
                Destroy(compass.GetChild(i).gameObject);

            // movePerDirection=160 → 90°=160px → 45°=80px
            float spacing = 80f;
            // Place N at x=0, then NE at 80, E at 160, etc. wrapping around
            // The strip scrolls: at yaw=0 (facing North), compass.localPosition.x=0
            // so N should be at x=0 on the strip
            for (int i = 0; i < DIRS.Length; i++)
            {
                var go = new GameObject(DIRS[i]);
                go.transform.SetParent(compass, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(60f, 30f);
                rt.anchoredPosition = new Vector2(i * spacing, 0f);

                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = DIRS[i];
                tmp.fontSize = DIRS[i].Length == 1 ? 16f : 13f; // N/E/S/W bigger
                tmp.fontStyle = DIRS[i].Length == 1 ? FontStyles.Bold : FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = DIRS[i].Length == 1 ? GOLD : TEXT;
            }
        }

        static void SetImageColor(Image img, Color c)
        {
            if (img != null) img.color = c;
        }
    }
}
