# Game Design Document — Treasure Royale

## Concept
Treasure Royale is a multiplayer FPS where players enter a Multiverse Transporter, select an Earth timeline, and compete to loot rare ancient artifacts before other players or teams do.

## Core Loop
1. Players queue in the lobby and enter the **Multiverse Transporter**
2. A timeline date is selected — resolved to the nearest historical anchor bucket
3. Players drop into a safe spawn point away from artifacts, creatures, and other players
4. Players loot, fight, and collect **Artifacts**
5. To extract, players must carry artifacts to the single **Extraction Beacon** and channel for 10 seconds
6. Extraction is broadcast to all players the moment it begins — the beacon is always a contested chokepoint
7. Successful extraction saves artifacts to AWS — they cannot be stolen post-extraction
8. Disconnecting with artifacts triggers a **full reset** — artifacts return to spawn with era boss creatures guarding them
9. Extracted artifacts can be used for power buffs or traded for in-game currency at the Vault

## Win Condition
- No last-man-standing requirement
- Score is based on **artifact value collected**, not kills
- Kills are a means to steal artifacts, not an end goal

## Player Progression
- Artifacts grant passive powers or can be sold
- **Chrono Shards** (free currency) earned from extractions, missions, and match performance
- **Void Coins** (premium currency) purchased with real money
- Both currencies spent at the **Vault** on upgrades, cosmetics, and timeline unlocks
- Upgrades raise the cross-era affinity floor on power items and mounts — never exclusive to paying players
- Rare artifacts from rare timelines have higher trade value
- Seasonal **Chrono Pass** offers cosmetics and Shard bonuses — no power upgrades on the paid track

## Game Modes
| Mode | Description |
|---|---|
| Solo Plunder | Free-for-all artifact hunt |
| Team Plunder | Squad-based, shared artifact pool |
| Timeline War | Two teams fight over a single high-value artifact |

## Differentiators vs Fortnite / The Finals
- No building mechanics
- Win condition is exploration + looting, not elimination
- Map variety driven by historical eras, not seasonal themes
- Multiverse narrative gives infinite content justification
