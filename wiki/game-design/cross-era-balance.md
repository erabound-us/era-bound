# Cross-Era Power Balance System

## The Core Problem

A futuristic hoverbike brought into the prehistoric era is god-mode. A rideable raptor brought into the modern era dies in two seconds. If cross-era items are allowed without a balancing system, whoever loots the highest-era item wins every match regardless of skill.

The solution is **three interlocking systems** that create the problem — and the **Upgrade System** that sells the solution. See [[upgrade-monetization]] for the full economy design.

1. **Era Affinity** — every power item has a native era. It performs at 100% in that era and degrades outside it. Upgrades raise the affinity floor.
2. **Environmental Counters** — the era itself fights back against out-of-era technology. Specific upgrades remove specific counters.
3. **Threat Scaling** — bringing a powerful out-of-era item attracts guardians. Guardian Ward upgrades reduce threat level.

The imbalance is intentional — it's the progression hook. Players feel the penalty, want to fix it, and the Vault shows them exactly how.

---

## Era Tier Ranking

Every era has a technology tier. This is the foundation of the balance math.

| Tier | Era | Representative Power Items |
|---|---|---|
| 0 | Deep Prehistoric | Rideable dinosaurs, creature bonds, primal artifacts |
| 1 | Prehistoric | Megafauna mounts, shamanic powers, elemental tools |
| 2 | Ancient | Chariots, siege weapons, divine relics |
| 3 | Medieval | War horses, siege engines, enchanted weapons |
| 4 | Renaissance | Cannons, early firearms, alchemical devices |
| 5 | Industrial | Steam vehicles, early aircraft, mechanical constructs |
| 6 | Modern | Motorcycles, helicopters, military weapons |
| 7 | Future | Hoverbikes, exosuits, energy weapons |

---

## Era Affinity — The Power Scaling Formula

Every power item has a `nativeEraTier` (the era it came from). When used in a different era, its effectiveness is modified by the **affinity multiplier**:

```
tierDelta = currentEraTier - nativeEraTier

if tierDelta > 0:  // using a lower-era item in a higher-era timeline
    affinityMultiplier = 1.0 - (tierDelta * 0.12)   // loses 12% per tier gap
    floor at 0.40  // never drops below 40% effectiveness

if tierDelta < 0:  // using a higher-era item in a lower-era timeline
    affinityMultiplier = 1.0 + (abs(tierDelta) * 0.15)  // gains 15% per tier gap
    cap at 1.60  // never exceeds 160% effectiveness — environmental counters kick in above this
    
if tierDelta == 0:  // native era
    affinityMultiplier = 1.0  // full power
```

### Examples

**Raptor mount (Tier 0) used in Modern era (Tier 6):**
```
tierDelta = 6 - 0 = 6
affinityMultiplier = 1.0 - (6 × 0.12) = 1.0 - 0.72 = 0.28 → floored to 0.40
Raptor speed: 40% of native | Raptor health: 40% of native
Result: Fast enough to be useful for flanking, dies quickly to gunfire — not useless, not dominant
```

**Hoverbike (Tier 7) used in Deep Prehistoric (Tier 0):**
```
tierDelta = 0 - 7 = -7
affinityMultiplier = 1.0 + (7 × 0.15) = 2.05 → capped at 1.60
Hoverbike speed: 160% of native | But environmental counters activate (see below)
Result: Extremely fast but the jungle, creatures, and terrain actively counter it
```

**Medieval war horse (Tier 3) used in Ancient era (Tier 2):**
```
tierDelta = 2 - 3 = -1
affinityMultiplier = 1.0 + (1 × 0.15) = 1.15
War horse speed: 115% of native — slight advantage, not game-breaking
```

---

## Environmental Counters — The Era Fights Back

When a player brings a high-tier item into a low-tier era, the environment itself becomes a hard counter. These are not soft penalties — they are specific, exploitable weaknesses that other players can use against the overpowered item.

### Counter Table

