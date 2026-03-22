using System;

namespace NINA.AstroCircular.SkyWaver.Utility {

    /// <summary>
    /// Coordinate conversion and astronomical calculation utilities.
    /// Ported from the HTML tool's JavaScript functions.
    /// </summary>
    public static class CoordinateUtils {

        /// <summary>Parse "H:M:S" string to decimal hours (0-24).</summary>
        public static double ParseHMS(string hms) {
            if (string.IsNullOrWhiteSpace(hms)) return 0;
            var parts = hms.Trim().Split(':');
            double h = parts.Length > 0 ? double.Parse(parts[0]) : 0;
            double m = parts.Length > 1 ? double.Parse(parts[1]) : 0;
            double s = parts.Length > 2 ? double.Parse(parts[2]) : 0;
            return h + m / 60.0 + s / 3600.0;
        }

        /// <summary>Parse "D:M:S" string to decimal degrees (-90 to +90).</summary>
        public static double ParseDMS(string dms) {
            if (string.IsNullOrWhiteSpace(dms)) return 0;
            string s = dms.Trim();
            bool negative = s.StartsWith("-");
            if (negative) s = s.Substring(1);
            if (s.StartsWith("+")) s = s.Substring(1);
            var parts = s.Split(':');
            double d = parts.Length > 0 ? double.Parse(parts[0]) : 0;
            double m = parts.Length > 1 ? double.Parse(parts[1]) : 0;
            double sec = parts.Length > 2 ? double.Parse(parts[2]) : 0;
            double result = Math.Abs(d) + m / 60.0 + sec / 3600.0;
            return negative ? -result : result;
        }

        /// <summary>Convert decimal hours to "H:MM:SS.S" string.</summary>
        public static string ToHMS(double hours) {
            hours = ((hours % 24) + 24) % 24;
            int h = (int)hours;
            double rm = (hours - h) * 60;
            int m = (int)rm;
            double s = Math.Max(0, (rm - m) * 60);
            return $"{h}:{m:D2}:{s:00.0}";
        }

        /// <summary>Convert decimal degrees to "+D:MM:SS.S" string.</summary>
        public static string ToDMS(double degrees) {
            bool negative = degrees < 0;
            degrees = Math.Abs(degrees);
            int d = (int)degrees;
            double rm = (degrees - d) * 60;
            int m = (int)rm;
            double s = Math.Max(0, (rm - m) * 60);
            return $"{(negative ? "-" : "+")}{d}:{m:D2}:{s:00.0}";
        }

        /// <summary>Compute Julian Date from a UTC DateTime.</summary>
        public static double GetJulianDate(DateTime utc) {
            int y = utc.Year;
            int m = utc.Month;
            double day = utc.Day + utc.Hour / 24.0 + utc.Minute / 1440.0 + utc.Second / 86400.0;
            if (m <= 2) { y--; m += 12; }
            int a = y / 100;
            int b = 2 - a + a / 4;
            return Math.Floor(365.25 * (y + 4716)) + Math.Floor(30.6001 * (m + 1)) + day + b - 1524.5;
        }

        /// <summary>Compute Local Sidereal Time in hours for a given UTC time and longitude.</summary>
        public static double GetLST(DateTime utc, double longitudeDeg) {
            double jd = GetJulianDate(utc);
            double t = (jd - 2451545.0) / 36525.0;
            double gmst = 280.46061837 + 360.98564736629 * (jd - 2451545.0)
                          + 0.000387933 * t * t - t * t * t / 38710000.0;
            return (((gmst + longitudeDeg) % 360 + 360) % 360) / 15.0;
        }

        /// <summary>Compute altitude of an object in degrees given observer latitude, object RA/Dec, and LST.</summary>
        public static double GetAltitude(double latDeg, double raHours, double decDeg, double lstHours) {
            double latR = latDeg * Math.PI / 180.0;
            double decR = decDeg * Math.PI / 180.0;
            double ha = lstHours - raHours;
            if (ha > 12) ha -= 24;
            if (ha < -12) ha += 24;
            double haR = ha * 15.0 * Math.PI / 180.0;
            return Math.Asin(
                Math.Sin(latR) * Math.Sin(decR) +
                Math.Cos(latR) * Math.Cos(decR) * Math.Cos(haR)
            ) * 180.0 / Math.PI;
        }

        /// <summary>
        /// Approximate nautical dusk time (sun at -12° below horizon).
        /// Returns UTC DateTime for the given date, latitude, and longitude.
        /// </summary>
        public static DateTime GetNauticalDusk(DateTime date, double latDeg, double lonDeg) {
            double latR = latDeg * Math.PI / 180.0;
            int doy = date.DayOfYear;

            // Solar declination approximation
            double decSun = 23.45 * Math.Sin(2 * Math.PI * (284 + doy) / 365.0) * Math.PI / 180.0;

            // Hour angle when sun is at -12°
            double sunAltR = -12.0 * Math.PI / 180.0;
            double cosHA = (Math.Sin(sunAltR) - Math.Sin(latR) * Math.Sin(decSun))
                           / (Math.Cos(latR) * Math.Cos(decSun));
            cosHA = Math.Max(-1, Math.Min(1, cosHA));
            double haHours = Math.Acos(cosHA) * 180.0 / Math.PI / 15.0;

            // Equation of time approximation
            double eqTime = (-7.655 * Math.Sin(2 * Math.PI * doy / 365.0)
                             + 9.873 * Math.Sin(2 * (2 * Math.PI * doy / 365.0) + 3.5932)) / 60.0;
            double solarNoonUTC = 12.0 - lonDeg / 15.0 - eqTime;
            double duskUTC = solarNoonUTC + haHours;

            var dusk = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            dusk = dusk.AddHours(duskUTC);
            return dusk;
        }
    }
}
