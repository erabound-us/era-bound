# Survival Loot System

## Overview

Survival loot is the third layer of the game's item ecosystem, below relics and artifacts. It is abundant, era-authentic, and consumed rather than extracted. Players who ignore it will die before reaching the beacon. Players who manage it well gain a meaningful edge over those who don't.

Survival loot is **never extracted** — it has no trade value and cannot be brought back to the lobby. It exists only within the match. This keeps the economy clean: artifacts and relics are the extractable economy, survival loot is the in-match resource layer.

---

## Loot Categories

| Category | Purpose | Inventory Slots |
|---|---|---|
| **Food & Rations** | Restore health over time | 4 slots |
| **Medicine & Healing** | Instant or fast health restore | 3 slots |
| **Repair Kits** | Restore armor durability | 2 slots |
| **Ammo & Weapons** | Replenish ammunition, find weapons | Weapon-specific |
| **Crafting Materials** | Combine into better items | 6 slots |
| **Utility Supplies** | Torches, rope, tools — situational use | 3 slots |

Survival loot shares **no inventory space** with artifacts and relics — it uses a separate survival inventory panel. A player's artifact haul is never crowded out by food.

---

## Spawn Rules

- Survival loot spawns **abundantly** — 3–5× more spawn points than artifacts
- Spawn locations are **not seeded** — they randomize fresh each match within zone boundaries
- Survival loot **does not appear on the minimap** — players must explore to find it
- Survival loot **respawns** at its location 90 seconds after being taken — the world replenishes
- Survival loot **cannot be stolen** from another player's inventory — only artifacts and relics drop on death

---

## FOOD & RATIONS

Food restores health **over time** (not instant) — eating takes 2 seconds and cannot be done while in combat (taking damage cancels it). This forces players to find safe moments to eat, creating tactical decisions.

### Health Restore Values
| Quality | Restore Amount | Restore Duration | Era Availability |
|---|---|---|---|
| Scraps | 10 HP | Instant | All eras |
| Basic Ration | 25 HP | Over 5 seconds | All eras |
| Good Meal | 50 HP | Over 8 seconds | All eras |
| Feast | 100 HP | Over 12 seconds | Rare, all eras |

### Era-Specific Food Items

**Deep Prehistoric / Prehistoric**
| Item | Quality | Description |
|---|---|---|
| Raw Meat Chunk | Scraps | Uncooked, risky — 20% chance of 5 HP loss instead of gain |
| Cooked Meat on Bone | Basic Ration | Found near fire pits, safe |
| Mammoth Jerky | Good Meal | Dried and preserved, dense nutrition |
| Honey Comb | Basic Ration | Found in tree hollows, sweet and caloric |
| Wild Berries (handful) | Scraps | Quick grab, small restore |
| Roasted Root Vegetables | Basic Ration | Found near hominid camps |
| Dinosaur Egg (cracked, cooked) | Good Meal | Enormous caloric value |
| Megafauna Bone Marrow | Good Meal | Cracked bone, rich fat content |

**Ancient Era**
| Item | Quality | Description |
|---|---|---|
| Flatbread Loaf | Basic Ration | Egyptian staple, found in granaries |
| Dried Fish | Basic Ration | Nile catch, wrapped in papyrus |
| Pomegranate | Scraps | Found in palace gardens |
| Olive Oil Flask | Good Meal | Greek staple, high caloric density |
| Roman Hardtack (Bucellatum) | Basic Ration | Military ration, lasts forever |
| Garum Sauce Jar | Scraps | Roman fish sauce, acquired taste |
| Lentil Stew Pot | Good Meal | Mesopotamian staple, warm |
| Dates and Figs Bundle | Basic Ration | Egyptian desert food |
| Pompeii Bread Loaf (fresh) | Good Meal | Still warm from the bakery oven |

**Medieval Era**
| Item | Quality | Description |
|---|---|---|
| Hardtack Biscuit | Scraps | Stale, dense, keeps forever |
| Salted Pork Strip | Basic Ration | Preserved meat, common soldier food |
| Pottage Bowl | Good Meal | Thick vegetable stew, warm |
| Monastery Bread | Good Meal | Well-made, monks baked well |
| Dried Herbs Bundle | Scraps | Minimal nutrition, used in crafting |
| Mead Flask | Basic Ration | Alcoholic — restores HP but blurs vision for 5 seconds |
| Roasted Pheasant Leg | Good Meal | Noble's table, high quality |
| Black Plague Ration | Scraps | Contaminated — 30% chance of 10 HP loss, 70% Basic Ration effect |

