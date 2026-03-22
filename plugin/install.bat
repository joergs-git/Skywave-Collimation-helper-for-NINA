@echo off
setlocal enabledelayedexpansion
title AstroCircular SkyWaver — NINA Plugin Installer
echo.
echo  ============================================
echo   AstroCircular SkyWaver for N.I.N.A.
echo   Plugin Installer v0.1.0
echo   by joergsflow
echo  ============================================
echo.

:: ── Find the DLL ──
set "DLL_PATH=%~dp0AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll"
if not exist "!DLL_PATH!" set "DLL_PATH=%~dp0NINA.AstroCircular.SkyWaver.dll"
if not exist "!DLL_PATH!" (
    echo  [ERROR] Cannot find NINA.AstroCircular.SkyWaver.dll
    echo  Make sure install.bat is next to the DLL or the
    echo  AstroCircular.SkyWaver subfolder.
    echo.
    pause
    exit /b 1
)
echo  Found: !DLL_PATH!
echo.

:: ── Detect NINA plugin directory ──
set "NINA_DIR="
if exist "%localappdata%\NINA\3\Plugins" (
    set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
) else if exist "%localappdata%\NINA\Plugins" (
    set "NINA_DIR=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
)

if not defined NINA_DIR (
    echo  Could not auto-detect NINA Plugins folder.
    echo.
    echo  Enter the full path to your NINA Plugins folder
    echo  (e.g. C:\Users\YourName\AppData\Local\NINA\3\Plugins):
    echo.
    set /p "PLUGINS_DIR=  Path: "
    set "NINA_DIR=!PLUGINS_DIR!\AstroCircular.SkyWaver"
)

echo  Target: !NINA_DIR!

:: ── Check for existing installation ──
if exist "!NINA_DIR!\NINA.AstroCircular.SkyWaver.dll" (
    echo.
    echo  Previous installation found — will be updated.
)
echo.

:: ── Create directory if needed ──
if not exist "!NINA_DIR!" (
    mkdir "!NINA_DIR!" 2>NUL
    if errorlevel 1 (
        echo  [ERROR] Cannot create: !NINA_DIR!
        echo  Try running as Administrator.
        echo.
        pause
        exit /b 1
    )
)

:: ── Copy DLL (overwrite existing) ──
echo  Copying plugin DLL...
copy /Y "!DLL_PATH!" "!NINA_DIR!\" >NUL 2>&1
if errorlevel 1 (
    echo.
    echo  [ERROR] Failed to copy DLL.
    echo    FROM: !DLL_PATH!
    echo    TO:   !NINA_DIR!\
    echo.
    echo  If N.I.N.A. is running, close it first and retry.
    echo.
    pause
    exit /b 1
)

echo.
echo  ============================================
echo   [OK] Installed successfully!
echo  ============================================
echo.
echo  Location: !NINA_DIR!
echo.
echo  Start N.I.N.A. and go to:
echo    Options ^> Plugins     — to configure settings
echo    Advanced Sequencer    — find "AstroCircular SKW"
echo.
echo  Available sequence instructions:
echo    - SKW Collimation Run   (full one-click workflow)
echo    - SKW Defocus
echo    - SKW Circular Capture
echo    - SKW Integrate Frames
echo.
pause
