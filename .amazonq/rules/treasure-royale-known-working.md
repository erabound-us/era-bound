# Treasure Royale — Known Working State & Enforcement Rules

## CRITICAL: Read Before Making Any Changes

This document captures confirmed working system states. **Never change these without explicit user approval and documenting the new working state here.**

---

## Map Generation — Known Working

### Constants (EraMapGenerator.cs)
```
VILLAGE = 2000f
MAP     = 8000f
PLAY    = 800f   ← Fortnite-scale playable radius
WT      = 0.22f
```

### ERA_Ground
- Single Unity Plane primitive named `ERA_Ground`
- Position: `(0, -0.05, 0)` — slightly below y=0 so water surface at y=0.02 is visible
- Scale: `PLAY*2f/10f` on X and Z
- Has `MeshCollider` — **always enabled, never disabled at runtime**
- `Physics.IgnoreCollision` is used per-player CC during swim, not global enable/disable

### World Floor
- GameObject `ERA_WorldFloor` at `y=-6`
- `BoxCollider` size `MAP*2 x 0.5 x MAP*2` (16000 x 0.5 x 16000)
- Catches all falling players — swim depth clamp at `y=-4` stays above it

### Ocean Boundary
- 4 side panels + 4 corner panels named `ERA_OceanN/S/E/W` and `ERA_OceanCorner`
- `ERA_OceanTrigger` at `y=-3`, height `4` (top at `y=-1`) — only fires outside 750-unit radius
- `ERA_OceanDrown` same position — drown trigger, not player-entry

---

## Water System — Known Working

### Swim Triggers (Lakes & Ponds)
- Center: `y=-1.8`, height `4` → **top at y=0.2**
- `BoxCollider.size` set directly (NOT via `localScale`) — e.g. `tbc.size = new Vector3(lw, 4f, ld)`
- Has kinematic `Rigidbody` (required for reliable `OnTriggerEnter` with CC)
- Named `ERA_WaterTrigger`
- Player at `y=1.5` (spawn height) is ABOVE trigger — cannot accidentally enter
- Player must walk to water edge (`y≈0`) for CC to overlap trigger top

### Drown Triggers
- Center: `y=-2.5`, height `2` → top at `y=-1.5`
- Named `ERA_WaterDrown` — these are NOT player-entry triggers
- **Never check drown triggers for "top below ground" errors in validator**

### River Swim Triggers
- Two segments split at bridge gap
- Center: `y=-1.8`, height `4` → top at `y=0.2`
- River trigger segment length capped to `PLAY*2` (1600 units) — NOT `MAP/2` (3997)
- This is critical: if segment length = MAP/2, triggers cover entire map and catch spawns

### Lake Floors
- `ERA_LakeFloor` at `y=-5` — prevents falling through world
- `ERA_RiverFloor` at `y=-5` — same

### Swim Entry (Kit_Movement_BootsOnGround.cs)
- Primary: `OnTriggerEnter` fires when CC overlaps `ERA_WaterTrigger`
- Backup poll: runs every frame, checks `bounds.Contains(pb.transform.position)` when `y < -0.1f`
- On entry: `swimmingWorldMoveDirection.y = -1f` to push player below ERA_Ground immediately
- ERA_Ground `Physics.IgnoreCollision(pb.cc, mcE, true)` called on entry **BEFORE** position teleport
- `swimExitCooldown = float.MaxValue` on spawn — unlocks to `0f` on first grounded frame

### Water Surface Enforcement (Kit_Movement_BootsOnGround.cs) — CONFIRMED WORKING
Two per-frame checks run in `CalculateMovementUpdate` every frame:

