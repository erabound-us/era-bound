# Extraction Beacon System

## Design Intent

Picking up an artifact is only half the challenge. Getting it home is the other half. The **Extraction Beacon** is the single point on the map where players can invoke a return transport. It creates a natural endgame chokepoint — everyone knows where it is, everyone knows when someone is using it, and the most dangerous moment in any match is the 10 seconds you stand still activating it.

Rage-quitting with loot is punished hard: disconnect with an artifact and it resets to its original spawn with a fresh wave of bosses and creatures guarding it.

---

## Core Rules

1. **One beacon per map** — single fixed location, visible on the minimap to all players from match start
2. **Artifact must be in inventory** — you cannot extract empty-handed (no griefing the beacon)
3. **Extraction is a 10-second channel** — player must stand still and hold the interact key, cancels if they move or take damage
4. **Beacon broadcasts to all players** — the moment extraction begins, every player on the map gets an audio + visual alert with the extractor's location
5. **Successful extraction = artifacts locked in** — they are saved to AWS inventory, cannot be stolen post-extraction
6. **Disconnect with artifacts = full reset** — artifacts return to spawn, new boss creatures spawn on top of them

---

## Extraction States

```
IDLE         → beacon is available, glowing softly
CHANNELING   → a player is extracting, beacon pulses rapidly, all players alerted
COOLDOWN     → 30s after a successful extraction, beacon recharges (prevents chain extractions)
CONTESTED    → another player is within 10m of an extracting player (channel pauses)
RESET        → triggered by disconnect-with-artifact, beacon resets all dropped artifacts
```

---

## The Extraction Flow

```
Player approaches beacon with artifact(s) in inventory
  → Prompt appears: "Hold [E] to invoke return transport"
  → Player holds E → channel begins (10 seconds)
  → Beacon pulses, ALL players on map receive:
       - Audio alert: era-appropriate alarm sound
       - Minimap ping: beacon location flashes red
       - HUD message: "[PlayerName] is extracting at the Beacon!"
  → If player moves > 1m during channel → channel cancels, 5s cooldown before retry
  → If player takes any damage during channel → channel cancels, 5s cooldown before retry
  → If enemy player enters 10m radius → CONTESTED state, channel pauses (not cancelled)
       - Channel resumes when contested player leaves or is killed
  → Channel completes → extraction animation plays (2s)
  → Artifacts saved to AWS via POST /inventory
  → Player transported back to lobby
  → Beacon enters 30s COOLDOWN for remaining players
```

---

## Contested Extraction

The contested state is the most important design decision here. Rather than cancelling outright when an enemy is nearby, the channel **pauses**. This means:

- The extracting player has incentive to fight back or hide — they don't lose all progress
- The contesting player has incentive to stay close — they don't need to kill, just delay
- Creates standoffs, flanking plays, and last-second rescues in team modes
- Progress bar is visible to both players — psychological pressure on both sides

```
Channel progress: [████████░░] 80% complete
Enemy enters 10m radius
  → Progress bar freezes, turns orange
  → HUD: "Extraction contested — eliminate threat or wait them out"
  → If enemy leaves 10m radius → channel resumes from where it paused
  → If extracting player dies → channel cancels, artifacts drop at death location
  → If contesting player dies → channel resumes
```

---

## Disconnect Penalty — The Reset

This is the anti-rage-quit mechanic. When a player disconnects (or force-quits) while carrying artifacts:

```
OnPhotonPlayerDisconnected fires on MasterClient
  → Check disconnected player's inventory for artifacts
  → For each artifact in their inventory:
       1. Teleport artifact back to its original spawn position
       2. Mark artifact as "reset" — rarity bumped up one tier (reward for reclaiming)
       3. Spawn boss creature at artifact location (era-appropriate)
       4. Broadcast to all remaining players: "[PlayerName] abandoned the timeline — artifacts have reset"
  → Minimap updates: artifact icons reappear at original locations
  → Boss creature icons appear on minimap for 5 seconds (warning)
```

### Rarity Bump on Reset
When an artifact resets it gets promoted one rarity tier — this is the reward for the player who goes back to claim it:

| Original Rarity | Reset Rarity |
|---|---|
| Common | Rare |
| Rare | Epic |
| Epic | Legendary |
| Legendary | Mythic |
| Mythic | Mythic + double trade value |

### Boss Creatures by Era

