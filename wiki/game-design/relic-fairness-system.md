# Relic Fairness System — Team Rewards, Solo Extraction & Elite Gear

## The Problem This Solves

Without this system:
- A 4-player team could coordinate a relic extraction with zero individual risk — one player channels while three guard
- The team leader or most vocal player would always claim the relic reward
- Solo players would simply be unable to extract relics, making them irrelevant to that content
- Relics would become a team farming mechanic rather than a high-stakes singular event

With this system:
- Teams cooperate to extract but **cannot control who receives the relic** — internal tension is built in
- Solo players can extract relics but **only with elite gear** — a genuine progression gate
- The relic remains singular and meaningful regardless of how many players are involved

---

## PART 1 — TEAM RELIC EXTRACTION

### The Core Rule
A relic extracted by a team is awarded to **one randomly selected team member** at the moment of extraction. The selection happens server-side, is announced to all players in the match, and cannot be influenced by any player action.

### Why Randomized, Not Earned
- Prevents the "carry" dynamic where one player does all the work and claims the reward
- Creates genuine internal team drama — everyone wants it, no one knows who gets it
- Stops pre-match agreements ("I'll guard, you extract, we split") from being reliable
- The multiverse narrative justification: *"The timeline chose who the relic belongs to"*

### Team Extraction Flow

```
Team carries relic to beacon
  → Any team member begins channel (10 seconds standard)
  → All team members within 20m contribute to a "team extraction pool"
  → Channel completes
  → MasterClient rolls: random.choice(teamMembersWithin20m)
  → Selected player receives the relic in their permanent inventory
  → All other team members within 20m receive: standard artifact trade value equivalent in Chrono Shards
  → Announcement to all players in match: "[PlayerName] extracted [RelicName] — awarded to [WinnerName]"
```

### Contribution Requirement
A team member must be **within 20m of the beacon at extraction completion** to be eligible for the relic award. This prevents:
- Players hiding safely across the map while teammates do the work
- AFK players receiving relics
- Players who died and respawned far away from claiming a reward they didn't earn

```
Eligible pool = team members within 20m at extraction completion
If only 1 team member within 20m → that player receives the relic (no roll needed)
If 0 team members within 20m → extraction fails, channel resets (edge case: channeling player died mid-channel)
```

### Consolation Reward — Non-Winners
Team members who were in the eligible pool but didn't win the relic roll receive:

| Relic Rarity | Consolation Reward |
|---|---|
| Epic | 300 Chrono Shards + extraction credit |
| Legendary | 600 Chrono Shards + extraction credit |
| Mythic | 1,200 Chrono Shards + extraction credit |

"Extraction credit" counts toward seasonal missions and battle pass progress — participating in a relic extraction has value even without winning the relic itself.

### Team Internal Tension — By Design
The randomization creates situations the game doesn't need to engineer artificially:
- A player who did most of the work might not win the roll
- A player who barely contributed might win
- Teams will argue about this. That's the point.
- The multiverse chose. No one can dispute the multiverse.

---

## PART 2 — SOLO RELIC EXTRACTION

### The Core Problem
A solo player attempting a relic extraction faces:
- No one to guard the beacon while they channel
- No one to contest enemies who approach
- The full 10-second channel with no support
- The relic guardian boss spawned at the relic's location

Without elite gear, this is a death sentence. With elite gear, it becomes the highest-skill play in the game.

### Solo Extraction Modifiers

When a player is **not in a team** and attempts to extract a relic, the following apply:

| Modifier | Standard Team | Solo (no elite gear) | Solo (with elite gear) |
|---|---|---|---|
| Channel duration | 10 seconds | 20 seconds | 14 seconds |
| Contest radius | 10m | 10m | 10m |
| Damage cancels channel | Yes | Yes | No — damage slows channel instead |
| Move distance to cancel | 1m | 0.5m | 1m |
| Relic guardian active | Yes | Yes, +50% health | Yes, standard health |
| Beacon broadcasts location | Yes | Yes + every 3s repeat | Yes, one broadcast only |
| Reward | Shared roll | Full relic, no sharing | Full relic, no sharing |

