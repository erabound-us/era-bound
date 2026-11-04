# Artifact System

## Overview
Artifacts are the primary objective. They are era-appropriate objects with randomized stats, rarity, and optional power effects.

## Artifact Anatomy
```
Artifact {
  id: UUID
  name: string              // e.g. "Obsidian Blade of the 3rd Dynasty"
  era: EraEnum
  nativeEraTier: int        // 0–7, used by CrossEraPowerCalculator
  universeVariant: int      // slight visual/stat shift per universe
  rarity: Common | Rare | Epic | Legendary | Mythic
  powerType: None | Combat | Speed | Vision | Stealth | Mount
  powerMagnitude: float     // 0.0 – 1.0
  tradeValue: int           // in-game currency units
  mountType: string | null  // if powerType == Mount: "raptor", "pterodactyl", "hoverbike" etc.
  upgradeKey: string | null // key into player's upgrades map in DynamoDB
}
```

## Rarity Distribution per Match
| Rarity | Spawn % | Power |
|---|---|---|
| Common | 50% | None |
| Rare | 25% | Minor |
| Epic | 15% | Moderate |
| Legendary | 8% | Strong |
| Mythic | 2% | Game-changing |

## Randomization Rules
- Base artifact pool is era-specific (e.g. Ancient era = scrolls, blades, urns)
- `universeVariant` shifts name prefix/suffix and stat values ±15%
- Mythic artifacts are unique per match — only one spawns per session
- Artifact positions are seeded from `universeSeed` so they're consistent within a match but different across matches

## Power Effects (examples)
- **Combat**: +damage for X seconds after pickup
- **Speed**: Permanent sprint speed boost for the holder
- **Vision**: Reveals nearby players on minimap
- **Stealth**: Reduces footstep audio radius

## Economy
- Artifacts are carried in inventory and extracted at match end
- Dropped on death — other players can steal
- Traded at the **Vault** (lobby shop) for currency or power upgrades