**Renaissance / Industrial**
| Item | Quality | Description |
|---|---|---|
| Ship's Biscuit | Scraps | Hard, weevil-ridden, desperate food |
| Spice Merchant's Ration | Good Meal | Exotic preserved foods, high quality |
| Victorian Tin of Sardines | Basic Ration | Early canned food, reliable |
| Army Field Ration (WWI) | Basic Ration | Corned beef and biscuits |
| Chocolate Bar (WWII issue) | Good Meal | High calorie military chocolate |
| Trench Coffee | Scraps | Weak, bitter, minimal nutrition |
| Canned Peaches | Good Meal | WWII era, surprisingly good |

**Modern / Future**
| Item | Quality | Description |
|---|---|---|
| Energy Bar | Basic Ration | Modern nutrition, reliable |
| MRE Pack | Good Meal | Military Meal Ready to Eat, complete nutrition |
| Protein Gel Pack | Basic Ration | Near-future sports nutrition |
| Nutrient Paste Tube | Good Meal | Mid-future complete meal in a tube |
| Synthesized Protein Block | Feast | Far-future lab-grown complete nutrition |
| Caloric Implant Charge | Feast | Deep Future — nutrition delivered directly to bloodstream |

---

## MEDICINE & HEALING

Medicine restores health **faster** than food but is rarer. Some medicines have side effects that create risk/reward decisions.

### Healing Values
| Type | Restore | Speed | Side Effect |
|---|---|---|---|
| Basic Herb/Bandage | 20 HP | 3 seconds | None |
| Poultice/Tincture | 40 HP | 2 seconds | None |
| Advanced Medicine | 60 HP | Instant | None |
| Emergency Treatment | Full HP | Instant | 30s vulnerability window after use |

### Era-Specific Medicine

**Deep Prehistoric / Prehistoric**
| Item | Type | Description |
|---|---|---|
| Medicinal Moss Poultice | Basic | Applied to wounds, found near water |
| Shaman's Healing Paste | Poultice | Ochre and animal fat, surprisingly effective |
| Willow Bark Strip | Basic | Natural aspirin, chew to use |
| Cave Mushroom Extract | Advanced | Psychoactive but healing — vision blurs for 10s |
| Mammoth Fat Salve | Poultice | Dense fat, seals wounds |

**Ancient Era**
| Item | Type | Description |
|---|---|---|
| Egyptian Honey Bandage | Basic | Honey has antibacterial properties, effective |
| Theriac Antidote | Poultice | Roman cure-all, partially effective |
| Opium Tincture | Advanced | Effective but causes 15s slow movement |
| Greek Physician's Kit | Advanced | Hippocratic medicine, reliable |
| Aloe Vera Wrap | Basic | Egyptian burn treatment |
| Roman Vinegar Compress | Basic | Battlefield wound treatment |

**Medieval Era**
| Item | Type | Description |
|---|---|---|
| Monastery Herbal Remedy | Basic | Monks were the best doctors of the era |
| Bloodletting Kit | Basic | Removes 10 HP but cures poison status |
| Plague Doctor's Tincture | Poultice | Smells terrible, works moderately |
| Crusader Field Dressing | Basic | Linen bandage, standard issue |
| Alchemist's Elixir | Advanced | Unpredictable — roll: 60% full heal, 40% 20 HP loss |

**Industrial / Modern**
| Item | Type | Description |
|---|---|---|
| Morphine Syrette (WWI) | Advanced | Instant pain relief, 10s movement slow |
| Sulfa Powder Pack (WWII) | Poultice | Early antibiotic, reliable |
| Field Medic Kit | Advanced | Complete battlefield medicine |
| Modern First Aid Kit | Advanced | Reliable, no side effects |
| Trauma Pack | Emergency | Full restore, 30s vulnerability |

**Future**
| Item | Type | Description |
|---|---|---|
| Nano-Med Injector | Advanced | Near-future nanobots, fast and clean |
| Cellular Repair Gel | Emergency | Mid-future full restore, no side effects |
| Bio-Regenerator Patch | Emergency | Far-future, regenerates tissue in seconds |
| Temporal Healing Shard | Emergency | Deep Future — heals wounds before they happen |

---

## REPAIR KITS

Armor degrades during combat. Without repairs, players become increasingly vulnerable. Repair kits are era-appropriate tools that restore armor durability.

### Armor Durability System
- All players start with 100% armor durability
- Each hit reduces durability by 5–15% depending on damage type
- At 0% durability: armor provides no damage reduction
- Repair kits restore durability in the field

