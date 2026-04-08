namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// An isolated star suitable for SKW collimation (magnitude matched to optical setup).
    /// </summary>
    public class StarPreset {
        public string Name { get; set; }
        public string RA { get; set; }   // H:M:S format
        public string Dec { get; set; }  // D:M:S format
        public double Magnitude { get; set; }
        public string Season { get; set; } // sp, su, fa, wi
        public string Constellation { get; set; } // IAU abbreviation (e.g. "Dra", "UMa")
        public string Note { get; set; }

        /// <summary>Display label for ComboBox: "Name (Constellation, mag X.X)"</summary>
        public string DisplayLabel => string.IsNullOrEmpty(Constellation)
            ? $"{Name}  (mag {Magnitude:F1})"
            : $"{Name}  ({Constellation}, mag {Magnitude:F1})";
    }
}
