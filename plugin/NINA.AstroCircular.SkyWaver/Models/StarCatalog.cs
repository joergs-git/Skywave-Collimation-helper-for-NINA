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

        /// <summary>16 pre-selected stars covering all seasons for mid-northern latitudes.</summary>
        public static readonly List<StarPreset> Presets = new List<StarPreset> {
            // Spring
            new StarPreset { Name = "θ Boo",  RA = "14:25:11.8", Dec = "51:51:02.7", Magnitude = 4.05, Season = "sp", Note = "Very isolated, ideal near zenith 52°N Apr/May" },
            new StarPreset { Name = "κ Dra",  RA = "12:33:28.9", Dec = "69:47:17.6", Magnitude = 3.87, Season = "sp", Note = "Draco, extremely clean field, year-round >45° alt" },
            new StarPreset { Name = "λ Dra",  RA = "11:31:24.2", Dec = "69:19:52.0", Magnitude = 3.84, Season = "sp", Note = "Near κ Dra, alternative in poor seeing" },
            new StarPreset { Name = "38 Lyn", RA = "09:18:50.6", Dec = "36:48:09.4", Magnitude = 3.82, Season = "sp", Note = "Lynx, very sparse field, lower Dec" },

            // Summer
            new StarPreset { Name = "42 Dra", RA = "18:25:59.1", Dec = "65:33:48.5", Magnitude = 4.82, Season = "su", Note = "Recommended! Perfect mag, extremely isolated" },
            new StarPreset { Name = "χ Dra",  RA = "18:21:03.4", Dec = "72:43:58.2", Magnitude = 3.57, Season = "su", Note = "Northern Draco, circumpolar, clean field" },
            new StarPreset { Name = "CQ Dra", RA = "19:22:35.5", Dec = "59:22:12.0", Magnitude = 4.96, Season = "su", Note = "Fainter, ideal for longer exposures" },
            new StarPreset { Name = "σ Dra",  RA = "19:32:21.6", Dec = "69:39:40.2", Magnitude = 4.67, Season = "su", Note = "Well isolated, very northern" },

            // Fall
            new StarPreset { Name = "ξ Cep",  RA = "22:03:47.5", Dec = "64:37:40.7", Magnitude = 4.29, Season = "fa", Note = "Cepheus, away from MW, well isolated" },
            new StarPreset { Name = "ι Cep",  RA = "22:49:40.8", Dec = "66:12:01.5", Magnitude = 3.52, Season = "fa", Note = "Brighter but very clean field" },
            new StarPreset { Name = "θ Cep",  RA = "20:29:34.9", Dec = "62:59:38.6", Magnitude = 4.22, Season = "fa", Note = "Early fall, good isolation" },

            // Winter
            new StarPreset { Name = "α Cam",  RA = "04:54:03.0", Dec = "66:20:33.6", Magnitude = 4.29, Season = "wi", Note = "Recommended! Sparsest field in the sky" },
            new StarPreset { Name = "β Cam",  RA = "05:03:25.1", Dec = "60:26:32.1", Magnitude = 4.03, Season = "wi", Note = "Near α Cam, slightly brighter" },
            new StarPreset { Name = "7 Cam",  RA = "04:57:17.2", Dec = "53:45:08.5", Magnitude = 4.47, Season = "wi", Note = "Lower Dec, closer to zenith for 52°N" },
            new StarPreset { Name = "CS Cam", RA = "03:29:04.1", Dec = "59:56:25.2", Magnitude = 4.21, Season = "wi", Note = "Early winter, Camelopardalis region" },
        };

        /// <summary>
        /// Find the best star for a given observing time and location.
        /// Selects the highest-altitude star with hour angle less than 3 hours.
        /// </summary>
        /// <param name="observingTimeUtc">Target observing time in UTC</param>
        /// <param name="latitudeDeg">Observer latitude in degrees north</param>
        /// <param name="longitudeDeg">Observer longitude in degrees east</param>
        /// <param name="customStars">Optional additional user-defined stars</param>
        /// <returns>Best star and its altitude, or null if none within ±3h HA</returns>
        public static (StarPreset Star, double AltitudeDeg)? FindBestStar(
            DateTime observingTimeUtc,
            double latitudeDeg,
            double longitudeDeg,
            List<StarPreset> customStars = null) {

            double lst = CoordinateUtils.GetLST(observingTimeUtc, longitudeDeg);

            var allStars = new List<StarPreset>(Presets);
            if (customStars != null) allStars.AddRange(customStars);

            StarPreset best = null;
            double bestAlt = -99;

            foreach (var star in allStars) {
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
            return (best, bestAlt);
        }
    }
}