| Item Brought In | Era It's Used In | Environmental Counter | How Opponents Exploit It |
|---|---|---|---|
| Hoverbike / hovercraft | Deep Prehistoric | Dense jungle canopy — max flight ceiling 8m, constant collision with branches | Force combat under heavy canopy, use terrain to funnel them into tight spaces |
| Hoverbike / hovercraft | Prehistoric / Ancient | Open terrain but T-Rex / mammoth aggro range doubles for loud engine noise | Lure creatures toward the vehicle — engine noise is a creature magnet |
| Modern helicopter | Deep Prehistoric | Pterodactyls attack aircraft on sight — 3+ pterodactyls will pursue any flying vehicle | Stay grounded, wait for pterodactyls to bring it down |
| Modern firearms | Deep Prehistoric | Gunfire triggers every creature within 80m — every shot is a beacon | Use creature noise to track armed players, let creatures do the work |
| Futuristic energy weapon | Any era below Tier 5 | Energy signature attracts era-specific "guardian" NPCs — the timeline's immune response | Follow the guardian NPCs to find the energy weapon carrier |
| Rideable raptor | Modern / Future | Raptor panics near engine noise — uncontrollable for 3s near any vehicle | Drive a vehicle near a mounted raptor to dismount the rider |
| Rideable pterodactyl | Industrial / Modern | Anti-aircraft fire from NPC soldiers, surface-to-air hazards | Stay near NPC military zones when fighting a pterodactyl rider |
| Mammoth mount | Renaissance / Industrial | Cannon fire — NPC artillery units target large creatures | Lure mammoth rider near NPC cannon emplacements |
| Alchemical bomb | Deep Prehistoric | High oxygen atmosphere — blast radius triples, user takes splash damage | Let them use it near themselves |
| Steam construct | Deep Prehistoric / Prehistoric | Water hazards destroy steam mechanisms instantly | Fight near rivers, swamps, tar pits |

---

## The Mount System

Mounts are a special category of power item — they are **spawnable creatures or vehicles** that come from eggs, relics, or vehicle artifacts. They are the most visible cross-era balance challenge.

### Mount Anatomy

```
Mount {
  id: string
  mountType: Creature | Vehicle | Mechanical
  nativeEraTier: int          // 0–7
  baseSpeed: float            // units/sec on flat ground
  baseHealth: float
  carryCapacity: int          // how many artifacts the rider can hold while mounted
  noiseRadius: float          // how far the mount's sound travels (creature aggro trigger)
  flightCapable: bool
  terrainPenalties: string[]  // terrain types that slow or damage this mount
}
```

### Mount Roster

#### Tier 0 — Deep Prehistoric Mounts
| Mount | Source | Base Speed | Flight | Noise Radius | Special |
|---|---|---|---|---|---|
| Velociraptor | Raptor Egg artifact | 14 m/s | No | 20m | Pack bond — 2 wild raptors follow the rider |
| Pterodactyl | Pterodactyl Egg artifact | 22 m/s | Yes, ceiling 40m | 35m | Aerial artifact grab — can snatch from ground while flying |
| Triceratops | Triceratops Horn relic | 10 m/s | No | 50m | Charge attack — knocks players and creatures aside |
| T-Rex (Legendary) | T-Rex Egg (Mythic only) | 12 m/s | No | 80m | All creatures flee — clears zones, but also clears cover |

#### Tier 1–2 — Prehistoric / Ancient Mounts
| Mount | Source | Base Speed | Flight | Noise Radius | Special |
|---|---|---|---|---|---|
| Saber-tooth cat | Smilodon Pelt relic | 16 m/s | No | 15m | Silent movement — no noise radius when walking |
| War mammoth | Mammoth Tusk artifact | 9 m/s | No | 60m | Artifact carry +3 — can hold more loot than any other mount |
| Egyptian chariot | Chariot Wheel relic | 18 m/s | No | 30m | Road speed bonus — 25 m/s on flat paved surfaces |
| Giant eagle | Eagle Talon relic | 20 m/s | Yes, ceiling 60m | 25m | Stealth flight — no minimap icon while airborne |

#### Tier 5–6 — Industrial / Modern Mounts
| Mount | Source | Base Speed | Flight | Noise Radius | Special |
|---|---|---|---|---|---|
| Steam motorcycle | Blueprint artifact | 28 m/s | No | 70m | Ramming damage — deals damage on collision |
| Military jeep | Vehicle Key artifact | 24 m/s | No | 65m | Passenger seat — second player can shoot from vehicle |
| Attack helicopter | Helicopter Key (Legendary) | 35 m/s | Yes, ceiling 120m | 90m | Minigun — suppression fire, not precision |

