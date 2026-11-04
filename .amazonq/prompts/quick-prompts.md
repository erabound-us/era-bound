# Quick Prompts — Treasure Royale

Paste these directly into Amazon Q chat.

## Restore Context After Token Reset
"I'm continuing work on Treasure Royale. Read @treasure-royale-rules and @treasure-royale-known-working before making any changes. The project is a Photon PUN2 Unity FPS with procedural era-based maps. Key working systems are documented in the known-working file — do not break them."

## Start a new Unity feature
"I'm working on Treasure Royale, a Photon PUN2 Unity FPS where players loot artifacts across multiverse timelines. Help me build [FEATURE]. Follow the project rules: use TextMeshPro, Newtonsoft.Json, check IsMasterClient for authority, keep scripts minimal. Do not change any system listed in treasure-royale-known-working.md."

## Fix a bug safely
"In Treasure Royale, I have this bug: [DESCRIBE BUG]. Before suggesting a fix, read @treasure-royale-known-working to understand what's currently working. Fix only the specific bug without changing any confirmed working system. If the fix would break a working system, find a different approach."

## Map/Movement change
"I need to change [SPECIFIC THING] in Treasure Royale's map/movement system. Read @treasure-royale-known-working first. After making the change, verify the regression checklist at the bottom of that file still passes. Document the new working state if anything changes."

## Start a new Lambda
"I'm building an AWS Lambda for Treasure Royale. Python 3.12, boto3, DynamoDB tables are: players (PK=pk, SK=sk), artifacts (PK=pk, SK=sk), matches (PK=pk, SK=sk). Build [FUNCTION NAME] that does [DESCRIPTION]. Keep it under 50 lines."

## Debug Photon issue
"In Treasure Royale (Photon PUN2 Unity), I have this issue: [DESCRIBE ISSUE]. The relevant script is [SCRIPT NAME]. Here's the code: [PASTE CODE]"

## Debug AWS issue
"In Treasure Royale's AWS backend (Lambda + DynamoDB + API Gateway), I have this issue: [DESCRIBE ISSUE]. Here's the relevant code or error: [PASTE]"

## Add a new artifact type
"Add a new artifact type to Treasure Royale for the [ERA] era. It needs: id, name, era, rarity, powerType (one of: None/Combat/Speed/Vision/Stealth), powerMagnitude (0.0-1.0), tradeValue. Generate 5 variants with slight multiverse differences."

## Add a new era map variant
"Design the prop swap variants for a new [ERA] era map in Treasure Royale. List 3 PropSets (each with 5-8 objects), 2 LightingProfiles, and an artifact spawn table with 8 artifacts appropriate to the era."

## Validate map before testing
"Run a map validation check on Treasure Royale. Check: spawn points not inside water triggers, world floor exists at y=-6, swim triggers have top at y=0, no drown triggers flagged as errors, all spawns at y=1.5 outside 80-unit center radius."