**Prevent walking on water:** When NOT swimming, cooldown clear, and `playerY < 0.5f`:
- Tests each `ERA_WaterTrigger` BoxCollider using `InverseTransformPoint` local-space XZ check
- If player XZ is inside any trigger footprint → `Physics.IgnoreCollision(cc, ERA_Ground, true)` → player sinks in
- If player XZ is NOT over any water → `Physics.IgnoreCollision(cc, ERA_Ground, false)` → ERA_Ground holds player up
- **CRITICAL: Use `InverseTransformPoint` local-space XZ test, NOT `bounds.Contains`** — bounds.Contains uses world-axis-aligned box which is too large for rotated river triggers and causes false positives far from water edge
- Guard: `swimExitCooldown == float.MaxValue` skips check at spawn; `y < 0.5f` prevents triggering while jumping

**Prevent swimming on land:** When IS swimming:
- Same `InverseTransformPoint` local-space XZ test on each trigger
- If player XZ is no longer over ANY water trigger → force exit swim, snap to `y=1.5`, restore ERA_Ground collision
- This fires immediately when player swims onto a bank

### Swim Exit
- Primary: `OnTriggerExitRelay` — checks `stillInWater` via `bounds.Contains` before exiting
- Backup poll: exits when `pb.transform.position.y >= 0f` AND `Time.time > swimEnteredAt + 0.5f`
- Per-frame force exit: fires when player XZ leaves all water trigger footprints (see above)
- On exit: CC height restored to `standHeight`, ERA_Ground collision restored, snap to `y=1.5`
- `swimExitCooldown = Time.time + 2.0f` — prevents immediate re-entry

### Swim Depth Clamp
```csharp
float swimTargetY = -4f  // max depth
// No ceiling clamp — player surfaces naturally
```

### Known Quirk (as of current working state)
- Player must have slight downward momentum to enter swim (handled by `swimmingWorldMoveDirection.y = -1f` on entry)
- Walking on water surface without entering: ERA_Ground holds player up — correct behavior
- Trigger top at `y=0.2` means CC overlaps at water edge even when standing on ERA_Ground (`y=-0.05`)

---

## Player Model — Known Working

### Twin Model Bug — Root Cause & Fix
The player prefab (`Kit_PlayerBehaviour`) has a `Player_ThirdPerson_Outfit4_Hairstyle1` child **baked into the prefab in the editor**. When `Kit_PlayerBehaviour.Start()` runs, it calls `Instantiate(pmcSp.information.prefab, transform, false)` which creates a second `(Clone)` copy. Both exist simultaneously — the original (no `(Clone)`) is fully visible, the clone is the one we configure. This causes the "conjoined twin" visual.

**Fix (Kit_PlayerBehaviour.cs, case 0 and case 1 in Start()):**
```csharp
go = Instantiate(pmcSp.information.prefab, transform, false);
// Destroy any pre-existing model instances baked into the prefab
foreach (var existing in GetComponentsInChildren<Kit_ThirdPersonPlayerModel>(true))
    if (existing.gameObject != go) Destroy(existing.gameObject);
```

**Rule: Never remove this Destroy loop.** The player prefab will always have a baked-in model child. Without this, two models render at the same position.

### Spawn — GetBestSpawn / SnapToGround
- `SnapToGround` must NOT mutate the spawn `Transform.position` — it returns a temporary GO
- Uses fixed safe positions `(±80, 1.5, ±80)` — never computed from trigger bounds
- `GameModeStageChanged` only calls `Start()` again for PvP (`currentPvPGameModeBehaviour != null`) — never for PvE

### Grass Tiling
- `ERA_Ground` grass texture tiling: `(160f, 160f)` — fixed value, not computed from PLAY
- Do not use `PLAY / 60f * 12f` — produces wrong tiling at PLAY=800

---


When no `Kit_PlayerSpawn` found in scene, 4 defaults created at:
- `( 80, 1.5,  80)`
- `(-80, 1.5,  80)`  
- `( 80, 1.5, -80)`
- `(-80, 1.5, -80)`

These are **80 units from center** — safely outside the river (width 30-60) and any lake (min distance 80 from center enforced in SpawnLake).

### SnapToGround
- Always sets `y=1.5` — above ERA_Ground at `y=-0.05`
- Checks if position is inside any swim trigger and moves outside if so

