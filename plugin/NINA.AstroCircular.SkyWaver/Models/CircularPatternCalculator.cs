using System;
using System.Collections.Generic;

namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// Computes ring positions around a center star for SKW circular capture.
    /// Ported from the HTML tool's calcPositions() function.
    /// </summary>
    public static class CircularPatternCalculator {

        /// <summary>
        /// Calculate N positions arranged in a circle (or ellipse) around a center coordinate.
        /// </summary>
        /// <param name="centerRAHours">Center RA in decimal hours</param>
        /// <param name="centerDecDeg">Center Dec in decimal degrees</param>
        /// <param name="fovWidthDeg">Field of view width in degrees</param>
        /// <param name="fovHeightDeg">Field of view height in degrees</param>
        /// <param name="numPositions">Number of ring positions (e.g. 8)</param>
        /// <param name="radiusPercent">Radius as percentage of FOV (e.g. 80)</param>
        /// <param name="useCircle">True for circular pattern, false for elliptical</param>
        /// <param name="includeCenter">True to include center position as first point</param>
        /// <returns>List of computed ring positions</returns>
        public static List<RingPosition> Calculate(
            double centerRAHours,
            double centerDecDeg,
            double fovWidthDeg,
            double fovHeightDeg,
            int numPositions,
            int radiusPercent,
            bool useCircle,
            bool includeCenter) {

            var positions = new List<RingPosition>();

            if (includeCenter) {
                positions.Add(new RingPosition {
                    RAHours = centerRAHours,
                    DecDegrees = centerDecDeg,
                    Label = "Center"
                });
            }

            double minFov = Math.Min(fovWidthDeg, fovHeightDeg);
            double radiusDeg = (radiusPercent / 100.0) * (minFov / 2.0);
            double cosDec = Math.Cos(centerDecDeg * Math.PI / 180.0);
            double step = 2.0 * Math.PI / numPositions;

            for (int i = 0; i < numPositions; i++) {
                double theta = i * step;
                double dRaDeg, dDecDeg;

                if (useCircle) {
                    // Circular pattern: uniform radius on sky
                    dDecDeg = radiusDeg * Math.Sin(theta);
                    dRaDeg = radiusDeg * Math.Cos(theta) / cosDec;
                } else {
                    // Elliptical pattern: follows sensor aspect ratio
                    dRaDeg = (radiusPercent / 100.0) * (fovWidthDeg / 2.0) * Math.Cos(theta) / cosDec;
                    dDecDeg = (radiusPercent / 100.0) * (fovHeightDeg / 2.0) * Math.Sin(theta);
                }

                positions.Add(new RingPosition {
                    RAHours = centerRAHours + dRaDeg / 15.0,
                    DecDegrees = centerDecDeg + dDecDeg,
                    Label = $"Ring {i + 1}"
                });
            }

            return positions;
        }

        /// <summary>
        /// Compute field of view in degrees from sensor and focal length.
        /// </summary>
        public static (double WidthDeg, double HeightDeg) ComputeFOV(
            double sensorWidthMm, double sensorHeightMm, double focalLengthMm) {
            double widthDeg = (sensorWidthMm / focalLengthMm) * (180.0 / Math.PI);
            double heightDeg = (sensorHeightMm / focalLengthMm) * (180.0 / Math.PI);
            return (widthDeg, heightDeg);
        }
    }
}
