using MarsFPSKit;
using UnityEngine;
using UnityEngine.Events;

namespace KillstreaksPlugin
{
    public class Killstreaks_LocationChooserModule : MonoBehaviour
    {
        /// <summary>
        /// Root object.
        /// </summary>
        public GameObject root;

        #region Runtime References
        private Kit_IngameMain main;
        private Killstreaks_Plugin plugin;
        private KillstreakRuntimeData krd;
        private Killstreak_KillstreakBase currentKillstreak;
        private Kit_PlayerBehaviour currentPlayer;
        #endregion

        #region Runtime
        [HideInInspector]
        /// <summary>
        /// Are we open?
        /// </summary>
        public bool isOpen;
        /// <summary>
        /// What to remove!
        /// </summary>
        private int currentIdToRemoveUponUse;
        #endregion

        #region Unity Calls
        private void Update()
        {
            if (isOpen)
            {
                MarsScreen.lockCursor = false;
                main.SetPauseMenuState(false, false);
            }
        }
        #endregion

        #region Custom Calls
        public void Setup(Kit_IngameMain newMain, Killstreaks_Plugin newPlugin, KillstreakRuntimeData newKrd)
        {
            //Assign
            main = newMain;
            plugin = newPlugin;
            krd = newKrd;
            Close();
        }

        public void Open(Killstreak_KillstreakBase newKillstreak, Kit_PlayerBehaviour newPlayer, int idToRemoveFromInventory)
        {
            //Assign Killstreak
            currentKillstreak = newKillstreak;
            currentPlayer = newPlayer;
            currentIdToRemoveUponUse = idToRemoveFromInventory;
            root.SetActive(true);
            isOpen = true;
        }

        public void Close()
        {
            if (root)
            {
                root.SetActive(false);
                isOpen = false;
            }
        }

        public void LocationWasPicked(Vector3 location)
        {
            if (currentKillstreak.CanBeUsed(main, krd) && currentIdToRemoveUponUse >= 0)
            {
                //Call Killstreak
                currentKillstreak.OnUserSelectedLocation(main, currentPlayer, krd, location);
                krd.inventory.RemoveAt(currentIdToRemoveUponUse);
                krd.hud.Redraw(main, plugin, krd);
            }
            else
            {
                currentKillstreak.UseFailed(main, krd);
                if (currentKillstreak.useAnimation >= 0)
                {
                    if (krd.putWeaponAwayAutomaticallyBackToWeapons[0] == krd.useWeaponSlot)
                    {
                        currentPlayer.weaponManager.PluginSelectWeapon(currentPlayer, 0, 0, false);
                    }
                    else
                    {
                        //Put away weapon
                        currentPlayer.weaponManager.PluginSelectWeapon(currentPlayer, krd.putWeaponAwayAutomaticallyBackToWeapons[0], krd.putWeaponAwayAutomaticallyBackToWeapons[1], false);
                    }
                    main.SetPauseMenuState(false, true);
                }
            }

            currentIdToRemoveUponUse = -1;
            currentPlayer = null;
            currentKillstreak = null;

            //Close
            Close();
        }

        //Abort mission, abort mission!
        public void LocalPlayerDied()
        {
            currentPlayer = null;
            currentKillstreak = null;
            currentIdToRemoveUponUse = -1;
            Close();
        }
        #endregion
    }
}