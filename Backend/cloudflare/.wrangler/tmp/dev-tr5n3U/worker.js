var __defProp = Object.defineProperty;
var __name = (target, value) => __defProp(target, "name", { value, configurable: true });

// .wrangler/tmp/bundle-xpB7Qo/checked-fetch.js
var urls = /* @__PURE__ */ new Set();
function checkURL(request, init) {
  const url = request instanceof URL ? request : new URL(
    (typeof request === "string" ? new Request(request, init) : request).url
  );
  if (url.port && url.port !== "443" && url.protocol === "https:") {
    if (!urls.has(url.toString())) {
      urls.add(url.toString());
      console.warn(
        `WARNING: known issue with \`fetch()\` requests to custom HTTPS ports in published Workers:
 - ${url.toString()} - the custom port will be ignored when the Worker is published using the \`wrangler deploy\` command.
`
      );
    }
  }
}
__name(checkURL, "checkURL");
globalThis.fetch = new Proxy(globalThis.fetch, {
  apply(target, thisArg, argArray) {
    const [request, init] = argArray;
    checkURL(request, init);
    return Reflect.apply(target, thisArg, argArray);
  }
});

// worker.js
var CORS = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type, Authorization"
};
var LISTING_FEE_RATE = 0.03;
var LISTING_FEE_CAP = 25e3;
var LISTING_TTL_SECS = 60 * 60 * 24 * 7;
var BASE_SLOTS = 16;
var ERA_MULT = {
  deep_prehistoric: 4,
  prehistoric: 3.5,
  ancient: 3,
  medieval: 2.5,
  renaissance: 2.2,
  industrial: 1.8,
  modern: 1.2,
  near_future: 1,
  mid_future: 0.9,
  far_future: 0.8,
  deep_future: 0.7
};
var RARITY_MULT = { common: 1, uncommon: 2, rare: 5, epic: 12, legendary: 30 };
var HISTORICAL_FIGURE_MULT = 3.5;
var worker_default = {
  async fetch(request, env) {
    if (request.method === "OPTIONS") return new Response(null, { headers: CORS });
    const url = new URL(request.url);
    const path = url.pathname;
    try {
      if (path.match(/^\/player\/[^/]+$/) && request.method === "GET")
        return getPlayer(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/inventory$/) && request.method === "GET")
        return getInventory(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/buy$/) && request.method === "POST")
        return buyFromShop(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/repair$/) && request.method === "POST")
        return repairItem(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/repair-estimate$/) && request.method === "GET")
        return repairEstimate(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/upgrade$/) && request.method === "POST")
        return buyUpgrade(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/cosmetics$/) && request.method === "GET")
        return getCosmetics(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/equip-cosmetic$/) && request.method === "POST")
        return equipCosmetic(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/equip-container$/) && request.method === "POST")
        return equipContainer(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/equip-pet$/) && request.method === "POST")
        return equipPet(request, env, playerId(path));
      if (path.match(/^\/player\/[^/]+\/equip-vehicle$/) && request.method === "POST")
        return equipVehicle(request, env, playerId(path));
      if (path === "/shop/catalog" && request.method === "GET")
        return getShopCatalog(request, env);
      if (path === "/auction/listings" && request.method === "GET")
        return getAuctionListings(request, env);
      if (path === "/auction/list" && request.method === "POST")
        return createListing(request, env);
      if (path.match(/^\/auction\/buy\/[^/]+$/) && request.method === "POST")
        return buyListing(request, env, path.split("/")[3]);
      if (path.match(/^\/auction\/cancel\/[^/]+$/) && request.method === "POST")
        return cancelListing(request, env, path.split("/")[3]);
      if (path === "/auction/sell-valuable" && request.method === "POST")
        return sellValuable(request, env);
      if (path === "/valuables/estimate" && request.method === "GET")
        return valuableEstimate(request, env);
      if (path === "/health")
        return json({ ok: true });
      if (path === "/guide" && request.method === "POST")
        return eraGuide(request, env);
      return json({ error: "Not found" }, 404);
    } catch (e) {
      console.error(e);
      return json({ error: e.message }, 500);
    }
  }
};
async function getPlayer(req, env, pid) {
  let player = await env.TR_DB.prepare(
    "SELECT * FROM players WHERE player_id = ?"
  ).bind(pid).first();
  if (!player) {
    await env.TR_DB.prepare(
      "INSERT INTO players (player_id, display_name) VALUES (?, ?)"
    ).bind(pid, pid).run();
    player = { player_id: pid, display_name: pid, chrono_coins: 1e4 };
  }
  return json(player);
}
__name(getPlayer, "getPlayer");
async function getInventory(req, env, pid) {
  const [gear, valuables, containers, pets, vehicles, upgrades] = await Promise.all([
    env.TR_DB.prepare(`
      SELECT i.*, c.name, c.category, c.subcategory, c.slot_type, c.slot_size,
             c.max_durability, c.is_consumable, c.description, c.era_tags
      FROM inventory i JOIN item_catalog c ON i.item_id = c.item_id
      WHERE i.player_id = ?
    `).bind(pid).all(),
    env.TR_DB.prepare("SELECT * FROM valuables WHERE player_id = ?").bind(pid).all(),
    env.TR_DB.prepare(`
      SELECT ct.*, c.name, c.description
      FROM containers ct JOIN item_catalog c ON ct.container_id = c.item_id
      WHERE ct.player_id = ?
    `).bind(pid).all(),
    env.TR_DB.prepare("SELECT * FROM pets WHERE player_id = ?").bind(pid).all(),
    env.TR_DB.prepare("SELECT * FROM vehicles WHERE player_id = ?").bind(pid).all(),
    env.TR_DB.prepare("SELECT * FROM upgrades WHERE player_id = ?").bind(pid).all()
  ]);
  const equippedContainer = containers.results?.find((c) => c.is_equipped);
  const activePet = pets.results?.find((p) => p.is_active);
  const activeVehicle = vehicles.results?.find((v) => v.is_active);
  const carryUpgrade = upgrades.results?.reduce((sum, u) => {
    if (u.upgrade_id === "upg_carry_1") return sum + 4;
    if (u.upgrade_id === "upg_carry_2") return sum + 8;
    if (u.upgrade_id === "upg_carry_3") return sum + 12;
    return sum;
  }, 0) ?? 0;
  const totalSlots = BASE_SLOTS + carryUpgrade + (equippedContainer?.extra_slots ?? 0) + (activePet?.extra_slots ?? 0) + (activeVehicle?.extra_slots ?? 0);
  return json({
    gear: gear.results ?? [],
    valuables: valuables.results ?? [],
    containers: containers.results ?? [],
    pets: pets.results ?? [],
    vehicles: vehicles.results ?? [],
    upgrades: upgrades.results ?? [],
    totalSlots,
    usedSlots: (gear.results ?? []).reduce((s, i) => s + (i.slot_size ?? 1), 0)
  });
}
__name(getInventory, "getInventory");
async function getShopCatalog(req, env) {
  const era = new URL(req.url).searchParams.get("era");
  let items;
  if (era) {
    items = await env.TR_DB.prepare(
      "SELECT * FROM item_catalog WHERE era_tags IS NULL OR era_tags LIKE ? ORDER BY category, shop_price"
    ).bind(`%"${era}"%`).all();
  } else {
    items = await env.TR_DB.prepare(
      "SELECT * FROM item_catalog ORDER BY category, shop_price"
    ).all();
  }
  return json(items.results ?? []);
}
__name(getShopCatalog, "getShopCatalog");
async function buyFromShop(req, env, pid) {
  const { item_id, quantity = 1 } = await req.json();
  if (!item_id) return json({ error: "item_id required" }, 400);
  const [item, player] = await Promise.all([
    env.TR_DB.prepare("SELECT * FROM item_catalog WHERE item_id = ?").bind(item_id).first(),
    env.TR_DB.prepare("SELECT * FROM players WHERE player_id = ?").bind(pid).first()
  ]);
  if (!item) return json({ error: "Item not found" }, 404);
  if (!player) return json({ error: "Player not found" }, 404);
  const total = item.shop_price * quantity;
  if (player.chrono_coins < total)
    return json({ error: "Insufficient Chrono-Coins", needed: total, have: player.chrono_coins }, 402);
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins - ? WHERE player_id = ?").bind(total, pid),
    env.TR_DB.prepare(`
      INSERT INTO inventory (player_id, item_id, quantity, durability)
      VALUES (?, ?, ?, ?)
      ON CONFLICT(player_id, item_id) DO UPDATE SET quantity = quantity + excluded.quantity
    `).bind(pid, item_id, quantity, item.max_durability)
  ]);
  return json({ ok: true, spent: total });
}
__name(buyFromShop, "buyFromShop");
function repairCost(currentDurability, maxDurability, shopPrice) {
  const missing = maxDurability - currentDurability;
  const ratio = missing / maxDurability;
  return Math.ceil(shopPrice * ratio * 0.4);
}
__name(repairCost, "repairCost");
async function repairEstimate(req, env, pid) {
  const inventoryId = new URL(req.url).searchParams.get("inventory_id");
  const row = await env.TR_DB.prepare(`
    SELECT i.*, c.max_durability, c.shop_price FROM inventory i
    JOIN item_catalog c ON i.item_id = c.item_id
    WHERE i.id = ? AND i.player_id = ?
  `).bind(inventoryId, pid).first();
  if (!row) return json({ error: "Item not found" }, 404);
  if (!row.max_durability) return json({ cost: 0, note: "Indestructible" });
  const cost = repairCost(row.durability, row.max_durability, row.shop_price);
  return json({ cost, current: row.durability, max: row.max_durability });
}
__name(repairEstimate, "repairEstimate");
async function repairItem(req, env, pid) {
  const { inventory_id } = await req.json();
  const [row, player] = await Promise.all([
    env.TR_DB.prepare(`
      SELECT i.*, c.max_durability, c.shop_price FROM inventory i
      JOIN item_catalog c ON i.item_id = c.item_id
      WHERE i.id = ? AND i.player_id = ?
    `).bind(inventory_id, pid).first(),
    env.TR_DB.prepare("SELECT chrono_coins FROM players WHERE player_id = ?").bind(pid).first()
  ]);
  if (!row) return json({ error: "Item not found" }, 404);
  if (!player) return json({ error: "Player not found" }, 404);
  if (!row.max_durability) return json({ ok: true, note: "Already indestructible" });
  if (row.durability >= row.max_durability) return json({ ok: true, note: "Already at full durability" });
  const cost = repairCost(row.durability, row.max_durability, row.shop_price);
  if (player.chrono_coins < cost)
    return json({ error: "Insufficient Chrono-Coins", needed: cost, have: player.chrono_coins }, 402);
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins - ? WHERE player_id = ?").bind(cost, pid),
    env.TR_DB.prepare("UPDATE inventory SET durability = ? WHERE id = ?").bind(row.max_durability, inventory_id),
    env.TR_DB.prepare("INSERT INTO repair_log (player_id, inventory_id, cost) VALUES (?, ?, ?)").bind(pid, inventory_id, cost)
  ]);
  return json({ ok: true, cost });
}
__name(repairItem, "repairItem");
async function buyUpgrade(req, env, pid) {
  const { upgrade_id } = await req.json();
  const [item, player, existing] = await Promise.all([
    env.TR_DB.prepare("SELECT * FROM item_catalog WHERE item_id = ? AND category = 'upgrade'").bind(upgrade_id).first(),
    env.TR_DB.prepare("SELECT chrono_coins FROM players WHERE player_id = ?").bind(pid).first(),
    env.TR_DB.prepare("SELECT id FROM upgrades WHERE player_id = ? AND upgrade_id = ?").bind(pid, upgrade_id).first()
  ]);
  if (!item) return json({ error: "Upgrade not found" }, 404);
  if (existing) return json({ error: "Already purchased" }, 409);
  if (!player) return json({ error: "Player not found" }, 404);
  if (player.chrono_coins < item.shop_price)
    return json({ error: "Insufficient Chrono-Coins", needed: item.shop_price }, 402);
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins - ? WHERE player_id = ?").bind(item.shop_price, pid),
    env.TR_DB.prepare("INSERT INTO upgrades (player_id, upgrade_id) VALUES (?, ?)").bind(pid, upgrade_id)
  ]);
  return json({ ok: true });
}
__name(buyUpgrade, "buyUpgrade");
async function getCosmetics(req, env, pid) {
  const rows = await env.TR_DB.prepare("SELECT * FROM cosmetics WHERE player_id = ?").bind(pid).all();
  return json(rows.results ?? []);
}
__name(getCosmetics, "getCosmetics");
async function equipCosmetic(req, env, pid) {
  const { cosmetic_id, slot, equip } = await req.json();
  if (equip && slot) {
    await env.TR_DB.prepare(
      "UPDATE cosmetics SET is_equipped = 0 WHERE player_id = ? AND slot = ?"
    ).bind(pid, slot).run();
  }
  await env.TR_DB.prepare(
    "UPDATE cosmetics SET is_equipped = ?, slot = ? WHERE player_id = ? AND cosmetic_id = ?"
  ).bind(equip ? 1 : 0, slot ?? null, pid, cosmetic_id).run();
  return json({ ok: true });
}
__name(equipCosmetic, "equipCosmetic");
async function equipContainer(req, env, pid) {
  const { container_db_id } = await req.json();
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE containers SET is_equipped = 0 WHERE player_id = ?").bind(pid),
    env.TR_DB.prepare("UPDATE containers SET is_equipped = 1 WHERE id = ? AND player_id = ?").bind(container_db_id, pid)
  ]);
  return json({ ok: true });
}
__name(equipContainer, "equipContainer");
async function equipPet(req, env, pid) {
  const { pet_db_id } = await req.json();
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE pets SET is_active = 0 WHERE player_id = ?").bind(pid),
    env.TR_DB.prepare("UPDATE pets SET is_active = 1 WHERE id = ? AND player_id = ?").bind(pet_db_id, pid)
  ]);
  return json({ ok: true });
}
__name(equipPet, "equipPet");
async function equipVehicle(req, env, pid) {
  const { vehicle_db_id } = await req.json();
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE vehicles SET is_active = 0 WHERE player_id = ?").bind(pid),
    env.TR_DB.prepare("UPDATE vehicles SET is_active = 1 WHERE id = ? AND player_id = ?").bind(vehicle_db_id, pid)
  ]);
  return json({ ok: true });
}
__name(equipVehicle, "equipVehicle");
function calcValuablePrice(era, rarity, isLegendary, historicalFigure, baseValue) {
  const eraMult = ERA_MULT[era] ?? 1;
  const rarityMult = RARITY_MULT[rarity] ?? 1;
  const legendMult = isLegendary ? 2.5 : 1;
  const figureMult = historicalFigure ? HISTORICAL_FIGURE_MULT : 1;
  return Math.ceil(baseValue * eraMult * rarityMult * legendMult * figureMult);
}
__name(calcValuablePrice, "calcValuablePrice");
async function valuableEstimate(req, env) {
  const p = new URL(req.url).searchParams;
  const price = calcValuablePrice(
    p.get("era"),
    p.get("rarity"),
    p.get("is_legendary") === "1",
    p.get("historical_figure"),
    parseInt(p.get("base_value") ?? "100")
  );
  return json({ estimated_price: price });
}
__name(valuableEstimate, "valuableEstimate");
async function sellValuable(req, env) {
  const { player_id, valuable_id } = await req.json();
  const [val, player] = await Promise.all([
    env.TR_DB.prepare("SELECT * FROM valuables WHERE id = ? AND player_id = ?").bind(valuable_id, player_id).first(),
    env.TR_DB.prepare("SELECT chrono_coins FROM players WHERE player_id = ?").bind(player_id).first()
  ]);
  if (!val) return json({ error: "Valuable not found" }, 404);
  if (!player) return json({ error: "Player not found" }, 404);
  const price = calcValuablePrice(val.era, val.rarity, val.is_legendary, val.historical_figure, val.base_value);
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins + ? WHERE player_id = ?").bind(price, player_id),
    env.TR_DB.prepare("DELETE FROM valuables WHERE id = ?").bind(valuable_id)
  ]);
  return json({ ok: true, earned: price });
}
__name(sellValuable, "sellValuable");
async function getAuctionListings(req, env) {
  const params = new URL(req.url).searchParams;
  const requestor = params.get("player_id") ?? "";
  const rows = await env.TR_DB.prepare(`
    SELECT l.*, p.display_name AS seller_name
    FROM auction_listings l JOIN players p ON l.seller_id = p.player_id
    WHERE l.status = 'active'
      AND l.expires_at > unixepoch()
      AND (l.is_private = 0 OR l.seller_id = ? OR l.allowed_buyers LIKE ?)
    ORDER BY l.created_at DESC
  `).bind(requestor, `%"${requestor}"%`).all();
  return json(rows.results ?? []);
}
__name(getAuctionListings, "getAuctionListings");
async function createListing(req, env) {
  const body = await req.json();
  const { seller_id, item_type, item_ref_id, asking_price, is_private, allowed_buyers } = body;
  const player = await env.TR_DB.prepare("SELECT chrono_coins FROM players WHERE player_id = ?").bind(seller_id).first();
  if (!player) return json({ error: "Player not found" }, 404);
  const fee = Math.min(Math.ceil(asking_price * LISTING_FEE_RATE), LISTING_FEE_CAP);
  if (player.chrono_coins < fee)
    return json({ error: "Insufficient coins for listing fee", fee }, 402);
  let snapshot = null;
  if (item_type === "inventory") {
    snapshot = await env.TR_DB.prepare(`
      SELECT i.*, c.name, c.category FROM inventory i
      JOIN item_catalog c ON i.item_id = c.item_id
      WHERE i.id = ? AND i.player_id = ?
    `).bind(item_ref_id, seller_id).first();
  } else {
    snapshot = await env.TR_DB.prepare(
      "SELECT * FROM valuables WHERE id = ? AND player_id = ?"
    ).bind(item_ref_id, seller_id).first();
  }
  if (!snapshot) return json({ error: "Item not found in your inventory" }, 404);
  const listingId = crypto.randomUUID();
  const expiresAt = Math.floor(Date.now() / 1e3) + LISTING_TTL_SECS;
  await env.TR_DB.batch([
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins - ? WHERE player_id = ?").bind(fee, seller_id),
    env.TR_DB.prepare(`
      INSERT INTO auction_listings
        (listing_id, seller_id, item_type, item_ref_id, item_snapshot, asking_price,
         is_private, allowed_buyers, listing_fee, expires_at)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    `).bind(
      listingId,
      seller_id,
      item_type,
      item_ref_id,
      JSON.stringify(snapshot),
      asking_price,
      is_private ? 1 : 0,
      allowed_buyers ? JSON.stringify(allowed_buyers) : null,
      fee,
      expiresAt
    )
  ]);
  return json({ ok: true, listing_id: listingId, fee });
}
__name(createListing, "createListing");
async function buyListing(req, env, listingId) {
  const { buyer_id } = await req.json();
  const [listing, buyer] = await Promise.all([
    env.TR_DB.prepare("SELECT * FROM auction_listings WHERE listing_id = ? AND status = 'active'").bind(listingId).first(),
    env.TR_DB.prepare("SELECT * FROM players WHERE player_id = ?").bind(buyer_id).first()
  ]);
  if (!listing) return json({ error: "Listing not found or already sold" }, 404);
  if (!buyer) return json({ error: "Player not found" }, 404);
  if (listing.seller_id === buyer_id) return json({ error: "Cannot buy your own listing" }, 400);
  if (listing.is_private) {
    const allowed = listing.allowed_buyers ? JSON.parse(listing.allowed_buyers) : [];
    if (!allowed.includes(buyer_id))
      return json({ error: "This is a private listing" }, 403);
  }
  if (buyer.chrono_coins < listing.asking_price)
    return json({ error: "Insufficient Chrono-Coins", needed: listing.asking_price }, 402);
  const snapshot = JSON.parse(listing.item_snapshot);
  await env.TR_DB.batch([
    // Deduct from buyer
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins - ? WHERE player_id = ?").bind(listing.asking_price, buyer_id),
    // Pay seller (price minus house fee already collected at listing time)
    env.TR_DB.prepare("UPDATE players SET chrono_coins = chrono_coins + ? WHERE player_id = ?").bind(listing.asking_price, listing.seller_id),
    // Mark listing sold
    env.TR_DB.prepare("UPDATE auction_listings SET status = 'sold' WHERE listing_id = ?").bind(listingId),
    // Transfer item to buyer
    listing.item_type === "inventory" ? env.TR_DB.prepare("UPDATE inventory SET player_id = ? WHERE id = ?").bind(buyer_id, listing.item_ref_id) : env.TR_DB.prepare("UPDATE valuables SET player_id = ? WHERE id = ?").bind(buyer_id, listing.item_ref_id),
    // Record transaction
    env.TR_DB.prepare(
      "INSERT INTO auction_transactions (listing_id, buyer_id, price_paid, fee_charged) VALUES (?, ?, ?, ?)"
    ).bind(listingId, buyer_id, listing.asking_price, listing.listing_fee)
  ]);
  return json({ ok: true, item: snapshot });
}
__name(buyListing, "buyListing");
async function cancelListing(req, env, listingId) {
  const { seller_id } = await req.json();
  const listing = await env.TR_DB.prepare(
    "SELECT * FROM auction_listings WHERE listing_id = ? AND seller_id = ? AND status = 'active'"
  ).bind(listingId, seller_id).first();
  if (!listing) return json({ error: "Listing not found" }, 404);
  await env.TR_DB.prepare(
    "UPDATE auction_listings SET status = 'cancelled' WHERE listing_id = ?"
  ).bind(listingId).run();
  return json({ ok: true });
}
__name(cancelListing, "cancelListing");
async function eraGuide(req, env) {
  const { era, question } = await req.json();
  const systemPrompt = `You are the Treasure Royale Era Guide \u2014 a knowledgeable multiverse historian.
  The player is about to enter the ${era} era. Answer survival and gear questions concisely (3-5 sentences max).
  Focus on: what gear to bring, environmental hazards, creature threats, artifact types, and extraction tips.
  Never break character. Never mention game mechanics by code name.`;
  const resp = await fetch("https://api.groq.com/openai/v1/chat/completions", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${env.GROQ_API_KEY}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      model: "llama3-8b-8192",
      messages: [
        { role: "system", content: systemPrompt },
        { role: "user", content: question }
      ],
      max_tokens: 200,
      temperature: 0.7
    })
  });
  if (!resp.ok) return json({ answer: "Guide unavailable. Check your gear and proceed carefully." });
  const data = await resp.json();
  return json({ answer: data.choices[0].message.content.trim() });
}
__name(eraGuide, "eraGuide");
function playerId(path) {
  return path.split("/")[2];
}
__name(playerId, "playerId");
function json(data, status = 200) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { ...CORS, "Content-Type": "application/json" }
  });
}
__name(json, "json");

