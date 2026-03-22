# AstroCircular SkyWaver — TODO

## Completed
- [x] Review HTML tool for correctness and NINA format compatibility
- [x] Plan native NINA plugin architecture
- [x] Phase 1: Plugin scaffolding — solution, csproj, AssemblyInfo, manifest
- [x] Phase 2: GitHub Actions CI workflow
- [x] Phase 3: Core math — CoordinateUtils, CircularPatternCalculator, StarCatalog
- [x] Phase 4: SkwDefocus sequence instruction (fully implemented)
- [x] Phase 5: SkwCircularCapture sequence instruction (fully implemented)
- [x] Phase 6: SkwIntegrateFrames + FitsAverager (fully implemented)
- [x] Phase 7: SkwCollimationRun orchestrator (fully implemented)
- [x] Phase 8: Options page XAML + SkwSettings
- [x] Phase 9: .gitignore, CHANGELOG, tasks, README update, icon

## Open — Post-CI Build
- [ ] Fix any compilation errors from CI build (no .NET SDK locally on macOS)
- [ ] Verify NINA API compatibility (exact method signatures, namespaces)
- [ ] Add plate-solve centering to SkwCollimationRun step 2 (currently blind slew)
- [ ] Wire up SkwSettings persistence to NINA's plugin settings store
- [ ] Test with NINA equipment simulators on Windows
- [ ] Test full workflow end-to-end: capture → integrate → SkyWave loads FITS
- [ ] Publish to NINA plugin repository (nina.plugin.manifests)

## Open — HTML Tool Fixes
- [ ] Fix TakeExposure.ExposureCount: 0 → 1 in generated NINA JSON
- [ ] Fix PixInsight script: use windowById instead of activeWindow
- [ ] Fix PixInsight script: update XPIXSZ/XBINNING headers after bin2

## Results
Initial scaffolding and implementation complete (2026-03-22).
All 9 phases implemented. Awaiting first CI build to verify compilation.
