using UnityEngine;
using MarsFPSKit;
using ExitGames.Client.Photon;

namespace KillstreaksPlugin
{
    public enum KillstreakType { ImmediateUse, RequiresLocation }

    public abstract class Killstreak_KillstreakBase : ScriptableObject
    {
        /// <summary>
        /// At how many skills do we unlock this killstreak?
        /// </summary>
        public int unlockedAtKills = 3;
        /// <summary>
        /// How it looks in the HUD!
        /// </summary>
        public Sprite hudDisplaySprite;
        /// <summary>
        /// Text for menu
        /// </summary>
        public string menuPreviewText;
        /// <summary>
        /// Sprite for menu
        /// </summary>
        public Sprite menuPreviewSprite;
        /// <summary>
        /// If this is >= 0, then this use animation weapon will be played by the weapon manager!
        /// </summary>
        public int useAnimation = -1;

        public virtual bool CanBeUsed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {
            return true;
        }

        public virtual bool CanBeUsed(Kit_IngameMain main, KillstreakBotData kbd)
        {
            return true;
        }

        public virtual void UseFailed(Kit_IngameMain main, KillstreakRuntimeData krd)
        {

        }

        public virtual void UseFailed(Kit_IngameMain main, KillstreakBotData kbd)
        {

        }

        public virtual void Use(Kit_IngameMain main, Kit_PlayerBehaviour player, KillstreakRuntimeData krd)
        {

        }

        public virtual void Use(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData kbd)
        {

        }

        public virtual KillstreakType GetKillstreakType()
        {
            return KillstreakType.ImmediateUse;
        }

        public virtual float ImmediateUseTime()
        {
            return 1f;
        }

        public virtual void OnUserSelectedLocation(Kit_IngameMain main, Kit_PlayerBehaviour player, KillstreakRuntimeData krd, Vector3 location)
        {

        }

        public virtual void OnUserSelectedLocation(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData kbd, Vector3 location)
        {

        }

        public virtual bool CanKillBeAddedToKillstreaks(Kit_IngameMain main, KillstreakRuntimeData krd, Hashtable deathInformation)
        {
            return true;
        }

        public virtual bool CanKillBeAddedToKillstreaks(Kit_IngameMain main, Kit_Bot bot, KillstreakBotData botData, Hashtable deathInformation)
        {
            return true;
        }

        /// <summary>
        /// Called by ingame main for photon events
        /// </summary>
        /// <param name="main"></param>
        /// <param name="eventCode"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        public virtual void OnPhotonEvent(Kit_IngameMain main, KillstreakRuntimeData krd, byte eventCode, object content, int senderId)
        {

        }
    }
}