### Spawn Safety Rule
- Lakes minimum distance from center: `80f` (enforced in `SpawnLake()`)
- Ponds minimum distance: `PLAY*0.2f = 160f`
- River exclusion zone: `80f` radius around origin

---

## Map Validation — Known Working (TR_MapValidator.cs)

### What Passes
- `ERA_WorldFloor` exists at `y <= -4`
- `ERA_Ground` exists
- Swim triggers (non-drown) have `col != null` and `top >= 0`
- No spawn points inside swim triggers
- Ocean panels exist

### What Is NOT an Error
- `ERA_WaterDrown` top below ground — **this is correct by design**
- `ERA_OceanDrown` top below ground — **correct by design**
- World floor at `y=-6` — valid (deeper than swim clamp of `-4`)

### AutoFix Safe Positions
Fixed grid, never computed from trigger bounds:
```
(±50, 1.5, ±50), (±100, 1.5, 0), (0, 1.5, ±100), (±80, 1.5, ±80)
```

---

## Spawn Flow — Known Working

### Sequence
1. `EraMapGenerator.Start()` → sets `mapGenerationComplete = false` → generates → sets `true`
2. `Kit_PvE_TreasureRoyale.GameModeProceed()` → starts `SpawnWhenReady` coroutine
3. Loading screen shown immediately
4. Coroutine waits for `mapGenerationComplete == true`
5. `TR_MapValidator.Validate()` runs
6. If errors: `TR_MapValidator.AutoFix()` runs, re-validates
7. Loading screen fades out
8. `main.Spawn(true)` called
9. Player spawns at `y=1.5` outside water
10. `swimExitCooldown = float.MaxValue` blocks swim until first grounded frame

---

## Swimming — Known Working

### Entry
- Player walks over river → per-frame XZ check detects player over water → `Physics.IgnoreCollision(cc, ERA_Ground, true)` → player sinks → `OnTriggerEnter` fires → swim state
- `swimExitCooldown` must be `0` (unlocked after first grounded frame) AND `playerY < 0.5f`
- XZ check uses `InverseTransformPoint` in trigger's local space — correctly handles rotated river triggers
- **NEVER use `bounds.Contains` for water XZ detection** — AABB of rotated trigger is too large

### During Swim
- `Physics.IgnoreCollision` kept true every frame
- Swim movement uses camera direction (full 3D)
- Depth clamp: `y >= -4f`
- Boulder/tree exit: `isGrounded && y > -4f` → exit swim

### Exit
- `Physics.IgnoreCollision(cc, ERA_Ground, false)` — restore collision
- Snap to `y=1.5` on swim exit — puts CC bottom at y=0.6, above trigger top at y=0
- `swimExitCooldown = Time.time + 2.0f` — 2 second re-entry block

### Camera During Swim
- **No separate swim camera block** — causes conjoined twin visual bug
- `mouseLookObject` uses standard `camPosStand` position
- The third-person model sinks with the transform — camera stays at eye level naturally

---

## Do Not Change Without Documenting

| System | File | Critical Value |
|--------|------|----------------|
| Swim trigger height | EraMapGenerator.cs | center y=-1.8, height 4 (top y=0.2) |
| Water XZ detection | Kit_Movement_BootsOnGround.cs | InverseTransformPoint local-space, NOT bounds.Contains |
| Water entry guard | Kit_Movement_BootsOnGround.cs | swimExitCooldown != MaxValue AND y < 0.5f |
| Water exit (land) | Kit_Movement_BootsOnGround.cs | per-frame XZ check, force exit when not over any trigger |
| Swim trigger sizing | EraMapGenerator.cs | BoxCollider.size set directly, NOT via localScale |
| River trigger length | EraMapGenerator.cs | capped to PLAY*2 (1600), NOT MAP/2 (3997) |
| World floor | EraMapGenerator.cs | y=-6 |
| Spawn height | Kit_PvE_TreasureRoyale.cs | y=1.5 |
| Spawn min distance | EraMapGenerator.cs | 80f from center |
| Swim depth clamp | Kit_Movement_BootsOnGround.cs | -4f |
| Swim exit snap | Kit_Movement_BootsOnGround.cs | y=1.5 |
| Swim exit cooldown | Kit_Movement_BootsOnGround.cs | 2.0f |
| Swim entry push | Kit_Movement_BootsOnGround.cs | swimmingWorldMoveDirection.y = -1f |
| Spawn cooldown | Kit_Movement_BootsOnGround.cs | float.MaxValue until grounded |
| ERA_Ground position | EraMapGenerator.cs | y=-0.05 |
| PLAY radius | EraMapGenerator.cs | 800f |

