# Changelog

All notable changes to Collimation Helper for SkyWave will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [2.1.2] - 2026-04-26

Triggered by a user report (Rams) on a Pegasus NYX-101 mount: plate-solve at the centre star reported success, but the imaging sequence still started from a totally different point near Regulus. Investigation showed the centring step was fine — the failure was downstream at the defocused ring slews, which by design cannot be plate-solved and depend entirely on the mount driver's epoch handling. Also tightened the toggle-row layout in the panel after feedback that labels visually attached to the wrong checkbox, and replaced the `1:1` zoom-reset button with a clearer fit-to-frame icon.

### Added
- **Mount sync after successful Center step** — once the Center instruction has plate-solved and physically parked the scope on the target star, the plugin now calls `telescopeMediator.Sync()` with the J2000 target coordinates. This anchors the mount's internal position model to the plate-solved truth so that subsequent blind ring slews (which happen while defocused and cannot plate-solve) start from a corrected reference point. Mitigates fixed-offset epoch quirks in ASCOM drivers like Pegasus NYX-101 and 10Micron. Sync is best-effort: drivers that don't support it log a warning and the run continues unchanged.

### Changed
- **Wider gap between toggle groups** — the four checkboxes in the panel (Center / AF first / Crop / Del subs) had a 12 px gap between groups but only 4 px between checkbox and its own label, which made labels visually attach to the next group's checkbox. Group spacing is now 36 px so each label clearly belongs to its own toggle.
- **Zoom-to-fit button now uses a fit-frame icon** — the `1:1` reset button on the camera-preview thumbnail is renamed to `▭` with a "Zoom to fit" tooltip. Behaviour is unchanged (resets `PreviewZoom` to 1.0 = fit), only the symbol matches the user's mental model: `−` zoom out, `+` zoom in, `▭` fit.

## [2.1.1] - 2026-04-23

Triggered by a user report (Jeff Morgan) of a consistent ~8–12 arcmin pattern offset on a 10Micron mount. Investigation showed the SKW helper's J2000 coordinates are correct (matches SIMBAD for λ Dra); the offset is caused by the mount's ASCOM driver misreporting its equatorial system (J2000 vs JNOW), which made NINA apply the wrong precession transformation. The plugin couldn't fix the driver bug, but it could have warned the user — instead it silently fell back to a blind slew when plate-solving failed, which is exactly the path that lets driver epoch bugs surface as pattern offsets.

### Fixed
- **Silent plate-solve fallback hid mount epoch bugs** — when the initial plate-solve centering step failed (wrong filter, no solver configured, solver timeout on dim/high-Dec stars), the plugin logged a quiet `Logger.Warning` and continued with a blind slew. The fallback status message was immediately overwritten by the next step, so users never realised they were running a blind pattern that inherited any ASCOM driver epoch mismatch (known 10Micron quirk). The fallback now fires a NINA `Notification.ShowWarning` toast explicitly calling out the epoch-offset risk and leaves a persistent `WARNING:` prefix in the status text.

## [2.1.0] - 2026-04-14

Focused on southern-hemisphere observers. Addresses a user report from Australia/NZ where "Find Best" was picking stars below the horizon, the capture filter was sometimes left on the plate-solve filter, and there was no way to run the ring pattern from an arbitrary point in the sky.

### Added
- **"Use Mount" button** — reads the mount's current RA/Dec and loads it as the target, so you can slew manually via Stellarium / framing wizard / hand controller and collimate from any point in the sky, bypassing the catalog. Essential when the preset catalog has no good option for your latitude and time.
- **14 new southern winter/spring stars** closing the previous RA 16h–21h meridian-crossing gap: η CMa (Aludra), σ Pup, α Ant, δ Crv (Algorab), δ TrA, ζ² Sco, η Sco, β Ara, α Tel, δ Sgr (Kaus Meridionalis), π Sgr (Albaldah), α Ind, β Pav, γ Pav. Catalog total is now 120 stars.

### Fixed
- **"Find Best" could pick stars below the horizon** — the altitude check now enforces a 30° minimum (with fallback to 20° then 10°) and returns a clear "no star visible now" message if nothing passes, instead of silently returning the "least bad" candidate.
- **Capture filter sometimes left on the plate-solve filter** — `SwitchFilter("Default")` was a silent no-op, so after NINA's Center instruction switched filters internally for plate-solving, the plugin never switched back. The target filter is now pre-resolved at the start of the run (before any filter change) and always used explicitly for capture, guaranteeing deterministic filter state regardless of what NINA does internally.

### Changed
- **"Find Best" scoring rewards zenith passage** — stars are now ranked by `currentAltitude + transitAltitude - 90` (equivalent to `altitude - |latitude - declination|`). A star currently at 60° that will pass within 5° of zenith now beats a star currently at 70° that will set in an hour.

## [2.0.2] - 2026-04-10

### Fixed
- **Filter wheel always selecting Slot 0** — filter slot index was hardcoded to 0 in capture sequences, autofocus, and filter switch commands, causing the plugin to always use the L filter regardless of user selection. Now correctly looks up the filter's actual slot position from the NINA profile.

