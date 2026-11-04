# Upgrade System & Monetization

## Design Philosophy

The cross-era affinity penalty is not a punishment — it's a **progression hook**. A player who loots a raptor egg in the prehistoric era and wants to ride it effectively in the modern era has a clear, satisfying path: upgrade it. The imbalance is the motivation to engage with the economy.

**Critical rule: upgrades never create pay-to-win.** Every upgrade available for real money is also earnable through gameplay. Real money buys speed of progression, not exclusive power. A free player who extracts consistently will eventually have the same upgrades as a paying player — the paying player just gets there faster.

---

## Two Currencies

| Currency | Name | How Earned | How Spent |
|---|---|---|---|
| **Chrono Shards** | Free currency | Extracting artifacts, daily missions, match performance | Upgrades, cosmetics, Vault trades |
| **Void Coins** | Premium currency | Real money purchase only | Accelerated upgrades, exclusive cosmetics, timeline unlocks |

### Chrono Shard Earn Rates
| Action | Shards Earned |
|---|---|
| Successful extraction (any artifact) | 50 + (artifact tradeValue × 0.1) |
| First extraction of the day | +100 bonus |
| Killing a player who held an artifact | 25 |
| Surviving a full match without dying | 30 |
| Daily mission complete | 75–200 depending on difficulty |
| Weekly mission complete | 500 |

A consistent player extracting 3–4 matches per day earns roughly **400–600 Chrono Shards/day** — enough to buy one minor upgrade every 2–3 days without spending real money.

---

## The Vault — Upgrade Shop

The Vault is the post-match lobby shop. Three tabs:

```
[ UPGRADES ]  [ COSMETICS ]  [ TIMELINE UNLOCKS ]
```

### UPGRADES Tab

This is where cross-era affinity penalties are addressed. Every power item and mount has an upgrade tree with **3 tiers**. Each tier reduces the affinity penalty for a specific era gap.

#### Upgrade Tree Structure

```
Power Item (e.g. Velociraptor Mount)
  └── Tier 1 Upgrade: "Temporal Conditioning I"
        Effect: Affinity floor raised from 40% → 55% in all eras
        Cost: 300 Chrono Shards OR 80 Void Coins
        
  └── Tier 2 Upgrade: "Temporal Conditioning II"  
        Requires: Tier 1
        Effect: Affinity floor raised from 55% → 70% in all eras
        Cost: 700 Chrono Shards OR 180 Void Coins
        
  └── Tier 3 Upgrade: "Full Temporal Mastery"
        Requires: Tier 2
        Effect: Affinity floor raised to 90% in all eras + removes panic behavior
        Cost: 1,500 Chrono Shards OR 350 Void Coins
```

At Tier 3, the item is effectively cross-era viable — not at 100% native performance, but close enough that skill determines the outcome, not the era gap. The environmental counters (canopy, creature aggro, noise) still apply — upgrades only address the stat penalty, not the world's physical response.

#### Upgrade Categories

**Mount Upgrades**
| Upgrade Name | Effect | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Temporal Conditioning I | Affinity floor 40% → 55% | 300 | 80 |
| Temporal Conditioning II | Affinity floor 55% → 70% | 700 | 180 |
| Full Temporal Mastery | Affinity floor 70% → 90%, removes panic | 1,500 | 350 |
| Noise Dampener I | Noise radius -25% in all eras | 400 | 100 |
| Noise Dampener II | Noise radius -50% in all eras | 900 | 220 |
| Reinforced Hide / Armor | Mount health +20% cross-era | 500 | 120 |
| Era Camouflage | Mount visually adapts to current era (cosmetic + -15% threat level) | 800 | 200 |

**Creature Mount Specific**
| Upgrade Name | Effect | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Raptor: Neural Bond | Prevents panic near vehicles | 600 | 150 |
| Raptor: Pack Retention | Pack bond persists 2 tiers above native era | 800 | 200 |
| Pterodactyl: Altitude Chip | Flight ceiling +20m in all eras | 500 | 120 |
| Pterodactyl: Stealth Feathers | No minimap icon while airborne in any era | 1,000 | 250 |
| Mammoth: Ballistic Plating | Immune to cannon fire (removes that environmental counter) | 1,200 | 300 |
| T-Rex: Temporal Apex | Threat level reduced by 3 in all eras | 2,000 | 500 |

**Vehicle Mount Specific**
| Upgrade Name | Effect | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Hoverbike: Canopy Cutter | Flight ceiling raised from 8m → 25m in jungle eras | 700 | 175 |
| Hoverbike: Creature Shield | Engine noise no longer triggers creature aggro | 1,000 | 250 |
| Helicopter: Raptor Deterrent | Pterodactyls no longer attack the helicopter | 1,200 | 300 |
| Steam Bike: Waterproofing | No longer destroyed by water hazards | 600 | 150 |

**Weapon / Power Item Upgrades**
| Upgrade Name | Effect | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Temporal Infusion I | Power magnitude affinity floor 40% → 60% | 250 | 65 |
| Temporal Infusion II | Power magnitude affinity floor 60% → 80% | 600 | 150 |
| Guardian Ward I | Threat level reduced by 1 | 400 | 100 |
| Guardian Ward II | Threat level reduced by 2 | 900 | 225 |
| Guardian Ward III | Threat level reduced to 0 (no guardians ever) | 2,000 | 500 |