// ../../../../Users/Upstairs/AppData/Roaming/npm/node_modules/wrangler/templates/middleware/middleware-ensure-req-body-drained.ts
var drainBody = /* @__PURE__ */ __name(async (request, env, _ctx, middlewareCtx) => {
  try {
    return await middlewareCtx.next(request, env);
  } finally {
    try {
      if (request.body !== null && !request.bodyUsed) {
        const reader = request.body.getReader();
        while (!(await reader.read()).done) {
        }
      }
    } catch (e) {
      console.error("Failed to drain the unused request body.", e);
    }
  }
}, "drainBody");
var middleware_ensure_req_body_drained_default = drainBody;

// ../../../../Users/Upstairs/AppData/Roaming/npm/node_modules/wrangler/templates/middleware/middleware-miniflare3-json-error.ts
function reduceError(e) {
  return {
    name: e?.name,
    message: e?.message ?? String(e),
    stack: e?.stack,
    cause: e?.cause === void 0 ? void 0 : reduceError(e.cause)
  };
}
__name(reduceError, "reduceError");
var jsonError = /* @__PURE__ */ __name(async (request, env, _ctx, middlewareCtx) => {
  try {
    return await middlewareCtx.next(request, env);
  } catch (e) {
    const error = reduceError(e);
    const body = JSON.stringify(error);
    const headers = {
      "Content-Type": "application/json",
      "MF-Experimental-Error-Stack": "true"
    };
    const encoded = encodeURIComponent(body);
    if (encoded.length <= 8192) {
      headers["MF-Experimental-Error-Stack-Payload"] = encoded;
    }
    return new Response(body, { status: 500, headers });
  }
}, "jsonError");
var middleware_miniflare3_json_error_default = jsonError;

