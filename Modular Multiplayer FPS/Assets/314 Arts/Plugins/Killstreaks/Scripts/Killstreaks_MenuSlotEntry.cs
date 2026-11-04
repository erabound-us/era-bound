using UnityEngine;
using UnityEngine.UI;

namespace KillstreaksPlugin
{
    /// <summary>
    /// This holds references for setting up the slots!
    /// </summary>
    public class Killstreaks_MenuSlotEntry : MonoBehaviour
    {
        /// <summary>
        /// Dropdown with available killstreaks
        /// </summary>
        public Dropdown dropdown;
        /// <summary>
        /// Preview for killstreak
        /// </summary>
        public Image previewImage;
        /// <summary>
        /// Text info about selected killstreak
        /// </summary>
        public Text previewText;
        /// <summary>
        /// How many kills we need
        /// </summary>
        public Text killsRequired;
    }
}