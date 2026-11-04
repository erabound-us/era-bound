using UnityEngine;
using MarsFPSKit;
using Photon.Pun;
using MarsFPSKit.UI;
using System.Collections.Generic;
using MarsFPSKit.Weapons;
using ExitGames.Client.Photon;

namespace KillstreaksPlugin
{
    public class KillstreakInventory
    {
        /// <summary>
        /// Which slot
        /// </summary>
        public int slotId;
        /// <summary>
        /// Id in slot
        /// </summary>
        public int id;
    }

    public class KillstreakRuntimeData
    {
        #region Player Related
        /// <summary>
        /// Our current killstreak (LOCAL PLAYER)
        /// </summary>
        public int currentKillstreak;
        /// <summary>
        /// Which killstreaks do we have selected?
        /// </summary>
        public int[] selectedKillstreaks;
        /// <summary>
        /// Which killstreaks do we have selected? Set by menu and copied on player spawn!
        /// </summary>
        public int[] desiredSelectedKillstreaks;
        /// <summary>
        /// Current killstreak inventory
        /// </summary>
        public List<KillstreakInventory> inventory = new List<KillstreakInventory>();
        /// <summary>
        /// Slot assigned to us by weapon manager!
        /// </summary>
        public int useWeaponSlot;
        #endregion

        #region Put Away Use Weapon Again
        /// <summary>
        /// Do we put it away or does the killstreak tell us to do so?
        /// </summary>
        public bool putWeaponAwayAutomatically;
        /// <summary>
        /// When do we put it away again
        /// </summary>
        public float putWeaponAwayAutomaticallyAt;
        /// <summary>
        /// Which weapons we go back to
        /// </summary>
        public int[] putWeaponAwayAutomaticallyBackToWeapons;
        #endregion

        /// <summary>
        /// Currently active HUD. Redraw is called at certain points by the plugin!
        /// </summary>
        public Killstreaks_HUD hud;
        /// <summary>
        /// Menu
        /// </summary>
        public Killstreaks_Menu menu;
        /// <summary>
        /// Location chooser (:
        /// </summary>
        public Killstreaks_LocationChooserModule locationChooser;
        /// <summary>
        /// Commander
        /// </summary>
        public Killstreaks_Commander commander;
    }

    public class KillstreakBotData
    {
        /// <summary>
        /// Bot killstreak
        /// </summary>
        public int currentKillstreak;
        /// <summary>
        /// Which killstreaks does this  bot have selected?
        /// </summary>
        public int[] selectedKillstreaks;
        /// <summary>
        /// Current killstreak inventory
        /// </summary>
        public List<KillstreakInventory> inventory = new List<KillstreakInventory>();
        /// <summary>
        /// Slot assigned to us by weapon manager!
        /// </summary>
        public int useWeaponSlot;
        #region Put Away Use Weapon Again
        /// <summary>
        /// Do we put it away or does the killstreak tell us to do so?
        /// </summary>
        public bool putWeaponAwayAutomatically;
        /// <summary>
        /// When do we put it away again
        /// </summary>
        public float putWeaponAwayAutomaticallyAt;
        /// <summary>
        /// Which weapons we go back to
        /// </summary>
        public int[] putWeaponAwayAutomaticallyBackToWeapons;
        #endregion
    }

    [System.Serializable]
    public class KillstreakSlot
    {
        public Killstreak_KillstreakBase[] possibleKillstreaks;
        /// <summary>
        /// Use Key for this slot
        /// </summary>
        public KeyCode useKey;
        /// <summary>
        /// So that we dont have to create these over and over we cache them here to be used later
        /// </summary>
        public KillstreakInventory[] associatedInventoryItems;
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Plugins/Killstreaks/Manager")]
    /// <summary>
    /// This plugin implements killstreaks
    /// </summary>
    public class Killstreaks_Plugin : Kit_Plugin
    {
        /// <summary>
        /// ID of our runtime data
        /// </summary>
        private int dataId = -1;

        /// <summary>
        /// The slots for our killstreaks (:
        /// </summary>
        public KillstreakSlot[] killstreakSlots;
        /// <summary>
        /// Can one and the same killstreak be in our inventory multiple times?
        /// </summary>
        public bool canGetKillstreakMultipleTimes;

