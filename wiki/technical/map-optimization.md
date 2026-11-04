# Map & Performance Optimization

## The Problem
Treasure Royale runs up to 16 players across large era maps, each with:
- Multiple character models active simultaneously
- Creature AI running on MasterClient
- Survival loot containers respawning every 90s
- Artifact pickups, boss spawns, beacon effects
- Era-variant props, terrain layers, post-processing profiles

Without optimization this would be unplayable on mobile and sluggish on desktop.

## Systems Overview

### 1. `MapVariantManager` — Prop Swap Variants
**Attaches to**: persistent scene manager object  
**What it does**: Reads `universeSeed` from Photon room properties at scene load. Uses a seeded `System.Random` to deterministically select one variant per prop group, one lighting profile, and one terrain layer set. All clients run the same seed → same result without any RPC.

**How to set up in editor**:
- Create `PropSwapGroup` entries — each group has N child GameObjects (one per variant)
- Create `LightingVariant` entries — each has a PostProcess profile + ambient/fog settings
- Create `TerrainLayerSet` entries — each is a full `TerrainLayer[]` array
- Assign `PostProcessVolume` (global) and `Terrain[]` references

**Result**: 5 prop groups × 3 variants × 3 lighting × 4 terrain = 180 combinations per era map. Across 3 era maps = 540+ distinct visual states.

---

### 2. `TR_ObjectPoolingExtended` — Extended Object Pool
**Attaches to**: `MarsFPSKit_IngamePrefab` (replaces `Kit_ObjectPoolingDefault`)  
**What it does**: Extends the kit's existing pool with:
- `WarmUp()` coroutine — preloads artifact pickups, survival containers, boss prefabs before match start, spread across frames to avoid a load spike
- `GetPooled<T>()` — typed wrapper so Treasure Royale scripts don't cast manually
- `ReturnToPool()` — returns objects instead of destroying them

**Usage**: Call `StartCoroutine(objectPooling.WarmUp())` from `Kit_PvE_TreasureRoyale.GamemodeSetup`.

---

### 3. `TR_LODGroupManager` — Global LOD Bias Control
**Attaches to**: persistent scene manager object  
**What it does**: Sets `QualitySettings.lodBias` from a 4-level preset (Low/Medium/High/Ultra). One call covers every LODGroup in every era map — no per-object setup needed.

**Integration**: Call `SetQualityLevel()` from the options menu or from `TR_PlatformQualityBootstrap`.

---

### 4. `TR_CharacterLOD` — Per-Character Distance LOD
**Attaches to**: Player prefab  
**What it does**: Runs only on remote players (`isController = false`). Every 0.2s checks distance from the local camera and swaps between renderer sets (full-quality / medium / low-poly silhouette). Also controls shadow casting mode per level.

**Why not Unity's built-in LODGroup?**: The kit's player model is assembled at runtime from `Kit_ThirdPersonPlayerModel` — static LODGroups can't reference dynamically instantiated renderers. This script handles it at runtime.

**Setup**: Assign 3 LOD levels in the inspector. Level 0 = full quality (0–30m), Level 1 = medium (30–60m), Level 2 = low-poly (60m+).

---

### 5. `TR_CreatureAIThrottle` — Creature AI Distance Culling
**Attaches to**: each creature prefab alongside `NavMeshAgent`  
**What it does**: MasterClient-only. Every 1s checks if any active player is within `activationRadius` (default 60m). If not, disables the `NavMeshAgent` and stops AI ticks. Re-enables when a player approaches.

**Why this matters**: Deep Prehistoric can have 20+ creatures active. At 60m cull radius, only creatures near players run AI — the rest are frozen. This is the single biggest CPU saving for creature-heavy eras.

**Rule**: Never sync creature position every frame — only RPC on state change (already in steering rules). This script enforces that by stopping the agent entirely when no player is nearby.

---

### 6. `TR_InterestManager` — Photon Interest Groups
**Attaches to**: persistent scene manager object  
**What it does**: Divides the map into named zones, each mapped to a Photon interest group byte (1–255). The local player subscribes only to zones they're near. Creatures, artifacts, and survival containers in distant zones stop sending updates to that player entirely.

**Group 0** is always subscribed — used for beacon state, match events, and global RPCs.

**Setup**:
1. Define zones in the inspector (center + radius + groupId)
2. Set the `interestGroup` on each creature/artifact/container `PhotonView` to match its zone
3. The manager handles subscribe/unsubscribe automatically every 2s

**Bandwidth impact**: In a 16-player match on a large map, this can cut per-client receive bandwidth by 40–60% when players are spread across zones.

---

### 7. `TR_AudioCuller` — World Audio Source Culling
**Attaches to**: persistent scene manager object  
**What it does**: At startup, registers all `AudioSource` components that are NOT on player prefabs. Every 1s, disables sources beyond `cullDistance` (default 80m) from the local camera. Re-enables them when the player moves close.

**Why**: Unity processes all enabled `AudioSource` components every frame even if they're inaudible. A large era map with ambient emitters, creature sounds, and survival containers can have 50–100 sources. Culling them beyond 80m eliminates that overhead.

---

### 8. `TR_PlatformQualityBootstrap` — Platform Detection
**Attaches to**: BootScene or `MarsFPSKit_IngamePrefab`  
**What it does**: Runs at `DefaultExecutionOrder(-100)` — before anything else. Detects `Application.isMobilePlatform` and applies:

| Setting | Desktop | Mobile |
|---|---|---|
| Shadow distance | 150 | 50 |
| LOD bias | 1.0 | 0.5 |
| Texture mipmap limit | Full | Half |
| Target frame rate | vsync | 60fps |

Works alongside `Kit_GameInformation.enableTouchscreenInput` — that flag activates the touch UI; this script adjusts rendering quality.

---

## How They Work Together

```
Scene Load
  └─ TR_PlatformQualityBootstrap  → sets global quality (runs first)
  └─ MapVariantManager.Start()    → reads universeSeed, swaps props/lighting/terrain
  └─ TR_ObjectPoolingExtended.WarmUp() → preloads prefabs across frames
  └─ TR_InterestManager.Start()   → subscribes to zone 0 (global)
  └─ TR_AudioCuller.Start()       → registers all world AudioSources

Per Frame (local player alive)
  └─ TR_InterestManager.Update()  → every 2s, update zone subscriptions
  └─ TR_CharacterLOD.Update()     → every 0.2s per remote player, swap renderers
  └─ TR_AudioCuller.Update()      → every 1s, cull distant audio sources

MasterClient Only
  └─ TR_CreatureAIThrottle.Update() → every 1s per creature, pause/resume NavMeshAgent
```

## Per-Era Recommended Settings

| Era | Creatures | Players | Recommended LOD | Shadow Distance |
|---|---|---|---|---|
| Deep Prehistoric | High (20+) | 6–8 | Low/Medium | 80 |
| Prehistoric | High | 10–12 | Medium | 100 |
| Ancient | Medium | 10–12 | Medium | 120 |
| Medieval | Medium | 12–14 | Medium | 120 |
| Renaissance+ | Low | 14–16 | High | 150 |
| Modern/Future | None | 16 | High/Ultra | 150 |
