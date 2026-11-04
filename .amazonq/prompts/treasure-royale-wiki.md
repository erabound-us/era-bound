# Treasure Royale — System Wiki

## Spawn Flow (Full Sequence)
```
EraMapGenerator.Start()
  → mapGenerationComplete = false
  → Apply(era, seed)
    → GenerateMap() — builds ground, water, nature, village
    → mapGenerationComplete = true

Kit_IngameMain.Start()
  → GameModeProceed()
    → StartCoroutine(SpawnWhenReady)
      → Show loading screen
      → Wait for mapGenerationComplete == true
      → TR_MapValidator.Validate()
      → If errors: TR_MapValidator.AutoFix() → re-validate
      → Fade out loading screen
      → main.Spawn(true)
        → Kit_PvE_TreasureRoyale.SnapToGround()
          → Sets y=1.5
          → Checks not inside swim trigger
        → Kit_PlayerBehaviour.Start()
          → customMovementData = new BootsOnGroundRuntimeData()
            → swimExitCooldown = float.MaxValue  ← BLOCKS SWIM AT SPAWN
        → CalculateMovementUpdate() first frame
          → isGrounded = true → swimExitCooldown = 0  ← UNLOCKS SWIM
```

## Swim System (Full Flow)
```
ENTRY:
  Player over river → river-local check → Physics.IgnoreCollision(cc, ERA_Ground, true)
  → Player sinks below y=0 → OnTriggerEnter(ERA_WaterTrigger)
  → swimExitCooldown check (must be 0)
  → Raycast down: if ERA_Ground directly below → return (on land, block)
  → state = 3, swimmingCurrent = trigger
  → Physics.IgnoreCollision(cc, ERA_Ground, true) [redundant safety]

DURING:
  Every frame:
    Physics.IgnoreCollision(cc, ERA_Ground, true) [keep ignored]
    Move via swimmingWorldMoveDirection
    Depth clamp: if y < -4f → y = -4f
    Boulder exit: if isGrounded && y > -0.5f → exit swim

EXIT (trigger):
  OnTriggerExit → state = 0
  Physics.IgnoreCollision(cc, ERA_Ground, false) [restore]
  pb.transform.position.y = 0.1f [snap above ground]
  swimExitCooldown = Time.time + 2.0f [block re-entry]

EXIT (boulder/tree):
  isGrounded && y > -0.5f → state = 0
  Physics.IgnoreCollision(cc, ERA_Ground, false)
  swimExitCooldown = Time.time + 2.0f
```

## Map Validation Rules
```
ERRORS (block spawn, attempt AutoFix):
  - ERA_WorldFloor missing or y > -4
  - ERA_Ground missing
  - Swim trigger (non-drown) col == null
  - Swim trigger (non-drown) top < y=0
  - Any spawn point inside a swim trigger

WARNINGS (log only, don't block):
  - No ocean boundary panels
  - Swim trigger top > y=5 (may trigger on land)
  - World floor y < -10

NOT ERRORS (correct by design):
  - ERA_WaterDrown top below ground
  - ERA_OceanDrown top below ground
  - World floor at y=-6
```

## AutoFix Safe Spawn Positions (fixed grid, never from trigger bounds)
```
(50, 1.5, 50), (-50, 1.5, 50), (50, 1.5, -50), (-50, 1.5, -50)
(100, 1.5, 0), (-100, 1.5, 0), (0, 1.5, 100), (0, 1.5, -100)
(80, 1.5, 80), (-80, 1.5, -80)
```

## Water Body Placement Rules
```
SpawnLake():
  - Minimum 80 units from origin
  - Uses Overlaps() check against placed[] list
  - Swim trigger: center y=-2, height 4 (top y=0)
  - Drown trigger: center y=-2.5, height 2 (top y=-1.5)
  - Lake floor: y=-5

River:
  - Width: 30-60 units
  - Stored in EraMapGenerator.riverWidth, riverAngle, hasRiver
  - Two swim trigger segments split at bridge gap
  - River floor: y=-5
  - Bridge at y=0.1 (above water surface at y=0.02)
```

## Physics Layer Rules
```
ERA_Ground MeshCollider:
  - ALWAYS enabled globally
  - Per-player Physics.IgnoreCollision during swim only
  - Never disable globally (affects all players)

ERA_WorldFloor BoxCollider:
  - Always enabled
  - Never modified at runtime

Swim triggers:
  - isTrigger = true
  - Rigidbody kinematic on most (required for OnTriggerEnter with CC)
  - Layer 0 (Default)

Ocean triggers:
  - Layer 2 (Ignore Raycast) — hides Rigidbody gizmo in Game view
```

## Pre-Compile Checklist
Before any code change compiles:
- [ ] No duplicate local variable names in same scope
- [ ] No `Physics.IgnoreCollision` calls that affect ERA_Ground globally
- [ ] No swim camera block (`else if (data.state == 3)` with camera lerp)
- [ ] swimExitCooldown initialized to float.MaxValue in BootsOnGroundRuntimeData constructor
- [ ] Swim trigger heights: center y=-2, height 4 in EraMapGenerator
- [ ] Spawn positions: y=1.5, min 80 units from center
- [ ] TR_MapValidator excludes drown triggers from "top below ground" error
- [ ] TR_MapValidator AutoFix uses fixed grid positions, not trigger bounds
- [ ] mapGenerationComplete set false in Start(), true after Apply() completes
