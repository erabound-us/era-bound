@echo off
:: Treasure Royale Hub — Local Dev Server
:: Starts worker at http://127.0.0.1:8788
:: In Unity, set TR_HubApi.workerBaseUrl = "http://127.0.0.1:8788"

echo ============================================
echo  Treasure Royale Hub — Local Dev
echo  Worker: http://localhost:8787
echo ============================================
echo.
echo Starting wrangler dev in a new window...
echo Close that window to stop the server.
echo.

start "TR Hub Worker" cmd /k "cd /d %~dp0 && wrangler dev --local"

echo Worker starting in new window.
echo.
echo Quick test endpoints:
echo   http://127.0.0.1:8788/player/test_player
echo   http://127.0.0.1:8788/shop/catalog
echo   http://127.0.0.1:8788/auction/listings?player_id=test_player
echo.
pause
