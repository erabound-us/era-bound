# AWS Architecture

## Overview
AWS handles everything that Photon doesn't: persistent player data, artifact economy, leaderboards, and content delivery.

## Services Map

```
┌─────────────────────────────────────────────────────┐
│                   Game Client (Unity)                │
└────────────┬────────────────────────┬────────────────┘
             │ REST / WebSocket        │ Asset download
             ▼                         ▼
┌────────────────────┐     ┌──────────────────────┐
│  API Gateway (HTTP)│     │   CloudFront + S3     │
│  + WebSocket API   │     │  (map assets, DLC)    │
└────────┬───────────┘     └──────────────────────┘
         │
         ▼
┌────────────────────┐
│   Lambda Functions │
│  (game logic APIs) │
└────┬───────────────┘
     │
     ├──► DynamoDB (player profiles, artifact inventory, currency)
     ├──► DynamoDB (artifact catalog, era definitions)
     ├──► ElastiCache Redis (leaderboards, active match state)
     └──► SNS / SQS (match events, notifications)
```

## DynamoDB Tables

### `players`
| PK | SK | Attributes |
|---|---|---|
| `PLAYER#<userId>` | `PROFILE` | username, currency, level |
| `PLAYER#<userId>` | `INVENTORY` | artifacts[] |
| `PLAYER#<userId>` | `STATS` | matches, kills, artifactsFound |

### `artifacts`
| PK | SK | Attributes |
|---|---|---|
| `ARTIFACT#<id>` | `META` | name, era, rarity, powerType, tradeValue |

### `matches`
| PK | SK | Attributes |
|---|---|---|
| `MATCH#<matchId>` | `META` | era, timelineDate, universeSeed, startTime |
| `MATCH#<matchId>` | `PLAYER#<id>` | score, artifactsCollected, kills |

## Lambda Functions
| Function | Trigger | Purpose |
|---|---|---|
| `getPlayerProfile` | API GW GET | Fetch player data |
| `updateInventory` | API GW POST | Add/remove artifacts post-match |
| `tradeArtifact` | API GW POST | Vault trade logic |
| `getLeaderboard` | API GW GET | Top players by era/global |
| `generateUniverseSeed` | API GW POST | Deterministic seed from era+date |
| `matchComplete` | SQS | Process match results, award currency |

## Cognito
- Player auth via Amazon Cognito User Pools
- JWT token passed in all API calls
- Guest play supported via Cognito Identity Pools (unauthenticated)

## S3 + CloudFront
- Era map asset bundles stored in S3
- CloudFront CDN for low-latency global delivery
- Unity AssetBundle.LoadFromCacheOrDownload() on client

## Cost Optimization
- Lambda on-demand — no idle cost
- DynamoDB on-demand capacity — scales with player count
- ElastiCache only for leaderboard hot path
- S3 Intelligent-Tiering for older era asset bundles
