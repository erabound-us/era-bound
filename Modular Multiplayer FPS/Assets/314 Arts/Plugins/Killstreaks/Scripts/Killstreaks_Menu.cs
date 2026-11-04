using MarsFPSKit;
using System.Collections.Generic;
using UnityEngine;

namespace KillstreaksPlugin
{
    public class Killstreaks_Menu : MonoBehaviour
    {
        [Header("UI")]
        /// <summary>
        /// Menu Root
        /// </summary>
        public GameObject root;
        /// <summary>
        /// Where the slots go
        /// </summary>
        public RectTransform slotGo;
        /// <summary>
        /// Prefab for the slots
        /// </summary>
        public GameObject slotPrefab;
        /// <summary>
        /// Active slots!
        /// </summary>
        public List<Killstreaks_MenuSlotEntry> slotActives = new List<Killstreaks_MenuSlotEntry>();

        #region Runtime
        private Kit_IngameMain main;
        private Killstreaks_Plugin plugin;
        private KillstreakRuntimeData krd;
        #endregion

        public void Setup(Kit_IngameMain newMain, Killstreaks_Plugin newPlugin, KillstreakRuntimeData newKrd)
        {
            //Assign
            main = newMain;
            plugin = newPlugin;
            krd = newKrd;
            //Subcribe to events
            main.pluginOnForceClose.AddListener(delegate { Close(); });
            //Redraw
            Redraw();
            //Make sure its closed
            Close();
        }

        public void Open()
        {
            Redraw();
            root.SetActive(true);
        }

        public void Close()
        {
            root.SetActive(false);
        }

        void Redraw()
        {
            //Setup initial slots!
            if (slotActives.Count == 0)
            {
                for (int i = 0; i < plugin.killstreakSlots.Length; i++)
                {
                    GameObject go = Instantiate(slotPrefab, slotGo, false);
                    //Get
                    Killstreaks_MenuSlotEntry slot = go.GetComponent<Killstreaks_MenuSlotEntry>();
                    List<string> killstreakOptions = new List<string>();
                    //Setup
                    for (int p = 0; p < plugin.killstreakSlots[i].possibleKillstreaks.Length; p++)
                    {
                        killstreakOptions.Add(plugin.killstreakSlots[i].possibleKillstreaks[p].name);
                    }
                    //Reset
                    slot.dropdown.ClearOptions();
                    //Add
                    slot.dropdown.AddOptions(killstreakOptions);
                    //Set slot 0
                    slot.dropdown.value = 0;
                    int id = i;
                    slot.dropdown.onValueChanged.AddListener(delegate { UpdateSlot(id, slot.dropdown.value); });
                    //Add
                    slotActives.Add(slot);
                    //Manually update first time
                    UpdateSlot(id, 0);
                }
            }

            //Make sure they display what was actually selected!
            for (int i = 0; i < krd.desiredSelectedKillstreaks.Length; i++)
            {
                slotActives[i].dropdown.value = krd.desiredSelectedKillstreaks[i];
            }
        }

        public void UpdateSlot(int slot, int value)
        {
            //Set new slot
            krd.desiredSelectedKillstreaks[slot] = value;
            //Update preview text
            slotActives[slot].previewText.text = plugin.killstreakSlots[slot].possibleKillstreaks[value].menuPreviewText;
            //Update preview image
            slotActives[slot].previewImage.sprite = plugin.killstreakSlots[slot].possibleKillstreaks[value].menuPreviewSprite;
            slotActives[slot].killsRequired.text = "Kills required: " + plugin.killstreakSlots[slot].possibleKillstreaks[value].unlockedAtKills;
        }
    }
}