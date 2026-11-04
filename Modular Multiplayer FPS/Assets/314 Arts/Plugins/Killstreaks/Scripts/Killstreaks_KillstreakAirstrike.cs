using ExitGames.Client.Photon;
using MarsFPSKit;
using Photon.Pun;
using UnityEngine;

namespace KillstreaksPlugin
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Plugins/Killstreaks/Airstrike")]
    public class Killstreaks_KillstreakAirstrike : Killstreak_KillstreakBase
    {
        /// <summary>
        /// Airstrike prefab
        /// </summary>
        public GameObject airstrikePrefab;
        /// <summary>
        /// Explosion prefab
        /// </summary>
        public GameObject explosionPrefab;

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakBotData kbd)
        {
            if (FindObjectOfType<Killstreaks_KillstreakAirstrikePlaneController>() || FindObjectOfType<Killstreaks_KillstreakAirstrikePlane>()) return false;
            return true;
        }

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            if (FindObjectOfType<Killstreaks_KillstreakAirstrikePlaneController>() || FindObjectOfType<Killstreaks_KillstreakAirstrikePlane>()) return false;
            return true;
        }

        public override void UseFailed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            main.DisplayMessage("Airspace is occupied");
            if (krd.locationChooser.isOpen)
            {
                //Close location chooser (just to make sure)...
                krd.locationChooser.Close();
                //Lock cursor
                main.SetPauseMenuState(false, true);
                MarsScreen.lockCursor = true;
            }
        }

        public override KillstreakType GetKillstreakType()
        {
            return KillstreakType.RequiresLocation;
        }

        public override void OnUserSelectedLocation(Kit_IngameMain main, Kit_PlayerBehaviour player, KillstreakRuntimeData krd, Vector3 location)
        {
            //Close location chooser (just to make sure)...
            krd.locationChooser.Close();
            if (useAnimation >= 0)
            {
                if (krd.putWeaponAwayAutomaticallyBackToWeapons[0] == krd.useWeaponSlot)
                {
                    player.weaponManager.PluginSelectWeapon(player, 0, 0, false);
                }
                else
                {
                    //Put away weapon
                    player.weaponManager.PluginSelectWeapon(player, krd.putWeaponAwayAutomaticallyBackToWeapons[0], krd.putWeaponAwayAutomaticallyBackToWeapons[1], false);
                }
            }
            //Lock cursor
            main.SetPauseMenuState(false, true);
            MarsScreen.lockCursor = true;
            //Send Photon Event
            object[] instData = new object[4];
            instData[0] = false;
            instData[1] = player.photonView.Owner.ActorNumber;
            instData[2] = player.GetInstanceID();
            instData[3] = location;

            //Find Settings
            Killstreaks_KillstreakAirstrikeMapSettings settings = FindObjectOfType<Killstreaks_KillstreakAirstrikeMapSettings>();

            if (settings)
            {
                //Pick random rotation from where they come
                float yRot = Random.Range(0, 360f);

                Hashtable table = new Hashtable();
                table[(byte)0] = airstrikePrefab.name;
                table[(byte)1] = location;
                table[(byte)2] = Quaternion.Euler(0, yRot, 0);
                table[(byte)3] = (byte)0;
                table[(byte)4] = instData;

                PhotonNetwork.RaiseEvent(6, table, new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });
            }
            else
            {
                throw new System.Exception("No Airstrike Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            }
        }

        public override void OnPhotonEvent(Kit_IngameMain main, KillstreakRuntimeData krd, byte eventCode, object content, int senderId)
        {
            if (eventCode == 30)
            {
                if (explosionPrefab)
                {
                    Hashtable table = content as Hashtable;

                    GameObject go = Instantiate(explosionPrefab, (Vector3)table["position"], Quaternion.identity);
                    if (go.GetComponent<Kit_Explosion>())
                    {
                        go.GetComponent<Kit_Explosion>().Explode(PhotonNetwork.IsMasterClient, (bool)table["ownerIsBot"], (int)table["ownerId"], "Airstrike");
                    }
                }
            }
        }

        public override void OnUserSelectedLocation(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData kbd, Vector3 location)
        {
            //Send Photon Event
            object[] instData = new object[4];
            instData[0] = true;
            instData[1] = bot.id;
            instData[2] = main.currentBotManager.GetAliveBot(bot).GetInstanceID();
            instData[3] = location;

            //Find Settings
            Killstreaks_KillstreakAirstrikeMapSettings settings = FindObjectOfType<Killstreaks_KillstreakAirstrikeMapSettings>();

            if (settings)
            {
                //Pick random rotation from where they come
                float yRot = Random.Range(0, 360f);

                Hashtable table = new Hashtable();
                table[(byte)0] = airstrikePrefab.name;
                table[(byte)1] = location;
                table[(byte)2] = Quaternion.Euler(0, yRot, 0);
                table[(byte)3] = (byte)0;
                table[(byte)4] = instData;

                PhotonNetwork.RaiseEvent(6, table, new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });
            }
            else
            {
                throw new System.Exception("No Airstrike Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            }
        }

        public override bool CanKillBeAddedToKillstreaks(Kit_IngameMain main, KillstreakRuntimeData krd, Hashtable deathInformation)
        {
            if (deathInformation[(byte)4].GetType() == typeof(int))
            {
                return true;
            }
            else if (deathInformation[(byte)4].GetType() == typeof(string))
            {
                string cause = (string)deathInformation[(byte)4];

                if (cause == "Airstrike")
                {
                    Killstreaks_KillstreakAirstrikePlane plane = FindObjectOfType<Killstreaks_KillstreakAirstrikePlane>();
                    if (plane)
                    {
                        if (!plane.ownerIsBot && plane.ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                        {
                            if (main.myPlayer)
                            {
                                if (main.myPlayer.GetInstanceID() == plane.ownerInstanceId) return true;
                                else return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                else return true;
            }

            return true;
        }

        public override bool CanKillBeAddedToKillstreaks(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData botData, Hashtable deathInformation)
        {
            if (deathInformation[(byte)4].GetType() == typeof(int))
            {
                return true;
            }
            else if (deathInformation[(byte)4].GetType() == typeof(string))
            {
                string cause = (string)deathInformation[(byte)4];

                if (cause == "Airstrike")
                {
                    Killstreaks_KillstreakAirstrikePlane plane = FindObjectOfType<Killstreaks_KillstreakAirstrikePlane>();
                    if (plane)
                    {
                        if (plane.ownerIsBot && plane.ownerId == bot.id)
                        {
                            Kit_PlayerBehaviour pb = main.currentBotManager.GetAliveBot(bot);
                            if (pb)
                            {
                                if (pb.GetInstanceID() == plane.ownerInstanceId) return true;
                                else return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                else return true;
            }

            return true;
        }
    }
}