---

### COSMETICS Tab

Cosmetics are purely visual — no gameplay effect. This is where Void Coins shine because cosmetics are the most visible status symbol.

**Player Cosmetics**
| Item | Description | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Era Skin Sets | Full outfit themed to a specific era (8 sets) | 2,000 | 500 |
| Transporter Aura | Glowing effect on player during arrival animation | — | 200 |
| Kill Effect | Visual effect when eliminating a player | — | 150 |
| Artifact Glow | Custom color on held artifacts | 500 | — |

**Mount Cosmetics**
| Item | Description | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Raptor Skin: Feathered Variant | Realistic feathered raptor appearance | — | 300 |
| Raptor Skin: Neon Bioluminescent | Glowing multiverse variant | — | 400 |
| Hoverbike Skin: Void Chrome | Black mirror finish | — | 350 |
| Pterodactyl Skin: Albino | White with red eyes | — | 300 |
| Mount Trail Effect | Particle trail behind any mount | — | 200 |

**Extraction Cosmetics**
| Item | Description | Shard Cost | Void Coin Cost |
|---|---|---|---|
| Custom Beacon Activation | Unique animation when you activate the beacon | — | 250 |
| Extraction Portal Color | Changes the color of your exit portal | 800 | — |
| Victory Pose | Animation played when extraction completes | — | 200 |

---

### TIMELINE UNLOCKS Tab

By default players can access all eras. Timeline Unlocks are **premium era variants** — special versions of existing eras with unique themes, exclusive artifacts, and harder challenges.

| Unlock | Description | Void Coin Cost | Earnable? |
|---|---|---|---|
| Atlantis Variant | Ancient era variant — sunken city, water combat, unique artifacts | 600 | No — Void Coins only |
| Camelot Variant | Medieval era variant — Arthurian legend theme, mythic sword artifacts | 600 | No |
| Tesla's World | Industrial era variant — alternate history where Tesla won, electric everything | 600 | No |
| Dinosaur Apex | Deep Prehistoric variant — all Tier 0 mounts spawn at 150% stats | 800 | No |
| Void Timeline | Future era variant — post-apocalyptic, highest artifact values in the game | 1,000 | No |

These are the only items that are **Void Coins exclusive** — they represent premium content, not power advantages. The artifacts found in these variants have higher trade values but the same power levels as standard variants.

---

## Upgrade Persistence & Ownership

Upgrades are tied to the **item type**, not a specific instance. When you upgrade "Velociraptor Mount — Temporal Conditioning I", every raptor egg you ever loot benefits from that upgrade. You're upgrading your knowledge of how to handle that creature across timelines, not a specific animal.

```
Player upgrades: Velociraptor → Temporal Conditioning II
  → Stored in DynamoDB: players table, SK=UPGRADES
  → { "velociraptor_mount": { "temporal_conditioning": 2, "noise_dampener": 1 } }
  → Every match: when player spawns a raptor, CrossEraPowerCalculator reads their upgrades
  → Affinity floor applied: 70% instead of 40%
```

This also means upgrades survive across matches — you invest once, benefit forever.

---

## Monetization Ethics — The Lines We Don't Cross

| Allowed | Not Allowed |
|---|---|
| Selling speed of progression (Void Coins buy upgrades faster) | Selling upgrades that free players can never access |
| Selling cosmetics for real money | Selling stat advantages unavailable through gameplay |
| Selling premium era variants (content, not power) | Locking base eras behind a paywall |
| Selling Chrono Shard bundles (buy currency, spend on upgrades) | Pay-to-win extraction advantages |
| Battle pass with cosmetics + Shard bonuses | Battle pass with exclusive power upgrades |

---

## Battle Pass — "The Chrono Pass"

A seasonal battle pass (8-week seasons, one per major era theme):

```
Season 1: "The Cretaceous Chronicles"
  Free track (100 levels):
    Levels 1–20:   Chrono Shard bundles (50–100 each)
    Levels 21–50:  Cosmetic items (artifact glows, player banners)
    Levels 51–80:  Chrono Shard bundles (100–200 each)
    Levels 81–100: Exclusive Chrono Shard cache (500 shards)
    
  Premium track (800 Void Coins):
    All free track rewards PLUS:
    Exclusive raptor skin (Cretaceous Feathered variant)
    Custom beacon activation (amber energy pillar)
    Season title: "Apex Plunderer"
    Bonus Chrono Shards at every level (+25 per level = 2,500 total bonus)
```

The premium track never contains upgrade unlocks — only cosmetics and Shard bonuses. A free player who completes the free track earns enough Shards to buy 2–3 meaningful upgrades per season.

---

## Revenue Projections (Design Reference)

| Segment | Behavior | Monthly Spend |
|---|---|---|
| Free players | Extract regularly, earn Shards, buy upgrades slowly | $0 |
| Casual spenders | Buy one Void Coin pack/month for cosmetics | $5–10 |
| Progression buyers | Buy Void Coins to accelerate upgrades | $15–30 |
| Collectors | Buy all cosmetics, all timeline unlocks | $50–100 |
| Battle pass buyers | Buy every season pass | $10/season |

The upgrade system targets the **progression buyers** — players who are engaged enough to want their raptor in the modern era but don't want to grind 3 weeks for it. That's the highest-value monetization segment in live service games.
