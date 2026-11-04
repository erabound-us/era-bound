# Map Eras & Timelines

## Design Philosophy
- Each era has one **base map** (pre-built in Unity)
- `MapVariantManager` swaps prop sets, lighting, and spawn tables at runtime using `universeSeed`
- This keeps build size manageable while delivering a fresh experience each match

## Era Map Specs

### Prehistoric
- Base: dense jungle with cave network
- Variant swaps: megafauna (mammoths, saber-tooth), plant density, water level
- Artifacts: bone tools, cave paintings, primitive jewelry

### Ancient (Egypt / Mesopotamia / Indus)
- Base: desert temple complex
- Variant swaps: dynasty faction banners, sand storm intensity, tomb layout
- Artifacts: scarabs, papyrus scrolls, ceremonial blades, urns

### Medieval
- Base: European castle + village
- Variant swaps: siege state (intact vs. ruined), season (snow/summer), faction crests
- Artifacts: swords, chalices, illuminated manuscripts, armor pieces

### Renaissance
- Base: port city with cathedral
- Variant swaps: trade goods on docks, ship types, market stalls
- Artifacts: astrolabes, paintings, alchemical equipment

### Industrial
- Base: factory district + rail yard
- Variant swaps: operational vs. abandoned, era (steam vs. early electric)
- Artifacts: blueprints, prototype devices, pocket watches

### Modern
- Base: urban city block
- Variant swaps: city nationality, time of day, conflict level
- Artifacts: classified documents, tech prototypes, currency

### Future Earth
- Base: megacity ruins
- Variant swaps: collapse level, faction control, tech tier
- Artifacts: data cores, nano-devices, energy cells

## Map Variant Manager (Unity)
```csharp
// MapVariantManager.cs — called after scene load
public void ApplyVariant(int universeSeed) {
    Random.InitState(universeSeed);
    SwapPropSet(propSets[Random.Range(0, propSets.Length)]);
    SetLighting(lightingProfiles[Random.Range(0, lightingProfiles.Length)]);
    SpawnArtifacts(artifactTable, universeSeed);
}
```
