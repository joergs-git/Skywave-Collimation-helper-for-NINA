# AstroCircular SkyWaver — TODO

## Completed
- [x] Review HTML tool for correctness and NINA format compatibility
- [x] Plan native NINA plugin architecture
- [x] Plugin scaffolding — solution, csproj, AssemblyInfo, manifest
- [x] GitHub Actions CI + release zip with installer
- [x] Core math ported from JS (coordinates, ring calc, star catalog, magnitude)
- [x] Sequence instructions: SkwDefocus, SkwCircularCapture, SkwIntegrateFrames, SkwCollimationRun
- [x] Native FITS integration (FitsAverager, RawFitsWriter, FitsHeaderWriter)
- [x] Options page with settings UI
- [x] Installer fixed for NINA 3.x versioned plugin path (Plugins\3.0.0\)
- [x] Plugin loads and displays in NINA

## Open — Architecture Rework (v0.2.0)
- [ ] **Dockable panel** — Primary UI as IDockableVM in NINA's imaging tab (like HocusFocus)
  - Run button that executes full collimation workflow directly
  - Star picker with presets + manual RA/Dec
  - Progress/status display during run
  - No sequence building needed
- [ ] **Settings persistence** — Wire SkwSettings into NINA's plugin settings store
  - Currently resets to defaults on every restart
  - Research how HocusFocus/other plugins persist settings
- [ ] **Folder browser** — SkyWave output path uses folder picker dialog, not text input
- [ ] **Equipment auto-read** — Read focal length, pixel size, sensor size, lat/lon from
  NINA's connected equipment and profile (not just defaults)
- [ ] **PixInsight Tools integration** — Optional path using isbeorn's PixInsight Tools plugin
  for integration step (command-line + file-polling IPC pattern)
- [ ] **Plugin icon** — WPF path geometry for NINA sidebar (not just SVG for web)

## Open — HTML Tool Fixes
- [ ] Fix TakeExposure.ExposureCount: 0 → 1 in generated NINA JSON
- [ ] Fix PixInsight script: use windowById instead of activeWindow
- [ ] Fix PixInsight script: update XPIXSZ/XBINNING headers after bin2

## Key Learnings
- NINA 3.x plugins must go in `Plugins\3.0.0\` (one-time migration from root)
- ResourceDictionary: use programmatic pack URI loading, NOT x:Class/InitializeComponent
- No WPF Hyperlink elements (crash plugin load without RequestNavigate handler)
- FilterInfo constructor needs (string, int, short) — cast position to short
- BinningMode constructor needs (short, short) — cast binning to short
- IImageSaveMediator is in NINA.WPF.Base, not NINA.Equipment

## Results
v0.1.0 scaffolding complete (2026-03-22). Plugin loads in NINA, options page works.
Next: rearchitect as dockable panel tool for v0.2.0.
