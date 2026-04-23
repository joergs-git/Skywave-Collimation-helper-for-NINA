# Lessons Learned

## [2026-04-23] — Silent fallbacks hide mount-side epoch bugs
- **Mistake:** The plate-solve centering step's catch block logged a Logger.Warning and overwrote StatusText with a neutral "plate-solve failed, slewing blind..." message that was immediately replaced by the next step's status. Users whose plate-solve consistently failed (filter misconfig, missing solver, solver timeout on high-Dec star) silently ran the whole pattern blind. When the mount ASCOM driver misreported its EquatorialSystem (known 10Micron quirk), every run was offset by the J2000→JNOW precession amount (~8–12 arcmin at Dec +69°) with no indication anything was wrong. User Jeff Morgan reported this for λ Dra, Apr 2026.
- **Root cause:** Treated the fallback as a minor log-only event. But plate-solving is the ONE step that masks mount/driver epoch bugs by physically parking the mount on the star regardless of what the driver advertises. Once plate-solve falls through, blind slews inherit any epoch misreport directly, and the user has no way to know their pattern is drifted.
- **Rule:** Any fallback that degrades correctness must surface to the user via NINA's Notification.ShowWarning (toast) AND leave a persistent "WARNING: ..." prefix in StatusText — never rely on Logger.Warning alone for user-visible regressions. If a code path can only produce a correct result when another path succeeded, a failure of that other path must be loud.
- **Applies to:** SkwPanelVM.RunCollimation plate-solve fallback; any future code path whose correctness depends on a successful plate-solve, autofocus, or mount sync.
- **Mistake:** `SwitchFilter("Default")` silently returned without moving the wheel, so after NINA's Center instruction switched filters internally (for plate-solving), the plugin never restored the user's chosen capture filter. Users saw captures taken on L (or worse, on NINA's profile-configured plate-solve filter).
- **Root cause:** The "Default" sentinel was interpreted as "keep whatever filter is currently active" — but that interpretation only holds if we haven't touched the wheel yet. Once the plugin itself switches to L for plate-solving, "Default" no longer means "the user's filter" — it means "L".
- **Rule:** Any filter token that means "the currently active filter" MUST be resolved to a concrete name at the very start of a run, before any code path touches the wheel. Cache the resolved name in a local and thread it through every downstream call site. Never rely on ambient "current filter" state after you've changed filters yourself.
- **Applies to:** SkwPanelVM.RunCollimation filter handling, any future feature that calls NINA Center/plate-solve with a user-selected capture filter

## [2026-04-14] — Altitude filter is a horizon floor, not a tie-breaker
- **Mistake:** StarCatalog.FindBestStar used `if (alt > bestAlt)` with no minimum altitude check, so for southern-hemisphere observers at LSTs where no southern star was in the ±3h HA window, the "best" returned star could be below the horizon.
- **Root cause:** "Pick the highest alt star that passes the HA filter" implicitly assumes the HA filter guarantees visibility. It doesn't — a star at the top of the HA window can still be below the horizon from a hostile latitude.
- **Rule:** Altitude must be an explicit hard floor (30°, with fallback to 20° then 10°), not a ranking tie-breaker. If nothing passes 10° return null and let the caller surface "no star visible now" — never silently pick a below-horizon star.
- **Applies to:** StarCatalog.FindBestStar, any future "best target" picker

## [2026-04-14] — Property setters that auto-persist need batching paths
- **Mistake:** SkwPanelVM property setters (StarName, TargetRA, TargetDec) each trigger `SaveSettings()` (20 NINA profile writes) and `RebuildMap()`. A multi-field update like UseMountPosition or SelectedPreset.set fires 3x SaveSettings + 2x RebuildMap when 1 of each would do.
- **Root cause:** Each setter is designed as a standalone edit from the UI — reasonable default. Nobody noticed the cost when multiple setters fired back-to-back from code paths that set 3+ fields at once.
- **Rule:** When setting multiple related backing fields in code, set them directly (bypass the public setter), raise PropertyChanged manually for each, then call SaveSettings + RebuildMap once at the end. Alternative: a suppressPersist flag on the VM checked by each setter's persist/rebuild calls.
- **Applies to:** SkwPanelVM.UseMountPosition; consider extending same pattern to SelectedPreset.set which has the same cost on every ComboBox click

## [2026-03-27] — Use NINA's official CreateManifest.ps1 for manifests
- **Mistake:** Manually crafted manifest.json and ZIP archive with custom CI packaging, leading to wrong ZIP structure (DLL nested in subfolder) and broken installs
- **Root cause:** Did not use the official tooling from nina.plugin.manifests repo. Replaced release asset without version bump, breaking checksum validation for existing users.
- **Rule:** Always use `CreateManifest.ps1` from isbeorn/nina.plugin.manifests/tools/ to generate both the ZIP archive and manifest.json. All metadata belongs in AssemblyInfo.cs assembly attributes. Never replace a release asset without bumping the version — users who already downloaded won't see an update.
- **Applies to:** NINA plugin packaging, CI workflow, manifest PRs

## [2026-03-27] — Never replace a GitHub release asset for a published version
- **Mistake:** Replaced v1.0.0 ZIP on the stable release to fix the nested folder issue, without bumping the version number
- **Root cause:** Thought replacing the asset + updating the checksum via a new manifest PR would fix it. But (a) the manifest PR hadn't merged yet so checksum mismatched immediately, (b) users who already tried v1.0.0 would never see an update since the version didn't change, (c) NINA caches manifests so the old checksum was still active
- **Rule:** NEVER replace a release asset for a version that's already in the NINA plugin repo. Always bump the version (even PATCH) and create a new release. Then submit a new manifest with the new version. Three cascading mistakes: wrong ZIP structure → replaced asset → checksum mismatch → had to do v1.0.1 anyway.
- **Applies to:** Any published NINA plugin release, GitHub release management

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
