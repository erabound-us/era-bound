# [CLASSIFIED] Simulation Layer — Secret Timeline Design

> This document is internal only. The Simulation Layer is never described to players anywhere in the UI, wiki, or marketing. Discovery is intentional — word of mouth and community speculation are the marketing.

---

## The Secret

The Simulation Layer is not listed as a Deep Future variant in the UI. The Deep Future picker shows four variants:
- Post-Human Earth
- Dyson Sphere Interior  
- Galactic Hub
- Abandoned Earth

The fifth slot shows nothing. No label. No description. Just a faint visual glitch on the selection screen — a single frame flicker — that players may or may not notice.

Players who pick a year beyond 3000 CE have a **seed-dependent chance** of landing in the Simulation Layer instead of the four visible variants. They cannot choose it directly. The seed rolls it silently.

```
universeSeed % 100 < 8  →  Simulation Layer  (8% chance per room)
universeSeed % 100 >= 8 →  one of the four visible variants
```

8% means it's rare enough to feel special but common enough that communities will find it within weeks of launch.

---

## What Players See When They Arrive

No loading screen label. The HUD era display reads:

```
"Earth — [REDACTED]"
```

The arrival effect is broken — the normal materialization particle plays for half a second then cuts to static. The player spawns standing normally but the world around them looks wrong immediately:

- Sky is a perfect flat grey — no clouds, no sun, no stars, just uniform light
- Shadows point in inconsistent directions
- Some props repeat in perfect grid patterns in the distance
- Distant geometry clips through itself
- Ambient audio is normal era sounds but with occasional digital artifacts — a skip, a repeat, a wrong sound playing briefly
- The minimap renders correctly but occasionally shows a second copy of the map offset by 200m

No tutorial prompt. No explanation. The game just starts.

---

## Core Mechanics — How It Differs From Every Other Timeline

### 1. The Beacon Moves

The Extraction Beacon is not fixed. Every 90 seconds it relocates to a random valid position on the map. Players who found it must find it again. The minimap icon updates but with a 10-second delay — by the time the icon updates, the beacon has already been at the new location for 10 seconds.

```
BeaconRelocateInterval = 90s
MinimapUpdateDelay = 10s
Relocation is silent — no audio cue, no visual flash at old location
New location: random DropZone marker that passes standard safety checks
```

### 2. Physics Glitches — Reality Tears

Every 45–120 seconds (random interval) a **Reality Tear** event fires at a random map location. Any player within 15m of the tear is teleported to a random position on the map. They keep their inventory. They keep their health. They just appear somewhere else.

- No warning
- No damage
- Completely disorienting
- The tear location is visible as a brief visual distortion for 3 seconds after it fires — players learn to watch for them

```
TearInterval = Random(45, 120) seconds
TearRadius = 15m
TearCount per event = 1–3 simultaneous tears
Teleport destination = random valid NavMesh position
```

### 3. Artifact Duplication — The Trap

This is the mechanic that will define the Simulation Layer's reputation. Artifacts in this timeline can **duplicate** — a copy of the artifact appears in the world near the original. The copy looks identical. Same name, same rarity glow, same visual model.

The copy has a hidden `isSimulacrum` flag that is **never shown to the player**.

```
Duplication trigger:
  Any artifact that has been on the ground (not held) for > 60 seconds
  has a 30% chance per minute to spawn a simulacrum within 10m of itself
  
Maximum simulacra per match: 5 (prevents map flooding)
Simulacrum appears with identical visual to original
```

#### What happens at extraction with a simulacrum

When a player attempts to extract with a simulacrum in their inventory:

```
Extraction channel completes normally
  → MasterClient checks each artifact: isSimulacrum?
  → If simulacrum present:
       - 60% chance: simulacrum dissolves silently
           → Player loses that artifact, keeps all real ones
           → No warning, no message — it just isn't in their post-match inventory
       - 40% chance: simulacrum "collapses" 
           → Collapses ALL artifacts in inventory — real and fake
           → Player extracts with nothing
           → HUD message (post-extraction, in lobby): 
             "Simulation integrity failure — cargo lost in transit"
```

The 40% collapse-all outcome is the trap. A player who grabs every artifact they can find — real and simulacra — is gambling their entire run. A careful player who identifies and avoids simulacra extracts safely. But there's no in-game tool to identify them — only observation and deduction.

#### How players can identify simulacra (no UI assistance)

These are the only tells — all subtle, all requiring attention:

| Tell | Description |
|---|---|
| Shadow direction | Simulacra cast shadows in the wrong direction — inconsistent with the flat grey sky |
| Weight sound | Picking up a real artifact plays a solid thud. Simulacra play the same sound but 2 frames late |
| Inventory glow | Real artifacts glow steadily in inventory. Simulacra pulse very slightly — barely visible |
| Age | Simulacra never existed before the match — they have no "dust" particle effect that real artifacts spawn with on first discovery |
| Proximity flicker | If two artifacts of the same type are within 5m of each other, one is almost certainly a simulacrum |