## [2.0.1] - 2026-04-08

### Added
- **26 southern hemisphere collimation stars** (Dec -25° to -79°) — catalog now covers both hemispheres with 106 stars total
- Southern circumpolar targets near the south celestial pole (β Hyi, γ Hyi, α Aps at Dec -77° to -79°)
- Stars in Hydrus, Reticulum, Phoenix, Grus, Columba, Pictor, Vela, Fornax, Horologium, Pyxis, Apus, Pavo, Centaurus

## [2.0.0] - 2026-04-08

### Added
- **80 collimation star presets** — expanded from 22 to 80 stars covering all seasons and declinations from -20° to +77°, using the curated SkyWave "Collimation Stars" list plus existing plugin presets
- Constellation field on each star preset — ComboBox now shows "Name (Constellation, mag X.X)" for easier identification
- Defocus setup hint banner in the UI explaining how to calculate the correct defocus steps from SkyWave's micron value and the focuser's µm/step ratio
- Detailed tooltips on Defocus, µm/step, and microns display fields with step-by-step calculation instructions

### Changed
- **Doubled sensor map and camera preview size** — both panels now 600px max width (was 300px), position dots scaled up proportionally, progress bar doubled
- **Zoomable camera preview** — zoom in up to 4× to inspect defocused donut detail while the routine runs, with +/−/1:1 controls and scroll-to-pan
- Star catalog now organized by season (Winter/Spring/Summer/Fall) with circumpolar stars at the top
- "Find Best" star selection benefits from 4× larger catalog — better coverage at any time of year
- **Focuser ratio corrected from "steps/µm" to "µm/step"** — the value represents microns of travel per focuser step, which depends on the specific focuser + OAZ combination (e.g. ZWO EAF + FeatherTouch OAZ ≈ 3 µm/step). Formula is now `steps × µm/step = total defocus µm`
- Preview thumbnail resolution doubled to 800px for crisp zoom

## [1.0.1] - 2026-03-27

### Fixed
- Plugin not loading after install from NINA plugin manager — ZIP archive now contains the DLL at the root level instead of inside a subfolder

### Changed
- CI now uses NINA's official `CreateManifest.ps1` script for manifest and archive generation
- All plugin metadata (descriptions, tags, URLs) moved into AssemblyInfo.cs assembly attributes as single source of truth

## [1.0.0] - 2026-03-27

First stable release for the official N.I.N.A. plugin repository.

### Added
- MAX stacking (each pixel keeps its maximum value across all frames)
- 22 star presets (mag 2–5, all seasons, mid-northern latitudes)
- "Find Best" star auto-selection based on time, location, and optical setup
- Magnitude advisor targeting ~60% ADU fill
- Steps/µm readout for focuser calibration
- Optional crop to ring pattern + 300px safety margin
- Optional autofocus before defocusing
- Camera preview with robust auto-stretch (median + MAD)
- Live sensor map with ring position progress
- Full cancellation support with focus/filter restoration
- Browser-based collimation helper for non-NINA users
- Plugin screenshots in README

### Changed
- Stacking mode changed from average to MAX — preserves every donut at full brightness
- Crop safety margin tripled from 100px to 300px for defocused stars on long sensor sides
- Star catalog expanded from 16 to 22 presets with brighter stars (down to mag 2)
- Renamed "Web Tool" / "Live demo" to "Browser-based Collimation Helper"

## [0.1.0] - 2026-03-22

### Added
- **HTML Tool (v5):** Web-based SKW Collimation Coordinator
  - Generates N.I.N.A. Advanced Sequencer JSON files for circular defocused star capture
  - Generates PixInsight integration scripts for combining sub-frames
  - 16 embedded star presets for all seasons (mid-northern latitudes)
  - Star finder with LST/altitude calculation and nautical dusk timing
  - Magnitude advisor based on optical setup
  - Sensor preview visualization with ring positions
  - Position table with RA/Dec coordinates

- **NINA Plugin (v0.1.0):** Native N.I.N.A. plugin scaffolding
  - `SkwDefocus` — Move focuser by configurable steps (extra/intra-focal)
  - `SkwCircularCapture` — Slew to N ring positions and capture defocused exposures
  - `SkwIntegrateFrames` — Native FITS averaging, crop-to-circle, bin 2x, 16-bit output
  - `SkwCollimationRun` — Full orchestrated workflow with try/finally refocus
  - Plugin options page with telescope, sensor, location, and imaging defaults
  - Star catalog with 16 embedded presets and FindBestStar algorithm
  - Coordinate utilities (HMS/DMS, LST, altitude, nautical dusk)
  - SVG icon with SkyWave-style wavefront rings and defocused star donuts
  - GitHub Actions CI for Windows builds

- **Project setup**
  - .gitignore, CHANGELOG, tasks/lessons.md, tasks/todo.md
  - GitHub Pages deployment for HTML tool
  - GPL-3.0 license
