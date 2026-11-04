# Treasure Royale — Amazon Q Steering Rules

## Project Identity
This is **Treasure Royale**, a Unity-based multiplayer FPS game where players traverse the multiverse to loot ancient artifacts across randomized historical Earth timelines. The base is a pre-existing "Modular Multiplayer FPS" Unity build using Photon PUN2 networking.

## Code Style Rules
- All Unity C# scripts must be minimal — no boilerplate comments, no region blocks
- Use `TextMeshPro` for all UI text, never legacy `UnityEngine.UI.Text`
- Use `Newtonsoft.Json` (already in project) for all JSON serialization
- Photon networking: always check `PhotonNetwork.IsMasterClient` before authoritative logic
- AWS Lambda: Python 3.12, boto3, keep functions under 50 lines unless complexity demands more
- CDK: Python, CDK v2, no L1 constructs unless L2/L3 unavailable

## Architecture Rules
- Photon PUN2 handles real-time in-match sync (player movement, artifact pickup RPCs)
- AWS handles persistence (player profiles, artifact inventory, leaderboards, asset delivery)
- Never mix concerns: don't store match state in DynamoDB during a live match
- All AWS API calls from Unity go through API Gateway — never call AWS SDKs directly from the client
- Cognito JWT auth is required on any route that writes player data

## Game Design Rules
- The win condition is **artifact score**, not kill count — never suggest kill-based scoring
- Maps are era-based with variant swapping via `universeSeed` — never suggest fully procedural generation
- Artifacts must have: id, era, rarity, powerType, tradeValue — never omit these fields
- The multiverse narrative justifies all variation — use it to explain randomness in suggestions
- Planets beyond Earth (Mars etc.) are future scope — do not include in current implementation
- Matchmaking uses **historical anchor points** — players are bucketed by nearest significant historical event, not exact year
- Bucket ranges scale with era density: prehistoric = ±500k yrs, modern = ±2 yrs
- Room naming pattern: `{bucketId}_{roomIndex}`, player cap varies by era (6–16), new room opens when full
- Player caps: deep_prehistoric 6–8, prehistoric/ancient 10–12, renaissance+ up to 16
- Creature AI runs on MasterClient only — never sync creature position every frame, RPC on state change only
- `universeSeed` is per-room (not per-bucket) — same anchor, different seed = parallel universe variant
- Never suggest exact-year matchmaking — it would fragment the player base
- Full timeline reference: Deep Prehistoric → Prehistoric → Ancient → Medieval → Renaissance → Industrial → Modern → Near Future → Mid Future → Far Future → Deep Future
- Anything beyond 2025 CE is the **Future Period** — speculative, multiverse-justified
- Deep Prehistoric (before 3M BCE) and Deep Future (after 3000 CE) are catch-all single buckets

## Naming Conventions
| Concept | Unity Script Name | AWS Resource Name |
|---|---|---|
| Timeline picker | `MultiverseTransporterUI` | — |
| Map variation | `MapVariantManager` | — |
| Artifact spawning | `ArtifactSpawner` | — |
| Artifact pickup | `ArtifactPickup` | — |
| Player inventory | `PlayerInventory` | `players` DynamoDB table |
| Match results | `MatchManager` | `matches` DynamoDB table |
| Artifact catalog | — | `artifacts` DynamoDB table |
| Seed generation | — | `generate_universe_seed` Lambda |
| Inventory save | — | `update_inventory` Lambda |
| Leaderboard | — | `get_leaderboard` Lambda |

## Do Not Suggest
- Unity UNET (deprecated) — use Photon PUN2 only
- Mirror networking — project is already on Photon
- Full procedural map generation — use prop-swap variant system
- Building mechanics (this is not Fortnite)
- Last-man-standing as primary win condition
- Direct DynamoDB calls from Unity client