### Repair Kit Types
| Kit | Durability Restored | Time to Use | Era |
|---|---|---|---|
| Field Patch | 20% | 4 seconds | All |
| Standard Repair | 40% | 6 seconds | All |
| Full Overhaul Kit | 100% | 10 seconds | All |

### Era-Specific Repair Items

**Deep Prehistoric / Prehistoric**
| Item | Type | Description |
|---|---|---|
| Hide Patch and Sinew | Field Patch | Animal hide and gut thread, basic repair |
| Bone Needle and Thread | Field Patch | Primitive but effective for hide armor |
| Mammoth Hide Reinforcement | Standard Repair | Thick hide, excellent protection |
| Flint Knapping Kit | Field Patch | Resharpens stone weapons, repairs flint armor |

**Ancient / Medieval**
| Item | Type | Description |
|---|---|---|
| Bronze Rivets and Hammer | Standard Repair | Repairs bronze armor plates |
| Chainmail Repair Links | Standard Repair | Replacement rings for chainmail |
| Blacksmith's Field Kit | Full Overhaul | Portable forge tools, complete repair |
| Leather Stitching Kit | Field Patch | Repairs leather armor quickly |
| Armorsmith's Whetstone | Field Patch | Sharpens blades, minor armor repair |

**Industrial / Modern**
| Item | Type | Description |
|---|---|---|
| Soldier's Repair Tape | Field Patch | Military duct tape equivalent |
| Welding Rod and Torch | Standard Repair | Industrial era metal repair |
| Combat Engineer's Kit | Full Overhaul | Complete field repair capability |
| Ballistic Patch Panel | Standard Repair | Modern body armor repair insert |

**Future**
| Item | Type | Description |
|---|---|---|
| Nano-Repair Spray | Standard Repair | Near-future self-assembling material |
| Molecular Bonder | Full Overhaul | Mid-future complete structural repair |
| Temporal Integrity Patch | Full Overhaul | Deep Future — repairs armor before it breaks |

---

## AMMO & WEAPONS

Ammo is era-specific — you cannot use modern bullets in a medieval crossbow. Weapons found in the world are functional but degrade with use.

### Ammo Types by Era

| Era | Ammo Type | Weapon |
|---|---|---|
| Deep Prehistoric | Throwing stones, bone spears | Primitive ranged, spear |
| Prehistoric | Flint-tipped arrows, atlatl darts | Bow, atlatl |
| Ancient | Bronze-tipped arrows, sling stones | Bow, sling |
| Medieval | Iron-tipped arrows, crossbow bolts | Longbow, crossbow |
| Renaissance | Black powder charges, lead balls | Musket, pistol |
| Industrial | Rifle cartridges, shotgun shells | Rifle, shotgun |
| Modern | 9mm, 5.56mm, 12-gauge | Pistol, rifle, shotgun |
| Future | Energy cells, plasma charges | Energy weapons |

### Weapon Condition System
Weapons found in the world have a condition rating: **Pristine / Good / Worn / Broken**
- Pristine: full damage, no penalty
- Good: 90% damage
- Worn: 70% damage, occasional misfire
- Broken: 40% damage, frequent misfire — needs repair kit

### Scavenged Weapons (era examples)

**Deep Prehistoric**
- Sharpened Bone Club (Worn) — melee, heavy
- Flint-Tipped Spear (Good) — thrown or melee
- Obsidian Hand Axe (Pristine) — melee, fast

**Ancient**
- Roman Gladius (Good) — short sword, fast
- Greek Composite Bow (Worn) — ranged, medium range
- Egyptian Khopesh (Pristine) — curved sword, unique attack arc

**Medieval**
- Crossbow (Good) — slow reload, high damage
- Longsword (Worn) — versatile melee
- Mace (Pristine) — armor-breaking melee

**Industrial**
- Lee-Enfield Rifle (Good) — bolt action, accurate
- Webley Revolver (Worn) — 6 shots, reliable
- Trench Shotgun (Pristine) — close range devastation

**Modern**
- M4 Carbine (Good) — standard assault rifle
- Desert Eagle (Worn) — high damage pistol
- SPAS-12 Shotgun (Pristine) — semi-auto shotgun

**Future**
- Pulse Rifle (Good) — energy weapon, no recoil
- Plasma Pistol (Worn) — overheats on rapid fire
- Void Disruptor (Pristine) — Simulation Layer only, ignores armor

---

## CRAFTING MATERIALS

Raw materials found in the world that can be combined at a **Crafting Station** (fixed locations on each map) to create better survival items.

