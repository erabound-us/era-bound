using ExitGames.Client.Photon;
using MarsFPSKit;
using Photon.Pun;
using UnityEngine;

namespace KillstreaksPlugin
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Plugins/Killstreaks/UAV")]
    public class Killstreaks_KillstreakUAV : Killstreak_KillstreakBase
    {
        /// <summary>
        /// The prefab of the UAV!
        /// </summary>
        public GameObject uavPrefab;

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakBotData kbd)
        {
            if (FindObjectOfType<Killstreaks_KillstreakUAVPlane>()) return false;
            return true;
        }

        public override bool CanBeUsed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            if (FindObjectOfType<Killstreaks_KillstreakUAVPlane>()) return false;
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
            Killstreaks_KillstreakUAVMapSettings settings = FindObjectOfType<Killstreaks_KillstreakUAVMapSettings>();

            if (settings)
            {
                PhotonNetwork.InstantiateSceneObject(uavPrefab.name, settings.startPosition.position, settings.startPosition.rotation, 0, instData);
            }
            else
            {
                throw new System.Exception("No UAV Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            }
        }

        public override void Use(Kit_IngameMain main, Kit_PlayerBehaviour player, KillstreakRuntimeData krd)
        {
            object[] instData = new object[3];
            instData[0] = false;
            instData[1] = player.photonView.Owner.ActorNumber;
            instData[2] = player.GetInstanceID();

            //Find Settings
            Killstreaks_KillstreakUAVMapSettings settings = FindObjectOfType<Killstreaks_KillstreakUAVMapSettings>();

            if (settings)
            {
                Hashtable table = new Hashtable();
                table[(byte)0] = uavPrefab.name;
                table[(byte)1] = settings.startPosition.position;
                table[(byte)2] = settings.startPosition.rotation;
                table[(byte)3] = (byte)0;
                table[(byte)4] = instData;

                PhotonNetwork.RaiseEvent(6, table, new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });

                //PhotonNetwork.InstantiateSceneObject(uavPrefab.name, settings.startPosition.position, settings.startPosition.rotation, 0, instData);
            }
            else
            {
                throw new System.Exception("No UAV Map settings where found! Did you forget to drag the KillstreakMapPrefab into your scene?");
            }
        }
    }
}