        public bool botsCanUseInstantUseKillstreaks;

        public bool botsCanUseLocationKillstreaks;
        /// <summary>
        /// Should we show the user with a popup?
        /// </summary>
        public bool displayWhenKillstreakIsUnlocked = true;
        /// <summary>
        /// If for some reason we get stuck (slot we assigned and last slot is the same), we use this instead!
        /// </summary>
        public int[] fallBackWeapon = new int[2] { 0, 0 };

        [Header("Killstreak use weapons")]
        public Kit_WeaponBase[] useWeapons = new Kit_WeaponBase[0];

        /// <summary>
        /// Killstreak HUD prefab!
        /// </summary>
        [Header("UI")]
        public GameObject hudPrefab;
        /// <summary>
        /// Killstreak menu prefab, where you select your killstreaks!
        /// </summary>
        public GameObject menuPrefab;
        /// <summary>
        /// Prefab for location chooser
        /// </summary>
        public GameObject locationChooserPrefab;

        public override void OnPreSetup(Kit_IngameMain main)
        {
            //Get ID (current count)
            dataId = main.pluginRuntimeData.Count;
            //Create data
            KillstreakRuntimeData krd = new KillstreakRuntimeData();
            krd.selectedKillstreaks = new int[killstreakSlots.Length];
            //Same upon start
            krd.desiredSelectedKillstreaks = new int[killstreakSlots.Length];
            main.pluginRuntimeData.Add(krd);

            //Create inventory items
            for (int i = 0; i < killstreakSlots.Length; i++)
            {
                killstreakSlots[i].associatedInventoryItems = new KillstreakInventory[killstreakSlots[i].possibleKillstreaks.Length];
                for (int p = 0; p < killstreakSlots[i].possibleKillstreaks.Length; p++)
                {
                    killstreakSlots[i].associatedInventoryItems[p] = new KillstreakInventory();
                    int slot = i;
                    int id = p;
                    killstreakSlots[i].associatedInventoryItems[p].slotId = slot;
                    killstreakSlots[i].associatedInventoryItems[p].id = id;
                }
            }

            //Create HUD
            GameObject go = Instantiate(hudPrefab, main.pluginPlayerActiveHudGo, false);
            krd.hud = go.GetComponent<Killstreaks_HUD>();

            //Create Menu
            GameObject menu = Instantiate(menuPrefab, main.pluginModuleGo, false);
            //Get Component
            krd.menu = menu.GetComponent<Killstreaks_Menu>();
            krd.menu.Setup(main, this, krd);

            //Create Button
            main.InjectButtonIntoPauseMenu("Killstreaks").onClick.AddListener(delegate { krd.menu.Open(); });

            //Create Location Chooser
            GameObject locationChooser = Instantiate(locationChooserPrefab, main.pluginModuleGo, false);
            //Get Component
            krd.locationChooser = locationChooser.GetComponent<Killstreaks_LocationChooserModule>();
            krd.locationChooser.Setup(main, this, krd);

            //Find commander
            krd.commander = FindObjectOfType<Killstreaks_Commander>();
        }

        public override void OnSetupDone(Kit_IngameMain main)
        {
            //Get Data
            KillstreakRuntimeData krd = main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
            //Redraw HUD
            krd.hud.Redraw(main, this, krd);
            //Setup reset event
            main.pluginOnResetStats.AddListener(delegate { ResetKillstreaks(main, krd); });
        }

        public override void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {
            //Get Data
            KillstreakRuntimeData krd = main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
            //Relay Photon Event
            for (int i = 0; i < killstreakSlots.Length; i++)
            {
                for (int o = 0; o < killstreakSlots[i].possibleKillstreaks.Length; o++)
                {
                    killstreakSlots[i].possibleKillstreaks[o].OnPhotonEvent(main, krd, eventCode, content, senderId);
                }
            }
        }

        public void ResetKillstreaks(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            //Reset inventory
            krd.inventory = new List<KillstreakInventory>();
            //Redraw hud
            krd.hud.Redraw(main, this, krd);
        }

