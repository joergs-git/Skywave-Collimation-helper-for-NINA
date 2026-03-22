# AstroCircular SkyWaver for N.I.N.A.

Automated [SkyWave (SKW)](https://www.innovationsforesight.com/aitelescopecollimation/) telescope collimation for [N.I.N.A.](https://nighttime-imaging.eu/) — circular defocused star pattern capture, native frame integration, and SkyWave-ready FITS output.

## What it does

1. **Select** a bright, isolated star (from 16 built-in presets or manual RA/Dec)
2. **Switch to L filter** and **plate-solve & center** on the star (in focus)
3. **Defocus** by a configurable number of focuser steps
4. **Switch to target filter** (e.g. R) for capture
5. **Capture** exposures at N positions around a circular ring pattern
6. **Integrate** sub-frames natively (simple average, optional square crop, optional bin 2x)
7. **Save** a 16-bit monochrome FITS to your configured output folder
8. **Refocus** — always returns the focuser, even on failure

<img width="1182" height="719" alt="SKW Collimation Coordinator" src="https://github.com/joergs-git/astrocircular-skywaver-for-nina/blob/main/astrocirular-skw-nina-helper.png" />

## Two tools, one workflow

### Web Tool (HTML)

A standalone browser-based tool that generates N.I.N.A. sequence files and PixInsight scripts:

- **[Live demo](https://joergs-git.github.io/astrocircular-skywaver-for-nina/)** — runs entirely in your browser
- Generates downloadable `.json` for N.I.N.A. Advanced Sequencer
- Generates downloadable `.js` PixInsight integration script
- Star finder with altitude/LST calculator
- Magnitude advisor based on your optical setup

### NINA Plugin — SkyWave Collimator Helper

A native N.I.N.A. plugin that does everything inside NINA — no external tools required:

- **Dockable tool panel** in NINA's imaging tab — click "Run Collimation" and it does everything
- **Star picker** with 16 presets and "Find Best" auto-selection based on time and location
- **Live sensor map** showing ring positions with progress (grey=pending, red=active, green=done)
- **Camera preview** of each captured frame with auto-stretch
- **Native FITS integration** — simple pixel average, no alignment, no rejection, no normalization
- **All settings persist** between NINA sessions
- **Slow mode** for inaccurate mounts — plate-solves every position

#### Installation

1. Download the zip from [Releases](https://github.com/joergs-git/astrocircular-skywaver-for-nina/releases)
2. Unzip and double-click `install.bat` (close NINA first)
3. Restart N.I.N.A. — find **SkyWave Collimator Helper** in the tool panels

#### Usage

1. Open the **SkyWave Collimator Helper** panel (imaging tab, tool windows)
2. Select a star from the presets or enter RA/Dec manually
3. Set defocus steps, exposure time, filter, gain, positions, radius
4. Set the output folder via the `...` browse button
5. Click **Run Collimation**
6. The integrated FITS appears in your output folder, ready for SkyWave

## Star presets

| Star | RA | Dec | Mag | Season | Notes |
|------|-----|-----|-----|--------|-------|
| θ Boo | 14:25:11.8 | +51:51:03 | 4.05 | Spring | Very isolated, ideal near zenith 52°N |
| κ Dra | 12:33:28.9 | +69:47:18 | 3.87 | Spring | Extremely clean field |
| 42 Dra | 18:25:59.1 | +65:33:49 | 4.82 | Summer | Recommended! Perfect mag, extremely isolated |
| χ Dra | 18:21:03.4 | +72:43:58 | 3.57 | Summer | Circumpolar, clean field |
| ξ Cep | 22:03:47.5 | +64:37:41 | 4.29 | Fall | Away from Milky Way |
| α Cam | 04:54:03.0 | +66:20:34 | 4.29 | Winter | Recommended! Sparsest field in the sky |

See the full list of 16 presets in the [web tool](https://joergs-git.github.io/astrocircular-skywaver-for-nina/).

## Tips

- **Camera rotation:** Set your camera to 0° or 180° rotation to avoid confusion with mirrored orientation in the integrated image. Since we capture in a circle, rotation doesn't affect collimation quality — it just makes visual interpretation easier.
- **Center position first:** The plugin always captures the center star position first (if enabled), then the ring positions. This matches SkyWave's expectation for field-dependent analysis.
- **Slow mode:** Enable this if your mount is not accurate enough for blind slewing. In slow mode, the plugin refocuses and plate-solves at every single ring position — much slower but ensures precise positioning. The workflow per position is: refocus → L filter → slew & center (plate-solve) → defocus → target filter → expose. Default is off (blind slew after initial centering).
- **Integration:** The integration is a simple pixel-by-pixel average — no alignment, no rejection, no normalization, no weighting. This is by design: each frame shows the defocused star at a different field position, and SkyWave needs the raw combined pattern.
- **Output format:** Always 16-bit unsigned FITS with proper headers (FOCALLEN, XPIXSZ, XBINNING, etc.). Never XISF — regardless of NINA's default format setting.
- **Sub-frames:** When "Auto-delete subs" is off, individual frames are kept in a `subframes_*` subfolder inside your output directory.

## Requirements

- **N.I.N.A. 3.0+** (.NET 8.0)
- GoTo mount with slew capability
- Electronic focuser
- Camera with FITS output
- Filter wheel (optional — for L filter plate-solving and target filter capture)
- [SkyWave Collimator](https://www.innovationsforesight.com/aitelescopecollimation/) for wavefront analysis

## License

[GPL-3.0](LICENSE)

---
If you find this useful, consider supporting my work via [Buy Me a Coffee](https://buymeacoffee.com/joergsflow)
