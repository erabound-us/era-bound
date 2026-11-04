@echo off
:: Treasure Royale Hub — One-Time Setup
:: Run this ONCE before dev.bat or deploy.bat

echo ============================================
echo  Treasure Royale Hub — First-Time Setup
echo ============================================
echo.

:: 1. Login check
echo Step 1: Checking Wrangler login...
wrangler whoami >nul 2>&1
if %errorlevel% neq 0 (
    echo Not logged in. Opening browser for Cloudflare login...
    wrangler login
) else (
    echo Already logged in.
)
echo.

:: 2. Create D1 database
echo Step 2: Creating D1 database "treasure-royale"...
wrangler d1 create treasure-royale 2>&1
echo.
echo ACTION REQUIRED: Copy the database_id printed above.
echo Then paste it into wrangler.toml replacing REPLACE_AFTER_CREATION
echo.
pause

:: 3. Run schema
echo Step 3: Running schema...
wrangler d1 execute TR_DB --local --file=schema.sql
if %errorlevel% neq 0 (
    echo ERROR: Schema failed. Check database_id in wrangler.toml and try again.
    pause
    exit /b 1
)
echo Schema applied.
echo.

:: 4. Seed item catalog
echo Step 4: Seeding NPC item catalog...
wrangler d1 execute TR_DB --local --file=seed_items.sql
if %errorlevel% neq 0 (
    echo ERROR: Seed failed.
    pause
    exit /b 1
)
echo Catalog seeded.
echo.

:: 5. Set Groq secret
echo Step 5: Setting GROQ_API_KEY secret...
echo Paste your Groq API key when prompted (same key as Thai4Family):
wrangler secret put GROQ_API_KEY
echo.

echo ============================================
echo  Setup complete! Run dev.bat to start local.
echo ============================================
pause