| Era | Reset Boss Spawned |
|---|---|
| Deep Prehistoric (Cretaceous) | Alpha T-Rex — larger, faster, higher aggro range than standard |
| Deep Prehistoric (Ice Age) | Smilodon Prime — stalks players, doesn't charge until within 5m |
| Deep Prehistoric (Volcanic) | Volcanic Dilophosaurus — spits fire, patrols in a tight circle around artifact |
| Prehistoric | Giant Cave Bear — territorial, won't leave a 20m radius of the artifact |
| Ancient | Tomb Guardian (armored NPC) — carries a weapon, patrols artifact, attacks on sight |
| Medieval | Berserker Knight (NPC) — full armor, charges on sight, high health |
| Renaissance | Inquisitor Squad (3 NPCs) — ranged crossbows, spread formation around artifact |
| Industrial | Automaton Sentinel (mechanical NPC) — slow but high damage, never loses aggro |
| Modern | Elite Soldier (NPC) — uses cover, flanks, throws grenades |

---

## Beacon Placement Rules

One beacon per map, placed by the level designer. Placement philosophy:

- **Central but not central** — close enough to all zones that it's reachable, but not so central that it's always contested from spawn
- **Open area** — no immediate cover at the beacon itself, forcing the extractor to clear the area first or accept vulnerability
- **Memorable landmark** — the beacon should be attached to the most visually striking object on the map so players always know where it is intuitively

### Era-Appropriate Beacon Appearance

The beacon is always the same mechanic but looks different per era:

| Era | Beacon Object | Activation Visual |
|---|---|---|
| Deep Prehistoric | Glowing meteor fragment embedded in rock | Amber energy pillar shoots skyward |
| Prehistoric | Standing stone with carved spiral | Blue lightning arcs between stones |
| Ancient | Obelisk with glowing hieroglyphs | Golden beam rises to sky |
| Medieval | Ruined altar with floating runes | Purple mist swirls upward |
| Renaissance | Brass orrery spinning rapidly | Clockwork light spiral |
| Industrial | Tesla coil crackling with energy | Electric column |
| Modern | Holographic portal frame | Digital shimmer column |

---

## Team Mode Extraction Rules

In **Team Plunder** mode the extraction rules change slightly:

- Any team member can activate the beacon for the whole team
- All team members within 20m of the beacon at extraction time have their artifacts saved
- Team members outside 20m at extraction time **do not** get their artifacts saved — they must reach the beacon before the channel completes
- This creates a "rally at the beacon" dynamic — teams must coordinate the extraction, not just one player sneaking out

```
Team extraction flow:
  Player A begins channel at beacon
  → All teammates get HUD alert: "Extraction initiated — reach the beacon!"
  → 10-second channel begins
  → At completion: all teammates within 20m extract together
  → Teammates outside 20m: artifacts stay in their inventory, they must wait for beacon cooldown
```

---

## Minimap & HUD Integration

| Event | Minimap | HUD |
|---|---|---|
| Match start | Beacon shown as gold icon | "Extraction Beacon located — collect artifacts to extract" |
| Extraction begins | Beacon flashes red, extractor name shown | "[Name] is extracting! — X seconds remaining" |
| Extraction contested | Beacon icon turns orange | "Extraction contested at beacon" |
| Extraction complete | Beacon grays out (cooldown) | "[Name] extracted successfully" |
| Artifact reset | Artifact icons reappear, boss icon flashes | "[Name] abandoned the timeline — artifacts reset, bosses spawned" |
| Beacon cooldown ends | Beacon returns to gold | "Extraction Beacon is available" |

---

## Scripts Required

| Script | Responsibility |
|---|---|
| `ExtractionBeacon.cs` | Channel logic, contested state, cooldown, RPC broadcasts |
| `ExtractionChannel.cs` | Client-side progress bar UI, input handling, cancel conditions |
| `ArtifactResetManager.cs` | MasterClient disconnect handler, artifact teleport, boss spawn |
| `BossSpawner.cs` | Era-appropriate boss instantiation at artifact reset positions |

---

## Anti-Exploit Notes

- **Server-side inventory lock** — artifacts are only written to AWS on successful extraction RPC, never on pickup. A client cannot fake an extraction.
- **MasterClient validates extraction** — the channel completion RPC is sent to MasterClient first, which validates the player was stationary, had artifacts, and wasn't in a contested state before confirming to AWS.
- **Disconnect detection** — Photon's `OnPlayerLeftRoom` fires within ~10 seconds of a hard disconnect. Artifacts held by that player reset immediately on MasterClient.
- **Alt-F4 / force quit** — same as disconnect. Photon detects the dropped connection. The 10-second Photon timeout means there's a brief window, but artifacts are never saved to AWS until extraction completes — so a force quit always results in a reset.
