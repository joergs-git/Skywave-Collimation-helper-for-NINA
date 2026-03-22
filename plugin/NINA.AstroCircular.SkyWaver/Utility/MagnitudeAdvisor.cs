using System;

namespace NINA.AstroCircular.SkyWaver.Utility {

    /// <summary>
    /// Recommends ideal star magnitude for a given optical setup and imaging parameters.
    /// Ported from the HTML tool's updateMagAdvice() function.
    /// </summary>
    public static class MagnitudeAdvisor {

        /// <summary>
        /// Compute the ideal star magnitude for SKW collimation.
        /// </summary>
        /// <param name="focalLengthMm">Telescope focal length in mm</param>
        /// <param name="apertureMm">Telescope aperture in mm</param>
        /// <param name="exposureSeconds">Exposure time in seconds</param>
        /// <param name="gain">Camera gain setting</param>
        /// <returns>Tuple of (ideal magnitude, low bound, high bound)</returns>
        public static (double Ideal, double Low, double High) GetIdealMagnitude(
            double focalLengthMm, double apertureMm, double exposureSeconds, int gain) {

            double fRatio = focalLengthMm / apertureMm;
            double fFactor = 2.5 * Math.Log10(fRatio * fRatio / 64.0);
            double eFactor = 2.5 * Math.Log10(exposureSeconds / 8.0);
            double gFactor = (gain - 100) * 0.01;

            double ideal = 4.5 + fFactor - eFactor - gFactor;
            double low = Math.Max(2.0, ideal - 1.0);
            double high = Math.Min(8.0, ideal + 1.0);

            return (ideal, low, high);
        }
    }
}