#### Tier 7 — Future Mounts
| Mount | Source | Base Speed | Flight | Noise Radius | Special |
|---|---|---|---|---|---|
| Hoverbike | Energy Cell artifact | 40 m/s | Yes, ceiling 8m | 45m | Silent hover — lower noise than vehicles |
| Exosuit (not a mount, worn) | Exosuit Core artifact | Player speed ×2 | No | 10m | Bullet resistance +40%, jump height ×3 |
| Void Skimmer (Mythic) | Void Crystal artifact | 50 m/s | Yes, ceiling 200m | 20m | Phase shift — 3s of intangibility, 60s cooldown |

---

## Cross-Era Mount Behavior Matrix

This is the practical balance table — what actually happens when you bring a mount into the wrong era:

| Mount | In Native Era | 2 Tiers Higher | 4 Tiers Higher | Max Tier Gap |
|---|---|---|---|---|
| Raptor | Full speed, pack bond active | 76% speed, pack bond lost | 40% speed (floor), panics near vehicles | Dismounted by engine noise |
| Pterodactyl | Full flight, aerial grab | 76% speed, ceiling reduced to 20m | 40% speed, pterodactyls in era attack it | Shot down by NPC anti-air |
| Hoverbike | Full speed, 8m ceiling | 115% speed, canopy collision in jungle | 160% speed (cap), constant creature aggro from engine | Pterodactyls attack, canopy blocks flight |
| Helicopter | Full speed, 120m ceiling | 115% speed, NPC soldiers shoot at it | 160% speed (cap), pterodactyls attack, T-Rex aggro | Brought down by pterodactyl swarms |
| Saber-tooth | Full speed, silent | 88% speed, loses stealth | 40% speed (floor), panics near gunfire | Killed by NPC soldiers in 3 hits |

---

## Threat Scaling — The Timeline's Immune Response

When a player brings a high-tier item into a low-tier era, the timeline generates a proportional response. This is the multiverse's way of "correcting" the anomaly — and it's the narrative justification for the balance mechanic.

```
threatLevel = max(0, nativeEraTier - currentEraTier)

threatLevel 0: No response — native era, no anomaly
threatLevel 1–2: Minor — nearby creatures become slightly more aggressive toward the carrier
threatLevel 3–4: Moderate — a "timeline guardian" NPC spawns and hunts the carrier
threatLevel 5–6: Major — multiple guardians spawn, all players on minimap can see the carrier's position
threatLevel 7: Critical — continuous guardian spawns until item is dropped or player extracts
```

### Timeline Guardians by Era

| Era | Guardian | Behavior |
|---|---|---|
| Deep Prehistoric | Alpha Raptor Pack (5 raptors) | Relentless pursuit, never loses aggro |
| Prehistoric | Giant Cave Bear | Slow but unkillable until item is dropped |
| Ancient | Pharaoh's Guard (3 armored NPCs) | Ranged + melee, calls for backup |
| Medieval | Inquisitor Knight | Tracks via "heresy sense" — ignores stealth |
| Renaissance | Witch Hunter Squad | Throws fire bombs, ignores cover |
| Industrial | Automaton Enforcer | Mechanical, immune to fire and explosions |
| Modern | Black Site Strike Team | Full military tactics, helicopter support |

---

## Balance Summary — The Design Principles

1. **No item is useless cross-era** — the affinity floor of 40% means even a raptor in the future era is still a viable flanking tool, just not dominant
2. **No item is god-mode cross-era** — the 160% cap plus environmental counters means a hoverbike in the prehistoric era is powerful but has hard counters
3. **The environment is the balancer, not invisible stats** — counters are physical, visible, and exploitable by other players
4. **Noise is the universal equalizer** — every mount and vehicle has a noise radius. In creature-heavy eras, noise is a death sentence regardless of how fast you are
5. **Skill can overcome the affinity penalty** — a skilled raptor rider who knows the modern era map can still outperform a hoverbike rider who doesn't know the terrain
6. **The multiverse narrative justifies everything** — guardians, creature aggro, environmental hostility are all "the timeline correcting an anomaly"