### Crafting Stations
- 2–3 per map, marked on minimap with a wrench icon
- Using a crafting station takes 5 seconds — vulnerable during crafting
- Crafting is available to all players — no ownership

### Material Types

| Material | Found In | Used For |
|---|---|---|
| Flint Shards | Prehistoric, Ancient | Weapon repair, arrowheads |
| Animal Hide | Prehistoric, Medieval | Armor patches, pouches |
| Medicinal Herbs | All eras | Healing poultices |
| Metal Scraps | Industrial, Modern | Weapon repair, armor repair |
| Gunpowder | Renaissance, Industrial | Ammo crafting |
| Chemical Compounds | Modern, Future | Advanced medicine |
| Nano-Particles | Future | Future repair kits |
| Bone Fragments | Prehistoric | Primitive tools, weapons |
| Plant Fiber | All eras | Rope, binding, basic crafting |
| Charcoal | All eras | Crafting component, fire starter |

### Crafting Recipes (examples)

| Recipe | Materials | Result |
|---|---|---|
| Basic Poultice | Medicinal Herbs + Animal Hide | Poultice/Tincture |
| Field Repair Kit | Metal Scraps × 2 + Plant Fiber | Standard Repair Kit |
| Improvised Arrows × 5 | Flint Shards + Plant Fiber + Bone Fragments | Flint-tipped arrows |
| Molotov Cocktail | Chemical Compounds + Plant Fiber | Throwable fire weapon |
| Smoke Bomb | Charcoal + Chemical Compounds | Utility smoke |
| Reinforced Bandage | Animal Hide + Medicinal Herbs | Advanced Medicine |
| Explosive Charge | Gunpowder × 2 + Metal Scraps | Deployable explosive |

---

## UTILITY SUPPLIES

Situational items that solve specific problems. Not health-related but often survival-critical.

| Item | Era | Use |
|---|---|---|
| Torch | Prehistoric–Medieval | Illuminates dark areas, deters some creatures |
| Rope Coil | All eras | Climb surfaces, set trip wires |
| Grappling Hook | Renaissance+ | Reach elevated positions |
| Lockpick Set | Medieval+ | Open locked containers for better loot |
| Compass | Ancient+ | Reveals player's position on minimap for 60s |
| Binoculars / Spyglass | Renaissance+ | Scout distant areas without moving |
| Flare | Industrial+ | Signals location — useful for teams, dangerous if enemies see it |
| Night Vision Goggles | Modern+ | See in darkness |
| Portable Scanner | Future | Reveals all survival loot within 30m for 60s |
| Fire Starter Kit | All eras | Start fires — warmth in cold variants, cooking raw meat |
| Antidote Vial | All eras | Cures poison from creature attacks |
| Creature Repellent | Prehistoric–Medieval | Reduces creature aggro radius by 50% for 3 minutes |

---

## LOOT CONTAINER TYPES BY ERA

Survival loot is found in era-appropriate containers scattered across the map:

| Era | Container Types |
|---|---|
| Deep Prehistoric | Animal carcasses, cave deposits, hominid camp bundles, hollow logs |
| Prehistoric | Burial mounds, cave caches, animal dens, fire pit remains |
| Ancient | Clay pots, wooden chests, market stalls, soldier's packs, tomb offerings |
| Medieval | Barrels, sacks, weapon racks, monastery stores, battlefield packs |
| Renaissance | Crates, merchant chests, ship cargo, apothecary shelves |
| Industrial | Ammunition crates, medical tents, supply depots, factory stores |
| Modern | Military crates, civilian backpacks, pharmacy shelves, vehicle trunks |
| Future | Supply pods, automated dispensers, emergency caches, drone drops |

---

## SURVIVAL PRESSURE DESIGN

The survival system creates pressure without being a survival game. Key design decisions:

- **No hunger/thirst meters** — food only matters when you're hurt. Players aren't managing meters, they're managing health.
- **Armor degradation is visible** — a cracked armor icon tells players they need to repair before the next fight, not after.
- **Ammo scarcity scales with era** — prehistoric ammo (stones, bone spears) is everywhere. Modern ammo is scarcer. Future ammo is rarest. This means future-era players must manage ammo more carefully.
- **Crafting is optional but rewarding** — a player who takes 5 seconds at a crafting station comes out ahead. A player who ignores it still survives, just with less margin.
- **Survival loot respawns** — the world isn't stripped bare. Players who explore continuously find resources continuously.
- **No survival loot on the minimap** — exploration is rewarded. Players who know the map know where the good loot containers are.