## Known Working State Enforcement
- Before changing ANY of these files, read `treasure-royale-known-working.md` first:
  - `EraMapGenerator.cs` — map generation, water bodies, triggers
  - `Kit_Movement_BootsOnGround.cs` — swim state, entry/exit, depth clamp
  - `Kit_PvE_TreasureRoyale.cs` — spawn flow, loading screen, validation
  - `TR_MapValidator.cs` — validation rules, AutoFix positions
  - `Kit_ThirdPersonModernPlayerModel.cs` — ragdoll force, joint freeing, LockJointsOnLand
- Never change a working system to fix a different system — isolate changes
- If a fix breaks something that was working, revert the fix and find a different approach
- Document every confirmed working state in `treasure-royale-known-working.md` before moving on
- The regression test checklist in `treasure-royale-known-working.md` must pass before any map/movement PR

## Pre-Compile Validation Rules (MUST CHECK BEFORE EVERY CODE CHANGE)
These must be verified before suggesting or applying any code change:

### Compile Safety
- No duplicate local variable names in the same scope
- No use of a variable before it is declared
- All `IEnumerator` coroutines have at least one `yield return`
- No missing closing braces — count `{` vs `}` in any method touched

### Swim System Invariants (Kit_Movement_BootsOnGround.cs)
- `swimExitCooldown` field initialized to `float.MaxValue` in `BootsOnGroundRuntimeData` — NEVER remove
- `Physics.IgnoreCollision(pb.cc, ERA_Ground_MeshCollider, true)` called on swim ENTRY
- `Physics.IgnoreCollision(pb.cc, ERA_Ground_MeshCollider, false)` called on swim EXIT
- `Physics.IgnoreCollision` kept true EVERY FRAME during swim state
- Depth clamp `y >= -4f` enforced during swim
- Snap to `y=0.1` on swim exit
- **NO** separate `else if (data.state == 3)` camera lerp block — causes conjoined twin
- Swim exit cooldown: `Time.time + 2.0f`

### Map Generation Invariants (EraMapGenerator.cs)
- Swim triggers: center `y=-2`, height `4` (top at `y=0`)
- Drown triggers: center `y=-2.5`, height `2` — NOT player-entry, never validate as errors
- World floor: `y=-6`, size `MAP*2 x 0.5 x MAP*2`
- `mapGenerationComplete = false` at START of `Start()`, `true` after `Apply()` completes
- Lakes minimum 80 units from origin
- River stored in `riverWidth`, `riverAngle`, `hasRiver` static fields

### Spawn Invariants (Kit_PvE_TreasureRoyale.cs)
- Default spawns at `(±80, 1.5, ±80)` — never at origin
- `SnapToGround` always sets `y=1.5`
- `SnapToGround` must NOT mutate the spawn transform — returns a temporary GO, destroyed after 5s
- `SnapToGround` uses fixed safe positions `(±80, 1.5, ±80)` — never computed from trigger bounds
- `SpawnWhenReady` waits for `mapGenerationComplete` before calling `main.Spawn()`
- `GameModeStageChanged` only calls `Start()` again when `currentPvPGameModeBehaviour != null` — NEVER for PvE

### Player Model Invariants (Kit_PlayerBehaviour.cs)
- After `Instantiate(prefab, transform, false)` in case 0 and case 1, ALWAYS destroy pre-existing `Kit_ThirdPersonPlayerModel` children that are not the new GO:
  ```csharp
  foreach (var existing in GetComponentsInChildren<Kit_ThirdPersonPlayerModel>(true))
      if (existing.gameObject != go) Destroy(existing.gameObject);
  ```
- This is required because the player prefab has a model baked in as a child in the editor. Without this, two models render at the same position (the "conjoined twin" bug).
- `TR_FPArmHider` is added in `Kit_PlayerBehaviour.Start()` after `SetupManager` — NEVER remove or move earlier
- `TR_FPArmHider.Awake()` hides all renderers on `Kit_FirstPersonArms` and `Kit_FirstPersonRendererBase` children

### Grass Tiling Invariant (EraMapGenerator.cs)
- `ERA_Ground` grass texture tiling: fixed `(160f, 160f)` — never compute from PLAY radius