        public override void LocalPlayerUpdate(Kit_PlayerBehaviour player)
        {
            //Get Data
            KillstreakRuntimeData krd = player.main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
            //Get through inventory
            for (int i = 0; i < krd.inventory.Count; i++)
            {
                if (!krd.putWeaponAwayAutomatically && !krd.locationChooser.isOpen)
                {
                    if (Input.GetKeyDown(killstreakSlots[krd.inventory[i].slotId].useKey))
                    {
                        if (killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].CanBeUsed(player.main, krd))
                        {
                            //Play Animation if necessary
                            if (killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].useAnimation >= 0)
                            {
                                //Save weapon either way
                                krd.putWeaponAwayAutomaticallyBackToWeapons = player.weaponManager.GetCurrentlySelectedWeapon(player);

                                if (killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].GetKillstreakType() == KillstreakType.ImmediateUse)
                                {
                                    krd.putWeaponAwayAutomatically = true;
                                    krd.putWeaponAwayAutomaticallyAt = Time.time + killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].ImmediateUseTime();
                                }

                                player.weaponManager.PluginSelectWeapon(player, krd.useWeaponSlot, killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].useAnimation, true);
                            }
                            if (killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].GetKillstreakType() == KillstreakType.ImmediateUse)
                            {
                                //Use
                                killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].Use(player.main, player, krd);
                                //Remove
                                krd.inventory.RemoveAt(i);
                                //Redraw
                                krd.hud.Redraw(player.main, this, krd);
                            }
                            else if (killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].GetKillstreakType() == KillstreakType.RequiresLocation)
                            {
                                int id = i;
                                //Initiate a location choose attempt.
                                krd.locationChooser.Open(killstreakSlots[krd.inventory[id].slotId].possibleKillstreaks[krd.inventory[id].id], player, id);
                            }
                            break;
                        }
                        else
                        {
                            killstreakSlots[krd.inventory[i].slotId].possibleKillstreaks[krd.inventory[i].id].UseFailed(player.main, krd);
                        }
                    }
                }
            }

            if (krd.putWeaponAwayAutomatically)
            {
                if (Time.time > krd.putWeaponAwayAutomaticallyAt)
                {
                    if (krd.putWeaponAwayAutomaticallyBackToWeapons[0] == krd.useWeaponSlot)
                    {
                        player.weaponManager.PluginSelectWeapon(player, fallBackWeapon[0], fallBackWeapon[1], false);
                    }
                    else
                    {
                        player.weaponManager.PluginSelectWeapon(player, krd.putWeaponAwayAutomaticallyBackToWeapons[0], krd.putWeaponAwayAutomaticallyBackToWeapons[1], false);
                    }
                    krd.putWeaponAwayAutomatically = false;
                }
            }
        }

        public override void PlayerUpdate(Kit_PlayerBehaviour player)
        {
            if (player.isBot && PhotonNetwork.IsMasterClient)
            {
                KillstreakBotData botData = null;
                for (int i = 0; i < player.main.currentBotManager.bots.Count; i++)
                {
                    if (player.main.currentBotManager.bots[i].id == player.botId)
                    {
                        //Get Bot Data
                        for (int o = 0; i < player.main.currentBotManager.bots[i].pluginCustomBotData.Count; o++)
                        {
                            if (player.main.currentBotManager.bots[i].pluginCustomBotData[o] is KillstreakBotData)
                            {
                                botData = player.main.currentBotManager.bots[i].pluginCustomBotData[o] as KillstreakBotData;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (botData != null)
                {
                    if (botData.putWeaponAwayAutomatically)
                    {
                        if (Time.time > botData.putWeaponAwayAutomaticallyAt)
                        {
                            if (botData.putWeaponAwayAutomaticallyBackToWeapons[0] == botData.useWeaponSlot)
                            {
                                player.weaponManager.PluginSelectWeapon(player, fallBackWeapon[0], fallBackWeapon[1], false);
                            }
                            else
                            {
                                player.weaponManager.PluginSelectWeapon(player, botData.putWeaponAwayAutomaticallyBackToWeapons[0], botData.putWeaponAwayAutomaticallyBackToWeapons[1], false);
                            }
                            botData.putWeaponAwayAutomatically = false;
                        }
                    }
                }
            }
        }

        public override void Reset(Kit_MenuManager menu)
        {
            dataId = -1;
        }

        public override void LocalPlayerSpawned(Kit_PlayerBehaviour player)
        {
            //Get Data
            KillstreakRuntimeData krd = player.main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
            //Set to desired
            krd.selectedKillstreaks = krd.desiredSelectedKillstreaks;
            //Reset killstreak
            krd.currentKillstreak = 0;
            //Redraw HUD
            krd.hud.Redraw(player.main, this, krd);
            //Reset Bool
            krd.putWeaponAwayAutomatically = false;
        }

        public override void LocalPlayerScoredKill(Kit_IngameMain main, Hashtable deathInformation)
        {
            //Get Data
            KillstreakRuntimeData krd = main.pluginRuntimeData[dataId] as KillstreakRuntimeData;

            bool canBeAdded = true;

            for (int i = 0; i < killstreakSlots.Length; i++)
            {
                for (int o = 0; o < killstreakSlots[i].possibleKillstreaks.Length; o++)
                {
                    if (!killstreakSlots[i].possibleKillstreaks[o].CanKillBeAddedToKillstreaks(main, krd, deathInformation))
                    {
                        canBeAdded = false;
                        break;
                    }
                }
            }

            if (canBeAdded)
            {
                //Increase Killstreak
                krd.currentKillstreak++;
            }

            for (int i = 0; i < krd.selectedKillstreaks.Length; i++)
            {
                if (killstreakSlots[i].possibleKillstreaks[krd.selectedKillstreaks[i]].unlockedAtKills == krd.currentKillstreak)
                {
                    Debug.Log("Unlocked killstreak: " + killstreakSlots[i].possibleKillstreaks[krd.selectedKillstreaks[i]]);

                    if (canGetKillstreakMultipleTimes || !krd.inventory.Contains(killstreakSlots[i].associatedInventoryItems[krd.selectedKillstreaks[i]]))
                    {
                        //Show the player!
                        if (main.levelingUi && displayWhenKillstreakIsUnlocked)
                        {
                            main.levelingUi.DisplayOther(killstreakSlots[i].possibleKillstreaks[krd.selectedKillstreaks[i]].hudDisplaySprite, killstreakSlots[i].possibleKillstreaks[krd.selectedKillstreaks[i]].name);
                        }

                        //Call commander
                        if (krd.commander)
                        {
                            krd.commander.ReadyForDeploy(killstreakSlots[i].possibleKillstreaks[krd.selectedKillstreaks[i]]);
                        }

                        krd.inventory.Add(killstreakSlots[i].associatedInventoryItems[krd.selectedKillstreaks[i]]);
                    }
                }
            }

            Debug.Log("Current Killstreak: " + krd.currentKillstreak);

            //Redraw HUD
            krd.hud.Redraw(main, this, krd);
        }

        public override void LocalPlayerDied(Kit_PlayerBehaviour player)
        {
            //Get Data
            KillstreakRuntimeData krd = player.main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
            krd.locationChooser.LocalPlayerDied();
        }

        public override void BotWasCreated(Kit_BotManager manager, Kit_Bot bot)
        {
            KillstreakBotData botData = new KillstreakBotData();
            //Select some random killstreaks
            botData.selectedKillstreaks = new int[killstreakSlots.Length];
            for (int i = 0; i < killstreakSlots.Length; i++)
            {
                botData.selectedKillstreaks[i] = Random.Range(0, killstreakSlots[i].possibleKillstreaks.Length);
            }
            bot.pluginCustomBotData.Add(botData);
        }

        public override void BotScoredKill(Kit_IngameMain main, Kit_Bot bot, Hashtable deathInformation)
        {
            KillstreakBotData botData = null;

            //Get Bot Data
            for (int i = 0; i < bot.pluginCustomBotData.Count; i++)
            {
                if (bot.pluginCustomBotData[i] is KillstreakBotData)
                {
                    botData = bot.pluginCustomBotData[i] as KillstreakBotData;
                    break;
                }
            }

            if (botData != null)
            {
                bool canBeAdded = true;

                for (int i = 0; i < killstreakSlots.Length; i++)
                {
                    for (int o = 0; o < killstreakSlots[i].possibleKillstreaks.Length; o++)
                    {
                        if (!killstreakSlots[i].possibleKillstreaks[o].CanKillBeAddedToKillstreaks(main, bot, botData, deathInformation))
                        {
                            canBeAdded = false;
                            break;
                        }
                    }
                }

                if (canBeAdded)
                {
                    //Increase killstreak
                    botData.currentKillstreak++;
                }

                for (int i = 0; i < botData.selectedKillstreaks.Length; i++)
                {
                    if (killstreakSlots[i].possibleKillstreaks[botData.selectedKillstreaks[i]].unlockedAtKills == botData.currentKillstreak)
                    {
                        Debug.Log("Unlocked killstreak for bot " + bot.name + " " + killstreakSlots[i].possibleKillstreaks[botData.selectedKillstreaks[i]]);

                        if (canGetKillstreakMultipleTimes || !botData.inventory.Contains(killstreakSlots[i].associatedInventoryItems[botData.selectedKillstreaks[i]]))
                        {
                            botData.inventory.Add(killstreakSlots[i].associatedInventoryItems[botData.selectedKillstreaks[i]]);
                            //Check if bot can use this . . .
                            if (killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].GetKillstreakType() == KillstreakType.ImmediateUse && botsCanUseInstantUseKillstreaks)
                            {
                                //Try to use it right now
                                if (killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].CanBeUsed(main, botData))
                                {
                                    killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].Use(main, bot, botData);
                                    //Play Animation if necessary
                                    if (killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].useAnimation >= 0)
                                    {
                                        Kit_PlayerBehaviour player = main.currentBotManager.GetAliveBot(bot);
                                        botData.putWeaponAwayAutomatically = true;
                                        botData.putWeaponAwayAutomaticallyAt = Time.time + killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].ImmediateUseTime();
                                        botData.putWeaponAwayAutomaticallyBackToWeapons = player.weaponManager.GetCurrentlySelectedWeapon(player);

                                        //Start play!
                                        player.weaponManager.PluginSelectWeapon(player, botData.useWeaponSlot, killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].useAnimation, true);
                                    }
                                    //Remove from inventory
                                    botData.inventory.RemoveAt(botData.inventory.Count - 1);
                                }
                                else
                                {
                                    killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].UseFailed(main, botData);
                                }
                            }
                            else if (killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].GetKillstreakType() == KillstreakType.RequiresLocation && botsCanUseLocationKillstreaks)
                            {
                                //Try to use it right now
                                if (killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].CanBeUsed(main, botData))
                                {
                                    Kit_PlayerBehaviour player = main.currentBotManager.GetAliveBot(bot);
                                    if (player)
                                    {
                                        killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].OnUserSelectedLocation(main, bot, botData, player.transform.position);
                                        //Remove from inventory
                                        botData.inventory.RemoveAt(botData.inventory.Count - 1);
                                    }
                                }
                                else
                                {
                                    killstreakSlots[botData.inventory[botData.inventory.Count - 1].slotId].possibleKillstreaks[botData.inventory[botData.inventory.Count - 1].id].UseFailed(main, botData);
                                }
                            }
                        }
                    }
                }

                Debug.Log("Current Killstreak for bot " + bot.name + " " + botData.currentKillstreak);
            }
            else
            {
                Debug.LogError("Could not find bot data");
            }
        }

        public override void BotWasKilled(Kit_IngameMain main, Kit_Bot bot)
        {
            KillstreakBotData botData = null;

            //Get Bot Data
            for (int i = 0; i < bot.pluginCustomBotData.Count; i++)
            {
                if (bot.pluginCustomBotData[i] is KillstreakBotData)
                {
                    botData = bot.pluginCustomBotData[i] as KillstreakBotData;
                    break;
                }
            }

            if (botData != null)
            {
                //Reset killstreak;
                botData.currentKillstreak = 0;
                //Reset put away too
                botData.putWeaponAwayAutomatically = false;
            }
            else
            {
                Debug.LogError("Could not find bot data");
            }
        }

        public override void BotManagerOnPhotonSerializeView(Kit_BotManager manager, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //Send contents
                for (int i = 0; i < manager.bots.Count; i++)
                {
                    KillstreakBotData botData = null;

                    //Get Bot Data
                    for (int o = 0; o < manager.bots[i].pluginCustomBotData.Count; o++)
                    {
                        if (manager.bots[i].pluginCustomBotData[o] is KillstreakBotData)
                        {
                            botData = manager.bots[i].pluginCustomBotData[o] as KillstreakBotData;
                            break;
                        }
                    }

                    //Send
                    stream.SendNext(botData.currentKillstreak);
                    stream.SendNext(botData.selectedKillstreaks.Length);
                    for (int p = 0; p < botData.selectedKillstreaks.Length; p++)
                    {
                        stream.SendNext(botData.selectedKillstreaks[p]);
                    }

                    stream.SendNext(botData.inventory.Count);

                    for (int a = 0; a < botData.inventory.Count; a++)
                    {
                        stream.SendNext(botData.inventory[a].slotId);
                        stream.SendNext(botData.inventory[a].id);
                    }
                }
            }
            else
            {
                //Get contents
                for (int i = 0; i < manager.bots.Count; i++)
                {
                    KillstreakBotData botData = null;

                    //Get Bot Data
                    for (int o = 0; o < manager.bots[i].pluginCustomBotData.Count; o++)
                    {
                        if (manager.bots[i].pluginCustomBotData[o] is KillstreakBotData)
                        {
                            botData = manager.bots[i].pluginCustomBotData[o] as KillstreakBotData;
                            break;
                        }
                    }

                    //Send
                    botData.currentKillstreak = (int)stream.ReceiveNext();

                    int length = (int)stream.ReceiveNext();

                    if (botData.selectedKillstreaks.Length != length) botData.selectedKillstreaks = new int[length];

                    for (int p = 0; p < botData.selectedKillstreaks.Length; p++)
                    {
                        botData.selectedKillstreaks[p] = (int)stream.ReceiveNext();
                    }

                    length = (int)stream.ReceiveNext();

                    if (length != botData.inventory.Count)
                    {
                        botData.inventory = new List<KillstreakInventory>();
                        for (int p = 0; p < length; p++)
                        {
                            KillstreakInventory inventory = new KillstreakInventory();
                            botData.inventory.Add(inventory);
                        }
                    }

                    for (int a = 0; a < botData.inventory.Count; a++)
                    {
                        botData.inventory[a].slotId = (int)stream.ReceiveNext();
                        botData.inventory[a].id = (int)stream.ReceiveNext();
                    }
                }
            }
        }

        public override WeaponsFromPlugin WeaponsToInjectIntoWeaponManager(Kit_PlayerBehaviour player)
        {
            WeaponsFromPlugin injection = new WeaponsFromPlugin();
            injection.weaponsInSlot = new WeaponAttachmentBundle[useWeapons.Length];

            for (int i = 0; i < useWeapons.Length; i++)
            {
                injection.weaponsInSlot[i] = new WeaponAttachmentBundle();
                injection.weaponsInSlot[i].weapon = useWeapons[i];
                injection.weaponsInSlot[i].attachments = new int[0];
            }

            return injection;
        }

        public override void ReportSlotOfInjectedWeapons(Kit_PlayerBehaviour player, int slotWhereTheyWereInjected)
        {
            if (player.isBot)
            {
                Kit_Bot bot = player.main.currentBotManager.GetBotWithID(player.botId);

                KillstreakBotData botData = null;

                //Get Bot Data
                for (int i = 0; i < bot.pluginCustomBotData.Count; i++)
                {
                    if (bot.pluginCustomBotData[i] is KillstreakBotData)
                    {
                        botData = bot.pluginCustomBotData[i] as KillstreakBotData;
                        break;
                    }
                }

                botData.useWeaponSlot = slotWhereTheyWereInjected;
            }
            else
            {
                if (player.photonView.Owner.IsLocal)
                {
                    //Get Data
                    KillstreakRuntimeData krd = player.main.pluginRuntimeData[dataId] as KillstreakRuntimeData;
                    krd.useWeaponSlot = slotWhereTheyWereInjected;
                }
            }
        }
    }
}