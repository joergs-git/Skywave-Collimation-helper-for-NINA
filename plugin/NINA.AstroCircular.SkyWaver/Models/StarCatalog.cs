using NINA.AstroCircular.SkyWaver.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// Embedded catalog of bright, isolated stars suitable for SKW collimation.
    /// Ported from the HTML tool's STAR_PRESETS array.
    /// </summary>
    public static class StarCatalog {

        /// <summary>Pre-selected stars covering all seasons for mid-northern latitudes (mag 2–5 range).</summary>
        public static readonly List<StarPreset> Presets = new List<StarPreset> {
            // Circumpolar — bright, available year-round
            new StarPreset { Name = "Kochab",  RA = "14:50:42.3", Dec = "74:09:19.8", Magnitude = 2.08, Season = "sp", Note = "β UMi, circumpolar, very isolated, bright — ideal for fast scopes" },
            new StarPreset { Name = "Errai",   RA = "23:39:20.8", Dec = "77:37:56.2", Magnitude = 3.21, Season = "fa", Note = "γ Cep, circumpolar, extremely isolated field" },

            // Spring
            new StarPreset { Name = "Eltanin", RA = "17:56:36.4", Dec = "51:29:20.0", Magnitude = 2.24, Season = "sp", Note = "γ Dra, bright and well isolated for typical FOVs" },
            new StarPreset { Name = "θ Boo",   RA = "14:25:11.8", Dec = "51:51:02.7", Magnitude = 4.05, Season = "sp", Note = "Very isolated, ideal near zenith 52°N Apr/May" },
            new StarPreset { Name = "κ Dra",   RA = "12:33:28.9", Dec = "69:47:17.6", Magnitude = 3.87, Season = "sp", Note = "Draco, extremely clean field, year-round >45° alt" },
            new StarPreset { Name = "λ Dra",   RA = "11:31:24.2", Dec = "69:19:52.0", Magnitude = 3.84, Season = "sp", Note = "Near κ Dra, alternative in poor seeing" },
            new StarPreset { Name = "38 Lyn",  RA = "09:18:50.6", Dec = "36:48:09.4", Magnitude = 3.82, Season = "sp", Note = "Lynx, very sparse field, lower Dec" },

            // Summer
            new StarPreset { Name = "Rastaban", RA = "17:30:26.0", Dec = "52:18:04.9", Magnitude = 2.79, Season = "su", Note = "β Dra, bright, well separated from Eltanin at typical FOVs" },
            new StarPreset { Name = "χ Dra",   RA = "18:21:03.4", Dec = "72:43:58.2", Magnitude = 3.57, Season = "su", Note = "Northern Draco, circumpolar, clean field" },
            new StarPreset { Name = "42 Dra",  RA = "18:25:59.1", Dec = "65:33:48.5", Magnitude = 4.82, Season = "su", Note = "Extremely isolated, use with longer exposures" },
            new StarPreset { Name = "CQ Dra",  RA = "19:22:35.5", Dec = "59:22:12.0", Magnitude = 4.96, Season = "su", Note = "Fainter, ideal for longer exposures" },
            new StarPreset { Name = "σ Dra",   RA = "19:32:21.6", Dec = "69:39:40.2", Magnitude = 4.67, Season = "su", Note = "Well isolated, very northern" },

            // Fall
            new StarPreset { Name = "Alfirk",  RA = "21:28:39.6", Dec = "70:33:38.6", Magnitude = 3.23, Season = "fa", Note = "β Cep, bright and well isolated" },
            new StarPreset { Name = "ι Cep",   RA = "22:49:40.8", Dec = "66:12:01.5", Magnitude = 3.52, Season = "fa", Note = "Brighter but very clean field" },
            new StarPreset { Name = "ξ Cep",   RA = "22:03:47.5", Dec = "64:37:40.7", Magnitude = 4.29, Season = "fa", Note = "Cepheus, away from MW, well isolated" },
            new StarPreset { Name = "θ Cep",   RA = "20:29:34.9", Dec = "62:59:38.6", Magnitude = 4.22, Season = "fa", Note = "Early fall, good isolation" },

            // Winter
            new StarPreset { Name = "Muscida", RA = "08:30:15.9", Dec = "60:43:05.4", Magnitude = 3.36, Season = "wi", Note = "ο UMa, bright and isolated in sparse field" },
            new StarPreset { Name = "β Cam",   RA = "05:03:25.1", Dec = "60:26:32.1", Magnitude = 4.03, Season = "wi", Note = "Near α Cam, slightly brighter" },
            new StarPreset { Name = "α Cam",   RA = "04:54:03.0", Dec = "66:20:33.6", Magnitude = 4.29, Season = "wi", Note = "Sparsest field in the sky" },
            new StarPreset { Name = "7 Cam",   RA = "04:57:17.2", Dec = "53:45:08.5", Magnitude = 4.47, Season = "wi", Note = "Lower Dec, closer to zenith for 52°N" },
            new StarPreset { Name = "CS Cam",  RA = "03:29:04.1", Dec = "59:56:25.2", Magnitude = 4.21, Season = "wi", Note = "Early winter, Camelopardalis region" },
        };

        /// <summary>
        /// Find the best star for a given observing time, location, and optical setup.
        /// Filters by magnitude range (to avoid overexposure, targeting ~60% ADU in ~8s)
        /// then selects the highest-altitude star within ±3h of the meridian.
        /// </summary>
        /// <param name="observingTimeUtc">Target observing time in UTC</param>
        /// <param name="latitudeDeg">Observer latitude in degrees north</param>
        /// <param name="longitudeDeg">Observer longitude in degrees east</param>
        /// <param name="focalLengthMm">Telescope focal length in mm (0 to skip mag filter)</param>
        /// <param name="apertureMm">Telescope aperture in mm (0 to skip mag filter)</param>
        /// <param name="exposureSeconds">Exposure time in seconds</param>
        /// <param name="gain">Camera gain setting</param>
        /// <param name="customStars">Optional additional user-defined stars</param>
        /// <returns>Best star, its altitude, and ideal mag range — or null if none found</returns>
        public static (StarPreset Star, double AltitudeDeg, double MagLow, double MagHigh)? FindBestStar(
            DateTime observingTimeUtc,
            double latitudeDeg,
            double longitudeDeg,
            double focalLengthMm = 0,
            double apertureMm = 0,
            double exposureSeconds = 8,
            int gain = 100,
            List<StarPreset> customStars = null) {

            double lst = CoordinateUtils.GetLST(observingTimeUtc, longitudeDeg);

            // Compute ideal magnitude range for this optical setup
            double magLow = 2.0;
            double magHigh = 8.0;
            if (focalLengthMm > 0 && apertureMm > 0) {
                var (_, low, high) = MagnitudeAdvisor.GetIdealMagnitude(focalLengthMm, apertureMm, exposureSeconds, gain);
                magLow = low;
                magHigh = high;
            }

            var allStars = new List<StarPreset>(Presets);
            if (customStars != null) allStars.AddRange(customStars);

            StarPreset best = null;
            double bestAlt = -99;

            foreach (var star in allStars) {
                // Filter by magnitude range (avoid overexposure, target ~60% ADU)
                if (star.Magnitude < magLow || star.Magnitude > magHigh) continue;

                double raH = CoordinateUtils.ParseHMS(star.RA);
                double decD = CoordinateUtils.ParseDMS(star.Dec);

                // Hour angle check: skip if too far from meridian
                double ha = lst - raH;
                if (ha > 12) ha -= 24;
                if (ha < -12) ha += 24;
                if (Math.Abs(ha) > 3) continue;

                double alt = CoordinateUtils.GetAltitude(latitudeDeg, raH, decD, lst);
                if (alt > bestAlt) {
                    bestAlt = alt;
                    best = star;
                }
            }

            if (best == null) return null;
            return (best, bestAlt, magLow, magHigh);
        }
    }
}
