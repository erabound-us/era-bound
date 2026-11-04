# Server Capacity & Performance Design

## The Core Constraint

Photon PUN2 is a **relay-based** network — there is no dedicated game server running your logic. Every client runs the game locally and Photon's cloud just relays messages between them. This means:

- **Photon's servers never bog down** from your game logic — they only move packets
- **The bottleneck is bandwidth per room** (Photon charges/limits by messages/sec) and **client CPU** (each player's machine simulates everything)
- The MasterClient runs authoritative logic (creature AI, artifact spawns, match timer) — if MasterClient has a bad connection or slow PC, everyone feels it

---

## What Consumes Bandwidth Per Player

Every networked object sends updates at a tick rate. Here's the cost breakdown for Treasure Royale:

| Networked Object | Updates/sec | Bytes/update | Bytes/sec per object |
|---|---|---|---|
| Player transform (pos + rot) | 20/sec | ~28 bytes | ~560 B/s |
| Player animator state | 10/sec | ~12 bytes | ~120 B/s |
| Player health/inventory sync | 2/sec | ~20 bytes | ~40 B/s |
| Artifact state (picked up / dropped) | Event-based | ~40 bytes | ~5 B/s avg |
| Creature AI position (per creature) | 10/sec | ~24 bytes | ~240 B/s |
| Match state (timer, scores) | 1/sec | ~60 bytes | ~60 B/s |
| Voice chat (Photon Voice) | Continuous | ~1,400 B/s | ~1,400 B/s |
| **Per player total (no voice)** | | | **~785 B/s** |
| **Per player total (with voice)** | | | **~2,185 B/s** |

### Photon Free Tier Limit
- **500 CCU** (concurrent users) free
- **~20 messages/sec per room** soft target for smooth play
- Each player transform = ~1 message/update → 20 players = 20 msg/sec just for movement

---

## Player Count Analysis by Room Type

### Formula
```
Total room bandwidth = players × per_player_bytes + creatures × per_creature_bytes + overhead
MasterClient CPU load = creature_count × AI_complexity + physics_objects + artifact_count
```

### Scenario Breakdown

#### 4 Players — "Squad" (Recommended minimum)
| Metric | Value |
|---|---|
| Player bandwidth | 4 × 785 = 3,140 B/s |
| Creatures (5 active) | 5 × 240 = 1,200 B/s |
| Total room bandwidth | ~4,500 B/s (~4.5 KB/s) |
| MasterClient CPU | Very low |
| Match feel | Tense, tactical — every player matters |
| Artifact count | 8–10 spawns |
| **Verdict** | ✅ Ideal for early access / low population |

#### 8 Players — "Standard" (Sweet spot)
| Metric | Value |
|---|---|
| Player bandwidth | 8 × 785 = 6,280 B/s |
| Creatures (8 active) | 8 × 240 = 1,920 B/s |
| Total room bandwidth | ~8,500 B/s (~8.5 KB/s) |
| MasterClient CPU | Low-moderate |
| Match feel | Busy, chaotic — good for artifact competition |
| Artifact count | 12–15 spawns |
| **Verdict** | ✅ Best balance of population and performance |

#### 12 Players — "Large" (Viable with optimization)
| Metric | Value |
|---|---|
| Player bandwidth | 12 × 785 = 9,420 B/s |
| Creatures (10 active) | 10 × 240 = 2,400 B/s |
| Total room bandwidth | ~12,500 B/s (~12.5 KB/s) |
| MasterClient CPU | Moderate — needs interest management |
| Match feel | Battle royale energy, lots of encounters |
| Artifact count | 16–20 spawns |
| **Verdict** | ⚠️ Viable but requires Photon interest management ON |

#### 16 Players — "Max" (Stress territory)
| Metric | Value |
|---|---|
| Player bandwidth | 16 × 785 = 12,560 B/s |
| Creatures (12 active) | 12 × 240 = 2,880 B/s |
| Total room bandwidth | ~16,000 B/s (~16 KB/s) |
| MasterClient CPU | High — creature AI + 16 player RPCs |
| Match feel | Chaotic, potentially laggy without tuning |
| Artifact count | 20–25 spawns |
| **Verdict** | ❌ Not recommended until optimizations are in place |

---

## Recommended Configuration Per Era

Different eras have different creature counts and map complexity, which changes the safe player cap:

| Era / Bucket | Active Creatures | Recommended Players | Max Players | Notes |
|---|---|---|---|---|
| `deep_prehistoric` (Cretaceous) | 8–12 | **6** | 8 | T-Rex AI is expensive — large patrol radius, physics interactions |
| `deep_prehistoric` (Volcanic) | 4–6 | **8** | 10 | Fewer creatures, lava events are timer-based not AI-driven |
| `deep_prehistoric` (Ice Age) | 6–8 | **8** | 10 | Mammoth stampede is physics-heavy |
| `deep_prehistoric` (Swamp) | 8–10 | **6** | 8 | Giant insect AI + fire spread system adds overhead |
| `deep_prehistoric` (Hominid) | 4–6 NPC tribes | **10** | 12 | NPC tribes are simpler AI than dinosaurs |
| `prehistoric` (3M–10k BCE) | 4–6 | **10** | 12 | Megafauna only, no complex AI |
| `ancient` through `medieval` | 0–2 (ambient) | **12** | 16 | No creature AI, mostly player vs player |
| `renaissance` through `modern` | 0 | **16** | 16 | Pure PvP, full 16 is fine |

---

## universeSeed Count — How Many Variants Can You Support?

The seed is an `int` (32-bit) — that's **4.2 billion possible seeds**. The practical limit is not the seed space, it's how many *meaningfully different* experiences you can generate from it.

### What the seed controls per room
```
universeSeed
  ├── World variant selection     (seed % 5 → picks 1 of 5 variants)
  ├── Prop set index              (seed % propSets.Length)
  ├── Lighting profile index      (seed % lightingProfiles.Length)
  ├── Artifact spawn positions    (Random.InitState(seed) → shuffle spawn points)
  ├── Artifact rarity rolls       (seeded weighted random per spawn point)
  ├── Creature spawn positions    (seeded shuffle of creature spawn points)
  ├── Weather state               (seed % weatherStates.Length)
  └── Time of day                 (seed % timeOfDayStates.Length)
```

### Meaningful variation matrix (deep_prehistoric example)
| Variable | Options | Source |
|---|---|---|
| World variant | 5 | seed % 5 |
| Prop set per variant | 3 | seed % 3 |
| Lighting profile | 4 | seed % 4 |
| Weather state | 3 | seed % 3 |
| Time of day | 3 | seed % 3 |
| Artifact layout | ~10,000+ | seeded shuffle |
| **Unique felt experiences** | **5 × 3 × 4 × 3 × 3 = 540 distinct visual states** | |

540 distinct visual states × effectively infinite artifact layouts = players will rarely feel like they've seen the exact same match twice, even in the same bucket.

### Seed generation — keeping it deterministic
```python
# generate_universe_seed Lambda
# Same bucketId + roomIndex always produces same seed (reproducible)
# Different roomIndex = different experience in same bucket

import hashlib

def get_seed(bucket_id: str, room_index: int) -> int:
    raw = f"{bucket_id}:{room_index}"
    return int(hashlib.sha256(raw.encode()).hexdigest()[:8], 16)
```

This means:
- `deep_prehistoric:0` → always seed `2847392847` → always Cretaceous Jungle, prop set 2, dawn lighting
- `deep_prehistoric:1` → always seed `9183746512` → always Volcanic Hellscape, prop set 1, ash lighting
- Players in room 0 always get the same experience — useful for debugging and for players who want to "revisit" a specific universe

---

## Photon Interest Management (required at 12+ players)

At 12+ players, enable Photon's built-in interest management to stop sending updates about players/objects that are far away:

```csharp
// On each PhotonView component:
// Set "Observe Option" to "Unreliable On Change"
// Enable "Use Interest Groups" in PhotonNetwork settings

// In MapVariantManager.cs — assign interest groups by map zone
void AssignInterestGroup(GameObject player) {
    int zone = GetZoneIndex(player.transform.position); // 0–4
    player.GetComponent<PhotonView>().Group = (byte)(zone + 1);
}

// Players only receive updates from their zone + adjacent zones
// Cuts bandwidth by ~60% on large maps
```

---

## Optimization Checklist Before Increasing Player Cap

- [ ] **Photon interest management** — zone-based, cuts bandwidth 40–60%
- [ ] **Reduce transform sync rate** — 20/sec → 10/sec for distant players (use `PhotonTransformView` with distance-based LOD)
- [ ] **Creature AI on MasterClient only** — never sync creature position every frame, use RPC for state changes only (patrol → chase → attack)
- [ ] **Artifact sync is event-based** — never poll artifact state, only fire RPC on pickup/drop
- [ ] **Voice chat proximity culling** — Photon Voice already supports this, set max voice distance per era map
- [ ] **Object pooling** — never Instantiate/Destroy during a match, pool all projectiles, effects, and creature spawns
- [ ] **LOD on creature meshes** — T-Rex at 100m+ should be lowest LOD, no animation updates

---

## Recommended Launch Configuration

| Phase | Room Size | Creature Count | Seed Variants | Rationale |
|---|---|---|---|---|
| Alpha / Early Access | 4–6 players | 4–6 | 5 world variants | Low population, stress-free, focus on fun |
| Beta | 6–8 players | 6–8 | 5 variants + 3 prop sets | Growing population, test interest management |
| Launch | 8–10 players | 8–10 | Full 540 visual states | Optimizations in, healthy server population |
| Post-launch | 12 players | 10–12 | Full + new era variants | Interest management proven, scale up |
| Target ceiling | 16 players | 12 | Full | Only with dedicated server migration off PUN2 relay |

> **Note on 16 players long-term**: Photon PUN2's relay model has a practical ceiling around 12–16 players for action games with creature AI. If Treasure Royale grows beyond that, the path is **Photon Fusion** (Photon's newer server-authoritative SDK) or **AWS GameLift** for dedicated servers. That's a future migration, not a current concern.
