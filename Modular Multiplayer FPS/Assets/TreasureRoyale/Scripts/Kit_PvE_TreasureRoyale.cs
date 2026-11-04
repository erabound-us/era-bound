using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Linq;
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

            List<Kit_PlayerSpawn> filtered = new List<Kit_PlayerSpawn>();
            if (allSpawns.Length > 0)
            {
                filtered = allSpawns.Where(s => s.coopGameModes.Contains(this)).ToList();
                if (filtered.Count == 0)
                    filtered = allSpawns.ToList();
            }

            // Always create safe default spawns at map corners (80 units from center)
            // This overrides any scene spawns that may be near the river
            string[] spawnNames = { "TR_DefaultSpawn_A", "TR_DefaultSpawn_B", "TR_DefaultSpawn_C", "TR_DefaultSpawn_D" };
            Vector3[] spawnPositions = {
                new Vector3( 80f, 1.5f,  80f),
                new Vector3(-80f, 1.5f,  80f),
                new Vector3( 80f, 1.5f, -80f),
                new Vector3(-80f, 1.5f, -80f)
            };
            // Move existing spawns to safe positions or create new ones
            if (filtered.Count > 0)
            {
                for (int i = 0; i < filtered.Count && i < spawnPositions.Length; i++)
                    filtered[i].transform.position = spawnPositions[i];
                // If fewer than 4, add more
                for (int i = filtered.Count; i < spawnPositions.Length; i++)
                {
                    var go = new GameObject(spawnNames[i]);
                    go.transform.position = spawnPositions[i];
                    filtered.Add(go.AddComponent<Kit_PlayerSpawn>());
                }
            }
            else
            {
                for (int i = 0; i < spawnNames.Length; i++)
                {
                    var go = new GameObject(spawnNames[i]);
                    go.transform.position = spawnPositions[i];
                    filtered.Add(go.AddComponent<Kit_PlayerSpawn>());
                }
                Debug.Log("[TR] Created 4 default spawns at map corners.");
            }

            main.internalSpawns = new List<InternalSpawns> { new InternalSpawns() };
            main.internalSpawns[0].spawns = filtered;
            main.timer = matchDurationSeconds;
        }

        public override void GameModeProceed(Kit_IngameMain main)
        {
            Debug.Log("[TR] Kit_PvE_TreasureRoyale.GameModeProceed called");
            main.pauseMenuState = PauseMenuState.main;
            MarsScreen.lockCursor = true;
            if (main.pauseMenu != null)
            {
                var buttons = main.pauseMenu.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                foreach (var btn in buttons)
                {
                    for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                    {
                        if (btn.onClick.GetPersistentMethodName(i) == "Disconnect")
                        {
                            btn.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
            if (!main.GetComponent<TR_HUDSkin>())
                main.gameObject.AddComponent<TR_HUDSkin>();
            // Wait for map generation before spawning
            main.StartCoroutine(SpawnWhenReady(main));
        }

        System.Collections.IEnumerator SpawnWhenReady(Kit_IngameMain main)
        {
            // Show loading overlay while map generates
            var canvas = new GameObject("TR_LoadingCanvas");
            var c = canvas.AddComponent<UnityEngine.Canvas>();
            c.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 999;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Dark background
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvas.transform, false);
            var titleText = titleGO.AddComponent<TMPro.TextMeshProUGUI>();
            titleText.text = "TREASURE ROYALE";
            titleText.fontSize = 48; titleText.fontStyle = TMPro.FontStyles.Bold;
            titleText.alignment = TMPro.TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0.2f);
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.55f); titleRect.anchorMax = new Vector2(0.9f, 0.75f);
            titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;

            // Status text
            var statusGO = new GameObject("Status");
            statusGO.transform.SetParent(canvas.transform, false);
            var statusText = statusGO.AddComponent<TMPro.TextMeshProUGUI>();
            statusText.text = "Generating multiverse timeline...";
            statusText.fontSize = 24;
            statusText.alignment = TMPro.TextAlignmentOptions.Center;
            statusText.color = Color.white;
            var statusRect = statusGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.42f); statusRect.anchorMax = new Vector2(0.9f, 0.55f);
            statusRect.offsetMin = statusRect.offsetMax = Vector2.zero;

            // Tip text
            string[] tips = {
                "Tip: Artifacts score points — kills don't win the match.",
                "Tip: Higher rarity artifacts are worth more score.",
                "Tip: The river and lakes are dangerous — watch your air.",
                "Tip: Each timeline has unique terrain and weather.",
                "Tip: The multiverse seed determines this map's layout."
            };
            var tipGO = new GameObject("Tip");
            tipGO.transform.SetParent(canvas.transform, false);
            var tipText = tipGO.AddComponent<TMPro.TextMeshProUGUI>();
            tipText.text = tips[Random.Range(0, tips.Length)];
            tipText.fontSize = 18;
            tipText.alignment = TMPro.TextAlignmentOptions.Center;
            tipText.color = new Color(0.7f, 0.9f, 1f);
            var tipRect = tipGO.GetComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.1f, 0.3f); tipRect.anchorMax = new Vector2(0.9f, 0.42f);
            tipRect.offsetMin = tipRect.offsetMax = Vector2.zero;

            // Wait for map generation
            float timeout = 30f;
            float dots = 0f;
            string[] dotFrames = { ".", "..", "..." };
            // Wait at least 2 frames for EraMapGenerator.Start() to complete
            yield return null;
            yield return null;
            while (!EraMapGenerator.mapGenerationComplete && timeout > 0f)
            {
                timeout -= UnityEngine.Time.deltaTime;
                dots += UnityEngine.Time.deltaTime * 2f;
                statusText.text = "Generating multiverse timeline" + dotFrames[(int)(dots % 3)];
                yield return null;
            }

            statusText.text = "Entering timeline...";
            yield return new WaitForSeconds(0.5f);

            // Run map validation
            var validation = TR_MapValidator.Validate();
            if (!validation.passed)
            {
                statusText.text = "<color=red>MAP VALIDATION FAILED — Regenerating...</color>";
                tipText.text = string.Join("\n", validation.errors.GetRange(0, Mathf.Min(3, validation.errors.Count)));
                tipText.color = new Color(1f, 0.4f, 0.4f);
                foreach (var e in validation.errors)
                    Debug.LogError("[TR MapValidator] ERROR: " + e);
                yield return new WaitForSeconds(2f);
                // Attempt to auto-fix critical issues
                TR_MapValidator.AutoFix();
                yield return new WaitForSeconds(0.5f);
                // Re-validate
                validation = TR_MapValidator.Validate();
                if (!validation.passed)
                {
                    statusText.text = "<color=red>CRITICAL MAP ERRORS — Contact Support</color>";
                    yield return new WaitForSeconds(3f);
                    // Spawn anyway as last resort
                }
                else
                {
                    statusText.text = "<color=green>Map issues resolved ✓</color>";
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else if (validation.warnings.Count > 0)
            {
                statusText.text = $"Map ready ({validation.warnings.Count} warnings)";
                foreach (var w in validation.warnings)
                    Debug.LogWarning("[TR MapValidator] " + w);
            }
            else
            {
                statusText.text = "Map validated ✓";
                Debug.Log("[TR MapValidator] All checks passed.");
            }
            yield return new WaitForSeconds(0.3f);

            // Fade out
            float fade = 1f;
            while (fade > 0f)
            {
                fade -= UnityEngine.Time.deltaTime * 3f;
                bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f * fade);
                titleText.color = new Color(1f, 0.85f, 0.2f, fade);
                statusText.color = new Color(1f, 1f, 1f, fade);
                tipText.color = new Color(0.7f, 0.9f, 1f, fade);
                yield return null;
            }
            Object.Destroy(canvas);

            yield return new WaitForSeconds(0.1f);
            var allPlayers = Object.FindObjectsOfType<Kit_PlayerBehaviour>();
            Debug.Log($"[TR] Spawn: active Kit_PlayerBehaviour count={allPlayers.Length}");
            main.Spawn(true);
            // Add FPArmHider immediately — before any frame renders
            if (main.myPlayer && !main.myPlayer.GetComponent<TR_FPArmHider>())
                main.myPlayer.gameObject.AddComponent<TR_FPArmHider>();
            yield return null; // wait one frame for player to be set up
            var allPlayersAfter = Object.FindObjectsOfType<Kit_PlayerBehaviour>();
            Debug.Log($"[TR] After Spawn: active Kit_PlayerBehaviour count={allPlayersAfter.Length}, myPlayer={(main.myPlayer != null ? main.myPlayer.name : "null")}");
            foreach (var p in allPlayersAfter) Debug.Log($"[TR]   player: {p.name} pos={p.transform.position} isMine={p.photonView.IsMine}");
            // Dump all renderers on the local player for twin diagnosis

            if (main.myPlayer && !main.myPlayer.GetComponent<TR_FPArmHider>())
                main.myPlayer.gameObject.AddComponent<TR_FPArmHider>();
        }

        public override void OnLocalPlayerDeathCameraEnded(Kit_IngameMain main)
        {
            main.Spawn(true);
            main.StartCoroutine(AddFPArmHiderNextFrame(main));
        }

        System.Collections.IEnumerator AddFPArmHiderNextFrame(Kit_IngameMain main)
        {
            yield return null;
            if (main.myPlayer && !main.myPlayer.GetComponent<TR_FPArmHider>())
                main.myPlayer.gameObject.AddComponent<TR_FPArmHider>();
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
            if (spawnSystemToUse == null) return SnapToGround(spawns[Random.Range(0, spawns.Count)].transform);
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var candidate = spawns[Random.Range(0, spawns.Count)];
                bool valid = player != null
                    ? spawnSystemToUse.CheckSpawnPosition(main, candidate.transform, player)
                    : spawnSystemToUse.CheckSpawnPosition(main, candidate.transform, bot);
                if (valid) return SnapToGround(candidate.transform);
            }
            return SnapToGround(spawns[0].transform);
        }

        // Fixed safe spawn positions — never computed from trigger bounds
        static readonly Vector3[] _safeSpawns = {
            new Vector3( 80f, 1.5f,  80f),
            new Vector3(-80f, 1.5f,  80f),
            new Vector3( 80f, 1.5f, -80f),
            new Vector3(-80f, 1.5f, -80f),
        };

        Transform SnapToGround(Transform t)
        {
            // Use fixed safe positions — never mutate the spawn transform
            // Pick the safe position closest to the original spawn transform
            Vector3 origin = new Vector3(t.position.x, 1.5f, t.position.z);
            Vector3 best = _safeSpawns[0];
            float bestDist = float.MaxValue;
            foreach (var s in _safeSpawns)
            {
                float d = Vector3.Distance(new Vector3(origin.x, 0, origin.z), new Vector3(s.x, 0, s.z));
                if (d < bestDist) { bestDist = d; best = s; }
            }
            // Don't mutate t — return a temporary transform
            var go = new GameObject("TR_SpawnPoint_Temp");
            go.transform.position = best;
            Object.Destroy(go, 5f);
            return go.transform;
        }

        public override bool CanSpawn(Kit_IngameMain main, Player player) => true;
        public override bool CanControlPlayer(Kit_IngameMain main) => true;

        public override bool CanDropWeapons(Kit_IngameMain main) => false;
        public override bool CanStartVote(Kit_IngameMain main) => false;

        static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

#if UNITY_EDITOR
        public override string[] GetSceneCheckerMessages() =>
            new[] { "Treasure Royale: ensure DropZoneMarkers and ExtractionBeacon are placed." };

        public override MessageType[] GetSceneCheckerMessageTypes() =>
            new[] { MessageType.Info };
#endif
    }
}