The solo player who succeeds keeps the entire relic — no roll, no sharing. That's the reward for the difficulty.

### The Repeat Broadcast Problem
For solo players without elite gear, the beacon broadcasts their location **every 3 seconds** for the duration of the channel. Every player on the map knows exactly where they are and that they're standing still for 20 seconds. This is intentionally brutal — it's designed to be nearly impossible without elite gear.

---

## PART 3 — ELITE GEAR SYSTEM

Elite gear is the solo player's equalizer. It doesn't make solo extraction easy — it makes it possible.

### What Elite Gear Is
Elite gear is a **loadout slot system** separate from artifacts and relics. Players equip gear before entering a timeline from the lobby. Gear is consumed on use or lost on death — it is not permanent like upgrades.

Gear is earned through:
- Chrono Shard purchases at the Vault
- Rare drops from boss creatures (not from standard artifact spawns)
- Completing elite missions

### Gear Slots
Each player has **3 gear slots**:

```
[ ARMOR ]  [ UTILITY ]  [ CONSUMABLE ]
```

### ARMOR Tier

| Gear | Description | Solo Relic Benefit | Era Restriction | Cost |
|---|---|---|---|---|
| Standard Wraps | Basic protection, no special properties | None | None | Free (default) |
| Temporal Weave | Armor that partially absorbs timeline energy | Damage slows channel instead of cancelling it | None | 150 Shards |
| Relic Ward Plate | Heavy armor specifically designed for relic extraction | Damage slows channel, +30% health during channel | None | 400 Shards |
| Void Carapace | Post-human armor from the Deep Future | Damage slows channel, +60% health, channel duration -3s | Future eras only | 800 Shards |
| Ancestral Bone Armor | Prehistoric megafauna bone plates | Damage slows channel, immune to creature attacks during channel | Prehistoric/Ancient only | 600 Shards |

### UTILITY Tier

| Gear | Description | Solo Relic Benefit | Era Restriction | Cost |
|---|---|---|---|---|
| Smoke Canister | Deploys a 10m smoke cloud for 30 seconds | Blocks enemy line of sight to beacon | None | 80 Shards |
| Temporal Anchor | Pins the beacon in place for 3 minutes (Simulation Layer only) | Prevents beacon relocation | Simulation Layer | 300 Shards |
| Guardian Seal | A ward that prevents boss creatures from approaching within 15m for 60 seconds | Keeps relic guardian at bay during channel | None | 250 Shards |
| Decoy Beacon | Places a fake beacon signal 50m away | Draws enemy players to wrong location for 20 seconds | None | 200 Shards |
| Extraction Amplifier | Boosts channel speed by 30% | Channel duration reduced by 30% | None | 350 Shards |
| Stealth Cloak | 15 seconds of invisibility | Approach beacon without triggering broadcast | None | 400 Shards |

### CONSUMABLE Tier

| Gear | Description | Solo Relic Benefit | Era Restriction | Cost |
|---|---|---|---|---|
| Stim Pack | Full health restore, instant | Survive damage during channel | None | 60 Shards |
| Adrenaline Shot | +50% movement speed for 10 seconds | Reach beacon faster after clearing area | None | 50 Shards |
| Revive Token | Auto-revive once if killed during extraction | Second chance if killed mid-channel | None | 200 Shards |
| Timeline Stabilizer | Prevents Reality Tears within 20m for 2 minutes | Essential for Simulation Layer solo | Simulation Layer | 400 Shards |
| Boss Lure | Throws a scent object that draws boss creatures to a target location | Redirect relic guardian away from beacon | None | 120 Shards |

### Recommended Solo Relic Loadout

For a player attempting a solo Mythic relic extraction in a standard era:

```
[ Relic Ward Plate ] + [ Guardian Seal ] + [ Revive Token ]

Strategy:
  1. Clear the area around the beacon before approaching
  2. Deploy Guardian Seal — boss creature held at 15m for 60 seconds
  3. Begin channel — damage slows but doesn't cancel
  4. If killed mid-channel — Revive Token triggers, channel resets but player is alive
  5. Complete 14-second channel (with Relic Ward Plate bonus)
  
Total gear cost: 400 + 250 + 200 = 850 Chrono Shards
Relic Mythic trade value: 3× standard Mythic = ~15,000 Shards equivalent
Risk/reward: strongly positive if successful
```

