# Historical Anchor Points — Timeline Matchmaking Database

## How It Works

A player picks any year. The system finds the nearest **anchor point** and assigns them to that anchor's matchmaking bucket. Players within the same bucket share a server regardless of the exact year they picked.

```
Player picks 3220 BCE
  → Nearest anchor: 3200 BCE (Early Dynastic Egypt)
  → Bucket: "ancient_egypt_early_dynastic"
  → Joins or creates room tagged with that bucketId
  → universeSeed = hash(bucketId + roomId) → unique map variant per room
```

The bucket range defines how wide the net is. Tighter ranges = more historically specific but fewer players per server. Wider = more populated but less thematic precision.

---

## Anchor Point Table

### DEEP PREHISTORIC ERA (Everything before 3,000,000 BCE)

> Single unified bucket. Any year older than 3,000,000 BCE lands here regardless of exact input. The universeSeed determines which of 5 world variants players get — Cretaceous Jungle, Volcanic Hellscape, Ice Age Crossover, Swamp World, or Hominid Warzone. Dinosaurs and cavemen coexist via multiverse divergence. See [[deep-prehistoric-era]] for full design.

| Anchor Year | Bucket Range | Bucket ID | Display Label | Variants |
|---|---|---|---|---|
| Any year < 3,000,000 BCE | Unlimited | `deep_prehistoric` | "Earth — The Before Times" | Cretaceous Jungle, Volcanic Hellscape, Ice Age Crossover, Swamp World, Hominid Warzone |

---

### PREHISTORIC ERA (3,000,000 BCE – 10,001 BCE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 3,000,000 BCE | ±500,000 yrs | Homo habilis emerges, first stone tools | East African savanna, rocky outcrops | Oldowan chopper, bone fragment, ochre pigment |
| 1,800,000 BCE | ±300,000 yrs | Homo erectus, controlled fire | Cave mouth, ash pits, dense forest | Hand axe, fire-hardened spear, shell bead |
| 400,000 BCE | ±100,000 yrs | Neanderthal peak, cave burials | European cave network, tundra edge | Flint blade, burial ochre, bear skull totem |
| 70,000 BCE | ±15,000 yrs | Toba catastrophe, human bottleneck | Volcanic ashscape, sparse jungle | Obsidian shard, survival kit, cave painting fragment |
| 40,000 BCE | ±8,000 yrs | Homo sapiens reach Europe, cave art explosion | Lascaux-style cave, open plains | Cave painting pigment, ivory figurine, bone flute |
| 20,000 BCE | ±5,000 yrs | Last Glacial Maximum, ice sheets | Frozen tundra, mammoth hunting grounds | Mammoth ivory carving, atlatl, fur-wrapped bundle |
| 12,000 BCE | ±2,000 yrs | Younger Dryas, megafauna extinction begins | Melting ice edge, wetlands | Clovis point, giant sloth bone, amber pendant |
| 10,500 BCE | ±1,000 yrs | Göbekli Tepe construction begins | Stone circle construction site, hilltop | Carved pillar fragment, ritual vessel, animal totem |

---

### ANCIENT ERA (10,000 BCE – 500 CE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 9,500 BCE | ±500 yrs | First permanent settlements, Natufian culture | Levant village, grain storage pits | Sickle blade, grinding stone, clay figurine |
| 7,500 BCE | ±500 yrs | Çatalhöyük at peak, 8,000 residents | Dense mudbrick city, rooftop pathways | Bull horn shrine, obsidian mirror, woven textile |
| 5,500 BCE | ±500 yrs | Copper Age begins, Vinča culture | Balkan settlement, copper smelting pits | Copper axe, proto-writing tablet, gold bead |
| 3,200 BCE | ±150 yrs | Early Dynastic Egypt, first pharaohs, hieroglyphs invented | Nile delta palace complex, reed boats | Narmer Palette replica, faience amulet, papyrus scroll |
| 3,100 BCE | ±150 yrs | Uruk period Mesopotamia, first cities, cuneiform | Ziggurat city, irrigation canals | Cylinder seal, clay tablet, lapis lazuli pendant |
| 2,560 BCE | ±100 yrs | Great Pyramid of Giza under construction | Active pyramid construction site, desert | Architect's papyrus, diorite statue fragment, gold canopic jar |
| 2,334 BCE | ±100 yrs | Akkadian Empire, Sargon of Akkad | Mesopotamian walled city, siege aftermath | Royal stele fragment, bronze sword, tribute chest |
| 1,750 BCE | ±100 yrs | Code of Hammurabi, Babylon at peak | Babylonian palace, market district | Law stele fragment, merchant ledger, silver shekel |
| 1,350 BCE | ±75 yrs | Tutankhamun's Egypt, New Kingdom peak | Valley of the Kings tomb complex | Golden death mask replica, canopic chest, scarab ring |
| 1,200 BCE | ±75 yrs | Bronze Age Collapse, civilizations fall simultaneously | Burning palace ruins, refugee camps | Broken bronze sword, Linear B tablet, sea peoples helmet |
| 776 BCE | ±50 yrs | First Olympic Games, Greek city-states | Olympia stadium, Greek agora | Olive wreath, discus, amphora with athlete scene |
| 509 BCE | ±50 yrs | Roman Republic founded | Early Rome, Forum construction | Roman fasces, toga clasp, wax writing tablet |
| 334 BCE | ±40 yrs | Alexander the Great's conquests begin | Persian palace, Greek-Macedonian camp | Macedonian sarissa tip, Persian gold daric, Alexander coin |
| 221 BCE | ±40 yrs | Qin Shi Huang unifies China, Great Wall begins | Chinese imperial palace, terracotta workshop | Terracotta soldier fragment, bronze crossbow trigger, jade burial suit piece |
| 44 BCE | ±30 yrs | Julius Caesar assassinated, Roman civil war | Roman Senate, military camp | Caesar's laurel wreath, gladius, Roman standard |
| 1 CE | ±30 yrs | Roman Empire at height, Pax Romana | Roman city, colosseum district | Roman coin hoard, gladiator helmet, marble bust |
| 79 CE | ±20 yrs | Eruption of Vesuvius, Pompeii buried | Pompeii mid-eruption, ash streets | Pompeii fresco fragment, Roman jewelry, carbonized bread |
| 300 CE | ±50 yrs | Fall of Rome beginning, Constantine era | Late Roman fortress, Christian catacombs | Chi-Rho amulet, Roman gold solidus, legionnaire's kit |

