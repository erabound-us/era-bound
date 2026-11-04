using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace MarsFPSKit
{
    /// <summary>
    /// Simple pause overlay for Treasure Royale PvE mode.
    /// Bypasses the kit's pause menu system which doesn't work in PvE.
    /// Add this component to a Canvas GameObject in the scene.
    /// </summary>
    public class TR_PauseOverlay : MonoBehaviour
    {
        public static TR_PauseOverlay instance;

        [Header("References")]
        public Kit_IngameMain main;

        // Built at runtime
        private GameObject panel;
        private bool isOpen;

        void Awake()
        {
            instance = this;
            BuildUI();
            panel.SetActive(false);
        }

        void BuildUI()
        {
            // Root canvas
            var canvas = gameObject.GetComponent<Canvas>();
            if (!canvas) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            if (!GetComponent<CanvasScaler>())
            {
                var cs = gameObject.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            }
            if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();

            // Dark background panel — fullscreen
            panel = new GameObject("TR_PausePanel");
            panel.transform.SetParent(transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);
            bg.raycastTarget = true;

            // Place 3 buttons manually centered, stacked vertically
            float btnW = 300f, btnH = 60f, gap = 15f;
            float totalH = btnH * 3 + gap * 2;
            float startY = totalH / 2f - btnH / 2f;

            AddButton(panel.transform, "Resume", 0, startY, btnW, btnH, Resume);
            AddButton(panel.transform, "Main Menu", 0, startY - (btnH + gap), btnW, btnH, QuitToMainMenu);
            AddButton(panel.transform, "Quit Game", 0, startY - (btnH + gap) * 2, btnW, btnH, QuitGame);
        }

        void AddButton(Transform parent, string label, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            btn.colors = colors;
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }

        public void Open()
        {
            isOpen = true;
            panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            isOpen = false;
            panel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public bool IsOpen => isOpen;

        void Resume()
        {
            Close();
        }

        void QuitToMainMenu()
        {
            if (main) main.isShuttingDown = false;
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            else
                Kit_SceneSyncer.instance.LoadScene("MainMenu");
        }

        void QuitGame()
        {
            if (main) main.isShuttingDown = true;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
