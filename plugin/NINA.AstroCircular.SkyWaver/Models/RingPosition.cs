namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// A computed position in the circular capture pattern.
    /// </summary>
    public class RingPosition {
        /// <summary>Right Ascension in hours (0-24)</summary>
        public double RAHours { get; set; }

        /// <summary>Declination in degrees (-90 to +90)</summary>
        public double DecDegrees { get; set; }

        /// <summary>Human-readable label (e.g. "Center", "Ring 1")</summary>
        public string Label { get; set; }
    }
}
