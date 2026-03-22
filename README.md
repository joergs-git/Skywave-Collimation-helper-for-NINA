# AstroCircular SkyWaver for N.I.N.A.

Automated [SkyWave (SKW)](https://www.innovationsforesight.com/aitelescopecollimation/) telescope collimation for [N.I.N.A.](https://nighttime-imaging.eu/) — circular defocused star pattern capture, native frame integration, and SkyWave-ready FITS output.

## What it does

1. **Select** a bright, isolated star (from 16 built-in presets or manual RA/Dec)
2. **Plate-solve & center** on the star (in focus)
3. **Defocus** by a configurable number of focuser steps
4. **Capture** exposures at N positions around a circular ring pattern
5. **Integrate** sub-frames natively (average, crop-to-circle, bin 2x)
6. **Save** a 16-bit monochrome FITS to your SkyWave watch folder
7. **Refocus** — always returns the focuser, even on failure

<img width="1182" height="719" alt="SKW Collimation Coordinator" src="https://github.com/joergs-git/astrocircular-skywaver-for-nina/blob/main/astrocirular-skw-nina-helper.png" />

## Two tools, one workflow

### Web Tool (HTML)

A standalone browser-based tool that generates N.I.N.A. sequence files and PixInsight scripts:

- **[Live demo](https://joergs-git.github.io/astrocircular-skywaver-for-nina/)** — runs entirely in your browser
- Generates downloadable `.json` for N.I.N.A. Advanced Sequencer
- Generates downloadable `.js` PixInsight integration script
- Star finder with altitude/LST calculator
- Magnitude advisor based on your optical setup

### NINA Plugin (in development)

A native N.I.N.A. plugin that does everything inside NINA — no external tools required:

- **Sequence instructions:** `SKW Defocus`, `SKW Circular Capture`, `SKW Integrate Frames`
- **One-click container:** `SKW Collimation Run` chains the full workflow
- **Native integration:** averages sub-frames, crops, bins 2x — no PixInsight needed
- **Settings page** for telescope, sensor, observer location, imaging defaults
- **16 star presets** covering all seasons for mid-northern latitudes

#### Installation (plugin)

1. Download `NINA.AstroCircular.SkyWaver.dll` from [Releases](https://github.com/joergs-git/astrocircular-skywaver-for-nina/releases)
2. Copy to `%localappdata%\NINA\Plugins\AstroCircular.SkyWaver\`
3. Restart N.I.N.A. — the plugin appears in Options > Plugins

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

## Requirements

- **N.I.N.A. 3.0+** (.NET 8.0)
- GoTo mount with slew capability
- Electronic focuser
- Camera with FITS output
- [SkyWave Collimator](https://www.innovationsforesight.com/aitelescopecollimation/) for wavefront analysis

## License

[GPL-3.0](LICENSE)

---
If you find this useful, consider supporting my work via [Buy Me a Coffee](https://buymeacoffee.com/joergsflow)
