using MarsFPSKit.UI;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_MenuTreasureRoyale : Kit_MenuPveGameModeBase
    {
        public override void OpenMenu()
        {
            TR_MatchmakingUI.Instance?.gameObject.SetActive(true);
        }
    }
}