---

## Cave System — DISABLED (EraMapGenerator.cs)

### Current State
- `BuildDetailCave()` method exists in code but is **NOT called** — the cave loop in `BuildNature()` has been removed
- Reason: procedural cave primitives at map scale (5-9 unit wide) rendered as small brown/dark rock clusters on the ground, indistinguishable from debris
- The cave loop previously placed 20 caves per map using `Prim()` with near-black interior color

### Root Cause of Black Objects
- Cave interior used `new Color(.03f, .03f, .03f)` — essentially black
- The 3 sphere primitives (RL, RR, OH) at that scale looked like 3 brown circles on the ground
- Confirmed: removing the cave loop eliminated all black/brown ground objects

### To Re-enable Caves
- Caves need to be significantly larger (50+ units) to read as landmarks at PLAY=800 scale
- OR use actual cave entrance prefabs with proper materials
- Do NOT re-add the `BuildDetailCave()` call without redesigning the scale and visual
- The `BuildDetailCave()` method body is preserved in code for future reference
- **NEVER use `new Color(.03f, .03f, .03f)` for any visible surface** — renders as black object

---

## Nature System — Known Working (EraMapGenerator.cs)

### Tree Prefabs (TreePaths in EraAssetAutoWirer.cs)
- 12 lowpoly-nature tree prefabs (tree1_1 through tree6_2) — Standard shader, `_MainTex`, converts to `Unlit/Texture`
- `tree_01.prefab` from "Mountain Terrain rocks and tree" pack — Standard shader, `_MainTex`, converts correctly
- **EXCLUDED from TreePaths**: `smalltree1-3`, `bush1-3`, `flower1-3`, `mushroom1-2` — all use `Shader Graphs/TerrainGrass`, render as black flat quads
- Runtime name filter in `BuildNature`: skips prefabs containing `grass`, `egrass`, `bush`, `flower`, `fern`, `heather`, `plant`, `mushroom`, `smalltree`, or named exactly `tree_01` (tree_01 is mountain-only)

### Mountain Trees
- `tree_01` placed exclusively by `BuildDetailMountain()` around boulder bases
- Scale: `20-35` units (proportional to 25-50 scale boulders)
- Placement: `y=1f`, radius `baseScaleForTrees*0.3` to `baseScaleForTrees*0.8`
- 12-19 trees per mountain
- All colliders destroyed (`DestroyImmediate(col)`) — trees are decorative only

### Rock Prefabs
- Rocks and Boulders 2 (Rock1A-6B): custom shader `fileID:4800000`, `_MainTex` assigned, converts to `Unlit/Texture`
- LowPolyMedieval (MossyBoulder, RockCluster, SmallRock): `Shader Graphs/` — render correctly with ambient boost, no conversion needed
- AncientRuins (Rock1, Rock2): custom shader, `_MainTex` assigned, converts to `Unlit/Texture`
- **rock_set_01-04 from Mountain Terrain pack**: EXCLUDED — Standard shader MeshColliders cause walk-through on non-static objects

### Structure Prefabs
- LowPolyMedieval buildings: Standard shader, convert via `MakeUnlit()`
- PolyRonin Desert Oasis ruins: custom shader `4800000`, `_MainTex` assigned, converts correctly
- **Symphonie URP archway**: EXCLUDED — URP Lit shader renders black without proper URP depth texture
- **AncientRuins Rock3/4/Shell as structures**: EXCLUDED — same custom shader issue when used as structures

