using NINA.AstroCircular.SkyWaver.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// Embedded catalog of bright, isolated stars suitable for SKW collimation.
    /// Combines the original plugin presets with the SkyWave "Collimation Stars" list
    /// covering all seasons and a wide range of declinations.
    /// v2.0.0: 80 northern + equatorial stars.
    /// v2.0.1: added 25 southern hemisphere stars (Dec -25° to -79°).
    /// v2.1.0: added 14 southern winter stars in the RA 7h/10h/16h–21h gap (Sco/Ara/Sgr/Tel/Ind/Pav)
    ///         so NZ / Australia observers always have a near-zenith option during austral winter.
    /// </summary>
    public static class StarCatalog {

        /// <summary>Pre-selected collimation stars covering all seasons (mag 2–5 range).</summary>
        public static readonly List<StarPreset> Presets = new List<StarPreset> {

            // ════════════════════════════════════════════════════════════════
            //  CIRCUMPOLAR — available year-round from mid-northern latitudes
            // ════════════════════════════════════════════════════════════════

            new StarPreset { Name = "Kochab",     RA = "14:50:42.3", Dec = "74:09:19.8", Magnitude = 2.08, Season = "sp", Constellation = "UMi", Note = "β UMi — circumpolar, very isolated, bright — ideal for fast scopes" },
            new StarPreset { Name = "Errai",      RA = "23:39:20.8", Dec = "77:37:56.2", Magnitude = 3.21, Season = "fa", Constellation = "Cep", Note = "γ Cep — circumpolar, extremely isolated field" },
            new StarPreset { Name = "Pherkad",    RA = "15:20:43.7", Dec = "71:50:02.5", Magnitude = 3.05, Season = "su", Constellation = "UMi", Note = "γ UMi — circumpolar, HR 5735" },

            // ════════════════════════════════════════════════════════════════
            //  WINTER — RA ~22h–6h
            // ════════════════════════════════════════════════════════════════

            new StarPreset { Name = "Muscida",    RA = "08:30:15.9", Dec = "60:43:05.4", Magnitude = 3.36, Season = "wi", Constellation = "UMa", Note = "ο UMa — bright and isolated in sparse field" },
            new StarPreset { Name = "β Cam",      RA = "05:03:25.1", Dec = "60:26:32.1", Magnitude = 4.03, Season = "wi", Constellation = "Cam", Note = "Near α Cam, slightly brighter" },
            new StarPreset { Name = "α Cam",      RA = "04:54:03.0", Dec = "66:20:33.6", Magnitude = 4.29, Season = "wi", Constellation = "Cam", Note = "Sparsest field in the sky" },
            new StarPreset { Name = "7 Cam",      RA = "04:57:17.2", Dec = "53:45:08.5", Magnitude = 4.47, Season = "wi", Constellation = "Cam", Note = "Lower Dec, closer to zenith for 52°N" },
            new StarPreset { Name = "CS Cam",     RA = "03:29:04.1", Dec = "59:56:25.2", Magnitude = 4.21, Season = "wi", Constellation = "Cam", Note = "Early winter, Camelopardalis region" },
            new StarPreset { Name = "Segin",      RA = "1:54:23.7",  Dec = "63:40:12.4", Magnitude = 3.37, Season = "wi", Constellation = "Cas", Note = "ε Cas — HR 542, well isolated" },
            new StarPreset { Name = "ζ Cas",      RA = "0:36:58.3",  Dec = "53:53:48.9", Magnitude = 3.66, Season = "wi", Constellation = "Cas", Note = "Zet Cas — HR 153, clean field" },
            new StarPreset { Name = "50 Cas",     RA = "2:03:26.1",  Dec = "72:25:16.7", Magnitude = 3.98, Season = "wi", Constellation = "Cas", Note = "HR 580 — high Dec, sparse field" },
            new StarPreset { Name = "ζ Cep",      RA = "22:10:51.3", Dec = "58:12:04.5", Magnitude = 3.35, Season = "wi", Constellation = "Cep", Note = "Zet Cep — HR 8465, northern" },
            new StarPreset { Name = "ι Cep",      RA = "22:49:40.8", Dec = "66:12:01.5", Magnitude = 3.52, Season = "wi", Constellation = "Cep", Note = "Iota Cep — HR 8694, very clean field" },
            new StarPreset { Name = "Haedus II",  RA = "5:06:30.9",  Dec = "41:14:04.1", Magnitude = 3.17, Season = "wi", Constellation = "Aur", Note = "η Aur — HR 1641, isolated near Capella" },
            new StarPreset { Name = "Hassaleh",   RA = "4:56:59.6",  Dec = "33:09:57.9", Magnitude = 2.69, Season = "wi", Constellation = "Aur", Note = "ι Aur (Al Kab) — HR 1577, bright" },
            new StarPreset { Name = "Elthor",     RA = "4:00:40.8",  Dec = "12:29:25.2", Magnitude = 3.47, Season = "wi", Constellation = "Tau", Note = "λ Tau — HR 1239, also Althor/Al Thaur" },
            new StarPreset { Name = "ο Tau",      RA = "3:24:48.8",  Dec = "9:01:43.9",  Magnitude = 3.60, Season = "wi", Constellation = "Tau", Note = "Omicron Tau — HR 1030" },
            new StarPreset { Name = "Menkar",     RA = "3:02:16.8",  Dec = "4:05:23.0",  Magnitude = 2.53, Season = "wi", Constellation = "Cet", Note = "α Cet — HR 911, bright, low Dec" },
            new StarPreset { Name = "Gorgonea Tertia", RA = "3:05:10.6", Dec = "38:50:25.0", Magnitude = 3.39, Season = "wi", Constellation = "Per", Note = "ρ Per — HR 921, variable (3.3–4.0)" },
            new StarPreset { Name = "υ Per",      RA = "1:37:59.6",  Dec = "48:37:41.6", Magnitude = 3.57, Season = "wi", Constellation = "Per", Note = "Upsilon Per (51 And) — HR 464" },
            new StarPreset { Name = "Azha",       RA = "2:56:25.6",  Dec = "-8:53:53.3", Magnitude = 3.89, Season = "wi", Constellation = "Eri", Note = "η Eri — HR 874, southern" },
            new StarPreset { Name = "Rana",       RA = "3:43:14.9",  Dec = "-9:45:48.2", Magnitude = 3.54, Season = "wi", Constellation = "Eri", Note = "δ Eri — HR 1136" },
            new StarPreset { Name = "π⁴ Ori",     RA = "4:51:12.4",  Dec = "5:36:18.4",  Magnitude = 3.69, Season = "wi", Constellation = "Ori", Note = "Pi4 Ori — HR 1552, away from Orion nebula" },
            new StarPreset { Name = "π⁵ Ori",     RA = "4:54:15.1",  Dec = "2:26:26.4",  Magnitude = 3.72, Season = "wi", Constellation = "Ori", Note = "Pi5 Ori — HR 1567, pair with π⁴" },
            new StarPreset { Name = "ζ Lep",      RA = "5:46:57.3",  Dec = "-14:49:19.0", Magnitude = 3.55, Season = "wi", Constellation = "Lep", Note = "Zet Lep — HR 1998, southern" },
            new StarPreset { Name = "η Lep",      RA = "5:56:24.3",  Dec = "-14:10:03.7", Magnitude = 3.71, Season = "wi", Constellation = "Lep", Note = "Eta Lep — HR 2085" },
            new StarPreset { Name = "μ Lep",      RA = "5:12:55.9",  Dec = "-16:12:19.7", Magnitude = 3.31, Season = "wi", Constellation = "Lep", Note = "Mu Lep — HR 1702" },
            new StarPreset { Name = "λ Peg",      RA = "22:46:31.9", Dec = "23:33:56.4", Magnitude = 3.95, Season = "wi", Constellation = "Peg", Note = "Lambda Peg — HR 8667" },
            new StarPreset { Name = "μ Peg",      RA = "22:50:00.2", Dec = "24:36:05.7", Magnitude = 3.48, Season = "wi", Constellation = "Peg", Note = "Mu Peg — HR 8684" },
            new StarPreset { Name = "Baham",      RA = "22:10:12.0", Dec = "6:11:52.3",  Magnitude = 3.53, Season = "wi", Constellation = "Peg", Note = "θ Peg — HR 8450, low Dec" },
            new StarPreset { Name = "γ Psc",      RA = "23:17:09.9", Dec = "3:16:56.2",  Magnitude = 3.69, Season = "wi", Constellation = "Psc", Note = "Gamma Psc — HR 8852" },
            new StarPreset { Name = "Skat",       RA = "22:54:39.0", Dec = "-15:49:14.9", Magnitude = 3.27, Season = "wi", Constellation = "Aqr", Note = "δ Aqr — HR 8709, southern" },
            new StarPreset { Name = "b¹ Aqr",     RA = "23:22:58.2", Dec = "-20:06:02.1", Magnitude = 3.97, Season = "wi", Constellation = "Aqr", Note = "98 Aqr — HR 8892, deep southern" },

            // ════════════════════════════════════════════════════════════════
            //  SPRING — RA ~6h–12h
            // ════════════════════════════════════════════════════════════════

            new StarPreset { Name = "Eltanin",    RA = "17:56:36.4", Dec = "51:29:20.0", Magnitude = 2.24, Season = "sp", Constellation = "Dra", Note = "γ Dra — bright and well isolated for typical FOVs" },
            new StarPreset { Name = "θ Boo",      RA = "14:25:11.8", Dec = "51:51:02.7", Magnitude = 4.05, Season = "sp", Constellation = "Boo", Note = "Very isolated, ideal near zenith 52°N Apr/May" },
            new StarPreset { Name = "κ Dra",      RA = "12:33:28.9", Dec = "69:47:17.7", Magnitude = 3.87, Season = "sp", Constellation = "Dra", Note = "Extremely clean field, year-round >45° alt" },
            new StarPreset { Name = "Gianfar",    RA = "11:31:24.2", Dec = "69:19:51.9", Magnitude = 3.84, Season = "sp", Constellation = "Dra", Note = "λ Dra — near κ Dra, alternative in poor seeing" },
            new StarPreset { Name = "38 Lyn",     RA = "09:18:50.6", Dec = "36:48:09.4", Magnitude = 3.82, Season = "sp", Constellation = "Lyn", Note = "Very sparse field, lower Dec" },
            new StarPreset { Name = "Alzirr",     RA = "6:45:17.4",  Dec = "12:53:44.1", Magnitude = 3.36, Season = "sp", Constellation = "Gem", Note = "ξ Gem — HR 2484, well isolated" },
            new StarPreset { Name = "ζ Hya",      RA = "8:55:23.6",  Dec = "5:56:44.0",  Magnitude = 3.11, Season = "sp", Constellation = "Hya", Note = "Zet Hya — HR 3547, equatorial" },
            new StarPreset { Name = "ι Hya",      RA = "9:39:51.4",  Dec = "-1:08:34.1", Magnitude = 3.91, Season = "sp", Constellation = "Hya", Note = "Iota Hya — HR 3845, near equator" },
            new StarPreset { Name = "μ Hya",      RA = "10:26:05.4", Dec = "-16:50:10.7", Magnitude = 3.81, Season = "sp", Constellation = "Hya", Note = "Mu Hya — HR 4094, southern" },
            new StarPreset { Name = "ν Hya",      RA = "10:49:37.5", Dec = "-16:11:37.1", Magnitude = 3.11, Season = "sp", Constellation = "Hya", Note = "Nu Hya — HR 4232, southern" },
            new StarPreset { Name = "Ras Elased Australis", RA = "9:45:51.1", Dec = "23:46:27.3", Magnitude = 2.98, Season = "sp", Constellation = "Leo", Note = "ε Leo (Algenubi) — HR 3873, head of Leo" },
            new StarPreset { Name = "Chertan",    RA = "11:14:14.4", Dec = "15:25:46.5", Magnitude = 3.34, Season = "sp", Constellation = "Leo", Note = "θ Leo (Chort/Coxa) — HR 4359" },
            new StarPreset { Name = "Rasalas",    RA = "9:52:45.8",  Dec = "26:00:25.0", Magnitude = 3.88, Season = "sp", Constellation = "Leo", Note = "μ Leo (Ras Elased Borealis) — HR 3905" },
            new StarPreset { Name = "Samoht",     RA = "7:41:14.8",  Dec = "-9:33:04.1", Magnitude = 3.93, Season = "sp", Constellation = "Mon", Note = "α Mon — HR 2970, isolated in sparse Monoceros" },
            new StarPreset { Name = "ν² CMa",     RA = "6:36:41.0",  Dec = "-19:15:21.2", Magnitude = 3.95, Season = "sp", Constellation = "CMa", Note = "Nu2 CMa — HR 2429, southern" },
            new StarPreset { Name = "Tania Borealis", RA = "10:17:05.8", Dec = "42:54:51.7", Magnitude = 3.45, Season = "sp", Constellation = "UMa", Note = "λ UMa — HR 4033, pair with Tania Australis" },
            new StarPreset { Name = "Tania Australis", RA = "10:22:19.7", Dec = "41:29:58.3", Magnitude = 3.05, Season = "sp", Constellation = "UMa", Note = "μ UMa — HR 4069, pair with Tania Borealis" },
            new StarPreset { Name = "Al Kaphrah", RA = "11:46:03.0", Dec = "47:46:45.9", Magnitude = 3.71, Season = "sp", Constellation = "UMa", Note = "χ UMa — HR 4518" },
            new StarPreset { Name = "ψ UMa",      RA = "11:09:39.8", Dec = "44:29:54.5", Magnitude = 3.01, Season = "sp", Constellation = "UMa", Note = "Psi UMa — HR 4335" },
            new StarPreset { Name = "Praecipua",  RA = "10:53:18.7", Dec = "34:12:53.5", Magnitude = 3.83, Season = "sp", Constellation = "LMi", Note = "46 LMi — HR 4247, brightest in Leo Minor" },
            new StarPreset { Name = "δ Crt",      RA = "11:19:20.4", Dec = "-14:46:42.8", Magnitude = 3.56, Season = "sp", Constellation = "Crt", Note = "Delta Crt — HR 4382, southern" },

            // ════════════════════════════════════════════════════════════════
            //  SUMMER — RA ~12h–18h
            // ════════════════════════════════════════════════════════════════

            new StarPreset { Name = "Rastaban",   RA = "17:30:26.0", Dec = "52:18:04.9", Magnitude = 2.79, Season = "su", Constellation = "Dra", Note = "β Dra — bright, well separated from Eltanin" },
            new StarPreset { Name = "χ Dra",      RA = "18:21:03.4", Dec = "72:43:58.2", Magnitude = 3.57, Season = "su", Constellation = "Dra", Note = "Northern Draco, circumpolar, clean field" },
            new StarPreset { Name = "42 Dra",     RA = "18:25:59.1", Dec = "65:33:48.5", Magnitude = 4.82, Season = "su", Constellation = "Dra", Note = "Extremely isolated, use with longer exposures" },
            new StarPreset { Name = "CQ Dra",     RA = "19:22:35.5", Dec = "59:22:12.0", Magnitude = 4.96, Season = "su", Constellation = "Dra", Note = "Fainter, ideal for longer exposures" },
            new StarPreset { Name = "σ Dra",      RA = "19:32:21.6", Dec = "69:39:40.2", Magnitude = 4.67, Season = "su", Constellation = "Dra", Note = "Well isolated, very northern" },
            new StarPreset { Name = "Nekkar",     RA = "15:01:56.8", Dec = "40:23:26.0", Magnitude = 3.50, Season = "su", Constellation = "Boo", Note = "β Boo (Meres) — HR 5602" },
            new StarPreset { Name = "π Her",      RA = "17:15:02.8", Dec = "36:48:33.0", Magnitude = 3.16, Season = "su", Constellation = "Her", Note = "Pi Her — HR 6418" },
            new StarPreset { Name = "θ Her",      RA = "17:56:15.2", Dec = "37:15:01.9", Magnitude = 3.86, Season = "su", Constellation = "Her", Note = "Theta Her — HR 6695" },
            new StarPreset { Name = "ξ Her",      RA = "17:57:45.9", Dec = "29:14:52.4", Magnitude = 3.70, Season = "su", Constellation = "Her", Note = "Xi Her — HR 6703" },
            new StarPreset { Name = "ε Ser",      RA = "15:50:49.0", Dec = "4:28:39.8",  Magnitude = 3.71, Season = "su", Constellation = "Ser", Note = "Epsilon Ser — HR 5892, equatorial" },
            new StarPreset { Name = "Zubeneschamali", RA = "15:17:00.4", Dec = "-9:22:58.5", Magnitude = 2.61, Season = "su", Constellation = "Lib", Note = "β Lib — HR 5685, bright, southern" },
            new StarPreset { Name = "κ Oph",      RA = "16:57:40.1", Dec = "9:22:30.1",  Magnitude = 3.20, Season = "su", Constellation = "Oph", Note = "Kappa Oph — HR 6299" },
            new StarPreset { Name = "Cebalrai",   RA = "17:43:28.4", Dec = "4:34:02.3",  Magnitude = 2.77, Season = "su", Constellation = "Oph", Note = "β Oph — HR 6603, bright" },
            new StarPreset { Name = "ζ Oph",      RA = "16:37:09.5", Dec = "-10:34:01.5", Magnitude = 2.56, Season = "su", Constellation = "Oph", Note = "Zet Oph — HR 6175, bright runaway star" },
            new StarPreset { Name = "ν Oph",      RA = "17:59:01.6", Dec = "-9:46:25.1", Magnitude = 3.34, Season = "su", Constellation = "Oph", Note = "Nu Oph — HR 6698, southern" },

            // ════════════════════════════════════════════════════════════════
            //  FALL — RA ~18h–22h
            // ════════════════════════════════════════════════════════════════

            new StarPreset { Name = "Alfirk",     RA = "21:28:39.6", Dec = "70:33:38.6", Magnitude = 3.23, Season = "fa", Constellation = "Cep", Note = "β Cep — bright and well isolated" },
            new StarPreset { Name = "ξ Cep",      RA = "22:03:47.5", Dec = "64:37:40.7", Magnitude = 4.29, Season = "fa", Constellation = "Cep", Note = "Away from MW, well isolated" },
            new StarPreset { Name = "θ Cep",      RA = "20:29:34.9", Dec = "62:59:38.6", Magnitude = 4.22, Season = "fa", Constellation = "Cep", Note = "Early fall, good isolation" },
            new StarPreset { Name = "ι² Cyg",     RA = "19:29:42.4", Dec = "51:43:47.2", Magnitude = 3.79, Season = "fa", Constellation = "Cyg", Note = "Iota2 Cyg — HR 7420" },
            new StarPreset { Name = "κ Cyg",      RA = "19:17:06.2", Dec = "53:22:06.4", Magnitude = 3.77, Season = "fa", Constellation = "Cyg", Note = "Kappa Cyg — HR 7328, northern Cygnus" },
            new StarPreset { Name = "ξ Cyg",      RA = "21:04:55.9", Dec = "43:55:40.3", Magnitude = 3.72, Season = "fa", Constellation = "Cyg", Note = "Xi Cyg — HR 8079" },
            new StarPreset { Name = "ρ Cyg",      RA = "21:33:58.9", Dec = "45:35:30.6", Magnitude = 4.02, Season = "fa", Constellation = "Cyg", Note = "Rho Cyg — HR 8252" },
            new StarPreset { Name = "γ Sge",      RA = "19:58:45.4", Dec = "19:29:31.7", Magnitude = 3.47, Season = "fa", Constellation = "Sge", Note = "Gamma Sge — HR 7635" },
            new StarPreset { Name = "Ionnina",    RA = "18:35:12.4", Dec = "-8:14:38.7", Magnitude = 3.85, Season = "fa", Constellation = "Sct", Note = "α Sct — HR 6973, southern" },
            new StarPreset { Name = "Albali",     RA = "20:47:40.5", Dec = "-9:29:44.8", Magnitude = 3.77, Season = "fa", Constellation = "Aqr", Note = "ε Aqr — HR 7950" },
            new StarPreset { Name = "Nashira",    RA = "21:40:05.5", Dec = "-16:39:44.3", Magnitude = 3.68, Season = "fa", Constellation = "Cap", Note = "γ Cap — HR 8278, southern" },

            // ════════════════════════════════════════════════════════════════
            //  SOUTHERN HEMISPHERE — Dec -25° to -79°
            //  Isolated stars for southern observers, covering all seasons.
            //  Season labels follow RA convention (same as northern section);
            //  "Find Best" computes actual altitude for any observer location.
            // ════════════════════════════════════════════════════════════════

            // -- Southern circumpolar (Dec < -60°) --
            new StarPreset { Name = "β Hyi",      RA = "0:25:45.1",  Dec = "-77:15:15.3", Magnitude = 2.80, Season = "wi", Constellation = "Hyi", Note = "Beta Hyi — HR 98, extremely isolated near south pole" },
            new StarPreset { Name = "α Hyi",      RA = "1:58:46.2",  Dec = "-61:34:11.5", Magnitude = 2.86, Season = "wi", Constellation = "Hyi", Note = "Alpha Hyi — HR 591, head of Hydrus" },
            new StarPreset { Name = "γ Hyi",      RA = "3:47:14.3",  Dec = "-74:14:20.3", Magnitude = 3.24, Season = "wi", Constellation = "Hyi", Note = "Gamma Hyi — HR 1208, deep south, sparse field" },
            new StarPreset { Name = "α Ret",      RA = "4:14:25.5",  Dec = "-62:28:25.8", Magnitude = 3.33, Season = "wi", Constellation = "Ret", Note = "Alpha Ret — HR 1336, very sparse Reticulum field" },
            new StarPreset { Name = "β Ret",      RA = "3:44:12.1",  Dec = "-64:48:24.6", Magnitude = 3.85, Season = "wi", Constellation = "Ret", Note = "Beta Ret — HR 1175, sparse field near α Ret" },
            new StarPreset { Name = "α Pic",      RA = "6:48:11.5",  Dec = "-61:56:29.0", Magnitude = 3.27, Season = "sp", Constellation = "Pic", Note = "Alpha Pic — HR 2550, clean Pictor field" },
            new StarPreset { Name = "α Aps",      RA = "14:47:51.7", Dec = "-79:02:41.0", Magnitude = 3.83, Season = "su", Constellation = "Aps", Note = "Alpha Aps — HR 5470, near south pole, very isolated" },
            new StarPreset { Name = "δ Pav",      RA = "20:08:43.6", Dec = "-66:10:55.4", Magnitude = 3.56, Season = "fa", Constellation = "Pav", Note = "Delta Pav — HR 7665, isolated Pavo field" },

            // -- Southern mid-latitude (Dec -40° to -55°) --
            new StarPreset { Name = "Acamar",     RA = "2:58:15.7",  Dec = "-40:18:17.0", Magnitude = 2.88, Season = "wi", Constellation = "Eri", Note = "θ Eri — HR 897, bright double (wide sep), clean field" },
            new StarPreset { Name = "φ Eri",      RA = "2:16:30.6",  Dec = "-51:30:44.2", Magnitude = 3.56, Season = "wi", Constellation = "Eri", Note = "Phi Eri — HR 669, deep southern Eridanus" },
            new StarPreset { Name = "χ Eri",      RA = "1:55:57.5",  Dec = "-51:36:32.0", Magnitude = 3.70, Season = "wi", Constellation = "Eri", Note = "Chi Eri — HR 566, pair with φ Eri" },
            new StarPreset { Name = "γ Phe",      RA = "1:28:21.9",  Dec = "-43:19:05.7", Magnitude = 3.41, Season = "wi", Constellation = "Phe", Note = "Gamma Phe — HR 429, isolated Phoenix field" },
            new StarPreset { Name = "κ Phe",      RA = "0:26:12.2",  Dec = "-43:40:47.4", Magnitude = 3.94, Season = "wi", Constellation = "Phe", Note = "Kappa Phe — HR 100, sparse field" },
            new StarPreset { Name = "Tiaki",      RA = "22:42:40.1", Dec = "-46:53:04.5", Magnitude = 2.07, Season = "fa", Constellation = "Gru", Note = "β Gru — HR 8636, very bright, isolated Grus" },
            new StarPreset { Name = "Al Dhanab",  RA = "21:53:55.7", Dec = "-37:21:53.5", Magnitude = 3.01, Season = "fa", Constellation = "Gru", Note = "γ Gru — HR 8353, clean field" },
            new StarPreset { Name = "δ Gru",      RA = "22:29:16.1", Dec = "-43:29:44.0", Magnitude = 3.97, Season = "fa", Constellation = "Gru", Note = "Delta Gru — HR 8556, isolated" },
            new StarPreset { Name = "α Hor",      RA = "4:14:00.1",  Dec = "-42:17:39.7", Magnitude = 3.86, Season = "wi", Constellation = "Hor", Note = "Alpha Hor — HR 1326, sparse Horologium" },
            new StarPreset { Name = "β Pic",      RA = "5:47:17.1",  Dec = "-51:03:59.5", Magnitude = 3.86, Season = "wi", Constellation = "Pic", Note = "Beta Pic — HR 2020, famous debris disk star, isolated" },
            new StarPreset { Name = "ψ Vel",      RA = "9:30:42.0",  Dec = "-40:28:00.2", Magnitude = 3.60, Season = "sp", Constellation = "Vel", Note = "Psi Vel — HR 3786, away from crowded Vela MW" },
            new StarPreset { Name = "μ Vel",      RA = "10:46:46.2", Dec = "-49:25:12.8", Magnitude = 2.69, Season = "sp", Constellation = "Vel", Note = "Mu Vel — HR 4216, bright, sparse field" },
            new StarPreset { Name = "φ Vel",      RA = "9:56:51.8",  Dec = "-54:34:04.0", Magnitude = 3.54, Season = "sp", Constellation = "Vel", Note = "Phi Vel — HR 3940, well isolated from MW plane" },

            // -- Southern low-latitude (Dec -25° to -40°) --
            new StarPreset { Name = "Dalim",      RA = "3:12:04.5",  Dec = "-28:59:15.4", Magnitude = 3.87, Season = "wi", Constellation = "For", Note = "α For — HR 963, sparse Fornax field" },
            new StarPreset { Name = "Phact",      RA = "5:39:38.9",  Dec = "-34:04:26.8", Magnitude = 2.64, Season = "wi", Constellation = "Col", Note = "α Col — HR 1956, bright, clean Columba field" },
            new StarPreset { Name = "Wazn",       RA = "5:50:57.6",  Dec = "-35:46:05.9", Magnitude = 3.12, Season = "wi", Constellation = "Col", Note = "β Col — HR 2040, pair with Phact" },
            new StarPreset { Name = "α Pyx",      RA = "8:43:35.5",  Dec = "-33:11:11.0", Magnitude = 3.68, Season = "sp", Constellation = "Pyx", Note = "Alpha Pyx — HR 3468, isolated Pyxis" },
            new StarPreset { Name = "ι Cen",      RA = "13:20:35.8", Dec = "-36:42:44.3", Magnitude = 2.73, Season = "su", Constellation = "Cen", Note = "Iota Cen — HR 5028, away from crowded Centaurus core" },

            // ════════════════════════════════════════════════════════════════
            //  v2.1.0 ADDITIONS — Close the austral winter gap (RA 16h–21h)
            //  Essential for NZ / Australia users during April–September evenings
            //  when the previous catalog had no southern star within ±3h of meridian.
            // ════════════════════════════════════════════════════════════════

            // -- RA 7h–8h: fills thin Puppis/CMa gap --
            new StarPreset { Name = "Aludra",     RA = "7:24:05.7",  Dec = "-29:18:11.2", Magnitude = 2.45, Season = "wi", Constellation = "CMa", Note = "η CMa — HR 2827, tail of Canis Major, bright but isolated" },
            new StarPreset { Name = "σ Pup",      RA = "7:29:13.8",  Dec = "-43:18:05.2", Magnitude = 3.25, Season = "wi", Constellation = "Pup", Note = "Sigma Pup — HR 2878, clean Puppis field" },

            // -- RA 10h–13h: reinforces spring coverage for southern observers --
            new StarPreset { Name = "α Ant",      RA = "10:27:09.1", Dec = "-31:04:04.0", Magnitude = 4.25, Season = "sp", Constellation = "Ant", Note = "Alpha Ant — HR 4104, brightest in sparse Antlia" },
            new StarPreset { Name = "Algorab",    RA = "12:29:51.8", Dec = "-16:30:55.6", Magnitude = 2.94, Season = "su", Constellation = "Crv", Note = "δ Crv — HR 4757, clean field south of Virgo" },

            // -- RA 16h–17h: fills the critical Scorpius/Ara gap --
            new StarPreset { Name = "δ TrA",      RA = "16:15:26.3", Dec = "-63:41:07.6", Magnitude = 3.85, Season = "su", Constellation = "TrA", Note = "Delta TrA — HR 6030, isolated Triangulum Australe" },
            new StarPreset { Name = "ζ² Sco",     RA = "16:54:35.0", Dec = "-42:21:40.7", Magnitude = 3.62, Season = "su", Constellation = "Sco", Note = "Zet2 Sco — HR 6271, south of Scorpius body" },
            new StarPreset { Name = "η Sco",      RA = "17:12:09.2", Dec = "-43:14:21.5", Magnitude = 3.33, Season = "su", Constellation = "Sco", Note = "Eta Sco — HR 6380, south of the Scorpius tail" },

            // -- RA 17h–18h: Ara + Telescopium (deep southern winter) --
            new StarPreset { Name = "β Ara",      RA = "17:25:17.9", Dec = "-55:31:47.4", Magnitude = 2.85, Season = "su", Constellation = "Ara", Note = "Beta Ara — HR 6461, bright, south of Scorpius" },
            new StarPreset { Name = "α Tel",      RA = "18:26:58.4", Dec = "-45:58:06.3", Magnitude = 3.51, Season = "fa", Constellation = "Tel", Note = "Alpha Tel — HR 6897, very sparse Telescopium field" },

            // -- RA 18h–19h: Sagittarius (Kaus) --
            new StarPreset { Name = "Kaus Meridionalis", RA = "18:20:59.6", Dec = "-29:49:41.2", Magnitude = 2.72, Season = "fa", Constellation = "Sgr", Note = "δ Sgr — HR 6859, upper Sgr teapot, bright but isolated" },
            new StarPreset { Name = "Albaldah",   RA = "19:09:45.8", Dec = "-21:01:25.0", Magnitude = 2.88, Season = "fa", Constellation = "Sgr", Note = "π Sgr — HR 7264, north of Sgr core, clean background" },

            // -- RA 20h–21h: Indus + Pavo (south of Capricornus) --
            new StarPreset { Name = "α Ind",      RA = "20:37:33.9", Dec = "-47:17:28.8", Magnitude = 3.11, Season = "fa", Constellation = "Ind", Note = "Alpha Ind — HR 7869, sparsest region, excellent isolation" },
            new StarPreset { Name = "β Pav",      RA = "20:44:57.4", Dec = "-66:12:11.6", Magnitude = 3.42, Season = "fa", Constellation = "Pav", Note = "Beta Pav — HR 7913, sparse southern Pavo" },
            new StarPreset { Name = "γ Pav",      RA = "21:26:26.6", Dec = "-65:21:58.3", Magnitude = 4.22, Season = "fa", Constellation = "Pav", Note = "Gamma Pav — HR 8181, very isolated, nearby sun-like star" },
        };

        /// <summary>
        /// Find the best star for a given observing time, location, and optical setup.
        /// Filters by magnitude range (to avoid overexposure), by ±3h hour angle, and
        /// by a minimum altitude floor. Scores remaining candidates by blending current
        /// altitude with transit altitude so the selection prefers stars that pass close
        /// to the local zenith — guaranteeing they will stay high for the duration of
        /// the collimation run and are not on their way below the horizon.
        /// If nothing passes the 30° altitude floor, retries at 20° then 10° before
        /// giving up — prevents silently picking a below-horizon star.
        /// </summary>
        /// <param name="observingTimeUtc">Target observing time in UTC</param>
        /// <param name="latitudeDeg">Observer latitude in degrees north (negative = south)</param>
        /// <param name="longitudeDeg">Observer longitude in degrees east</param>
        /// <param name="focalLengthMm">Telescope focal length in mm (0 to skip mag filter)</param>
        /// <param name="apertureMm">Telescope aperture in mm (0 to skip mag filter)</param>
        /// <param name="exposureSeconds">Exposure time in seconds</param>
        /// <param name="gain">Camera gain setting</param>
        /// <param name="customStars">Optional additional user-defined stars</param>
        /// <returns>Best star, its altitude, and ideal mag range — or null if nothing above 10°</returns>
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

            IReadOnlyList<StarPreset> allStars;
            if (customStars != null && customStars.Count > 0) {
                var merged = new List<StarPreset>(Presets.Count + customStars.Count);
                merged.AddRange(Presets);
                merged.AddRange(customStars);
                allStars = merged;
            } else {
                allStars = Presets;
            }

            // Progressive altitude floors: 30° ideal (good seeing, clear of most obstacles),
            // 20° usable, 10° last resort. Return null below 10° so the caller shows a clear
            // "no suitable star visible now" message instead of silently picking a horizon star.
            double[] minAltitudes = { 30.0, 20.0, 10.0 };
            foreach (var minAlt in minAltitudes) {
                var pick = PickBestAbove(allStars, lst, latitudeDeg, magLow, magHigh, minAlt);
                if (pick != null) {
                    return (pick.Value.Star, pick.Value.Alt, magLow, magHigh);
                }
            }
            return null;
        }

        /// <summary>
        /// Core scorer: returns the highest-scoring star that passes the ±3h hour-angle
        /// gate, the magnitude window, and the minimum altitude floor.
        /// Score = (current altitude) + (transit altitude) - 90, equivalent to
        /// alt - |lat - dec|. This rewards stars that pass close to local zenith,
        /// so the selection does not drift toward the horizon during longer runs.
        /// </summary>
        private static (StarPreset Star, double Alt)? PickBestAbove(
            IReadOnlyList<StarPreset> stars, double lst, double latitudeDeg,
            double magLow, double magHigh, double minAltitudeDeg) {

            StarPreset best = null;
            double bestScore = double.NegativeInfinity;
            double bestAlt = 0;

            foreach (var star in stars) {
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
                if (alt < minAltitudeDeg) continue;

                // Score rewards stars on better visibility arcs, not just current altitude.
                // transitAlt (max altitude this star ever reaches from this observer) is
                // 90 - |lat - dec|; a star at 60° now heading for a 85° transit beats a
                // star at 70° now that will set in an hour. Equivalent to alt - |lat - dec|.
                double transitAlt = 90.0 - Math.Abs(latitudeDeg - decD);
                double score = alt + transitAlt - 90.0;

                if (score > bestScore) {
                    bestScore = score;
                    best = star;
                    bestAlt = alt;
                }
            }

            if (best == null) return null;
            return (best, bestAlt);
        }
    }
}
