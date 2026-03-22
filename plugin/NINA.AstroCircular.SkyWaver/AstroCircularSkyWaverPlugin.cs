using NINA.AstroCircular.SkyWaver.Models;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace NINA.AstroCircular.SkyWaver {

    [Export(typeof(IPluginManifest))]
    public class AstroCircularSkyWaverPlugin : PluginBase, INotifyPropertyChanged {
        private readonly IProfileService profileService;

        public SkwSettings Settings { get; }

        [ImportingConstructor]
        public AstroCircularSkyWaverPlugin(IProfileService profileService) {
            this.profileService = profileService;
            Settings = new SkwSettings();

            // Auto-populate equipment values from NINA profile
            try { LoadFromProfile(); } catch { /* non-critical */ }
        }

        private void LoadFromProfile() {
            var profile = profileService?.ActiveProfile;
            if (profile == null) return;

            // Telescope
            try {
                double fl = profile.TelescopeSettings.FocalLength;
                double fr = profile.TelescopeSettings.FocalRatio;
                if (fl > 0) Settings.FocalLengthMm = fl;
                if (fr > 0 && fl > 0) Settings.ApertureMm = fl / fr;
            } catch { }

            // Camera pixel size
            try {
                double px = profile.CameraSettings.PixelSize;
                if (px > 0) Settings.PixelSizeUm = px;
            } catch { }

            // Location
            try {
                double lat = profile.AstrometrySettings.Latitude;
                double lon = profile.AstrometrySettings.Longitude;
                if (lat != 0) Settings.ObserverLatitude = lat;
                if (lon != 0) Settings.ObserverLongitude = lon;
            } catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
