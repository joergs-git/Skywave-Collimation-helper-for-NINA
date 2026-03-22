using NINA.AstroCircular.SkyWaver.Models;
using NINA.Core.Utility;
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

            // Try to auto-populate from NINA profile — completely optional
            try {
                LoadFromProfile();
            } catch (Exception ex) {
                Logger.Warning($"SKW: Could not read NINA profile defaults: {ex.Message}");
            }
        }

        private void LoadFromProfile() {
            var profile = profileService?.ActiveProfile;
            if (profile == null) return;

            try {
                var ts = profile.TelescopeSettings;
                if (ts != null && ts.FocalLength > 0) Settings.FocalLengthMm = ts.FocalLength;
                if (ts != null && ts.FocalRatio > 0 && ts.FocalLength > 0) Settings.ApertureMm = ts.FocalLength / ts.FocalRatio;
            } catch { }

            try {
                var cs = profile.CameraSettings;
                if (cs != null && cs.PixelSize > 0) Settings.PixelSizeUm = cs.PixelSize;
            } catch { }

            try {
                var loc = profile.AstrometrySettings;
                if (loc != null && loc.Latitude != 0) Settings.ObserverLatitude = loc.Latitude;
                if (loc != null && loc.Longitude != 0) Settings.ObserverLongitude = loc.Longitude;
            } catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
