@echo off
:: Treasure Royale Hub — Deploy to Cloudflare Production

echo ============================================
echo  Treasure Royale Hub — Deploy to Production
echo ============================================
echo.

:: Apply schema to remote D1 (safe — uses IF NOT EXISTS)
echo Step 1: Applying schema to remote D1...
wrangler d1 execute TR_DB --file=schema.sql
if %errorlevel% neq 0 (
    echo ERROR: Schema failed. Check database_id in wrangler.toml.
    pause
    exit /b 1
)
echo Schema applied.
echo.

:: Seed catalog (uses INSERT OR IGNORE — safe to re-run)
echo Step 2: Seeding item catalog...
wrangler d1 execute TR_DB --file=seed_items.sql
if %errorlevel% neq 0 (
    echo ERROR: Seed failed.
    pause
    exit /b 1
)
echo Catalog seeded.
echo.

:: Deploy worker
echo Step 3: Deploying worker...
wrangler deploy
if %errorlevel% neq 0 (
    echo ERROR: Worker deploy failed.
    pause
    exit /b 1
)
echo.
echo ============================================
echo  Deployed! Update Unity TR_HubApi.workerBaseUrl
echo  to your worker URL shown above.
echo ============================================
pause