// .wrangler/tmp/bundle-xpB7Qo/middleware-insertion-facade.js
var __INTERNAL_WRANGLER_MIDDLEWARE__ = [
  middleware_ensure_req_body_drained_default,
  middleware_miniflare3_json_error_default
];
var middleware_insertion_facade_default = worker_default;

// ../../../../Users/Upstairs/AppData/Roaming/npm/node_modules/wrangler/templates/middleware/common.ts
var __facade_middleware__ = [];
function __facade_register__(...args) {
  __facade_middleware__.push(...args.flat());
}
__name(__facade_register__, "__facade_register__");
function __facade_invokeChain__(request, env, ctx, dispatch, middlewareChain) {
  const [head, ...tail] = middlewareChain;
  const middlewareCtx = {
    dispatch,
    next(newRequest, newEnv) {
      return __facade_invokeChain__(newRequest, newEnv, ctx, dispatch, tail);
    }
  };
  return head(request, env, ctx, middlewareCtx);
}
__name(__facade_invokeChain__, "__facade_invokeChain__");
function __facade_invoke__(request, env, ctx, dispatch, finalMiddleware) {
  return __facade_invokeChain__(request, env, ctx, dispatch, [
    ...__facade_middleware__,
    finalMiddleware
  ]);
}
__name(__facade_invoke__, "__facade_invoke__");

