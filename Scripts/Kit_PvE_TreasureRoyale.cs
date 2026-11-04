using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "TreasureRoyale/Game Mode/Treasure Royale")]
    public class Kit_PvE_TreasureRoyale : Kit_PvE_GameModeBase
    {
        [Header("Treasure Royale")]
        public int matchDurationSeconds = 900;
        public string[] eraIds;

        public override PlayerModelConfig GetPlayerModel(Kit_PlayerBehaviour pb)
        {
            return spawnPlayerModels[Random.Range(0, spawnPlayerModels.Length)];
        }

        public PlayerModelConfig[] spawnPlayerModels;

        public override Loadout GetSpawnLoadout()
        {
            return spawnLoadouts[Random.Range(0, spawnLoadouts.Length)];
        }

        public Loadout[] spawnLoadouts;

        public override void GamemodeSetup(Kit_IngameMain main)
        {
            Kit_PlayerSpawn[] allSpawns = Object.FindObjectsOfType<Kit_PlayerSpawn>();
            if (allSpawns.Length == 0) throw new System.Exception("No spawns in scene.");

            List<Kit_PlayerSpawn> filtered = allSpawns
                .Where(s => s.coopGameModes.Contains(this))
                .ToList();

            main.internalSpawns = new List<InternalSpawns> { new InternalSpawns() };
            main.internalSpawns[0].spawns = filtered;

            main.timer = matchDurationSeconds;
        }

        public override void GameModeProceed(Kit_IngameMain main)
        {
            main.pauseMenuState = PauseMenuState.main;
            main.Spawn(true);
        }

        public override void OnLocalPlayerDeathCameraEnded(Kit_IngameMain main)
        {
            main.Spawn(true);
        }

        public override void GameModeUpdate(Kit_IngameMain main) { }

        public override void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed) { }

        public override void TimeRunOut(Kit_IngameMain main)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            // Trigger end-of-match — highest artifact score wins
            // Kit_IngameMain.EndGame is called by TreasureRoyalePlugin which tracks scores
        }

        public override Transform GetSpawn(Kit_IngameMain main, Player player)
        {
            return GetBestSpawn(main, player, null);
        }

        public override Transform GetSpawn(Kit_IngameMain main, Kit_Bot bot)
        {
            return GetBestSpawn(main, null, bot);
        }

        Transform GetBestSpawn(Kit_IngameMain main, Player player, Kit_Bot bot)
        {
            var spawns = main.internalSpawns[0].spawns;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var candidate = spawns[Random.Range(0, spawns.Count)];
                bool valid = player != null
                    ? spawnSystemToUse.CheckSpawnPosition(main, candidate.transform, player)
                    : spawnSystemToUse.CheckSpawnPosition(main, candidate.transform, bot);
                if (valid) return candidate.transform;
            }
            return spawns[0].transform;
        }

        public override bool CanSpawn(Kit_IngameMain main, Player player) => true;
        public override bool CanControlPlayer(Kit_IngameMain main) => true;
        public override bool CanStartVote(Kit_IngameMain main) => false;

#if UNITY_EDITOR
        public override string[] GetSceneCheckerMessages() =>
            new[] { "Treasure Royale: ensure DropZoneMarkers and ExtractionBeacon are placed." };

        public override MessageType[] GetSceneCheckerMessageTypes() =>
            new[] { MessageType.Info };
#endif
    }
}
