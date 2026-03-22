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

set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
if not exist "%NINA_DIR%" (
    set "NINA_DIR=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
)

if exist "%NINA_DIR%" (
    echo  Removing: %NINA_DIR%
    rmdir /S /Q "%NINA_DIR%"
    echo.
    echo  [OK] AstroCircular SkyWaver uninstalled.
) else (
    echo  Plugin not found. Nothing to remove.
)
echo.
pause