For Simulation Layer solo (hardest content in the game):

```
[ Void Carapace ] + [ Temporal Anchor ] + [ Timeline Stabilizer ]

Strategy:
  1. Find Reality Anchor relic first — stops beacon from moving
  2. Deploy Timeline Stabilizer — no Reality Tears near beacon for 2 minutes
  3. Void Carapace: channel duration reduced to 11 seconds, damage slows not cancels
  4. Temporal Anchor: beacon cannot relocate during channel
  5. Complete channel

Total gear cost: 800 + 300 + 400 = 1,500 Chrono Shards
This is the most expensive solo loadout in the game. It should be.
```

---

## PART 4 — RELIC GUARDIAN SCALING

The boss creature guarding a relic scales based on how many players are in the match and whether a relic is present. This prevents large teams from trivially overwhelming the guardian.

```
guardianHealth = baseHealth × (1 + (playerCount × 0.15))
guardianDamage = baseDamage × (1 + (playerCount × 0.10))

Solo modifier (no team): guardianHealth × 1.5, guardianDamage × 1.25
```

### Guardian Health by Player Count

| Players in Match | Guardian Health Multiplier | Guardian Damage Multiplier |
|---|---|---|
| 1 (solo) | 1.5× base | 1.25× base |
| 2 | 1.3× base | 1.2× base |
| 4 | 1.6× base | 1.4× base |
| 6 | 1.9× base | 1.6× base |
| 8 | 2.2× base | 1.8× base |
| 10 | 2.5× base | 2.0× base |
| 12+ | 2.8× base | 2.2× base |

A solo player faces a weaker guardian than a 12-player lobby — but the solo player has no teammates to help kill it. The net difficulty is higher for solo, which is why elite gear is required.

---

## PART 5 — RELIC FAIRNESS RULES SUMMARY

| Scenario | Channel Duration | Who Gets Relic | Broadcast Behavior |
|---|---|---|---|
| Solo, no elite gear | 20 seconds | Solo player | Every 3 seconds |
| Solo, with elite gear | 14 seconds | Solo player | Once only |
| Team, 2 players | 10 seconds | Random of eligible | Once only |
| Team, 3–4 players | 10 seconds | Random of eligible | Once only |
| Team, 5+ players | 10 seconds | Random of eligible | Once only |
| Simulation Layer, solo | 25 seconds | Solo player | Every 3 seconds + beacon moves |
| Simulation Layer, team | 25 seconds | Random of eligible | Every 5 seconds + beacon moves |

---

## PART 6 — ANTI-GAMING RULES

Situations players will try to exploit and how the system handles them:

| Exploit Attempt | Prevention |
|---|---|
| Team member leaves team right before extraction to claim relic solo | Relic ownership is locked to team at the moment the relic is picked up — leaving the team after pickup doesn't change the roll |
| Player kills their own teammate to reduce the eligible pool | Friendly fire on a teammate within 20m of beacon during channel flags the killer — they are excluded from the relic roll |
| Solo player joins a team of 1 just to get team extraction rules | Team must have 2+ members present at beacon to use team extraction rules |
| Player disconnects after winning the relic roll but before lobby | Relic is saved to AWS at the moment the roll completes, before the player returns to lobby — disconnect after roll doesn't lose the relic |
| Team pre-agrees to trade the relic after extraction | Cannot be prevented — but the randomization means the "wrong" person might win the roll, and in-game trading is not implemented (relics are account-bound) |

---

## Scripts Required

| Script | Responsibility |
|---|---|
| `RelicExtractionManager.cs` | Relic-specific extraction rules, team roll logic, solo modifier application |
| `GearLoadout.cs` | Player gear slot management, gear consumption on use/death |
| `GearEffectHandler.cs` | Applies gear effects during extraction channel (damage slow, health boost, etc.) |
| `RelicGuardianScaler.cs` | Scales guardian health/damage based on player count and solo status |