### Material Conversion Rules
- `MakeUnlit()` converts Standard/custom shaders to `Unlit/Texture` (if `_MainTex`/`_BaseMap` found) or `Unlit/Color` (fallback)
- `_Surface > 0f` skip REMOVED — was leaving URP transparent materials unconverted (black)
- `Shader Graphs/` materials: NEVER convert — skip with `continue`, they render correctly with ambient boost
- Brightness boost: any `Unlit/Color` with luminance < 0.05 gets boosted to grey `(0.4,0.4,0.4)` or proportionally boosted
- `HideKitRenderers` uses `FindObjectsOfType<Renderer>()` (active only, NOT `true`) — inactive pooled objects (bullet impacts, muzzle flash) must NOT be touched at startup or their renderers will be disabled when activated at runtime
- PSR loop only disables PSRs on **inactive** root GOs — active root GOs are skipped to preserve runtime-spawned effects (bullet impacts, muzzle flash)
- `"Pool"` is in `keepRoots` AND `Pool` is NOT in `DisableKitTriggers` — pool must stay active for bullet impact particle systems
- **NEVER disable PSRs on active root GOs** — breaks bullet impact effects spawned at runtime

---

## Ragdoll System — Known Working (Kit_ThirdPersonModernPlayerModel.cs)

### Overview
Ragdoll is triggered by `CreateRagdoll()` → `ApplyForce()` coroutine → `LockJointsOnLand()` coroutine.

### CharacterController Shutdown
- `kpb.cc.enabled = false` immediately in `CreateRagdoll()` — prevents CC pushing bones through ERA_Ground
- `Physics.IgnoreCollision(cc, ERA_Ground_col, false)` called first to restore before disabling

### Rigidbody Activation
- All `rigidbodies[]` set: `isKinematic=false`, `useGravity=true`, `velocity=zero`, `angularVelocity=zero`
- `CollisionDetectionMode.ContinuousSpeculative` on all bones
- All bone colliders: `isTrigger=false`, layer set to `11`

### Joint Freeing (CRITICAL — do not revert)
- All `CharacterJoint` limits set to ±177° at ragdoll activation so gravity can flatten the body
- `enablePreprocessing = false` and `enableCollision = false` on every joint
- Springs zeroed: `twistLimitSpring` and `swingLimitSpring` both `{ spring=0, damper=0 }`
- **Do NOT destroy CharacterJoints at activation** — bones stretch/fly apart
- **Do NOT just zero springs without setting limits** — joint rest axis still holds body upright

### Inter-Bone Collision
- `Physics.IgnoreCollision` called between every bone pair — prevents bones pushing each other (especially head sphere pushing off spine)
- Ragdoll layer (11) collision with all other layers restored via `Physics.IgnoreLayerCollision(11, L, false)`

### Animator Destruction
- `Destroy(anim)` called **before** `yield return new WaitForFixedUpdate()` in `ApplyForce()`
- **CRITICAL: destroy animator before physics steps** — if destroyed after, bones are held in standing pose for one frame and body lands sitting upright

### Force Values (confirmed good feel)
```
force = Clamp(ragdollForce * 0.4f, 20f, 38f)
kick  = outward * force * 0.5f + Vector3.up * force * 0.55f
```
- `ragdollForce` and `ragdollPoint` cached into locals **before** `transform.parent = null` — kpb becomes unreliable after detach
- Head bone (`rigidbodies[10]`) gets extra `AddForce(Vector3.down * 50f, ForceMode.Impulse)` to overcome joint rest axis

### Extra Gravity
- Starts after `0.4f` second delay (waits for body to peak before pulling down)
- Runs for 3 seconds, `Vector3.down * 15f` acceleration per fixed update
- Delay prevents "double explosion" feel from gravity fighting the upward kick

