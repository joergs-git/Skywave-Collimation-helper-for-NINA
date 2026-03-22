@echo off
title AstroCircular SkyWaver — NINA Plugin Installer
echo.
echo  ============================================
echo   AstroCircular SkyWaver for N.I.N.A.
echo   Plugin Installer v0.1.0
echo   by joergsflow
echo  ============================================
echo.

:: Find the DLL — it should be in AstroCircular.SkyWaver subfolder next to this bat
set "DLL_PATH=%~dp0AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll"
if not exist "%DLL_PATH%" (
    :: Maybe DLL is directly next to the bat
    set "DLL_PATH=%~dp0NINA.AstroCircular.SkyWaver.dll"
)
if not exist "%DLL_PATH%" (
    echo  [ERROR] Cannot find NINA.AstroCircular.SkyWaver.dll
    echo  Make sure install.bat is in the same folder as the DLL
    echo  or the AstroCircular.SkyWaver subfolder.
    echo.
    pause
    exit /b 1
)
echo  Found DLL: %DLL_PATH%

:: Detect NINA plugin directory — try all known paths
set "NINA_DIR="

:: NINA 3.x with version subfolder
if exist "%localappdata%\NINA\3" (
    set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
    goto :found
)

:: NINA 3.x CoreCLR layout
if exist "%programfiles%\N.I.N.A" (
    set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
    goto :found
)

:: NINA without version subfolder
if exist "%localappdata%\NINA" (
    set "NINA_DIR=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
    goto :found
)

:: Nothing found — ask user
echo  [!] Could not auto-detect NINA installation.
echo.
echo  Please enter the full path to your NINA Plugins folder:
echo  (e.g. C:\Users\YourName\AppData\Local\NINA\3\Plugins)
echo.
set /p PLUGINS_DIR="Plugins path: "
set "NINA_DIR=%PLUGINS_DIR%\AstroCircular.SkyWaver"

:found

echo  Target: %NINA_DIR%
echo.

:: Check if NINA is running
tasklist /FI "IMAGENAME eq N.I.N.A.exe" 2>NUL | find /I "N.I.N.A.exe" >NUL
if not errorlevel 1 (
    echo  [!] N.I.N.A. is currently running.
    echo      Please close N.I.N.A. before installing.
    echo.
    pause
    exit /b 1
)

:: Create plugin directory
if not exist "%NINA_DIR%" (
    echo  Creating plugin directory...
    mkdir "%NINA_DIR%" 2>NUL
    if errorlevel 1 (
        echo  [ERROR] Failed to create directory: %NINA_DIR%
        echo  Try running as Administrator, or create the folder manually.
        echo.
        pause
        exit /b 1
    )
)

:: Copy DLL
echo  Installing plugin DLL...
copy /Y "%DLL_PATH%" "%NINA_DIR%\" >NUL 2>&1
if errorlevel 1 (
    echo.
    echo  [ERROR] Failed to copy plugin DLL to: %NINA_DIR%
    echo.
    echo  Try manually copying:
    echo    FROM: %DLL_PATH%
    echo    TO:   %NINA_DIR%\
    echo.
    pause
    exit /b 1
)

echo.
echo  [OK] AstroCircular SkyWaver installed successfully!
echo  Installed to: %NINA_DIR%
echo.
echo  Next steps:
echo    1. Start N.I.N.A.
echo    2. Go to Options ^> Plugins to configure settings
echo    3. In Advanced Sequencer, find "AstroCircular SKW" category
echo.
echo  Sequence instructions available:
echo    - SKW Defocus
echo    - SKW Circular Capture
echo    - SKW Integrate Frames
echo    - SKW Collimation Run (full workflow)
echo.
pause
