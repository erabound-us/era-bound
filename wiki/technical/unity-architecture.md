# Unity Architecture

## Stack
- **Engine**: Unity — Modular Multiplayer FPS base (MarsFPSKit, namespace `MarsFPSKit`)
- **Networking**: Photon PUN2 + Photon Voice + Photon Chat
- **Serialization**: Newtonsoft.Json
- **Rendering**: Post-processing stack
- **UI**: TextMeshPro (all UI text)
- **Mobile**: `Kit_MobileInputManager` + `Kit_TouchscreenDefault` (already in base)

## What the Base Kit Provides (reused as-is)
| System | Kit Class | Notes |
|---|---|---|
| Player movement | `Kit_Movement_BootsOnGround` | CharacterController, footsteps, swimming |
| Mouse look | `Kit_BasicMouseLook` | Recoil, lean, perspective toggle |
| Weapon system | `Kit_ModernWeaponManager` + `Kit_ModernWeaponScript` | Full FPS weapon pipeline |
| Vitals / health | `Kit_SimpleVitals` | hitPoints 0–100, heal/damage API |
| Spawn protection | `Kit_SpawnProtectionDefault` | Configurable invulnerability window |
| Input (PC) | `Kit_DefaultInputManager` | WASD, mouse, weapon slots |
| Input (Mobile) | `Kit_MobileInputManager` + `Kit_TouchscreenDefault` | Touch joystick, buttons |
| Bot AI | `Kit_BotManager` + `Kit_PlayerDefaultBotControl` | Nav points, creature AI base |
| Spawn system | `Kit_SpawnSystemDistanceCheck` | Distance-based spawn validation |
| Spectator | `Kit_SpectatorManagerDefault` | Free-roam spectate on death |
| Minimap | `Kit_MinimapDefault` | Player markers, local player tracking |
| Kill feed | `Kit_KillFeedManager` | Repurposed for artifact steal feed |
| Scoreboard | `Kit_ScoreboardMain` | Repurposed for artifact score display |
| HUD | `Kit_PlayerHUD` | Health, ammo, spawn protection timer |
| Chat | `Kit_SimpleChat` | In-match text chat |
| Voice | `Kit_PhotonVoiceChat` | Proximity voice via Photon Voice |
| Object pooling | `Kit_ObjectPoolingDefault` | Bullet marks, particles |
| Post-processing | Unity Post-processing Stack | Per-era profiles |

## Treasure Royale Scripts
All scripts live in `Assets/314 Arts/MarsFPSKit/Scripts/TreasureRoyale/` and use namespace `MarsFPSKit`.

| Script | Extends | Responsibility |
|---|---|---|
| `Kit_PvE_TreasureRoyale.cs` | `Kit_PvE_GameModeBase` | Game mode ScriptableObject — spawn, timer, era config |
| `TreasureRoyalePlugin.cs` | `Kit_Plugin` | Cross-cutting hooks — artifact score, player left, end-game |
| `TreasureRoyaleEventIDs.cs` | — | Photon event byte constants (11–23, avoids kit 0–10) |
| `TreasureRoyaleUtilities.cs` | — | `DropZoneMarker`, `ArrivalEffect`, `CrossEraPowerCalculator`, `SurvivalLootTable` |
| `ExtractionBeacon.cs` | `MonoBehaviourPun` | Beacon state machine — uses `allActivePlayers` for contested check |
| `ExtractionChannel.cs` | `Kit_InteractableObject` | F-key hold-to-extract — kit weapon manager handles prompt |
| `TransporterDropZone.cs` | `MonoBehaviourPun` | MasterClient drop zone validator, `universeSeed`-seeded shuffle |
| `PlayerSpawnManager.cs` | `MonoBehaviourPun` | Bridges drop zone → `Kit_IngameMain.Spawn()` + arrival sequence |
| `ArtifactResetManager.cs` | `MonoBehaviourPun` + `IInRoomCallbacks` | Disconnect penalty, artifact reset, boss trigger |
| `BossSpawner.cs` | `MonoBehaviour` | Era-keyed boss prefab via `PhotonNetwork.InstantiateSceneObject` |
| `RelicExtractionManager.cs` | `MonoBehaviourPun` | Team roll, solo modifiers, repeat broadcast — uses `allActivePlayers` |
| `RelicGuardianScaler.cs` | `MonoBehaviourPun` | Health/damage scaling from `allActivePlayers.Count` |
| `GearLoadout.cs` | `MonoBehaviour` | 3 gear slots, relic extraction bonus queries |
| `SurvivalInventory.cs` | `MonoBehaviour` | Survival loot inventory — heals via `vitalsManager.ApplyHeal` |
| `SurvivalLootContainer.cs` | `Kit_InteractableObject` | World container — kit handles F-key, 90s respawn |

## Scripts Still To Build
| Script | Extends | Responsibility |
|---|---|---|
| `MultiverseTransporterUI.cs` | `MonoBehaviour` | Timeline picker, era selection, room creation |
| `MapVariantManager.cs` | `MonoBehaviour` | Applies `universeSeed` to swap props/lighting/spawns |
| `ArtifactSpawner.cs` | `MonoBehaviourPun` | Seeds artifact positions and rarity from `universeSeed` |
| `ArtifactPickup.cs` | `Kit_InteractableObject` | Pickup, inventory add, power activation, reset data registration |
| `PlayerInventory.cs` | `MonoBehaviourPun` | Holds artifacts/relics, calculates score, commits extraction |
| `MatchManager.cs` | `MonoBehaviourPun` | Timer display, end-match trigger relay to `TreasureRoyalePlugin` |
| `VaultUI.cs` | `MonoBehaviour` | Post-match trade/upgrade screen |

## Photon Room Properties
```csharp
Hashtable props = new Hashtable {
    { "gameModeType", 1 },            // 1 = Coop PvE (Kit convention)
    { "gameMode",     0 },            // index into allCoopGameModes
    { "era",          selectedEra },  // string
    { "timelineDate", timelineDate }, // int
    { "universeSeed", seed }          // int
};
PhotonNetwork.CurrentRoom.SetCustomProperties(props);
```

## Networking Model
- `gameModeType = 1` (Coop PvE) — no team selection screen, immediate spawn
- MasterClient runs: creature AI, drop zone validation, beacon tick, artifact reset, boss spawns
- All Treasure Royale Photon events use codes 11–23 (`TREventIDs`)
- Kit's `allActivePlayers` list used for proximity checks (contested, relic roll, guardian scale)
- AWS backend handles persistent inventory and leaderboards (see [[aws/aws-architecture]])

## Mobile Support
- `Kit_MobileInputManager` handles touch input — assign on Player prefab
- `Kit_TouchscreenDefault` prefab provides virtual joystick + buttons
- `Kit_IngameMain.enableTouchscreenInput = true` activates it at runtime
- Interaction prompt from `Kit_InteractableObject` displays on mobile tap button