### Primitive Collider Rule (EraMapGenerator.cs)
- `Prim()` always calls `DestroyImmediate(go.GetComponent<Collider>())` — ALL primitives created by `Prim()` are decorative only
- Never remove this collider destruction from `Prim()`
- Any gameplay colliders (rocks, trees) must be added explicitly AFTER calling `Prim()` or `Instantiate()`

### Rock/Tree Prefab Material Rule (EraMapGenerator.cs) — CONFIRMED FIX
- **NEVER call `MakeUnlit()` on rock or tree prefabs** — causes black objects
- **NEVER call `material.mainTexture`** on prefab materials — `Shader Graphs/` shaders throw errors. Always use `HasProperty` first.
- **NEVER convert `Shader Graphs/` materials** — skip them entirely (`sn.StartsWith("Shader Graphs/")`) continue). They render correctly with their own shader. Converting them to `Unlit/Color` using `_BaseColor` produces solid grey/dark objects (bushes, grass).
- For URP/Lit materials only: convert to `Unlit/Texture` when texture found via `HasProperty` on `_BaseMap`, `_MainTex`, `_Albedo`, `_Diffuse`
- Only add collider logic after instantiation: make MeshColliders convex, add fallback SphereCollider (rocks) or CapsuleCollider (trees) if no collider exists
- `MakeUnlit()` must NEVER be called on any nature prefab (trees, rocks, mountains, caves)
- **NEVER include flat ground-cover prefabs in the tree list** — `grass1`, `grass2`, `egrass1`, `egrass2` from lowpoly-nature and ALL TerrainSampleAssets prefabs (Bush_A/B, Fern_A/B, Heather_A, Plant_A/B/C) use `Shader Graphs/TerrainGrass` and render as black flat quads with CapsuleCollider circles when placed as trees

### Water Material Rule (EraMapGenerator.cs) — CONFIRMED
- Always use `t.water` era color for `Unlit/Color` water planes — solid color, works reliably
- **SUIMONO is permanently disabled** — `SUIMONO_Module.InitLayers()` causes player fall-through; root cause unresolved
- **WaterWorks SSR_Water is permanently disabled** — requires URP Depth/Opaque Texture which darkens all `Unlit/` materials in this scene
- Never use `assets.waterMaterial` or `assets.waterPrefab` for water visuals
- Lake water trigger size must match visual plane: `tbc.size = new Vector3(lw, 4f, ld)` — NOT `lw*0.5f`

### Ambient Light Rule (EraMapGenerator.cs) — CONFIRMED
- Always boost ambient by +0.45 on each channel: `new Color(Mathf.Min(1f, t.ambient.r+0.45f), ...)`
- Raw `t.ambient` values are too dark for `Shader Graphs/TerrainGrass` materials (MossyBoulder, grass, rocks) to render visibly
- This boost makes all `Shader Graphs/` materials render at proper brightness without material conversion

### Mountain Invariants (EraMapGenerator.cs)
- Only use `Rock*` prefabs (Rocks and Boulders 2) for mountains - they have working MeshColliders that give climbable surfaces
- **NEVER replace the original MeshCollider** - the non-convex MeshCollider is what makes mountains climbable like boulders
- **NEVER add BoxCollider to mountains** - causes invisible walls far from the visual mesh
- **NEVER set convex=true on mountain MeshColliders** - destroys the surface detail needed for climbing
- **NEVER set isStatic=true at runtime** - breaks the scene
- **NEVER use SphereCollider for mountains** - radius calculation wrong at non-uniform scale
- **NEVER use MossyBoulder/RockCluster for mountains** - they have no collider, fallback colliders fail at mountain scale
- Only call `col.isTrigger=false` on existing colliders - that is the only collider change allowed
- Boulder filter: `pfb.name.StartsWith("Rock")` - selects only Rocks and Boulders 2 prefabs
- The confirmed working collider code: `foreach(var col in go.GetComponentsInChildren<Collider>()) col.isTrigger=false;`

