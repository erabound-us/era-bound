# Drop Zone System — Timeline Arrival

## Design Intent

When a player transports into a timeline they should feel like they've just materialized in an unknown world. They need a moment to orient — look around, get their bearings, hear the environment — before the danger finds them. Dropping next to an artifact, a creature, or another player immediately kills that feeling and creates frustrating spawn kills.

### Three Hard Rules
1. **No players within 30m** of the drop point at spawn time
2. **No creatures within 40m** of the drop point at spawn time (creatures move, so larger buffer)
3. **No artifacts within 25m** of the drop point (prevents instant loot, forces movement)

---

## Drop Zone Architecture

### Pre-placed vs Runtime-validated

Drop zones are **pre-placed in the Unity scene** as empty GameObjects tagged `DropZone`, spread across the map away from artifact clusters and creature patrol paths. At runtime, the system validates each candidate against current world state before assigning it.

This is better than pure runtime generation because:
- Designers control the feel of each arrival spot (cover nearby, interesting sightline, era-appropriate)
- No NavMesh sampling needed at runtime
- Validation is a fast overlap check, not a pathfinding query

```
Map design time:
  Level designer places 20–30 DropZone markers per map
  Each marker is positioned: away from artifact spawns, away from creature patrol paths,
  near some cover (rock, tree, ruin) so the player isn't immediately exposed

Runtime:
  TransporterDropZone.cs validates each marker against live world state
  Picks the best available one for each arriving player
```

---

## Validation Algorithm

```
For each arriving player:
  1. Get all DropZone markers on the map (pre-placed, static list)
  2. Shuffle the list using universeSeed + playerActorNumber (so each player gets different order)
  3. For each candidate in shuffled list:
       a. Sphere overlap check radius 30m → any Player layer objects? → skip
       b. Sphere overlap check radius 40m → any Creature layer objects? → skip
       c. Sphere overlap check radius 25m → any Artifact layer objects? → skip
       d. Is this point already reserved for another player arriving this frame? → skip
       e. PASS → reserve this point, assign to player
  4. If no candidate passes → relax constraints by 20% and retry once
  5. If still no candidate → use furthest available point from all players (fallback)
```

The shuffle using `universeSeed + actorNumber` means players in the same room never get assigned the same drop zone, and the order is deterministic (reproducible for debugging).

---

## The Arrival Sequence

The drop isn't instant — there's a short arrival animation that masks the validation time and adds atmosphere:

```
Player confirms transport in lobby
  → Screen fades to white (multiverse transit effect)
  → Client sends arrival request to MasterClient via RPC
  → MasterClient runs validation, reserves drop zone, sends back position
  → Client receives position
  → Player spawns at position, invisible + invulnerable
  → 2-second "materialization" particle effect plays (era-appropriate)
  → Invulnerability ends, player gains control
  → HUD fades in
```

### Arrival States
| State | Duration | Player Can | Notes |
|---|---|---|---|
| In Transit | 0.5s | Nothing | Screen white, loading drop zone |
| Materializing | 2.0s | Look around (mouse only) | Particle effect, no movement, invulnerable |
| Orienting | 1.5s | Move slowly (50% speed) | Still invulnerable, HUD fading in |
| Active | — | Full control | Invulnerability drops, match begins for this player |

Total protected window: **4 seconds** from spawn before any damage is possible.

---

## Era-Specific Arrival Effects

The materialization effect is themed per era — same mechanic, different visual:

| Era | Arrival Effect | Sound |
|---|---|---|
| Deep Prehistoric | Amber-colored energy dissolve, leaves glowing footprints | Deep resonant hum, jungle sounds rush in |
| Prehistoric | Crackling static electricity, brief silhouette outline | Electric snap, wind |
| Ancient | Golden light beam from above, sand particles swirl | Ethereal tone, distant crowd noise |
| Medieval | Mist coalesces into player form | Bell toll, distant battle |
| Renaissance | Clockwork gear particles assemble | Ticking, then silence |
| Industrial | Steam burst, player steps out of vapor | Steam hiss, factory ambience |
| Modern | Digital glitch effect, pixels resolve | Static, then city noise |

