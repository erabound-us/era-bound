# Treasure Royale Hub — Cloudflare Deploy Guide

## Prerequisites
- Wrangler CLI installed: `npm install -g wrangler`
- Logged in: `wrangler login`

## 1. Create D1 Database
```
wrangler d1 create treasure-royale
```
Copy the `database_id` from the output into `wrangler.toml`.

## 2. Run Schema + Seed
```
wrangler d1 execute TR_DB --file=schema.sql
wrangler d1 execute TR_DB --file=seed_items.sql
```

## 3. Set Secrets
```
wrangler secret put GROQ_API_KEY
```
Paste your Groq API key (same one from Thai4Family works fine).

## 4. Update wrangler.toml
- Replace `YOUR_ACCOUNT_ID` with your Cloudflare account ID
- Replace `REPLACE_AFTER_CREATION` with the D1 database_id from step 1

## 5. Deploy
```
wrangler deploy
```
Your worker URL will be: `https://treasure-royale-hub.YOUR_SUBDOMAIN.workers.dev`

## 6. Update Unity
In the Unity Inspector, set `TR_HubApi.workerBaseUrl` to your worker URL.

## Tailscale Local Dev
To test locally before deploying:
```
wrangler dev --local
```
This runs on `http://localhost:8787` — point Unity at that during dev.

## Endpoints Reference
| Method | Path | Description |
|---|---|---|
| GET | /player/:id | Profile + coin balance |
| GET | /player/:id/inventory | Full inventory |
| POST | /player/:id/buy | Buy from NPC shop |
| GET | /player/:id/repair-estimate | Repair cost preview |
| POST | /player/:id/repair | Repair item |
| POST | /player/:id/upgrade | Buy permanent upgrade |
| GET | /shop/catalog?era= | NPC catalog (optional era filter) |
| GET | /auction/listings?player_id= | Active listings |
| POST | /auction/list | Create listing |
| POST | /auction/buy/:id | Purchase listing |
| POST | /auction/cancel/:id | Cancel listing |
| POST | /auction/sell-valuable | Instant sell to fence |
| GET | /valuables/estimate | Price estimate |
| GET | /player/:id/cosmetics | Owned cosmetics |
| POST | /player/:id/equip-cosmetic | Equip/unequip cosmetic |
| POST | /player/:id/equip-container | Equip backpack |
| POST | /player/:id/equip-pet | Set active pet |
| POST | /player/:id/equip-vehicle | Set active vehicle |
| POST | /guide | Era AI chatbot |
