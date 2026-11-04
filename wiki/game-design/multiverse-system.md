# Multiverse System

## Core Concept
Players pick any year in history. The system maps that year to the nearest **historical anchor point** — a real, significant event. All players whose chosen year falls within that anchor's bucket range share the same matchmaking pool, keeping servers populated while maintaining thematic accuracy.

```
Player A picks 3200 BCE  →  anchor: Early Dynastic Egypt  →  bucket: ancient_egypt_early_dynastic
Player B picks 3220 BCE  →  anchor: Early Dynastic Egypt  →  bucket: ancient_egypt_early_dynastic
Player C picks 3180 BCE  →  anchor: Early Dynastic Egypt  →  bucket: ancient_egypt_early_dynastic
→ All three join the same server pool
→ Each room within that pool gets a unique universeSeed → different map variant
```

See [[master-timeline]] for the full anchor point database (100 distinct timeline experiences across 11 eras).
See [[historical-anchors]] for the matchmaking bucket logic.

---

## Bucket Range Philosophy
Bucket ranges are not uniform — they scale with historical density:

| Era | Typical Bucket Range | Reason |
|---|---|---|
| Deep Prehistoric | Unlimited catch-all | Single bucket, seed picks variant |
| Prehistoric | ±500,000 – ±1,000 yrs | Events are sparse, dates are approximate |
| Ancient | ±50 – ±150 yrs | Civilizations rise/fall over centuries |
| Medieval | ±15 – ±30 yrs | Events are more precisely dated |
| Renaissance+ | ±5 – ±15 yrs | History is well-documented |
| Modern | ±2 – ±4 yrs | Events are exact, themes are specific |
| Near Future | ±2 – ±5 yrs | Speculative but anchored to plausible events |
| Mid Future | ±10 – ±20 yrs | Broader speculation, wider tolerance |
| Far Future | ±25 – ±100 yrs | Highly speculative, large ranges acceptable |
| Deep Future | Unlimited catch-all | Single bucket, seed picks variant |

---

## Timeline Selection UI Flow

```
MultiverseTransporterUI
  → Player spins a "timeline dial" or types a year
  → Client calls POST /seed with { year: int }
  → Lambda resolves nearest anchor → returns { bucketId, anchorYear, eventName, eraLabel }
  → UI displays: "Earth, 3200 BCE — Early Dynastic Egypt"
  → Player confirms → PhotonNetwork.JoinOrCreateRoom(bucketId, ...)
  → Room CustomProperties set: { bucketId, anchorYear, eventName, universeSeed }
  → Scene loads: era base map
  → MapVariantManager.ApplyVariant(universeSeed)
```

---

## Room Naming & Population Strategy

Rooms are named `{bucketId}_{roomIndex}` with a max player cap (e.g. 16 players).
When a bucket fills a room, a new room opens automatically:

```
ancient_egypt_early_dynastic_0   → 16/16 players (full)
ancient_egypt_early_dynastic_1   → 8/16 players  (joinable)
ancient_egypt_early_dynastic_2   → just created
```

Each room gets its own `universeSeed` so the map variant is unique per room, even within the same historical bucket. This is the multiverse — infinite parallel versions of the same moment in history.

---

## Universe Seed Generation

```python
# generate_universe_seed Lambda
seed_input = f"{bucketId}:{roomId}:{requestId}"
hash_hex = hashlib.sha256(seed_input.encode()).hexdigest()
universeSeed = int(hash_hex[:8], 16)  # deterministic, unique per room
```

The seed drives everything inside a match:
- Prop set selection (which variant of the era map)
- Artifact spawn positions and rarity distribution
- Weather / time of day
- Faction presence (which historical group controls the map)
- Ambient audio profile

---

## Era Base Maps (Unity Scenes)

| Era | Scene Name | Anchor Count | Variant Drivers |
|---|---|---|---|
| Deep Prehistoric | `deep_prehistoric_base` | 1 (catch-all) | World variant (5 types), megafauna, vegetation |
| Prehistoric | `prehistoric_base` | 10 | Megafauna type, vegetation density, water level |
| Ancient | `ancient_base` | 22 | Civilization (Egypt/Mesopotamia/China), dynasty state |
| Medieval | `medieval_base` | 13 | Region (Europe/Asia/Americas), conflict state |
| Renaissance | `renaissance_base` | 9 | Nation, trade vs. conflict, season |
| Industrial | `industrial_base` | 10 | Industry type, era (steam/electric), conflict level |
| Modern | `modern_base` | 12 | City nationality, time of day, crisis level |
| Near Future | `near_future_base` | 10 | Tech level, climate state, political faction |
| Mid Future | `mid_future_base` | 8 | Civilization type, Earth vs off-world, conflict |
| Far Future | `far_future_base` | 6 | Collapse vs utopia, alien presence, tech tier |
| Deep Future | `deep_future_base` | 1 (catch-all) | World variant (5 types), post-human state |

---

## Future Planets (Post-Launch)
Mars, Moon, and exoplanet timelines use the same anchor system with separate base map sets. Out of current scope.
