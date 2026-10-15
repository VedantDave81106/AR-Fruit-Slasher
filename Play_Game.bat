@echo off
title AR 3D Fruit Slasher Launcher
cd /d "%~dp0"

echo ======================================================
echo    AR 3D Fruit Slasher - Quick Launcher
echo ======================================================
echo.
echo Launching your game in the browser...
echo.

:: Open default browser to local server
start http://localhost:3000

:: Start the server
node server.js

:: Fallback if node isn't running or encounters an error
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Node server encountered an issue. Opening HTML directly...
    start "" "%~dp0WebSimulator\index.html"
)
pause
