# Collimation Helper for SkyWave — TODO

## Completed
- [x] Review HTML tool for correctness and NINA format compatibility
- [x] Plan native NINA plugin architecture
- [x] Plugin scaffolding — solution, csproj, AssemblyInfo, manifest
- [x] GitHub Actions CI + release zip with installer
- [x] Core math ported from JS (coordinates, ring calc, star catalog, magnitude)
- [x] Sequence instructions: SkwDefocus, SkwCircularCapture, SkwIntegrateFrames, SkwCollimationRun
- [x] Native FITS integration (FitsAverager, RawFitsWriter, RawFitsReader, FitsHeaderWriter)
- [x] Options page with settings UI
- [x] Installer fixed for NINA 3.x versioned plugin path (Plugins\3.0.0\)
- [x] Plugin loads and displays in NINA
- [x] v0.2.0: Dockable panel with one-click Run Collimation
- [x] v0.2.0: Star picker with 16 presets and Find Best
- [x] v0.2.0: Sensor map, camera preview, all toggles, equipment auto-read
- [x] v0.2.0: Self-contained FITS pipeline (RawFitsWriter/Reader)
- [x] v1.0.0: MAX stacking (replaced average)
- [x] v1.0.0: 22 star presets (expanded from 16, down to mag 2)
- [x] v1.0.0: Steps/µm readout, magnitude advisor
- [x] v1.0.0: Crop safety margin tripled to 300px
- [x] v1.0.0: README rewrite with SkyWave background, screenshots, settings table
- [x] v1.0.0: Renamed "Web Tool" to "Browser-based Collimation Helper"
- [x] v1.0.0: Stable release v1.0.0 on GitHub
- [x] v1.0.0: Manifest PR submitted to isbeorn/nina.plugin.manifests (#383)
- [x] v1.0.0: Global git identity locked to joergsflow

## Open — Awaiting External
- [ ] **NINA manifest PR review** — isbeorn/nina.plugin.manifests#383, awaiting merge by NINA maintainers

## Open — Future Enhancements
- [ ] **PixInsight Tools integration** — Optional integration path using isbeorn's PixInsight Tools plugin for stacking
- [ ] **Verify output files** — Confirm integrated FITS loads correctly in SkyWave
- [ ] **Stacked image thumbnail** — Show preview of integrated FITS in completion popup
- [ ] **Pushover via Ground Station** — Optional notifications (autofocus prompt, job complete)

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
- Options page Settings object is disconnected from panel VM — use PluginOptionsAccessor
- NINA's SaveToDisk may change filenames/formats — use own RawFitsWriter instead
- CheckBox Content text is invisible in NINA's dark theme — use explicit TextBlock

## Results
- v0.1.0 (2026-03-22): Plugin scaffolding, sequence instructions, CI, installer
- v0.2.0 (2026-03-23): Dockable panel, direct equipment control, self-contained FITS pipeline
- v1.0.0 (2026-03-27): First stable release — MAX stacking, 22 stars, manifest PR submitted
