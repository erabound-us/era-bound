using UnityEngine;
using UnityEngine.UI;
using MarsFPSKit;

// Attach to the ERA_ExitPortal root. Detects the local player within range,
// shows a hold-E gauge, and calls Kit_IngameMain.Disconnect() on completion.
[DisallowMultipleComponent]
public class MultiverseExitPortal : MonoBehaviour
{
    const float HOLD_TIME    = 3f;
    const float TRIGGER_RADIUS = 5f;

    // Built at runtime by EraMapGenerator — no inspector wiring needed.
    [HideInInspector] public Color accentColor;

    // ── World-space canvas ────────────────────────────────────────────────────
    Canvas _canvas;
    Image  _gaugeBar;

    float _holdProgress;
    bool  _playerInRange;
    bool  _activated;

    Kit_IngameMain _main;

    void Start()
    {
        _main = Object.FindObjectOfType<Kit_IngameMain>();
        BuildUI();
        SetUIVisible(false);
    }

    void BuildUI()
    {
        // World-space canvas parented to this object, floating above the monument
        var cgo = new GameObject("ExitPortal_Canvas");
        cgo.transform.SetParent(transform, false);
        cgo.transform.localPosition = new Vector3(0f, 28f, 0f); // above the spire tip
        cgo.transform.localScale    = Vector3.one * 0.04f;

        _canvas = cgo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        var rt = _canvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 80f);

        // Background panel
        var bg = new GameObject("BG");
        bg.transform.SetParent(cgo.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.65f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        // Gauge track
        var track = new GameObject("GaugeTrack");
        track.transform.SetParent(cgo.transform, false);
        var trackImg = track.AddComponent<Image>();
        trackImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        var trt = track.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.1f); trt.anchorMax = new Vector2(0.95f, 0.9f);
        trt.offsetMin = Vector2.zero;              trt.offsetMax = Vector2.zero;

        // Gauge fill
        var fill = new GameObject("GaugeFill");
        fill.transform.SetParent(track.transform, false);
        _gaugeBar       = fill.AddComponent<Image>();
        _gaugeBar.color = accentColor != default ? accentColor : new Color(0.2f, 0.9f, 0.6f);
        _gaugeBar.type  = Image.Type.Filled;
        _gaugeBar.fillMethod = Image.FillMethod.Horizontal;
        _gaugeBar.fillAmount = 0f;
        var frt = _gaugeBar.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(2f, 2f); frt.offsetMax = new Vector2(-2f, -2f);
    }

    void Update()
    {
        if (_activated) return;

        // Always face the camera
        if (Camera.main != null)
            _canvas.transform.LookAt(Camera.main.transform);

        // Proximity check against local player
        _playerInRange = false;
        if (_main != null && _main.myPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, _main.myPlayer.transform.position);
            _playerInRange = dist <= TRIGGER_RADIUS;
        }

        SetUIVisible(_playerInRange);

        if (_playerInRange && Input.GetKey(KeyCode.E))
        {
            _holdProgress += Time.deltaTime;
            _gaugeBar.fillAmount = _holdProgress / HOLD_TIME;

            if (_holdProgress >= HOLD_TIME)
            {
                _activated = true;
                _gaugeBar.fillAmount = 1f;
                Invoke(nameof(DoExit), 0.4f);
            }
        }
        else
        {
            // Decay quickly when key released
            _holdProgress = Mathf.Max(0f, _holdProgress - Time.deltaTime * 2f);
            _gaugeBar.fillAmount = _holdProgress / HOLD_TIME;
        }
    }

    void DoExit()
    {
        if (_main != null)
            _main.Disconnect();
    }

    void SetUIVisible(bool v)
    {
        if (_canvas != null && _canvas.gameObject.activeSelf != v)
            _canvas.gameObject.SetActive(v);
    }
}
