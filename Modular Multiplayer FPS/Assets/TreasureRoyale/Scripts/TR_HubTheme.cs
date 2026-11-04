using UnityEngine;
using TMPro;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "Treasure Royale/Hub Theme", fileName = "TR_HubTheme_Default")]
    public class TR_HubTheme : ScriptableObject
    {
        [Header("Colors")]
        public Color panelBg        = new Color(0.10f, 0.10f, 0.16f, 1f);
        public Color tabBarBg       = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        public Color tabBtnNormal   = new Color(0.18f, 0.18f, 0.25f, 1f);
        public Color tabBtnHover    = new Color(0.30f, 0.30f, 0.50f, 1f);
        public Color tabBtnPressed  = new Color(0.50f, 0.40f, 0.10f, 1f);
        public Color coinBarBg      = new Color(0.00f, 0.00f, 0.00f, 0.85f);
        public Color coinTextColor  = new Color(1.00f, 0.85f, 0.20f, 1f);
        public Color listBg         = new Color(0.00f, 0.00f, 0.00f, 0.30f);
        public Color textPrimary    = Color.white;
        public Color textSecondary  = new Color(0.70f, 0.70f, 0.70f, 1f);
        public Color textAccent     = new Color(0.40f, 0.90f, 0.40f, 1f);
        public Color actionBtnColor = new Color(0.20f, 0.50f, 0.80f, 1f);
        public Color rowBg          = new Color(1.00f, 1.00f, 1.00f, 0.05f);

        [Header("Fonts")]
        public TMP_FontAsset font;

        [Header("Sizes")]
        public float tabBarHeight  = 60f;
        public float coinBarHeight = 40f;
        public float fontTitle     = 28f;
        public float fontSubtitle  = 20f;
        public float fontBody      = 16f;
        public float fontSmall     = 14f;
        public float rowHeight     = 60f;
    }
}
