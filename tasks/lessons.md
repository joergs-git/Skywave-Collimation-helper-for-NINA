# Lessons Learned

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

## [2026-03-22] — Duplicate HTML files are intentional
- **Mistake:** Flagged astrocircular-skw-nina-helper.html as redundant
- **Root cause:** User wants both files to exist for different access patterns
- **Rule:** index.html and astrocircular-skw-nina-helper.html must always stay in sync
- **Applies to:** Any modification to the HTML tool
