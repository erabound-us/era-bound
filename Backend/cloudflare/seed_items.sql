-- Treasure Royale — NPC Shop Seed Data
-- Run after schema.sql: wrangler d1 execute TR_DB --file=seed_items.sql

-- ── Light Weapons ─────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_pistol',       'Flintlock Pistol',    'weapon_light',  'sidearm',    'weapon', 1, 800,   0, 100, NULL, '["medieval","renaissance","industrial"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_revolver',     'Revolver',            'weapon_light',  'sidearm',    'weapon', 1, 1200,  0, 120, NULL, '["industrial","modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_crossbow',     'Crossbow',            'weapon_light',  'ranged',     'weapon', 1, 950,   0, 80,  NULL, '["medieval","renaissance"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_knife',        'Combat Knife',        'weapon_light',  'melee',      'weapon', 1, 300,   0, 200, NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_spear',        'Hunting Spear',       'weapon_light',  'melee',      'weapon', 1, 400,   0, 150, NULL, '["prehistoric","ancient"]');

-- ── Medium Weapons ────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_rifle',        'Bolt-Action Rifle',   'weapon_medium', 'rifle',      'weapon', 1, 2500,  0, 150, NULL, '["industrial","modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_smg',          'Submachine Gun',      'weapon_medium', 'smg',        'weapon', 1, 2200,  0, 130, NULL, '["modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_longbow',      'Longbow',             'weapon_medium', 'ranged',     'weapon', 1, 1100,  0, 100, NULL, '["medieval","renaissance"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_musket',       'Musket',              'weapon_medium', 'rifle',      'weapon', 1, 1400,  0, 90,  NULL, '["renaissance","industrial"]');

-- ── Heavy Weapons ─────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_shotgun',      'Pump Shotgun',        'weapon_heavy',  'shotgun',    'weapon', 2, 3200,  0, 120, NULL, '["industrial","modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_lmg',          'Light Machine Gun',   'weapon_heavy',  'lmg',        'weapon', 2, 4500,  0, 100, NULL, '["modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_rpg',          'Rocket Launcher',     'weapon_heavy',  'explosive',  'weapon', 2, 6000,  0, 80,  NULL, '["modern","near_future"]');
INSERT OR IGNORE INTO item_catalog VALUES ('wpn_cannon',       'Hand Cannon',         'weapon_heavy',  'explosive',  'weapon', 2, 3800,  0, 60,  NULL, '["renaissance","industrial"]');

-- ── Explosives ────────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('exp_grenade',      'Frag Grenade',        'gear',          'explosive',  'gear',   1, 500,   1, NULL, NULL, '["modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('exp_dynamite',     'Dynamite',            'gear',          'explosive',  'gear',   1, 600,   1, NULL, NULL, '["industrial","modern"]');
INSERT OR IGNORE INTO item_catalog VALUES ('exp_molotov',      'Molotov Cocktail',    'gear',          'explosive',  'gear',   1, 350,   1, NULL, NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('exp_mine',         'Proximity Mine',      'gear',          'explosive',  'gear',   1, 800,   0, 3,   NULL, '["modern","near_future"]');

-- ── Survival Gear ─────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('gear_shovel',      'Folding Shovel',      'gear',          'survival',   'gear',   1, 400,   0, 80,  'Dig up buried artifacts and loot caches', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_detector',    'Metal Detector',      'gear',          'survival',   'gear',   1, 1800,  0, 60,  'Pings nearby buried metal within 15m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_lidar',       'Portable LiDAR',      'gear',          'survival',   'gear',   1, 4500,  0, 40,  'Reveals hidden structures and passages within 30m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_sonar',       'Handheld Sonar',      'gear',          'survival',   'gear',   1, 3200,  0, 40,  'Detects underwater artifacts and passages', '["ancient","medieval"]');
INSERT OR IGNORE INTO item_catalog VALUES ('gear_rope',        'Climbing Rope (20m)', 'gear',          'survival',   'gear',   1, 250,   0, 50,  NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_grapple',     'Grappling Gun',       'gear',          'survival',   'gear',   1, 2200,  0, 70,  'Fires a hook up to 40m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_tent',        'Compact Tent',        'gear',          'survival',   'gear',   2, 900,   0, 30,  'Temporary shelter — restores stamina over time', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_flashlight',  'Tactical Flashlight', 'gear',          'survival',   'gear',   1, 200,   0, 120, NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_torch',       'Torch',               'gear',          'survival',   'gear',   1, 80,    1, NULL, NULL, '["prehistoric","ancient","medieval"]');
INSERT OR IGNORE INTO item_catalog VALUES ('gear_compass',     'Compass',             'gear',          'survival',   'gear',   1, 150,   0, NULL, NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_binoculars',  'Binoculars',          'gear',          'survival',   'gear',   1, 600,   0, 80,  NULL, NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_lockpick',    'Lockpick Set',        'gear',          'survival',   'gear',   1, 700,   0, 20,  'Opens locked chests and doors', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_firekit',     'Fire Starting Kit',   'gear',          'survival',   'gear',   1, 180,   0, 15,  NULL, NULL);

-- ── Diving Gear ───────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('gear_scuba',       'Scuba Set',           'gear',          'diving',     'gear',   2, 3500,  0, 50,  'Unlimited underwater breathing', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_rebreather',  'Rebreather',          'gear',          'diving',     'gear',   1, 2200,  0, 40,  '30 min underwater breathing', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_wetsuit',     'Wetsuit',             'gear',          'diving',     'gear',   1, 1200,  0, 60,  'Cold water protection + swim speed +20%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_flippers',    'Dive Flippers',       'gear',          'diving',     'gear',   1, 400,   0, 80,  'Swim speed +30%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_underwater_light', 'Dive Light',     'gear',          'diving',     'gear',   1, 350,   0, 60,  NULL, NULL);

-- ── Space / Hazard Suits ──────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('gear_spacesuit',   'EVA Space Suit',      'gear',          'space',      'gear',   3, 12000, 0, 30,  'Required for vacuum/radiation environments', '["near_future","mid_future","far_future"]');
INSERT OR IGNORE INTO item_catalog VALUES ('gear_hazmat',      'Hazmat Suit',         'gear',          'space',      'gear',   2, 5500,  0, 40,  'Chemical and biological protection', '["industrial","modern","near_future"]');
INSERT OR IGNORE INTO item_catalog VALUES ('gear_heatsuit',    'Thermal Suit',        'gear',          'space',      'gear',   2, 4200,  0, 45,  'Extreme heat protection', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('gear_coldsuit',    'Arctic Survival Suit','gear',          'space',      'gear',   2, 3800,  0, 45,  'Extreme cold protection', NULL);

-- ── Consumables ───────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('con_healthkit',    'Health Kit',          'consumable',    'medical',    'consumable', 1, 350, 1, NULL, 'Restores 50 HP', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_bandage',      'Bandage Roll',        'consumable',    'medical',    'consumable', 1, 120, 1, NULL, 'Restores 20 HP over 10s', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_antidote',     'Antidote',            'consumable',    'medical',    'consumable', 1, 280, 1, NULL, 'Cures poison and disease', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_stimpack',     'Stim Pack',           'consumable',    'medical',    'consumable', 1, 500, 1, NULL, 'Speed +40% for 30s', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_ration',       'Field Ration',        'consumable',    'food',       'consumable', 1, 90,  1, NULL, 'Restores stamina', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_water',        'Canteen',             'consumable',    'food',       'consumable', 1, 60,  1, NULL, 'Hydration restore', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('con_energydrink',  'Energy Drink',        'consumable',    'food',       'consumable', 1, 200, 1, NULL, 'Stamina +100% for 60s', NULL);

-- ── Containers / Backpacks ────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('bag_starter',      'Starter Pack',        'container',     'backpack',   'container', 0, 0,    0, NULL, '16 slots — default loadout', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('bag_daypack',      'Day Pack',            'container',     'backpack',   'container', 0, 2000, 0, NULL, '+8 slots (24 total)', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('bag_rucksack',     'Military Rucksack',   'container',     'backpack',   'container', 0, 5000, 0, NULL, '+16 slots (32 total)', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('bag_cargo',        'Cargo Box (Wearable)','container',     'backpack',   'container', 0, 9000, 0, NULL, '+24 slots (40 total)', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('bag_expedition',   'Expedition Frame',    'container',     'backpack',   'container', 0, 15000,0, NULL, '+32 slots (48 total) — slows movement', NULL);

-- ── Pets ──────────────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('pet_raptor',       'Raptor Companion',    'pet',           'dinosaur',   'pet',    0, 25000, 0, NULL, '+8 carry slots, attacks nearby enemies', '["deep_prehistoric","prehistoric"]');
INSERT OR IGNORE INTO item_catalog VALUES ('pet_wolf',         'Wolf Companion',      'pet',           'animal',     'pet',    0, 18000, 0, NULL, '+4 carry slots, detects enemies within 40m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('pet_drone',        'Scout Drone',         'pet',           'tech',       'pet',    0, 22000, 0, 60,  '+6 carry slots, aerial recon 80m radius', '["modern","near_future","mid_future"]');
INSERT OR IGNORE INTO item_catalog VALUES ('pet_horse',        'War Horse',           'pet',           'animal',     'pet',    0, 20000, 0, NULL, '+12 carry slots, mounted movement speed', '["ancient","medieval","renaissance","industrial"]');

-- ── Vehicles ──────────────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('veh_jeep',         'Armored Jeep',        'vehicle',       'land',       'vehicle', 0, 35000, 0, 80,  '+20 carry slots, fast land travel', '["modern","near_future"]');
INSERT OR IGNORE INTO item_catalog VALUES ('veh_raft',         'Inflatable Raft',     'vehicle',       'water',      'vehicle', 0, 8000,  0, 40,  '+10 carry slots, river/sea travel', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('veh_glider',       'Hang Glider',         'vehicle',       'air',        'vehicle', 0, 12000, 0, 50,  '+6 carry slots, aerial traversal', NULL);

-- ── Upgrades catalog ──────────────────────────────────────────────────────────
INSERT OR IGNORE INTO item_catalog VALUES ('upg_carry_1',      'Carry Capacity I',    'upgrade',       'character',  'upgrade', 0, 3000,  0, NULL, '+4 base inventory slots', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_carry_2',      'Carry Capacity II',   'upgrade',       'character',  'upgrade', 0, 7000,  0, NULL, '+8 base inventory slots', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_carry_3',      'Carry Capacity III',  'upgrade',       'character',  'upgrade', 0, 14000, 0, NULL, '+12 base inventory slots', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_extract_1',    'Extraction Speed I',  'upgrade',       'character',  'upgrade', 0, 4000,  0, NULL, 'Beacon channel time -15%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_extract_2',    'Extraction Speed II', 'upgrade',       'character',  'upgrade', 0, 9000,  0, NULL, 'Beacon channel time -30%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_loot_1',       'Loot Radius I',       'upgrade',       'character',  'upgrade', 0, 3500,  0, NULL, 'Metal detector range +10m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_loot_2',       'Loot Radius II',      'upgrade',       'character',  'upgrade', 0, 8000,  0, NULL, 'Metal detector range +25m', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_durability_1', 'Gear Durability I',   'upgrade',       'character',  'upgrade', 0, 5000,  0, NULL, 'All gear durability +25%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_durability_2', 'Gear Durability II',  'upgrade',       'character',  'upgrade', 0, 11000, 0, NULL, 'All gear durability +50%', NULL);
INSERT OR IGNORE INTO item_catalog VALUES ('upg_repair_1',     'Field Repair I',      'upgrade',       'character',  'upgrade', 0, 6000,  0, NULL, 'Repair cost -20%', NULL);