### LockJointsOnLand (CRITICAL — prevents upper body rising after landing)
- Waits minimum `0.8f` seconds before checking — **without this it fires at frame 0 when velocity=0 and locks joints in standing pose**
- Then polls every fixed update until `rigidbodies[0].velocity.magnitude < 1.5f`
- On land: destroys all `CharacterJoint` components, replaces each with `FixedJoint` to same connected body
- `FixedJoint.breakForce = Infinity`, `breakTorque = Infinity` — bones locked flat permanently
- 4 second timeout prevents infinite loop if body gets stuck

### Ragdoll Bone Array (rigidbodies[] index → ragdollId)
| Index | ragdollId | Bone |
|-------|-----------|------|
| 0 | 0 | Hips (root) |
| 1 | 1 | — |
| 2 | 2 | — |
| 3 | 3 | — |
| 4 | 4 | — |
| 5 | 5 | Spine/Neck |
| 6 | 6 | — |
| 7 | 7 | — |
| 8 | 8 | — |
| 9 | 9 | — |
| 10 | 10 | Head |

### Prefab Joint Values (Player_ThirdPerson_Outfit4_Hairstyle1.prefab)
- Head joint (`&5039441874197203029`): Swing1=90, Swing2=60, LowTwist=-40, HighTwist=40
- Spine joint (`&203005476031395739`): Swing1=90, Swing2=60, LowTwist=-90, HighTwist=90
- These are overridden at runtime anyway by the ±177° freeing — prefab values only matter if runtime freeing is removed

### Layer Setup
- Ragdoll bones use layer 11
- ERA_Ground MeshCollider must be on layer 0 (Default) for ragdoll to land on it
- `Physics.IgnoreLayerCollision(11, 0, false)` called in `CreateRagdoll()` to ensure this

### Do Not Change Without Documenting
| Value | Location | Current |
|-------|----------|---------|
| Force floor | ApplyForce() | 20f |
| Force ceiling | ApplyForce() | 38f |
| Upward multiplier | ApplyForce() | 0.55f |
| Outward multiplier | ApplyForce() | 0.5f |
| Head impulse | ApplyForce() | Vector3.down * 50f |
| ExtraGravity delay | ExtraGravity() | 0.4f seconds |
| ExtraGravity force | ExtraGravity() | 15f acceleration |
| ExtraGravity duration | ExtraGravity() | 3f seconds |
| LockJointsOnLand min wait | LockJointsOnLand() | 0.8f seconds |
| LockJointsOnLand velocity threshold | LockJointsOnLand() | 1.5f |
| LockJointsOnLand timeout | LockJointsOnLand() | 4f seconds |

---

## Regression Test Checklist

Before shipping any map/movement change, verify:
- [ ] Player spawns on land at y≈1.5, not underground
- [ ] Player can walk, run, jump normally on land
- [ ] Player cannot walk on water surface (sinks immediately on contact)
- [ ] Player cannot swim on land (exits swim immediately when XZ leaves water trigger footprint)
- [ ] Player enters swim state when stepping into river/lake
- [ ] Player can swim in 3D (look up/down to change depth)
- [ ] Player cannot swim below y=-4
- [ ] Player can surface and exit water onto land
- [ ] After exiting water, player walks normally (not stuck swimming)
- [ ] Bridge is above water, player can walk across it
- [ ] Player cannot swim under the bridge from water
- [ ] Falling off map edge enters ocean swim state
- [ ] No conjoined twin visual during swim
- [ ] No conjoined twin visual at spawn (only one player model visible)
- [ ] Grass texture tiles correctly (not stretched)
- [ ] Loading screen shows before spawn
- [ ] Map validation passes (or AutoFix resolves issues) before spawn
- [ ] On death: ragdoll launches with outward + upward force
- [ ] Ragdoll body falls flat — does not land sitting upright
- [ ] Ragdoll upper body stays flat after landing — does not rise back up
- [ ] Ragdoll head falls flat — does not stay propped up
- [ ] No bone stretching or flying apart on death
- [ ] Bullet impact particle effects visible when shooting surfaces
- [ ] No black/brown debris objects on the ground (caves disabled)
