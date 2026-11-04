using ExitGames.Client.Photon;
using MarsFPSKit;
using Photon.Pun;
using UnityEngine;

namespace KillstreaksPlugin
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Plugins/Killstreaks/Helicopter")]
    public class Killstreaks_KillstreakHelicopter : Killstreak_KillstreakBase
    {
        /// <summary>
        /// The prefab of the Helicopter!
        /// </summary>
        public GameObject helicopterPrefab;

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakBotData kbd)
        {
            Killstreaks_KillstreakHelicopterAI ai = FindObjectOfType<Killstreaks_KillstreakHelicopterAI>();
            if (ai && ai.helicopterState != 3) return false;
            return true;
        }

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            Killstreaks_KillstreakHelicopterAI ai = FindObjectOfType<Killstreaks_KillstreakHelicopterAI>();
            if (ai && ai.helicopterState != 3) return false;
            return true;
        }

        public override void UseFailed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            main.DisplayMessage("Airspace is occupied");
        }

        public override void Use(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData kbd)
        {
            object[] instData = new object[3];
            instData[0] = true;
            instData[1] = bot.id;
            instData[2] = main.currentBotManager.GetAliveBot(bot).GetInstanceID();

            //Find Settings
            Killstreaks_KillstreakHelicopterMapSettings settings = FindObjectOfType<Killstreaks_KillstreakHelicopterMapSettings>();

            if (settings)
            {
                int spawn = Random.Range(0, settings.spawnPoints.Length);

                PhotonNetwork.InstantiateSceneObject(helicopterPrefab.name, settings.spawnPoints[spawn].position, settings.spawnPoints[spawn].rotation, 0, instData);
            }
            else
            {
                throw new System.Exception("No Helicopter Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            }
        }

        public override void Use(Kit_IngameMain main, Kit_PlayerBehaviour player, KillstreakRuntimeData krd)
        {
            object[] instData = new object[3];
            instData[0] = false;
            instData[1] = player.photonView.Owner.ActorNumber;
            instData[2] = player.GetInstanceID();

            //Find Settings
            Killstreaks_KillstreakHelicopterMapSettings settings = FindObjectOfType<Killstreaks_KillstreakHelicopterMapSettings>();

            if (settings)
            {
                int spawn = Random.Range(0, settings.spawnPoints.Length);

                Hashtable table = new Hashtable();
                table[(byte)0] = helicopterPrefab.name;
                table[(byte)1] = settings.spawnPoints[spawn].position;
                table[(byte)2] = settings.spawnPoints[spawn].rotation;
                table[(byte)3] = (byte)0;
                table[(byte)4] = instData;

                PhotonNetwork.RaiseEvent(6, table, new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });

                //PhotonNetwork.InstantiateSceneObject(helicopterPrefab.name, settings.spawnPoints[spawn].position, settings.spawnPoints[spawn].rotation, 0, instData);
            }
            else
            {
                throw new System.Exception("No Helicopter Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
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

                if (cause == "Helicopter")
                {
                    Killstreaks_KillstreakHelicopterAI heli = FindObjectOfType<Killstreaks_KillstreakHelicopterAI>();
                    if (heli)
                    {
                        if (!heli.ownerIsBot && heli.ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
                        {
                            if (main.myPlayer)
                            {
                                if (main.myPlayer.GetInstanceID() == heli.ownerInstanceId) return true;
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

                if (cause == "Helicopter")
                {
                    Killstreaks_KillstreakHelicopterAI heli = FindObjectOfType<Killstreaks_KillstreakHelicopterAI>();
                    if (heli)
                    {
                        if (heli.ownerIsBot && heli.ownerId == bot.id)
                        {
                            Kit_PlayerBehaviour pb = main.currentBotManager.GetAliveBot(bot);
                            if (pb)
                            {
                                if (pb.GetInstanceID() == heli.ownerInstanceId) return true;
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