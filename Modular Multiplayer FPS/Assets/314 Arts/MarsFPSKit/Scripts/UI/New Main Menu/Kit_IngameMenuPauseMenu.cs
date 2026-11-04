using Photon.Pun;
using TMPro;
using UnityEngine;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_IngameMenuPauseMenu : MonoBehaviour
        {
            /// <summary>
            /// Main reference
            /// </summary>
            public Kit_IngameMain main;
            /// <summary>
            /// ID for the pause menu
            /// </summary>
            public int pauseMenuId = 1;
            /// <summary>
            /// Button that takes us to the loadout menu
            /// </summary>
            public GameObject loadoutButton;
            /// <summary>
            /// Spawn  Button
            /// </summary>
            public TextMeshProUGUI spawnButtonText;
            /// <summary>
            /// Because this button controls both, changing teams and commiting suicide, we need to adjust its text
            /// </summary>
            public TextMeshProUGUI changeTeamButtonText;
            /// <summary>
            /// The change team button
            /// </summary>
            public GameObject changeTeamButton;
            /// <summary>
            /// Button that lets us vote
            /// </summary>
            public GameObject voteButton;
            /// <summary>
            /// Transform where the button goes
            /// </summary>
            public RectTransform pluginButtonGo;
            /// <summary>
            /// Prefab for inejcting a button into the pause menu
            /// </summary>
            public GameObject pluginButtonPrefab;

            private void Start()
            {
                //Is loadout supported?
                if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.LoadoutMenuSupported())
                {
                    loadoutButton.SetActive(true);
                }
                else
                {
                    loadoutButton.SetActive(false);
                }

                // Inject Disconnect button using the plugin button system
                if (pluginButtonPrefab && pluginButtonGo)
                {
                    // Disconnect button — returns to main menu
                    var disconnectBtn = Instantiate(pluginButtonPrefab, pluginButtonGo);
                    var disconnectText = disconnectBtn.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (disconnectText) disconnectText.text = "Disconnect";
                    var disconnectTmp = disconnectBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (disconnectTmp) disconnectTmp.text = "Disconnect";
                    var disconnectButton = disconnectBtn.GetComponent<UnityEngine.UI.Button>();
                    if (disconnectButton) { disconnectButton.interactable = true; disconnectButton.onClick.AddListener(QuitToMainMenu); }
                }
            }

            /// <summary>
            /// Disconnects from Photon and returns to the main menu scene.
            /// OnDisconnected in Kit_IngameMain handles the scene load.
            /// </summary>
            public void QuitToMainMenu()
            {
                main.isShuttingDown = false;
                TR_OriginTimelineManager.OfflineMatchProps = null;
                TR_OriginTimelineManager.IsOfflineMatch = false;
                // Leave the offline room and disable offline mode before loading MainMenu
                // Otherwise Photon stays "connected" and MainMenu skips normal flow
                if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
                PhotonNetwork.OfflineMode = false;
                Kit_SceneSyncer.instance.LoadScene("MainMenu");
            }

            /// <summary>
            /// Quits the application entirely.
            /// </summary>
            public void QuitGame()
            {
                main.isShuttingDown = true;
                Application.Quit();
            }

            private void Update()
            {
                #region Team - Suicide Button
                if (main.currentPvEGameModeBehaviour)
                {
                    changeTeamButton.gameObject.SetActiveOptimized(false);
                }
                else if (main.myPlayer)
                {
                    changeTeamButtonText.text = "Suicide";
                    changeTeamButton.gameObject.SetActiveOptimized(true);
                }
                else if (main.currentPvPGameModeBehaviour)
                {
                    changeTeamButtonText.text = "Change Team";
                    changeTeamButton.gameObject.SetActiveOptimized(true);
                }
                else
                {
                    changeTeamButton.gameObject.SetActiveOptimized(false);
                }
                #endregion

                #region Spawn/Resume Button
                if (main.myPlayer)
                {
                    spawnButtonText.text = "Resume";
                }
                else
                {
                    if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.CanSpawn(main, PhotonNetwork.LocalPlayer))
                    {
                        spawnButtonText.text = "Spawn";
                    }
                    else
                    {
                        spawnButtonText.text = "Close";
                    }
                }
                #endregion

                #region Vote Button
                //Voting only in pvp game modes
                voteButton.SetActiveOptimized(main.currentPvPGameModeBehaviour);
                #endregion
            }
        }
    }
}