// .wrangler/tmp/bundle-xpB7Qo/middleware-loader.entry.ts
var __Facade_ScheduledController__ = class ___Facade_ScheduledController__ {
  constructor(scheduledTime, cron, noRetry) {
    this.scheduledTime = scheduledTime;
    this.cron = cron;
    this.#noRetry = noRetry;
  }
  scheduledTime;
  cron;
  static {
    __name(this, "__Facade_ScheduledController__");
  }
  #noRetry;
  noRetry() {
    if (!(this instanceof ___Facade_ScheduledController__)) {
      throw new TypeError("Illegal invocation");
    }
    this.#noRetry();
  }
};
function wrapExportedHandler(worker) {
  if (__INTERNAL_WRANGLER_MIDDLEWARE__ === void 0 || __INTERNAL_WRANGLER_MIDDLEWARE__.length === 0) {
    return worker;
  }
  for (const middleware of __INTERNAL_WRANGLER_MIDDLEWARE__) {
    __facade_register__(middleware);
  }
  const fetchDispatcher = /* @__PURE__ */ __name(function(request, env, ctx) {
    if (worker.fetch === void 0) {
      throw new Error("Handler does not export a fetch() function.");
    }
    return worker.fetch(request, env, ctx);
  }, "fetchDispatcher");
  return {
    ...worker,
    fetch(request, env, ctx) {
      const dispatcher = /* @__PURE__ */ __name(function(type, init) {
        if (type === "scheduled" && worker.scheduled !== void 0) {
          const controller = new __Facade_ScheduledController__(
            Date.now(),
            init.cron ?? "",
            () => {
            }
          );
          return worker.scheduled(controller, env, ctx);
        }
      }, "dispatcher");
      return __facade_invoke__(request, env, ctx, dispatcher, fetchDispatcher);
    }
  };
}
__name(wrapExportedHandler, "wrapExportedHandler");
function wrapWorkerEntrypoint(klass) {
  if (__INTERNAL_WRANGLER_MIDDLEWARE__ === void 0 || __INTERNAL_WRANGLER_MIDDLEWARE__.length === 0) {
    return klass;
  }
  for (const middleware of __INTERNAL_WRANGLER_MIDDLEWARE__) {
    __facade_register__(middleware);
  }
  return class extends klass {
    #fetchDispatcher = /* @__PURE__ */ __name((request, env, ctx) => {
      this.env = env;
      this.ctx = ctx;
      if (super.fetch === void 0) {
        throw new Error("Entrypoint class does not define a fetch() function.");
      }
      return super.fetch(request);
    }, "#fetchDispatcher");
    #dispatcher = /* @__PURE__ */ __name((type, init) => {
      if (type === "scheduled" && super.scheduled !== void 0) {
        const controller = new __Facade_ScheduledController__(
          Date.now(),
          init.cron ?? "",
          () => {
          }
        );
        return super.scheduled(controller);
      }
    }, "#dispatcher");
    fetch(request) {
      return __facade_invoke__(
        request,
        this.env,
        this.ctx,
        this.#dispatcher,
        this.#fetchDispatcher
      );
    }
  };
}
__name(wrapWorkerEntrypoint, "wrapWorkerEntrypoint");
var WRAPPED_ENTRY;
if (typeof middleware_insertion_facade_default === "object") {
  WRAPPED_ENTRY = wrapExportedHandler(middleware_insertion_facade_default);
} else if (typeof middleware_insertion_facade_default === "function") {
  WRAPPED_ENTRY = wrapWorkerEntrypoint(middleware_insertion_facade_default);
}
var middleware_loader_entry_default = WRAPPED_ENTRY;
export {
  __INTERNAL_WRANGLER_MIDDLEWARE__,
  middleware_loader_entry_default as default
};
//# sourceMappingURL=worker.js.map
