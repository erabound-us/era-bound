using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MarsFPSKit
{
    // Attach to the Chatbot panel root.
    // Calls the Worker /guide endpoint which proxies to an AI model.
    public class TR_EraChatbotPanel : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown eraDropdown;
        [SerializeField] TMP_InputField questionInput;
        [SerializeField] Button askBtn;
        [SerializeField] TextMeshProUGUI responseText;
        [SerializeField] GameObject loadingIndicator;

        static readonly string[] EraKeys = {
            "deep_prehistoric","prehistoric","ancient","medieval",
            "renaissance","industrial","modern","near_future",
            "mid_future","far_future","deep_future"
        };

        void Awake()
        {
            if (eraDropdown != null)
            {
                eraDropdown.ClearOptions();
                eraDropdown.AddOptions(new System.Collections.Generic.List<string> {
                    "Deep Prehistoric","Prehistoric","Ancient","Medieval",
                    "Renaissance","Industrial","Modern","Near Future",
                    "Mid Future","Far Future","Deep Future"
                });
            }
            if (askBtn != null) askBtn.onClick.AddListener(OnAsk);
        }

        void OnAsk()
        {
            string era      = EraKeys[eraDropdown.value];
            string question = questionInput.text.Trim();
            if (string.IsNullOrEmpty(question)) question = "What should I bring?";

            loadingIndicator.SetActive(true);
            responseText.text = "";
            askBtn.interactable = false;

            TR_HubApi.Instance.Post("/guide", new { era, question }, (json, err) =>
            {
                loadingIndicator.SetActive(false);
                askBtn.interactable = true;

                if (err != null) { responseText.text = "Could not reach guide. Try again."; return; }
                var result = JsonUtility.FromJson<GuideResult>(json);
                responseText.text = result?.answer ?? json;
            });
        }
        [System.Serializable] class GuideResult { public string answer; }
    }
}
