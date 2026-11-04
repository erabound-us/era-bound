# Treasure Royale — Project Context

## What This Project Is
Treasure Royale is a Unity multiplayer FPS built on the "Modular Multiplayer FPS" kit using Photon PUN2.
Players traverse randomized historical Earth timelines (eras) to loot ancient artifacts.
Win condition is artifact score — not kills.

## Active Scene
- Single game scene with procedurally generated map via `EraMapGenerator.cs`
- Map generates on `Start()` before players spawn
- Players wait on loading screen until `EraMapGenerator.mapGenerationComplete == true`

## Key Scripts and Their Roles
| Script | Role | Touch With Caution |
|--------|------|--------------------|
| `EraMapGenerator.cs` | Generates entire map, water, terrain | YES — many interdependencies |
| `Kit_Movement_BootsOnGround.cs` | All player movement including swim | YES — swim system is fragile |
| `Kit_PvE_TreasureRoyale.cs` | Game mode, spawn flow, loading screen | YES — spawn timing critical |
| `TR_MapValidator.cs` | Validates map before spawn, AutoFix | Safe to modify |
| `TR_MapDiagnostic.cs` | Runtime diagnostics, read-only | Safe |
| `Kit_PlayerBehaviour.cs` | Core player — DO NOT MODIFY | NEVER |
| `Kit_IngameMain.cs` | Core game loop — DO NOT MODIFY | NEVER |

## Current Working State Summary
See `treasure-royale-known-working.md` for full details. Key values:
- PLAY radius: 800f (Fortnite scale)
- Swim triggers: center y=-2, height 4 (top at y=0)
- World floor: y=-6
- Spawn positions: (±80, 1.5, ±80) — 80 units from center
- Swim depth clamp: y=-4f
- ERA_Ground: always enabled, Physics.IgnoreCollision per-player during swim

## Known Fragile Systems (Handle With Extreme Care)
1. **Swim entry/exit** — Physics.IgnoreCollision on ERA_Ground MeshCollider
2. **Spawn timing** — mapGenerationComplete flag, swimExitCooldown = float.MaxValue
3. **Swim camera** — NO separate camera block during swim (causes conjoined twin)
4. **Trigger heights** — swim trigger top must be at y=0, drown triggers below ground is CORRECT
5. **Spawn positions** — must be >80 units from center, y=1.5, outside all swim triggers

## What Has Been Broken Before (Do Not Repeat)
- Removing Physics.IgnoreCollision → player walks on water, no swimming
- Adding swim camera block → conjoined twin visual bug
- Swim trigger top > y=0 → player swims on land
- Swim trigger top < y=0 → no swimming at all
- Spawns at origin → player spawns inside river trigger
- ERA_Ground enable/disable globally → other players affected, race conditions
- Shore walls at water edges → player gets trapped on exit
- Y teleport on swim entry → player spawns underground
- River-local ground check without cooldown → player sinks everywhere near river