---

## Network Sync — MasterClient Authority

Drop zone assignment is **MasterClient-authoritative** to prevent two players claiming the same spot:

```csharp
// TransporterDropZone.cs — MasterClient only
// Called when a player joins the scene

[PunRPC]
void RPC_RequestDropZone(int actorNumber) {
    if (!PhotonNetwork.IsMasterClient) return;

    Vector3 spawnPos = FindValidDropZone(actorNumber);
    photonView.RPC("RPC_AssignDropZone", 
        PhotonNetwork.CurrentRoom.GetPlayer(actorNumber),
        spawnPos.x, spawnPos.y, spawnPos.z);
}

[PunRPC]
void RPC_AssignDropZone(float x, float y, float z) {
    // Called on the specific arriving client only
    PlayerSpawnManager.Instance.SpawnAt(new Vector3(x, y, z));
}
```

Reserved zones are tracked in a `HashSet<int>` (zone index) on MasterClient, cleared when a player moves 15m away from their drop point (they've oriented and moved on).

---

## Drop Zone Placement Guidelines (for map designers)

When placing `DropZone` markers in the Unity scene editor:

### Do
- Place near natural cover — a boulder, a tree, a ruined wall, a cave entrance
- Spread evenly across the map — at least one per major zone
- Place at ground level with clear NavMesh beneath
- Aim for 20–30 markers per map (more = more options = better spread)
- Place some in elevated positions for players who want a scouting advantage

### Don't
- Place within 30m of any artifact spawn point (check `ArtifactSpawner` spawn list)
- Place within 40m of any creature patrol path waypoint
- Place in dead-end corridors with no escape route
- Place two markers within 15m of each other (wastes slots)
- Place in water, lava, or any instant-death zone

### Deep Prehistoric specific
- Avoid placing near T-Rex patrol route entirely (use a 60m buffer — T-Rex aggro range is large)
- At least 4 markers should be inside the jungle canopy (low visibility = safer arrival)
- At least 2 markers on the volcanic ridge (high risk, high reward positioning)

---

## Drop Zone Count by Map Size

| Map Size | Recommended Markers | Min Markers | Notes |
|---|---|---|---|
| Small (deep_prehistoric 6-player) | 20 | 15 | Tighter map, fewer options needed |
| Medium (prehistoric 10-player) | 25 | 18 | Standard |
| Large (ancient+ 12-player) | 30 | 22 | More players = more simultaneous arrivals |
| Max (modern 16-player) | 35 | 25 | Urban maps have natural cover everywhere |

---

## Fallback Handling

Edge cases that need graceful handling:

| Scenario | Handling |
|---|---|
| All zones occupied / too close to players | Relax radius by 20%, retry. If still failing, use furthest point from all players. |
| Player joins mid-match (late join) | Same validation — late joiners get same safety guarantee |
| MasterClient disconnects during spawn | New MasterClient takes over, pending spawn requests re-sent automatically via Photon's MasterClient migration |
| Map has fewer than 15 valid zones | Log warning, use all available zones with relaxed constraints |
| Two players request simultaneously | MasterClient processes sequentially, `reservedZones` HashSet prevents double-assignment |

---

## Script List — New Scripts Required

| Script | Responsibility |
|---|---|
| `TransporterDropZone.cs` | MasterClient-side validation, zone reservation, RPC assignment |
| `PlayerSpawnManager.cs` | Client-side spawn execution, arrival animation sequence, invulnerability timer |
| `ArrivalEffect.cs` | Era-appropriate particle effect + audio, triggered on spawn |
| `DropZoneMarker.cs` | Lightweight MonoBehaviour on each pre-placed marker, exposes position + metadata to TransporterDropZone |

Add `TransporterDropZone` and `PlayerSpawnManager` to the key scripts table in [[unity-architecture]].
