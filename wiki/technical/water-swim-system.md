# Water & Swim System — Technical Reference

## Overview

The swim system uses a dual-path entry approach: `OnTriggerEnter` as the primary path and a per-frame poll as a backup. Exit uses `OnTriggerExitRelay` as primary and a per-frame poll as backup.

---

## Trigger Geometry (EraMapGenerator.cs)

### Lake & Pond Triggers (`ERA_WaterTrigger`)
- GO position: `(lx, -1.8f, lz)` — center at y=-1.8
- `BoxCollider.size = new Vector3(lw, 4f, ld)` — set directly on the collider, NOT via `localScale`
- Trigger top: `y = -1.8 + 2.0 = 0.2` — slightly above ERA_Ground surface
- Has kinematic `Rigidbody` — required for reliable `OnTriggerEnter` with CharacterController
- Layer: 0 (Default)

### River Triggers (`ERA_WaterTrigger`)
- Same geometry as lakes: center `y=-1.8`, height `4`, top `y=0.2`
- **Segment length capped to `PLAY*2` (1600 units)** — critical, do NOT use `(MAP - bridgeGap) * 0.5` (~3997)
- If segment length = ~3997, triggers cover the entire map and catch players at spawn

### Drown Triggers (`ERA_WaterDrown`)
- Center: `y=-2.5`, height `2` — top at `y=-1.5`
- These are NOT player-entry triggers — never validate their top position as an error

### Lake/River Floors
- `ERA_LakeFloor` and `ERA_RiverFloor` at `y=-5`
- `BoxCollider.size` set directly

---

## Swim Entry (Kit_Movement_BootsOnGround.cs)

### Spawn Guard
```csharp
swimExitCooldown = float.MaxValue  // set on BootsOnGroundRuntimeData creation
// Unlocks to 0f on first grounded frame
if (data.swimExitCooldown == float.MaxValue) data.swimExitCooldown = 0f;
```

### Primary Entry — OnTriggerEnter
- Fires when CC overlaps `ERA_WaterTrigger`
- Blocked if `Time.time < bogrd.swimExitCooldown`
- On entry:
  - `state = 3`, `swimEnteredAt = Time.time`
  - `swimmingWorldMoveDirection = moveDirection`
  - **`swimmingWorldMoveDirection.y = -1f`** — pushes player below ERA_Ground immediately
  - `Physics.IgnoreCollision(pb.cc, ERA_Ground_MeshCollider, true)`

### Backup Entry — Per-Frame Poll
```csharp
// Runs every frame in CalculateMovementUpdate
if (!data.isSwimming && data.swimExitCooldown != float.MaxValue 
    && Time.time >= data.swimExitCooldown && pb.transform.position.y < -0.1f)
{
    // check bounds.Contains(pb.transform.position) for each ERA_WaterTrigger
}
```
- Only fires when player is below `y=-0.1` (actually in water, not on land)

---

## Swim State (Kit_Movement_BootsOnGround.cs)

- CC height: `swimmingHeight = 0.6f`, center: `swimmingHeight / 2f`
- ERA_Ground collision ignored every frame: `Physics.IgnoreCollision(pb.cc, mcF, true)`
- Depth clamp: `y >= -4f`
- Movement: camera-direction based (full 3D)
- Boulder/tree exit: `cc.isGrounded && transform.y > 0.5f` → exit swim

---

## Swim Exit (Kit_Movement_BootsOnGround.cs)

### Primary Exit — OnTriggerExitRelay
- Checks `stillInWater` via `bounds.Contains(pb.transform.position)` before exiting
- On confirmed exit:
  - `state = 0`, `wasSwimming = false`
  - CC height restored to `standHeight`
  - `Physics.IgnoreCollision(pb.cc, ERA_Ground, false)`
  - Snap to `y=1.5`
  - `swimExitCooldown = Time.time + 2.0f`

### Backup Exit — Per-Frame Poll
```csharp
if (data.isSwimming && pb.transform.position.y >= 0f && Time.time > data.swimEnteredAt + 0.5f)
{
    // exit swim — same cleanup as OnTriggerExitRelay
}
```
- 0.5s minimum swim time prevents immediate exit on entry

---

## Critical Values

| Value | Setting |
|-------|---------|
| Trigger center Y | -1.8f |
| Trigger height | 4f (top at y=0.2) |
| River segment max length | PLAY*2 = 1600f |
| Swim depth clamp | -4f |
| Swim exit snap Y | 1.5f |
| Swim exit cooldown | 2.0f |
| Entry push Y | -1f (swimmingWorldMoveDirection.y) |
| Spawn cooldown | float.MaxValue until first grounded frame |

---

## Known Quirks

- **Trigger top at y=0.2 vs ERA_Ground at y=-0.05**: The CC bottom when standing on ERA_Ground is at `y≈0`, which is below the trigger top at `y=0.2`. This means `OnTriggerEnter` fires at the water edge even while the player is technically still on land. The `swimmingWorldMoveDirection.y = -1f` push ensures the player sinks through ERA_Ground immediately.
- **`OnTriggerEnter` unreliable for CC vs kinematic triggers**: Unity doesn't always fire `OnTriggerEnter` when a CC enters a kinematic Rigidbody trigger. The per-frame poll is the fallback.
- **River trigger XZ footprint covers land**: The river trigger is wide (30-60 units) and long (1600 units). Players on land near the river are inside the trigger's XZ bounds. The `y < -0.1` poll guard and the `swimmingWorldMoveDirection.y = -1f` entry push prevent false activation.

---

## Files Modified

| File | Changes |
|------|---------|
| `EraMapGenerator.cs` | Trigger center y=-1.8, BoxCollider.size direct, river length capped to PLAY*2 |
| `Kit_Movement_BootsOnGround.cs` | Dual-path entry (OnTriggerEnter + poll), poll exit, entry push y=-1, debug logging |
