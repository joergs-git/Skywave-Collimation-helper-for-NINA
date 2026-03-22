@echo off
title AstroCircular SkyWaver — Uninstaller
echo.
echo  ============================================
echo   AstroCircular SkyWaver — Uninstaller
echo  ============================================
echo.

:: Check if NINA is running
tasklist /FI "IMAGENAME eq N.I.N.A.exe" 2>NUL | find /I "N.I.N.A.exe" >NUL
if not errorlevel 1 (
    echo  [!] N.I.N.A. is currently running.
    echo      Please close N.I.N.A. before uninstalling.
    echo.
    pause
    exit /b 1
)

:: Try all known paths
set "FOUND="

if exist "%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver" (
    set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
    set "FOUND=1"
)
if exist "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver" (
    set "NINA_DIR=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
    set "FOUND=1"
)

if not defined FOUND (
    echo  Plugin not found in default locations. Nothing to remove.
    echo.
    pause
    exit /b 0
)

echo  Removing: %NINA_DIR%
rmdir /S /Q "%NINA_DIR%"
echo.
echo  [OK] AstroCircular SkyWaver uninstalled.
echo.
pause
