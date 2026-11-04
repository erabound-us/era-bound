# Amazon Q Prompts — Treasure Royale

A curated prompt library for use with Amazon Q Developer in this project.
Copy any prompt into the Amazon Q chat or save as a `.md` file in `.amazonq/prompts/`.

---

## Unity — Multiverse Transporter UI

```
Build a Unity C# script called MultiverseTransporterUI.cs.
It should display a UI panel with:
- A dropdown to select an Era (Prehistoric, Ancient, Medieval, Renaissance, Industrial, Modern, Future)
- A randomized timeline date label within that era's date range
- A "Enter Multiverse" button that creates a Photon PUN2 room with custom properties: era (string), timelineDate (int), universeSeed (int)
- universeSeed should be generated from era + timelineDate using a deterministic hash
Use TextMeshPro for all UI text. Keep it minimal.
```

---

## Unity — Map Variant Manager

```
Build a Unity C# script called MapVariantManager.cs.
It reads universeSeed from PhotonNetwork.CurrentRoom.CustomProperties.
It uses Random.InitState(universeSeed) then:
- Picks one PropSet from a serialized array and activates it (disables others)
- Picks one LightingProfile ScriptableObject and applies it via RenderSettings
- Calls ArtifactSpawner.Spawn(universeSeed)
Run this in Start() after scene load. No coroutines needed.
```

---

## Unity — Artifact Spawner

```
Build a Unity C# script called ArtifactSpawner.cs.
Given an int universeSeed and a list of Transform spawnPoints:
- Use Random.InitState(universeSeed) to shuffle spawn points
- Pick N spawn points (N = configurable, default 10)
- Instantiate an Artifact prefab at each, assigning rarity based on weighted random:
  Common 50%, Rare 25%, Epic 15%, Legendary 8%, Mythic 2%
- Only the MasterClient should run this (check PhotonNetwork.IsMasterClient)
Sync artifact state via PhotonNetwork.Instantiate so all clients see them.
```

---

## Unity — Artifact Pickup & Inventory

```
Build two Unity C# scripts:
1. ArtifactPickup.cs — attached to artifact prefab. On trigger enter with player tag:
   - Call photonView.RPC("RPC_Claim", RpcTarget.AllBuffered, artifactId, playerId)
   - RPC_Claim disables the GameObject and adds artifact to PlayerInventory
2. PlayerInventory.cs — MonoBehaviour that holds List<ArtifactData> and int score.
   - AddArtifact(ArtifactData) adds to list and updates score by tradeValue
   - GetScore() returns total score
   - Serialize inventory to JSON string for sending to AWS on match end
Use Newtonsoft.Json (already in project) for serialization.
```

---

## AWS Lambda — Generate Universe Seed

```
Write a Python 3.12 AWS Lambda function called generate_universe_seed.
Input event: { "era": "Ancient", "timelineDate": -3200, "requestId": "uuid" }
Logic:
- Combine era + timelineDate + requestId into a deterministic SHA-256 hash
- Return the first 8 hex chars converted to int as the universeSeed
- Also return a human-readable timelineLabel like "Earth, 3200 BCE"
Output: { "universeSeed": int, "timelineLabel": string }
Keep it under 30 lines.
```

---

## AWS Lambda — Update Player Inventory

```
Write a Python 3.12 AWS Lambda function called update_inventory.
It receives a POST body: { "playerId": str, "matchId": str, "artifacts": [ { "id": str, "rarity": str, "tradeValue": int } ] }
It should:
- Validate the JWT from the Authorization header using Cognito (use boto3 cognito-idp get_user)
- Write each artifact to DynamoDB table "artifacts" with PK=ARTIFACT#<id>
- Update the player's inventory in table "players" PK=PLAYER#<playerId> SK=INVENTORY using a list_append update expression
- Add total tradeValue to player's currency attribute
Return 200 on success, 400/401 on error. Use boto3. Keep error handling minimal.
```

---

## AWS Lambda — Leaderboard

```
Write a Python 3.12 AWS Lambda function called get_leaderboard.
Query param: era (optional, filters by era) and limit (default 10).
- Read from ElastiCache Redis sorted set "leaderboard:<era>" or "leaderboard:global"
- Return top N players as [ { "rank": int, "playerId": str, "score": int } ]
- If Redis is cold, fall back to DynamoDB scan on "matches" table with a GSI on score
Use redis-py for Redis. Return JSON. Keep under 40 lines.
```

---

## AWS CDK — Core Infrastructure

```
Write an AWS CDK v2 Python stack called TreasureRoyaleStack.
Create:
- DynamoDB table "players" with PK=pk (S), SK=sk (S), on-demand billing
- DynamoDB table "artifacts" with PK=pk (S), SK=sk (S), on-demand billing
- DynamoDB table "matches" with PK=pk (S), SK=sk (S), on-demand billing
- S3 bucket "treasure-royale-assets" with versioning enabled, blocked public access
- CloudFront distribution pointing to the S3 bucket with OAC
- Cognito User Pool with email sign-in and a User Pool Client
Output the User Pool ID, Client ID, and CloudFront URL.
Keep it concise, no comments.
```

---

## AWS CDK — API Gateway + Lambdas

```
Extend the TreasureRoyaleStack CDK Python stack to add:
- HTTP API Gateway (api_gw.HttpApi)
- Lambda functions for: generate_universe_seed, update_inventory, get_leaderboard
  Each Lambda: Python 3.12, 256MB, 10s timeout, code from "lambda/<function_name>/" folder
- Cognito JWT authorizer on update_inventory route
- Routes:
  POST /seed → generate_universe_seed (no auth)
  POST /inventory → update_inventory (Cognito auth)
  GET /leaderboard → get_leaderboard (no auth)
Output the API endpoint URL.
```

---

## Debugging — Photon Room Properties Not Syncing

```
In my Unity Photon PUN2 game, custom room properties set with SetCustomProperties are not visible to late-joining clients.
My properties are: era (string), timelineDate (int), universeSeed (int).
Show me the correct way to:
1. Set these as custom properties visible in the lobby
2. Read them reliably in OnJoinedRoom on all clients
Use PUN2 API only.
```

---

## Debugging — AWS Lambda Cold Start

```
My AWS Lambda function update_inventory has cold start latency of 2-3 seconds which is too slow for post-match inventory saves.
The function uses Python 3.12, boto3, and connects to DynamoDB.
Suggest the minimal changes to reduce cold start to under 500ms.
Consider: provisioned concurrency trade-offs, connection reuse, package size.
```
