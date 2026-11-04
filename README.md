# ERA Bound

A Unity multiplayer FPS where players traverse randomized historical Earth timelines to loot ancient artifacts.

## CI/CD — Auto Deploy to itch.io

Every push to `main` automatically builds and deploys to [erabound-us.itch.io/era-bound](https://erabound-us.itch.io/era-bound).

---

## First-Time Setup

### 1. Create the itch.io game page
1. Go to [itch.io](https://itch.io) → Create new project
2. Set the URL slug to `era-bound`
3. Set kind to **Executable**
4. Save (can be draft — butler will upload regardless)

### 2. Get your itch.io API key
1. itch.io → Account Settings → API keys → Generate new API key
2. Copy the key — you'll add it as a GitHub secret

### 3. Get your Unity license secret (one-time)
1. Push this repo to GitHub as `erabound-us/era-bound`
2. Go to **Actions** → **Get Unity License File** → **Run workflow**
3. Download the `.alf` artifact it produces
4. Go to [license.unity3d.com](https://license.unity3d.com/manual) and upload the `.alf`
5. Download the `.ulf` license file it gives back
6. Open the `.ulf` in a text editor, copy all contents

### 4. Add GitHub Secrets
Go to your repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

| Secret name | Value |
|---|---|
| `UNITY_LICENSE` | Full contents of the `.ulf` file |
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password |
| `BUTLER_API_KEY` | Your itch.io API key |

### 5. Push to main
Any push to `main` will now:
1. Build ERA Bound for Windows (StandaloneWindows64)
2. Upload to itch.io channel `windows` via butler
3. Build number increments automatically with each run

---

## Manual Deploy
Go to **Actions** → **Build and Deploy to itch.io** → **Run workflow**

## Unity Version
`2021.3.45f1` — update `get-unity-license.yml` if you upgrade Unity.
