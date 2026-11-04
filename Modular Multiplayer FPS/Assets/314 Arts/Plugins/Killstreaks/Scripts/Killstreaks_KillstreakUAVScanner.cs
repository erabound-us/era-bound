using MarsFPSKit;
using UnityEngine;

namespace KillstreaksPlugin
{
    public class Killstreaks_KillstreakUAVScanner : MonoBehaviour
    {
        #region Runtime
        private Kit_IngameMain main;
        #endregion

        #region Unity Calls
        private void Awake()
        {
            main = FindObjectOfType<Kit_IngameMain>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Kit_PlayerBehaviour player = other.GetComponentInParent<Kit_PlayerBehaviour>();
            if (player)
            {
                if (main.currentPvPGameModeBehaviour.AreWeEnemies(main, player.isBot, player.id))
                {
                    if (main.minimap)
                    {
                        main.minimap.PlayerShoots(player, false);
                    }
                }
            }
        }
        #endregion
    }
}