### Ragdoll Invariants (Kit_ThirdPersonModernPlayerModel.cs)
- `Destroy(anim)` called BEFORE `yield return new WaitForFixedUpdate()` in `ApplyForce()` — if after, body lands sitting upright
- All `CharacterJoint` limits set to ±177° at activation — do NOT destroy joints (bones stretch) and do NOT just zero springs (rest axis holds body up)
- `enablePreprocessing = false` and `enableCollision = false` on every joint
- `Physics.IgnoreCollision` between every bone pair — prevents head sphere collider pushing off spine
- `ragdollForce` and `ragdollPoint` cached before `transform.parent = null` — kpb unreliable after detach
- `LockJointsOnLand()` has 0.8s minimum wait before checking velocity — without it fires at frame 0 (velocity=0) and locks joints in standing pose
- `LockJointsOnLand()` replaces CharacterJoints with FixedJoints on land — prevents upper body rising after landing
- `ExtraGravity()` delayed 0.4s — prevents double-explosion feel from fighting upward kick
- Head bone is `rigidbodies[10]` — gets extra `Vector3.down * 50f` impulse to overcome joint rest axis
- Force: `Clamp(ragdollForce * 0.4f, 20f, 38f)`, kick = `outward * 0.5f + up * 0.55f`

### HideKitRenderers Invariants (EraMapGenerator.cs)
- `FindObjectsOfType<Renderer>()` — MUST use active-only (NO `true` param) — using `true` disables pooled impact effect renderers that are inactive at startup, breaking bullet impacts when activated at runtime
- `keepRoots` set MUST include: `"Pool"`, `"_Projectiles"`, `"_BulletMarks"`, `"_ImpactEffects"` — these contain runtime-spawned particle effects
- PSR (ParticleSystemRenderer) loop: **only disable PSRs on inactive root GOs** — `if (root2.activeSelf) continue;` guard is REQUIRED
- **NEVER disable PSRs on active root GOs** — breaks bullet impact effects, muzzle flash, and any runtime-spawned particle systems
- `"Pool"` must NOT be in `DisableKitTriggers` list — pool must stay active for bullet impacts
- `_Surface > 0f` skip in `MakeUnlit()` is REMOVED — was leaving URP transparent materials unconverted (black)

### Cave System Invariants (EraMapGenerator.cs)
- `BuildDetailCave()` call is REMOVED from `BuildNature()` — caves render as black/brown debris at map scale
- `BuildDetailCave()` method body preserved for future redesign
- **NEVER use `new Color(.03f, .03f, .03f)` for any visible surface** — renders as black object
- To re-enable caves: must be 50+ units wide OR use proper cave entrance prefabs

- Drown triggers (`name.Contains("Drown")`) excluded from ALL error checks
- Ocean triggers (`ERA_OceanTrigger`, `ERA_OceanDrown`) excluded from ALL error checks
- AutoFix uses fixed grid positions — NEVER computes positions from trigger bounds
- World floor at `y=-6` is VALID — only error if `y > -4`

## Critical Values — Never Change Without Approval
- `PLAY = 800f` — map radius
- Swim trigger: center `y=-2`, height `4` (top at `y=0`)
- World floor: `y=-6`
- Spawn height: `y=1.5`
- Spawn min distance from center: `80f`
- Swim depth clamp: `-4f`
- Swim exit snap: `y=1.5`
- Swim exit cooldown: `2.0f`
- Spawn swim cooldown: `float.MaxValue` until first grounded frame
- Ragdoll force floor: `20f`
- Ragdoll force ceiling: `38f`
- Ragdoll upward multiplier: `0.55f`
- Ragdoll outward multiplier: `0.5f`
- Ragdoll head impulse: `Vector3.down * 50f`
- Ragdoll ExtraGravity delay: `0.4f` seconds
- Ragdoll ExtraGravity force: `15f` acceleration
- Ragdoll LockJointsOnLand min wait: `0.8f` seconds
- Ragdoll LockJointsOnLand velocity threshold: `1.5f`
