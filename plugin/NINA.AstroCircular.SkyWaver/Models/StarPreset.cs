namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// A bright, isolated star suitable for SKW collimation.
    /// </summary>
    public class StarPreset {
        public string Name { get; set; }
        public string RA { get; set; }   // H:M:S format
        public string Dec { get; set; }  // D:M:S format
        public double Magnitude { get; set; }
        public string Season { get; set; } // sp, su, fa, wi
        public string Note { get; set; }
    }
}