---

### MEDIEVAL ERA (500 CE – 1499 CE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 570 CE | ±30 yrs | Birth of Muhammad, Arabian Peninsula | Mecca merchant city, desert caravan route | Merchant ledger, Arabian dagger, incense burner |
| 618 CE | ±40 yrs | Tang Dynasty China, Silk Road peak | Chang'an imperial city, silk market | Tang horse figurine, silk bolt, bronze mirror |
| 793 CE | ±30 yrs | Viking Age begins, Lindisfarne raid | Norse longship, coastal monastery raid | Viking sword, runestone fragment, monastery chalice |
| 800 CE | ±30 yrs | Charlemagne crowned Holy Roman Emperor | Frankish palace, cathedral construction | Carolingian crown jewel, illuminated manuscript, knight's sword |
| 1066 CE | ±20 yrs | Norman Conquest of England | English castle siege, battlefield | Norman sword, Bayeux Tapestry fragment, chainmail coif |
| 1095 CE | ±20 yrs | First Crusade launched | Jerusalem siege, crusader camp | Crusader cross, Saracen scimitar, holy relic box |
| 1206 CE | ±25 yrs | Genghis Khan unifies Mongols | Mongol steppe camp, burning city | Mongol composite bow, yurt artifact, silk road plunder |
| 1215 CE | ±15 yrs | Magna Carta signed | English castle, baron's council | Magna Carta seal, baron's ring, royal decree scroll |
| 1347 CE | ±20 yrs | Black Death sweeps Europe | Plague-ravaged European city, mass graves | Plague doctor mask, apothecary chest, death toll ledger |
| 1368 CE | ±25 yrs | Ming Dynasty founded, Forbidden City planned | Chinese imperial construction site | Ming vase, imperial edict scroll, jade dragon |
| 1415 CE | ±15 yrs | Battle of Agincourt, Hundred Years War | French battlefield, English longbow lines | English longbow, French knight's armor piece, battle standard |
| 1440 CE | ±20 yrs | Aztec Empire at peak, Tenochtitlan | Aztec pyramid city, market | Obsidian sacrificial blade, jade mask, cacao currency |
| 1453 CE | ±15 yrs | Fall of Constantinople, end of Byzantine Empire | Constantinople walls under siege | Byzantine icon, Ottoman cannon fragment, Greek fire vessel |

---

### RENAISSANCE & EARLY MODERN ERA (1500 CE – 1799 CE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 1492 CE | ±10 yrs | Columbus reaches Americas, Age of Exploration | Spanish port, Caribbean island landing | Navigator's astrolabe, Spanish doubloon, indigenous gold idol |
| 1517 CE | ±10 yrs | Martin Luther's 95 Theses, Reformation | German cathedral town, printing press shop | Luther's theses broadsheet, papal indulgence, printing block |
| 1543 CE | ±10 yrs | Copernicus publishes heliocentric model | Renaissance scholar's study, observatory | Copernicus manuscript, brass orrery, telescope prototype |
| 1588 CE | ±10 yrs | Spanish Armada defeated | English coastal fort, naval battle aftermath | Armada cannon, English privateer's map, naval compass |
| 1600 CE | ±15 yrs | East India Company founded, global trade begins | London dockyard, spice warehouse | East India Company charter, spice chest, merchant's ledger |
| 1620 CE | ±10 yrs | Mayflower voyage, Plymouth Colony | Pilgrim ship, New England coast | Mayflower compact copy, musket, native wampum belt |
| 1687 CE | ±10 yrs | Newton publishes Principia Mathematica | Cambridge university, Royal Society | Newton's Principia first edition, prism, reflecting telescope |
| 1776 CE | ±10 yrs | American Revolution, Declaration of Independence | Philadelphia colonial city, battlefield | Declaration of Independence copy, Continental musket, liberty bell fragment |
| 1789 CE | ±8 yrs | French Revolution, storming of the Bastille | Paris streets, Bastille fortress | Revolutionary cockade, guillotine blade fragment, Declaration of Rights |

