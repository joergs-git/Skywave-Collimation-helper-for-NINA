# Lessons Learned

## [2026-03-27] — Always verify git identity in temporary clones
- **Mistake:** Committed in /tmp/nina.plugin.manifests with system identity (real name + hostname)
- **Root cause:** Fresh clone had no repo-level git config and no global config was set
- **Rule:** Always set global git config to joergsflow. Before committing in any new clone or fork, verify with `git config user.name`. Fix with --amend --reset-author if it slips through.
- **Applies to:** Any work in temporary directories, forks, or fresh clones

## [2026-03-22] — No plate-solving when defocused
- **Mistake:** Considered adding plate-solve on ring positions
- **Root cause:** Defocused images can't be plate-solved, and precise centering isn't needed for collimation positions
- **Rule:** Ring positions must always use blind SlewToRaDec, never plate-solve. Only the initial center star (pre-defocus) gets plate-solving.
- **Applies to:** NINA JSON generation (HTML tool), SkwCircularCapture, SkwCollimationRun

## [2026-03-22] — Nautical dusk is intentional
- **Mistake:** Almost "fixed" the -12° dusk calculation to astronomical -18°
- **Root cause:** Collimation work doesn't require full astronomical darkness
- **Rule:** Keep nautical dusk (-12°) in all star finder calculations
- **Applies to:** HTML tool getDuskTime(), CoordinateUtils.GetNauticalDusk()

## [2026-04-08] — Star catalogs must cover both hemispheres
- **Mistake:** v1.0 shipped with only northern stars (Dec +37° to +78°), completely unusable for southern observers
- **Root cause:** Original catalog was built for mid-northern latitudes without considering global users
- **Rule:** Any embedded star catalog must cover Dec -70° to +70° minimum. Always test "Find Best" with southern latitudes (e.g. -32°) before release.
- **Applies to:** StarCatalog.cs, any future star list changes

## [2026-04-08] — Focuser ratio is µm/step, not steps/µm
- **Mistake:** Labelled and computed focuser conversion as "steps per micron" (division) instead of "microns per step" (multiplication)
- **Root cause:** Assumed ZWO EAF spec without verifying with actual focuser+OAZ combos
- **Rule:** The physically measured quantity is µm/step (move N steps, measure distance, divide). Formula: steps × µm/step = total µm. Always verify units against how users actually measure.
- **Applies to:** SkwPanelVM defocus conversion, DockableTemplates tooltips

## [2026-04-10] — Never hardcode filter wheel slot indices
- **Mistake:** Created `new FilterInfo(name, 0, (short)0)` with hardcoded slot 0 in 4 locations
- **Root cause:** FilterInfo constructor requires a slot index, and 0 was used as a placeholder without realizing it physically selects slot 0 on the filter wheel
- **Rule:** Always look up FilterInfo from the NINA profile (`FilterWheelSettings.FilterWheelFilters`) to get the correct slot index. Use `FilterUtils.LookupFilterInfo()` instead of constructing FilterInfo directly.
- **Applies to:** Any code that creates FilterInfo for CaptureSequence, ChangeFilter, or StartAutoFocus

## [2026-03-22] — Duplicate HTML files are intentional
- **Mistake:** Flagged astrocircular-skw-nina-helper.html as redundant
- **Root cause:** User wants both files to exist for different access patterns
- **Rule:** index.html and astrocircular-skw-nina-helper.html must always stay in sync
- **Applies to:** Any modification to the HTML tool