None of these are documented anywhere in the game. Players discover them through loss, observation, and community sharing.

---

## Extraction — Near Impossible By Design

Standard extraction is already tense. In the Simulation Layer it's a gauntlet:

### Modified Extraction Rules

| Rule | Standard Timeline | Simulation Layer |
|---|---|---|
| Channel duration | 10 seconds | 25 seconds |
| Beacon location | Fixed | Relocates every 90s |
| Damage cancels channel | Yes | Yes |
| Movement cancels channel | Yes, >1m | Yes, >0.5m (tighter) |
| Contest radius | 10m | 15m |
| Beacon cooldown after success | 30s | 120s |
| Channel reset on glitch | N/A | Reality Tear within 15m resets channel to 0 |
| Simulacrum collapse risk | N/A | Active at extraction |

### The Extraction Sequence (Simulation Layer specific)

```
Player finds beacon (it may have moved since they last checked)
  → Approaches beacon — prompt appears: "Hold [E] — WARNING: Simulation integrity unstable"
  → Channel begins: 25 seconds
  → Every 5 seconds: 15% chance a Reality Tear fires within 30m of beacon
       → If tear fires within 15m of player: channel resets to 0
  → If player moves >0.5m: channel cancels
  → If beacon relocates mid-channel (90s timer): channel cancels, beacon gone
  → If contested: channel pauses (same as standard)
  → On completion: simulacrum check fires
       → All simulacra in inventory: 60% dissolve silently / 40% collapse all
  → Successful extraction: player returns to lobby
```

A full 25-second channel with no interruptions, no tears nearby, no enemies, and no simulacra in inventory is genuinely difficult. Most players will fail their first several attempts. That's intentional.

---

## Rewards — Why It's Worth It

The difficulty is justified by the reward. Artifacts extracted from the Simulation Layer have the highest trade values in the game and a unique property no other era has:

### Simulation Layer Exclusive Artifacts

| Name | Rarity | Power | Trade Value | Special Property |
|---|---|---|---|---|
| Glitch Fragment | Common | None | 200 | Sells for 4× standard Common value |
| Echo Object | Rare | Vision — reveals simulacra in inventory with a faint pulse | 500 | Only way to detect simulacra mid-match |
| Null Pointer | Epic | Stealth — makes player invisible to Reality Tear targeting | 800 | Tears never fire within 15m of holder |
| Reality Anchor | Legendary | Fixes beacon — beacon cannot relocate for 3 minutes after pickup | 2,000 | Most sought-after item in the game |
| The Source Code | Mythic | On extraction: duplicates ALL extracted artifacts — doubles the entire haul | 10,000 | One per match. If holder has simulacra, collapse chance applies to doubled haul |

The **Reality Anchor** is the item that will drive the Simulation Layer's economy. It makes extraction viable. Players will fight over it. Its trade value reflects that — 2,000 Chrono Shards equivalent, the highest Legendary value in the game.

The **Source Code** is the ultimate gamble. Double your haul — but if you picked up any simulacra, the collapse risk applies to twice as many artifacts. High risk, legendary reward.

---

## What Players Are NOT Told

The following is never communicated anywhere in the game:

- That the Simulation Layer exists
- That it has an 8% spawn chance
- That artifacts can be simulacra
- That simulacra have a collapse risk at extraction
- That the beacon moves
- That Reality Tears can reset the channel
- That the Echo Object detects simulacra
- That the Reality Anchor stops beacon relocation

All of this is discovered through play, loss, and community knowledge sharing. The first player to successfully extract from the Simulation Layer and post about it will generate more organic marketing than any trailer.

---

## Community Discovery Timeline (Predicted)

| Week | Expected Discovery |
|---|---|
| Week 1 | Players notice "Earth — [REDACTED]" in Deep Future. Screenshots shared. |
| Week 2 | First successful extraction. Player notices missing artifacts post-match. Confusion. |
| Week 3 | Community identifies simulacra concept. First guide attempts. |
| Week 4 | Shadow direction tell discovered. Echo Object identified as simulacrum detector. |
| Week 6 | Reality Anchor identified as beacon stopper. Price spikes in Vault trading. |
| Week 8 | First coordinated team extraction of The Source Code. Clip goes viral. |

---

## Implementation Notes

- `isSimulacrum` flag is server-side only — never sent to clients until extraction validation
- Simulacrum visual is identical client-side — the flag lives only in MasterClient's artifact registry
- Reality Tear positions are MasterClient-generated, broadcast via RPC 0.5s before firing (visual distortion warning)
- Beacon relocation uses same `TransporterDropZone` validation as player spawns — beacon never relocates into a wall or off the map
- The channel reset on tear proximity is handled in `ExtractionBeacon.cs` — add `SimulationLayerMode` bool that enables the extended rules when era == `simulation_layer`