---

### INDUSTRIAL ERA (1800 CE – 1949 CE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 1815 CE | ±8 yrs | Battle of Waterloo, Napoleon's defeat | Belgian battlefield, French imperial camp | Napoleon's hat replica, musket, battle map |
| 1848 CE | ±8 yrs | Year of Revolutions across Europe | Barricaded European city, parliament | Revolutionary pamphlet, pistol, workers' banner |
| 1865 CE | ±5 yrs | American Civil War ends, Lincoln assassinated | Washington DC, battlefield ruins | Lincoln's top hat replica, Confederate sword, Emancipation Proclamation |
| 1876 CE | ±5 yrs | Telephone invented, Industrial Revolution peak | Victorian factory, inventor's workshop | Bell's telephone prototype, steam engine part, patent document |
| 1903 CE | ±5 yrs | Wright Brothers first flight | Kitty Hawk airfield, early aviation workshop | Wright Flyer component, aviation goggles, flight log |
| 1912 CE | ±4 yrs | Titanic sinks, WWI approaching | Titanic wreck site, Edwardian port | Titanic first-class artifact, life preserver, captain's log |
| 1917 CE | ±3 yrs | WWI peak, Russian Revolution | Western Front trenches, Petrograd streets | WWI gas mask, Bolshevik pamphlet, trench art |
| 1929 CE | ±4 yrs | Wall Street Crash, Great Depression begins | New York financial district, Hooverville | Stock ticker tape, breadline token, gangster's ledger |
| 1939 CE | ±3 yrs | WWII begins | European city under occupation, bunker | Enigma machine fragment, WWII dog tags, resistance radio |
| 1945 CE | ±3 yrs | WWII ends, atomic age begins | Hiroshima aftermath, V-E Day celebration | Atomic age document, victory medal, occupation currency |

---

### MODERN ERA (1950 CE – 2024 CE)

| Anchor Year | Bucket Range | Event | Map Theme | Key Artifacts |
|---|---|---|---|---|
| 1957 CE | ±4 yrs | Sputnik launched, Space Race begins | Soviet launch facility, American suburb | Sputnik replica, NASA mission patch, Cold War propaganda |
| 1963 CE | ±3 yrs | JFK assassination, Cold War peak | Dallas street, Berlin Wall | Classified CIA document, Cold War spy kit, protest sign |
| 1969 CE | ±3 yrs | Moon landing, Woodstock | NASA mission control, festival grounds | Apollo mission log, moon rock sample, counterculture artifact |
| 1989 CE | ±3 yrs | Berlin Wall falls, Cold War ends | Berlin Wall, Eastern European city | Wall fragment, Stasi file, protest candle |
| 2001 CE | ±2 yrs | 9/11, War on Terror begins | New York skyline, Kabul outskirts | First responder badge, classified intelligence brief, satellite phone |
| 2008 CE | ±2 yrs | Global financial crisis, smartphone era | Financial district, Silicon Valley | Lehman Brothers document, first-gen iPhone, bailout memo |
| 2020 CE | ±2 yrs | COVID-19 pandemic, global lockdown | Empty city, field hospital | Pandemic research data, vaccine vial, lockdown protest sign |

---

## Matchmaking Logic Summary

```
function getAnchorBucket(yearInput):
    if yearInput < -3,000,000:
        return "deep_prehistoric"
    find anchor where abs(yearInput - anchor.year) <= anchor.bucketRange
    if multiple matches → pick closest anchor
    if no match → assign to nearest anchor regardless of range
    return anchor.bucketId
```

### Key design decisions
- **Deep prehistoric is a catch-all** — anything older than 3M BCE is one bucket, universeSeed picks the world variant
- **Overlapping ranges are intentional** — a player picking 3150 BCE could match with 3200 BCE (Egypt) or 3100 BCE (Uruk). Closest anchor wins.
- **Bucket range shrinks as history gets denser** — prehistoric buckets are ±500,000 years, modern era is ±2 years
- **universeSeed is per-room, not per-bucket** — two rooms in the same bucket get different map variants, keeping each match fresh
- **The multiverse justifies everything** — same anchor, different seed = parallel universe of the same historical moment
