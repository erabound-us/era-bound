using MarsFPSKit.UI;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public class TR_HubBootstrap : MonoBehaviour
    {
        public void OnLogin()
        {
            TR_HubApi.PlayerId = PhotonNetwork.LocalPlayer.NickName;

            // Hide the kit's menu canvas so TR_HubCanvas gets all clicks
            var menuManager = FindObjectOfType<Kit_MenuManager>();
            if (menuManager != null)
            {
                foreach (var screen in menuManager.menuScreens)
                    if (screen.root) screen.root.SetActive(false);
            }

            // Show the hub
            if (TR_HubManager.Instance != null)
                TR_HubManager.Instance.gameObject.SetActive(true);
        }
    }
}
