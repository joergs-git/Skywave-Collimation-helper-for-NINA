@echo off
title AstroCircular SkyWaver — NINA Plugin Installer
echo.
echo  ============================================
echo   AstroCircular SkyWaver for N.I.N.A.
echo   Plugin Installer v0.1.0
echo   by joergsflow
echo  ============================================
echo.

:: Detect NINA plugin directory
set "NINA_DIR=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"

:: Fallback for older NINA installs
if not exist "%localappdata%\NINA\3" (
    set "NINA_DIR=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
)

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
    mkdir "%NINA_DIR%"
)

:: Copy DLL
echo  Installing plugin DLL...
copy /Y "%~dp0NINA.AstroCircular.SkyWaver.dll" "%NINA_DIR%\" >NUL
if errorlevel 1 (
    echo.
    echo  [ERROR] Failed to copy plugin DLL.
    echo  Check permissions for: %NINA_DIR%
    echo.
    pause
    exit /b 1
)

echo.
echo  [OK] AstroCircular SkyWaver installed successfully!
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
