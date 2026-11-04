-- Treasure Royale — Cloudflare D1 Schema
-- Run: wrangler d1 execute TR_DB --file=schema.sql

-- ── Players ───────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS players (
    player_id     TEXT PRIMARY KEY,
    display_name  TEXT NOT NULL,
    chrono_coins  INTEGER NOT NULL DEFAULT 10000,
    created_at    INTEGER NOT NULL DEFAULT (unixepoch()),
    last_seen     INTEGER NOT NULL DEFAULT (unixepoch())
);

-- ── Item catalog (NPC shop — static, seeded at deploy) ───────────────────────
CREATE TABLE IF NOT EXISTS item_catalog (
    item_id       TEXT PRIMARY KEY,
    name          TEXT NOT NULL,
    category      TEXT NOT NULL,   -- weapon_light|weapon_medium|weapon_heavy|gear|consumable|cosmetic|upgrade|container|pet|vehicle
    subcategory   TEXT,            -- e.g. "diving"|"space"|"survival"|"explosive"
    slot_type     TEXT NOT NULL,   -- weapon|gear|consumable|artifact|cosmetic|container|pet|vehicle
    slot_size     INTEGER NOT NULL DEFAULT 1,
    shop_price    INTEGER NOT NULL DEFAULT 0,
    is_consumable INTEGER NOT NULL DEFAULT 0,  -- 1 = one-use
    max_durability INTEGER,        -- NULL = indestructible (legendaries, cosmetics)
    description   TEXT,
    era_tags      TEXT             -- JSON array of compatible eras e.g. '["ancient","medieval"]'
);

-- ── Player inventory ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS inventory (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    item_id       TEXT NOT NULL REFERENCES item_catalog(item_id),
    quantity      INTEGER NOT NULL DEFAULT 1,
    durability    INTEGER,         -- NULL = indestructible
    slot_index    INTEGER,         -- assigned hotbar/slot position
    acquired_at   INTEGER NOT NULL DEFAULT (unixepoch()),
    UNIQUE(player_id, item_id)
);

-- ── Valuables (looted items with dynamic value) ───────────────────────────────
CREATE TABLE IF NOT EXISTS valuables (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    item_id       TEXT NOT NULL,   -- references artifact catalog
    name          TEXT NOT NULL,
    era           TEXT NOT NULL,
    rarity        TEXT NOT NULL,   -- common|uncommon|rare|epic|legendary
    is_legendary  INTEGER NOT NULL DEFAULT 0,
    historical_figure TEXT,        -- e.g. "Athena" — drives price multiplier
    base_value    INTEGER NOT NULL,
    passive_ability TEXT,          -- JSON: {type, activationChance, description}
    acquired_at   INTEGER NOT NULL DEFAULT (unixepoch())
);

-- ── Containers (backpacks, rucksacks, cargo boxes) ───────────────────────────
CREATE TABLE IF NOT EXISTS containers (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    container_id  TEXT NOT NULL,   -- references item_catalog
    extra_slots   INTEGER NOT NULL,
    is_equipped   INTEGER NOT NULL DEFAULT 0
);

-- ── Pets ──────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS pets (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    pet_type      TEXT NOT NULL,   -- raptor|wolf|drone|horse etc.
    name          TEXT NOT NULL DEFAULT 'Companion',
    extra_slots   INTEGER NOT NULL DEFAULT 4,
    is_active     INTEGER NOT NULL DEFAULT 0,
    skin_id       TEXT
);

-- ── Vehicles ──────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS vehicles (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    vehicle_type  TEXT NOT NULL,
    extra_slots   INTEGER NOT NULL DEFAULT 8,
    is_active     INTEGER NOT NULL DEFAULT 0,
    skin_id       TEXT
);

-- ── Cosmetics ─────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS cosmetics (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    cosmetic_id   TEXT NOT NULL,
    cosmetic_type TEXT NOT NULL,   -- outfit|weapon_skin|emote
    is_equipped   INTEGER NOT NULL DEFAULT 0,
    slot          TEXT             -- outfit_head|outfit_body|outfit_legs|weapon_primary etc.
);

-- ── Upgrades (permanent character upgrades) ───────────────────────────────────
CREATE TABLE IF NOT EXISTS upgrades (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL REFERENCES players(player_id),
    upgrade_id    TEXT NOT NULL,   -- carry_capacity|extraction_speed|loot_radius|etc.
    level         INTEGER NOT NULL DEFAULT 1,
    purchased_at  INTEGER NOT NULL DEFAULT (unixepoch()),
    UNIQUE(player_id, upgrade_id)
);

-- ── Auction listings ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS auction_listings (
    listing_id    TEXT PRIMARY KEY,  -- UUID
    seller_id     TEXT NOT NULL REFERENCES players(player_id),
    item_type     TEXT NOT NULL,     -- inventory|valuable
    item_ref_id   INTEGER NOT NULL,  -- FK to inventory.id or valuables.id
    item_snapshot TEXT NOT NULL,     -- JSON snapshot of item at listing time
    asking_price  INTEGER NOT NULL,
    is_private    INTEGER NOT NULL DEFAULT 0,
    allowed_buyers TEXT,             -- JSON array of player_ids for private listings
    status        TEXT NOT NULL DEFAULT 'active',  -- active|sold|cancelled|expired
    listing_fee   INTEGER NOT NULL DEFAULT 0,
    created_at    INTEGER NOT NULL DEFAULT (unixepoch()),
    expires_at    INTEGER NOT NULL   -- unixepoch + 7 days default
);

-- ── Auction bids / purchases ──────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS auction_transactions (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    listing_id    TEXT NOT NULL REFERENCES auction_listings(listing_id),
    buyer_id      TEXT NOT NULL REFERENCES players(player_id),
    price_paid    INTEGER NOT NULL,
    fee_charged   INTEGER NOT NULL,
    completed_at  INTEGER NOT NULL DEFAULT (unixepoch())
);

-- ── Repair log ────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS repair_log (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    player_id     TEXT NOT NULL,
    inventory_id  INTEGER NOT NULL,
    cost          INTEGER NOT NULL,
    repaired_at   INTEGER NOT NULL DEFAULT (unixepoch())
);

-- ── Indexes ───────────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_inventory_player   ON inventory(player_id);
CREATE INDEX IF NOT EXISTS idx_valuables_player   ON valuables(player_id);
CREATE INDEX IF NOT EXISTS idx_auction_status     ON auction_listings(status);
CREATE INDEX IF NOT EXISTS idx_auction_seller     ON auction_listings(seller_id);
CREATE INDEX IF NOT EXISTS idx_cosmetics_player   ON cosmetics(player_id);
CREATE INDEX IF NOT EXISTS idx_upgrades_player    ON upgrades(player_id);
