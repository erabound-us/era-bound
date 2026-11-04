using System.Linq;
using TMPro;
using UnityEngine;

namespace MarsFPSKit
{
    namespace UI
    {
        [CreateAssetMenu(menuName = "MarsFPSKit/Options/Audio/Voice Chat Mode")]
        public class Kit_OptionsVoiceChatMode : Kit_OptionBase
        {
            public override string GetDisplayName()
            {
                return "Voice Chat Mode";
            }

            public override string GetHoverText()
            {
                return "How you want to communicate in-game with the voice chat.";
            }

            public override OptionType GetOptionType()
            {
                return OptionType.Dropdown;
            }

            public override void OnDropdownStart(TextMeshProUGUI txt, TMP_Dropdown dropdown)
            {
                //Load
                int selected = PlayerPrefs.GetInt("voiceMode", 0);
                //Clamp
                selected = Mathf.Clamp(selected, 0, 2);
                //Clear
                dropdown.ClearOptions();
                //Add
                dropdown.AddOptions((new string[] { "Push To Talk", "Detection", "Disabled" }).ToList());
                //Set default value
                dropdown.value = selected;
                //Use that value
                OnDropdowChange(txt, selected);
            }

            public override void OnDropdowChange(TextMeshProUGUI txt, int newValue)
            {
                //Set
                Kit_GameSettings.voiceChatTransmitMode = (VoiceTransmitMode)newValue;
                //Save
                PlayerPrefs.SetInt("voiceMode", newValue);
            }
        }
    }
}