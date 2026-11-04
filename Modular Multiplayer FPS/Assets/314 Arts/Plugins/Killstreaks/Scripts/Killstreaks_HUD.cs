using UnityEngine;
using MarsFPSKit;
using UnityEngine.UI;
using System.Collections.Generic;

namespace KillstreaksPlugin
{
    public class Killstreaks_HUD : MonoBehaviour
    {
        /// <summary>
        /// Where the layouts go
        /// </summary>
        public RectTransform layoutGo;
        /// <summary>
        /// Prefab for the layout
        /// </summary>
        public GameObject layoutPrefab;
        /// <summary>
        /// Active layouts
        /// </summary>
        public List<GameObject> layoutActive = new List<GameObject>();

        /// <summary>
        /// Redraw killstreaks that are in our inventory!
        /// </summary>
        /// <param name="main"></param>
        /// <param name="krd"></param>
        public void Redraw(Kit_IngameMain main, Killstreaks_Plugin plugin, KillstreakRuntimeData krd)
        {
            List<KillstreakInventory> individualRedraws = new List<KillstreakInventory>();

            for (int i = 0; i < krd.inventory.Count; i++)
            {
                if (!individualRedraws.Contains(krd.inventory[i]))
                {
                    individualRedraws.Add(krd.inventory[i]);
                }
            }

            while (individualRedraws.Count < layoutActive.Count)
            {
                GameObject toRemove = layoutActive[layoutActive.Count - 1];
                //Remove
                layoutActive.Remove(toRemove);
                Destroy(toRemove);
            }

            while (individualRedraws.Count > layoutActive.Count)
            {
                GameObject go = Instantiate(layoutPrefab, layoutGo, false);
                layoutActive.Add(go);
            }

            //Redraw
            for (int i = 0; i < individualRedraws.Count; i++)
            {
                layoutActive[i].GetComponent<Image>().sprite = plugin.killstreakSlots[individualRedraws[i].slotId].possibleKillstreaks[individualRedraws[i].id].hudDisplaySprite;
            }
        }
    }
}