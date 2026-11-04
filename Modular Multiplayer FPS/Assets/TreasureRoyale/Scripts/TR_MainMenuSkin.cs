using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MarsFPSKit
{
    // Add to any persistent GO in MainMenu scene.
    // Recolors the MM FPS main menu to Treasure Royale's dark-gold aesthetic.
    [DefaultExecutionOrder(200)]
    public class TR_MainMenuSkin : MonoBehaviour
    {
        static readonly Color BG_DARK   = new Color(0.05f, 0.04f, 0.08f, 0.90f);
        static readonly Color BG_MID    = new Color(0.10f, 0.08f, 0.14f, 0.85f);
        static readonly Color GOLD      = new Color(0.85f, 0.68f, 0.20f, 1f);
        static readonly Color GOLD_DIM  = new Color(0.55f, 0.42f, 0.10f, 1f);
        static readonly Color TEXT      = new Color(0.92f, 0.88f, 0.78f, 1f);
        static readonly Color BTN_NORM  = new Color(0.12f, 0.10f, 0.18f, 0.95f);
        static readonly Color BTN_HIGH  = new Color(0.22f, 0.18f, 0.08f, 1.00f);
        static readonly Color BTN_PRESS = new Color(0.08f, 0.06f, 0.04f, 1.00f);

        void Start() => Apply();

        void Apply()
        {
            // Recolor all background panels
            foreach (var img in FindObjectsOfType<Image>())
            {
                if (img.sprite == null)
                    img.color = img.rectTransform.sizeDelta.magnitude > 400f ? BG_DARK : BG_MID;
            }

            // Recolor all buttons
            foreach (var btn in FindObjectsOfType<Button>())
            {
                var colors = btn.colors;
                colors.normalColor      = BTN_NORM;
                colors.highlightedColor = BTN_HIGH;
                colors.pressedColor     = BTN_PRESS;
                colors.selectedColor    = BTN_HIGH;
                btn.colors = colors;

                // Button label text → gold
                foreach (var t in btn.GetComponentsInChildren<TextMeshProUGUI>())
                    t.color = GOLD;
            }

            // All other TMP text → parchment
            foreach (var t in FindObjectsOfType<TextMeshProUGUI>())
                if (t.color != GOLD) t.color = TEXT;

            // Title text → gold + larger if it contains the game name
            foreach (var t in FindObjectsOfType<TextMeshProUGUI>())
            {
                var txt = t.text;
                if (txt.Contains("Modular") || txt.Contains("FPS Kit") || txt.Contains("Mars") || txt.Contains("ERA Bound") || txt.Contains("ERA BOUND"))
                {
                    t.text  = "ERA BOUND";
                    t.color = GOLD;
                    t.fontSize = Mathf.Max(t.fontSize, 48f);
                }
            }

            // Input fields → dark bg, gold caret
            foreach (var field in FindObjectsOfType<TMP_InputField>())
            {
                field.caretColor = GOLD;
                var bg = field.GetComponent<Image>();
                if (bg) bg.color = BG_MID;
            }
        }
    }
}
