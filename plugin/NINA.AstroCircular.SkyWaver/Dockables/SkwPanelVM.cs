using NINA.Astrometry;
using NINA.AstroCircular.SkyWaver.Imaging;
using NINA.AstroCircular.SkyWaver.Models;
using NINA.AstroCircular.SkyWaver.Utility;
using NINA.Core.Model;
using NINA.Core.Model.Equipment;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Model;
using NINA.Image.FileFormat;
using NINA.Image.Interfaces;
using NINA.PlateSolving;
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem.Platesolving;
using NINA.WPF.Base.Interfaces;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NINA.AstroCircular.SkyWaver.Dockables {

    [Export(typeof(NINA.Equipment.Interfaces.ViewModel.IDockableVM))]
    public class SkwPanelVM : DockableVM {

        // UI token meaning "use whatever filter is currently active on the wheel".
        // Resolved to a concrete filter name via ResolveFilterName() before any wheel motion.
        private const string DefaultFilterToken = "Default";

        // Filter used for plate-solving the centering step — Luminance gives the best SNR
        // for solving across most cameras. Fallback if ResolveFilterName has nothing to return.
        private const string PlateSolveFilterName = "L";

        private readonly ITelescopeMediator telescopeMediator;
        private readonly ICameraMediator cameraMediator;
        private readonly IFocuserMediator focuserMediator;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly IImagingMediator imagingMediator;
        private readonly IImageDataFactory imageDataFactory;
        private readonly IGuiderMediator guiderMediator;
        private readonly IDomeMediator domeMediator;
        private readonly IDomeFollower domeFollower;
        private readonly IAutoFocusVMFactory autoFocusVMFactory;

        private CancellationTokenSource runCts;

        [ImportingConstructor]
        public SkwPanelVM(
            IProfileService profileService,
            ITelescopeMediator telescopeMediator,
            ICameraMediator cameraMediator,
            IFocuserMediator focuserMediator,
            IFilterWheelMediator filterWheelMediator,
            IImagingMediator imagingMediator,
            IImageDataFactory imageDataFactory,
            IGuiderMediator guiderMediator,
            IDomeMediator domeMediator,
            IDomeFollower domeFollower,
            IAutoFocusVMFactory autoFocusVMFactory
        ) : base(profileService) {
            this.telescopeMediator = telescopeMediator;
            this.cameraMediator = cameraMediator;
            this.focuserMediator = focuserMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.imagingMediator = imagingMediator;
            this.imageDataFactory = imageDataFactory;
            this.guiderMediator = guiderMediator;
            this.domeMediator = domeMediator;
            this.domeFollower = domeFollower;
            this.autoFocusVMFactory = autoFocusVMFactory;

            Title = "Collimation Helper for SkyWave";

            // Icon — load from DockableTemplates.xaml using PathGeometry (same pattern as InjectAutofocus)
            try {
                var dict = new ResourceDictionary();
                dict.Source = new Uri("NINA.CollimationHelper.SkyWave;component/Dockables/DockableTemplates.xaml", UriKind.RelativeOrAbsolute);
                ImageGeometry = (System.Windows.Media.GeometryGroup)dict["SkwCollimationSVG"];
                ImageGeometry.Freeze();
            } catch (Exception ex) {
                Logger.Warning($"SKW: Icon load failed (non-critical): {ex.Message}");
            }

            // Commands
            RunCommand = new AsyncCommand<bool>(RunCollimation, (o) => !IsRunning);
            CancelCommand = new RelayCommand((o) => Cancel());
            BrowseFolderCommand = new RelayCommand((o) => BrowseFolder());
            FindBestStarCommand = new RelayCommand((o) => FindBestStar());
            UseMountPositionCommand = new RelayCommand((o) => UseMountPosition());
            ZoomInCommand = new RelayCommand((o) => ZoomIn());
            ZoomOutCommand = new RelayCommand((o) => ZoomOut());
            ZoomResetCommand = new RelayCommand((o) => ZoomReset());

            // Load settings — safe even without devices
            try { LoadSettings(); } catch (Exception ex) {
                Logger.Warning($"SKW: Settings load failed (non-critical): {ex.Message}");
            }

            // Populate filter dropdown — devices may not be connected yet
            try { RefreshAvailableFilters(); } catch (Exception ex) {
                Logger.Warning($"SKW: Filter list refresh failed (non-critical): {ex.Message}");
            }

            // Try to populate from NINA profile
            try { LoadFromProfile(); } catch { }

            // Build initial map
            try { BuildMapPositions(); } catch { }
        }

        public override bool IsTool => true;

        // ── Commands ──

        public ICommand RunCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand FindBestStarCommand { get; }
        public ICommand UseMountPositionCommand { get; }

        // ── State ──

        private bool isRunning;
        public bool IsRunning {
            get => isRunning;
            set { isRunning = value; RaisePropertyChanged(); }
        }

        private string statusText = "Ready";
        public string StatusText {
            get => statusText;
            set { statusText = value; RaisePropertyChanged(); }
        }

        private int progress;
        public int Progress {
            get => progress;
            set { progress = value; RaisePropertyChanged(); }
        }

        // ── Star Selection ──

        private string starName = "theta Boo";
        public string StarName {
            get => starName;
            set { starName = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private string targetRA = "14:25:11.8";
        public string TargetRA {
            get => targetRA;
            set { targetRA = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private string targetDec = "51:51:02.7";
        public string TargetDec {
            get => targetDec;
            set { targetDec = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        // ── Defocus ──

        private int defocusSteps = 2442;
        public int DefocusSteps {
            get => defocusSteps;
            set { defocusSteps = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(DefocusMicronsText)); SaveSettings(); }
        }

        private double micronsPerStep = 3;
        /// <summary>
        /// Microns of travel per focuser step. Depends on your specific focuser + OAZ combination.
        /// To measure: move N steps (e.g. 10000), measure physical distance from scope backplate,
        /// divide distance by N. Example: ZWO EAF + FeatherTouch OAZ on RC12 ≈ 3 µm/step.
        /// </summary>
        public double MicronsPerStep {
            get => micronsPerStep;
            set { micronsPerStep = value > 0 ? value : 0.1; RaisePropertyChanged(); RaisePropertyChanged(nameof(DefocusMicronsText)); SaveSettings(); }
        }

        /// <summary>Read-only info: converts defocus steps to microns for comparison with SkyWave.</summary>
        public string DefocusMicronsText => micronsPerStep > 0
            ? $"= {(defocusSteps * micronsPerStep):F0} µm"
            : "";

        private int defocusDirection = 1;
        public int DefocusDirection {
            get => defocusDirection;
            set { defocusDirection = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(DefocusDirectionIndex)); SaveSettings(); }
        }

        /// <summary>ComboBox index: 0 = Extra-focal (+1), 1 = Intra-focal (-1)</summary>
        public int DefocusDirectionIndex {
            get => DefocusDirection == 1 ? 0 : 1;
            set { DefocusDirection = value == 0 ? 1 : -1; }
        }

        // ── Imaging ──

        private double exposureTime = 8;
        public double ExposureTime {
            get => exposureTime;
            set { exposureTime = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private string filterName = DefaultFilterToken;
        public string FilterName {
            get => filterName;
            set { filterName = value; RaisePropertyChanged(); SaveSettings(); }
        }

        /// <summary>Filter names from the connected filter wheel, plus "Default" (= active filter).</summary>
        private ObservableCollection<string> availableFilters = new ObservableCollection<string> { DefaultFilterToken };
        public ObservableCollection<string> AvailableFilters {
            get => availableFilters;
            set { availableFilters = value; RaisePropertyChanged(); }
        }

        /// <summary>Refreshes the filter dropdown from the NINA profile filter wheel settings.</summary>
        private void RefreshAvailableFilters() {
            var filters = new ObservableCollection<string> { DefaultFilterToken };
            var profileFilters = profileService?.ActiveProfile?.FilterWheelSettings?.FilterWheelFilters;
            if (profileFilters != null) {
                foreach (var f in profileFilters) {
                    if (!string.IsNullOrWhiteSpace(f.Name)) filters.Add(f.Name);
                }
            }
            AvailableFilters = filters;
            if (!filters.Contains(filterName)) {
                FilterName = DefaultFilterToken;
            }
        }

        private int gain = 100;
        public int Gain {
            get => gain;
            set { gain = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int offset = 0;
        public int Offset {
            get => offset;
            set { offset = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int binning = 1;
        public int Binning {
            get => binning;
            set { binning = 1; } // SkyWave only supports unbinned images
        }

        // ── Pattern ──

        private int ringPositions = 8;
        public int RingPositions {
            get => ringPositions;
            set { ringPositions = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private int radiusPercent = 80;
        public int RadiusPercent {
            get => radiusPercent;
            set { radiusPercent = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private int settleSeconds = 3;
        public int SettleSeconds {
            get => settleSeconds;
            set { settleSeconds = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private bool includeCenter = true;
        public bool IncludeCenter {
            get => includeCenter;
            set { includeCenter = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        // ── Autofocus ──

        private bool runAutofocus = false;
        public bool RunAutofocus {
            get => runAutofocus;
            set { runAutofocus = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Integration ──

        private bool cropAfterStack = false;
        public bool CropAfterStack {
            get => cropAfterStack;
            set { cropAfterStack = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private bool autoCleanSubFrames = false;
        public bool AutoCleanSubFrames {
            get => autoCleanSubFrames;
            set { autoCleanSubFrames = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Output ──

        private string skyWaveOutputDirectory = "";
        public string SkyWaveOutputDirectory {
            get => string.IsNullOrEmpty(skyWaveOutputDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SKW_Output")
                : skyWaveOutputDirectory;
            set { skyWaveOutputDirectory = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Sensor Map ──

        public ObservableCollection<MapPosition> MapPositions { get; } = new ObservableCollection<MapPosition>();

        private string progressText = "";
        public string ProgressText {
            get => progressText;
            set { progressText = value; RaisePropertyChanged(); }
        }

        // Map canvas dimensions — dynamically sized
        private double mapWidth = 560;
        public double MapWidth {
            get => mapWidth;
            set { mapWidth = value; RaisePropertyChanged(); }
        }

        private double mapHeight = 400;
        public double MapHeight {
            get => mapHeight;
            set { mapHeight = value; RaisePropertyChanged(); }
        }

        private double ringCanvasRadius;
        public double RingCanvasRadius {
            get => ringCanvasRadius;
            set { ringCanvasRadius = value; RaisePropertyChanged(); }
        }
        public double RingCanvasCenterX => MapWidth / 2;
        public double RingCanvasCenterY => MapHeight / 2;

        /// <summary>
        /// Build the map positions for visualization based on current settings.
        /// The map frame represents the sensor — aspect ratio matches the real sensor.
        /// </summary>
        private void BuildMapPositions() {
            MapPositions.Clear();

            double sensorW = 36.0, sensorH = 24.0;
            try {
                var px = profileService?.ActiveProfile?.CameraSettings?.PixelSize ?? 0;
                if (px > 0) {
                    var camInfo = cameraMediator.GetInfo();
                    if (camInfo.XSize > 0) sensorW = camInfo.XSize * px / 1000.0;
                    if (camInfo.YSize > 0) sensorH = camInfo.YSize * px / 1000.0;
                }
            } catch { }

            double fl = 1946;
            try { fl = profileService?.ActiveProfile?.TelescopeSettings?.FocalLength ?? 1946; if (fl <= 0) fl = 1946; } catch { }

            // Set map dimensions to match sensor aspect ratio (fit within 600px max)
            double sensorAspect = sensorW / sensorH;
            double maxW = 600;
            if (sensorAspect >= 1) {
                MapWidth = maxW;
                MapHeight = maxW / sensorAspect;
            } else {
                MapHeight = maxW;
                MapWidth = maxW * sensorAspect;
            }

            var (fovW, fovH) = CircularPatternCalculator.ComputeFOV(sensorW, sensorH, fl);
            double centerRA = CoordinateUtils.ParseHMS(TargetRA);
            double centerDec = CoordinateUtils.ParseDMS(TargetDec);

            var positions = CircularPatternCalculator.Calculate(
                centerRA, centerDec, fovW, fovH,
                RingPositions, RadiusPercent, true, IncludeCenter);

            // Map sky positions to canvas coordinates — centered in the frame
            double pad = 12;
            double drawW = MapWidth - 2 * pad;
            double drawH = MapHeight - 2 * pad;
            double cx = MapWidth / 2.0;
            double cy = MapHeight / 2.0;
            double cosDec = Math.Cos(centerDec * Math.PI / 180.0);

            // Ring radius on canvas
            double minFov = Math.Min(fovW, fovH);
            double ringDeg = (RadiusPercent / 100.0) * (minFov / 2.0);
            double minDraw = Math.Min(drawW, drawH);
            RingCanvasRadius = (ringDeg / (minFov / 2.0)) * (minDraw / 2.0);

            foreach (var pos in positions) {
                double dRaDeg = (pos.RAHours - centerRA) * 15.0 * cosDec;
                double dDecDeg = pos.DecDegrees - centerDec;

                // Scale relative to FOV, centered in canvas
                double canvasX = cx + (dRaDeg / fovW) * drawW;
                double canvasY = cy - (dDecDeg / fovH) * drawH;

                MapPositions.Add(new MapPosition {
                    Label = pos.Label,
                    CanvasX = canvasX,
                    CanvasY = canvasY,
                    IsCenter = pos.Label == "Center",
                    State = PositionState.Pending
                });
            }

            ProgressText = $"0 / {positions.Count} positions";
        }

        private void RebuildMap() {
            if (!IsRunning) {
                try { BuildMapPositions(); } catch { }
            }
        }

        // ── Last Captured Image Preview ──

        private System.Windows.Media.Imaging.BitmapSource lastCapturedImage;
        public System.Windows.Media.Imaging.BitmapSource LastCapturedImage {
            get => lastCapturedImage;
            set { lastCapturedImage = value; RaisePropertyChanged(); }
        }

        // ── Preview Zoom ──

        private double previewZoom = 1.0;
        /// <summary>Zoom level for the camera preview (1.0 = fit, up to 4.0 = 4x pixel zoom).</summary>
        public double PreviewZoom {
            get => previewZoom;
            set { previewZoom = Math.Max(1.0, Math.Min(4.0, value)); RaisePropertyChanged(); RaisePropertyChanged(nameof(PreviewZoomText)); }
        }

        /// <summary>Read-only display text for current zoom level.</summary>
        public string PreviewZoomText => previewZoom > 1.01 ? $"{previewZoom:F1}×" : "Fit";

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ZoomResetCommand { get; }

        private void ZoomIn() { PreviewZoom += 0.5; }
        private void ZoomOut() { PreviewZoom -= 0.5; }
        private void ZoomReset() { PreviewZoom = 1.0; }

        /// <summary>
        /// Create a stretched thumbnail from image data for the preview panel.
        /// Uses midtone transfer function (MTF) similar to NINA's auto-stretch.
        /// </summary>
        private void UpdatePreviewImage(NINA.Image.Interfaces.IImageData imageData) {
            try {
                if (imageData == null) return;
                var pixels = imageData.Data.FlatArray;
                int w = imageData.Properties.Width;
                int h = imageData.Properties.Height;

                // Downsample to thumbnail (max 800px wide for crisp preview at doubled panel size)
                int scale = Math.Max(1, w / 800);
                int tw = w / scale;
                int th = h / scale;

                // Sample pixels for statistics (every Nth pixel for speed)
                int sampleStep = Math.Max(1, pixels.Length / 10000);
                var samples = new List<ushort>(10000);
                for (int i = 0; i < pixels.Length; i += sampleStep) {
                    samples.Add(pixels[i]);
                }
                samples.Sort();

                // Robust statistics: median and MAD (median absolute deviation)
                ushort median = samples[samples.Count / 2];
                var deviations = new List<double>(samples.Count);
                foreach (var s in samples) {
                    deviations.Add(Math.Abs(s - median));
                }
                deviations.Sort();
                double mad = deviations[deviations.Count / 2] * 1.4826; // scale to sigma

                // Moderate stretch: clip at median - 1*sigma, stretch to median + 8*sigma
                double clipLow = Math.Max(0, median - 1.0 * mad);
                double clipHigh = Math.Min(65535, median + 8.0 * mad);
                double range = Math.Max(1, clipHigh - clipLow);

                // Create 8-bit grayscale bitmap with moderate gamma stretch
                byte[] bmpData = new byte[tw * th];
                for (int y = 0; y < th; y++) {
                    for (int x = 0; x < tw; x++) {
                        int srcIdx = (y * scale) * w + (x * scale);
                        if (srcIdx < pixels.Length) {
                            double normalized = (pixels[srcIdx] - clipLow) / range;
                            normalized = Math.Max(0, Math.Min(1, normalized));
                            // Gentle gamma for natural look (0.6 = moderate boost)
                            normalized = Math.Pow(normalized, 0.6);
                            bmpData[y * tw + x] = (byte)(255.0 * normalized);
                        }
                    }
                }

                var bmp = System.Windows.Media.Imaging.BitmapSource.Create(
                    tw, th, 96, 96,
                    System.Windows.Media.PixelFormats.Gray8, null,
                    bmpData, tw);
                bmp.Freeze();

                System.Windows.Application.Current?.Dispatcher?.Invoke(() => {
                    LastCapturedImage = bmp;
                });
            } catch { }
        }

        // ── Star Presets (for ComboBox) ──

        public List<StarPreset> StarPresets => StarCatalog.Presets;

        private StarPreset selectedPreset;
        public StarPreset SelectedPreset {
            get => selectedPreset;
            set {
                selectedPreset = value;
                if (value != null) {
                    StarName = value.Name;
                    TargetRA = value.RA;
                    TargetDec = value.Dec;
                }
                RaisePropertyChanged();
            }
        }

        // ── Folder Browser ──

        private void BrowseFolder() {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.Description = "Select SkyWave watch folder";
                if (!string.IsNullOrEmpty(SkyWaveOutputDirectory)) {
                    dialog.SelectedPath = SkyWaveOutputDirectory;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    SkyWaveOutputDirectory = dialog.SelectedPath;
                }
            }
        }

        // ── Use Current Mount Position ──

        /// <summary>
        /// Reads the mount's current pointing and loads it into TargetRA / TargetDec,
        /// bypassing the preset catalog. Lets the user slew manually (hand controller,
        /// Stellarium, NINA framing wizard) to any point on the sky — including zenith —
        /// then run the ring pattern from there. Essential for southern observers whose
        /// local sky is not well covered by the preset catalog.
        /// </summary>
        private void UseMountPosition() {
            try {
                var info = telescopeMediator.GetInfo();
                if (info == null || !info.Connected) {
                    StatusText = "Mount not connected — cannot read current position";
                    return;
                }

                // NINA's TelescopeInfo exposes RightAscension in decimal hours and
                // Declination in decimal degrees. Native epoch depends on the mount;
                // we treat it as J2000 downstream. Center will plate-solve the actual
                // sky at these coordinates, so any epoch offset resolves on its own.
                double raHours = info.RightAscension;
                double decDeg = info.Declination;

                // Batch update: set backing fields directly, raise PropertyChanged once
                // per field, then SaveSettings + RebuildMap once. Going through the public
                // setters would fire 3 SaveSettings (60 profile writes) and 2 RebuildMaps.
                SelectedPreset = null;
                starName = "Mount position";
                targetRA = CoordinateUtils.ToHMS(raHours);
                targetDec = CoordinateUtils.ToDMS(decDeg);
                RaisePropertyChanged(nameof(StarName));
                RaisePropertyChanged(nameof(TargetRA));
                RaisePropertyChanged(nameof(TargetDec));
                SaveSettings();
                RebuildMap();

                StatusText = $"Using mount position: RA {TargetRA}, Dec {TargetDec}";
            } catch (Exception ex) {
                StatusText = $"Mount position read failed: {ex.Message}";
                Logger.Warning($"SKW: UseMountPosition failed: {ex}");
            }
        }

        // ── Find Best Star ──

        private void FindBestStar() {
            try {
                double lat = profileService?.ActiveProfile?.AstrometrySettings?.Latitude ?? 52.17;
                double lon = profileService?.ActiveProfile?.AstrometrySettings?.Longitude ?? 7.25;

                // Pass optical setup so MagnitudeAdvisor can filter for ~60% ADU
                double fl = profileService?.ActiveProfile?.TelescopeSettings?.FocalLength ?? 0;
                double aperture = fl > 0 && profileService?.ActiveProfile?.TelescopeSettings?.FocalRatio > 0
                    ? fl / profileService.ActiveProfile.TelescopeSettings.FocalRatio : 0;

                var result = StarCatalog.FindBestStar(
                    DateTime.UtcNow, lat, lon,
                    fl, aperture, ExposureTime, Gain);

                if (result.HasValue) {
                    SelectedPreset = result.Value.Star;
                    StatusText = $"Best star: {result.Value.Star.Name} (alt {result.Value.AltitudeDeg:F0}°, mag {result.Value.Star.Magnitude:F1}, ideal range {result.Value.MagLow:F1}–{result.Value.MagHigh:F1})";
                } else {
                    // Fall back without magnitude filter
                    var fallback = StarCatalog.FindBestStar(DateTime.UtcNow, lat, lon);
                    if (fallback.HasValue) {
                        SelectedPreset = fallback.Value.Star;
                        StatusText = $"No star in ideal mag range — using {fallback.Value.Star.Name} (mag {fallback.Value.Star.Magnitude:F1}, alt {fallback.Value.AltitudeDeg:F0}°). Check exposure time.";
                    } else {
                        StatusText = "No suitable star within 3h of meridian";
                    }
                }
            } catch (Exception ex) {
                StatusText = $"Star finder error: {ex.Message}";
            }
        }

        // ── Filter Helpers ──

        /// <summary>Returns the actual filter name, resolving "Default" to the currently active filter.</summary>
        private string ResolveFilterName(string name) {
            if (string.Equals(name, DefaultFilterToken, StringComparison.OrdinalIgnoreCase)) {
                var info = filterWheelMediator.GetInfo();
                if (info?.Connected == true && !string.IsNullOrWhiteSpace(info.SelectedFilter?.Name))
                    return info.SelectedFilter.Name;
                return PlateSolveFilterName;
            }
            return name;
        }

        /// <summary>
        /// Switches to the named filter. Callers must pass a concrete filter name —
        /// resolve "Default" to the actual name via ResolveFilterName() before calling,
        /// otherwise the lookup falls back to slot -1 and the wheel may not move.
        /// </summary>
        private async Task SwitchFilter(string name, CancellationToken ct) {
            try {
                if (string.IsNullOrWhiteSpace(name)) return;
                var target = FilterUtils.LookupFilterInfo(name, profileService);
                await filterWheelMediator.ChangeFilter(target, ct);
            } catch (Exception ex) {
                Logger.Warning($"SKW: Filter switch to '{name}' failed ({ex.Message})");
            }
        }

        // ── Cancel ──

        private void Cancel() {
            runCts?.Cancel();
            StatusText = "Cancelling...";
        }

        // ══════════════════════════════════════════════════════════
        // MAIN WORKFLOW — Run Collimation
        // ══════════════════════════════════════════════════════════

        private async Task<bool> RunCollimation() {
            if (IsRunning) return false;

            // Pre-flight checks
            if (!telescopeMediator.GetInfo().Connected) {
                StatusText = "Error: Mount not connected";
                return false;
            }
            if (!cameraMediator.GetInfo().Connected) {
                StatusText = "Error: Camera not connected";
                return false;
            }
            if (!focuserMediator.GetInfo().Connected) {
                StatusText = "Error: Focuser not connected";
                return false;
            }

            IsRunning = true;
            Progress = 0;
            runCts = new CancellationTokenSource();
            var ct = runCts.Token;
            var progressReporter = new Progress<ApplicationStatus>(s => StatusText = s.Status);

            int originalFocuserPos = focuserMediator.GetInfo().Position;
            int relativeDefocus = DefocusSteps * DefocusDirection;
            bool hasDefocused = false; // Track whether defocus actually happened
            string originalFilter = null; // Track filter before we switch to L
            bool success = false;
            string successOutputFile = null;
            string outputDir = SkyWaveOutputDirectory;
            string sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string subFrameDir = Path.Combine(outputDir, "subframes_" + sessionId);

            try {
                // Get equipment params for FOV calculation
                double fl = profileService?.ActiveProfile?.TelescopeSettings?.FocalLength ?? 1946;
                double sensorW = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().XSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0
                    : 36.0;
                double sensorH = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().YSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0
                    : 24.0;

                // Step 1: Capture filter state, then switch to L for plate-solve.
                // Pre-resolving the capture filter BEFORE we touch the wheel is critical:
                // if we resolve "Default" later, after we've already changed filters,
                // "Default" would wrongly mean "whatever filter plate-solving happened to leave us on".
                var fwInfo = filterWheelMediator.GetInfo();
                if (fwInfo?.Connected == true && fwInfo.SelectedFilter != null)
                    originalFilter = fwInfo.SelectedFilter.Name;
                string captureFilter = ResolveFilterName(FilterName);

                StatusText = $"Switching to {PlateSolveFilterName} filter for plate-solve...";
                Progress = 3;
                await SwitchFilter(PlateSolveFilterName, ct);

                // Step 2: Slew & Center on target star (plate-solve, in focus, L filter)
                StatusText = $"Centering on {StarName} (slew + plate-solve)...";
                Progress = 10;
                var coords = new Coordinates(
                    Angle.ByHours(CoordinateUtils.ParseHMS(TargetRA)),
                    Angle.ByDegree(CoordinateUtils.ParseDMS(TargetDec)),
                    Epoch.J2000);

                try {
                    var centerInstruction = new Center(
                        profileService, telescopeMediator, imagingMediator, filterWheelMediator,
                        guiderMediator, domeMediator, domeFollower,
                        new PlateSolverFactoryProxy(), new NINA.Core.Utility.WindowService.WindowServiceFactory()
                    ) {
                        Coordinates = new NINA.Astrometry.InputCoordinates(coords)
                    };
                    await centerInstruction.Execute(progressReporter, ct);

                    // Sync the mount to the plate-solved J2000 coordinates. The Center step has
                    // physically parked the scope on the star, so syncing here anchors the mount's
                    // internal position model to the truth. If the ASCOM driver has a fixed epoch
                    // offset (e.g. NYX-101, 10Micron J2000↔JNOW quirks), subsequent blind ring slews
                    // — which can't plate-solve while defocused — at least start from a corrected
                    // reference point. Sync is best-effort: not all drivers support it, so a failure
                    // here is logged but never aborts the run.
                    try {
                        bool synced = await telescopeMediator.Sync(coords);
                        if (synced)
                            Logger.Info($"SKW: Mount synced to plate-solved coords for {StarName} (J2000 RA={coords.RA:F4}h Dec={coords.Dec:F4}°)");
                        else
                            Logger.Warning("SKW: Mount sync returned false — driver may not support sync. Ring slews will rely on driver epoch handling alone.");
                    } catch (Exception syncEx) {
                        Logger.Warning($"SKW: Mount sync after Center failed ({syncEx.Message}). Continuing — ring slews may drift if driver misreports epoch.");
                    }

                    StatusText = $"Centered on {StarName} successfully";
                } catch (Exception psEx) {
                    // Plate-solve is the one step that normally hides mount-side epoch bugs
                    // (e.g. 10Micron ASCOM drivers that misreport EquatorialSystem): a successful
                    // plate-solve physically parks the mount on the star regardless of the
                    // driver's advertised epoch. Once we fall through to a blind slew, any
                    // J2000 ↔ JNOW mismatch in the driver translates directly into a pattern
                    // offset of ~8–12 arcmin at high declinations. Make this path LOUD so
                    // users who thought they were plate-solving don't silently ship an offset run.
                    Logger.Warning($"SKW: Plate-solve centering failed ({psEx.Message}). Falling back to blind slew. " +
                                   "If the mount ASCOM driver misreports its equatorial system (J2000 vs JNOW), " +
                                   "the ring pattern will be offset by the precession amount.");
                    NINA.Core.Utility.Notification.Notification.ShowWarning(
                        $"SKW: Plate-solve failed — blind-slewing to {StarName}.\n" +
                        "The pattern may be offset if the mount's ASCOM driver misreports " +
                        "its epoch (J2000 vs JNOW). Check the driver's equatorial system setting.");
                    StatusText = $"WARNING: Plate-solve failed — blind slew to {StarName} (pattern may drift)";
                    await telescopeMediator.SlewToCoordinatesAsync(coords, ct);
                }

                // Step 3: Switch to target filter for capture (before defocus).
                // Always uses the pre-resolved captureFilter — never "Default" — so plate-solving
                // cannot leave us on the wrong filter regardless of what NINA's Center instruction
                // did internally (it may have switched to the profile's plate-solve filter).
                StatusText = $"Switching to {captureFilter} filter for capture...";
                Progress = 15;
                await SwitchFilter(captureFilter, ct);

                // Step 3b: Autofocus before defocusing (if enabled)
                if (RunAutofocus) {
                    StatusText = "Running autofocus...";
                    Progress = 17;
                    var af = autoFocusVMFactory.Create();
                    var afFilter = FilterUtils.LookupFilterInfo(captureFilter, profileService);
                    await af.StartAutoFocus(afFilter, ct, progressReporter);
                    Logger.Info("SKW: Autofocus completed before defocus");
                    StatusText = "Autofocus complete";
                }

                // Step 4: Defocus
                StatusText = $"Defocusing {(relativeDefocus > 0 ? "+" : "")}{relativeDefocus} steps...";
                Progress = 18;
                await focuserMediator.MoveFocuserRelative(relativeDefocus, ct);
                hasDefocused = true;

                // Step 5: Compute positions and capture
                var (fovW, fovH) = CircularPatternCalculator.ComputeFOV(sensorW, sensorH, fl);
                double centerRA = CoordinateUtils.ParseHMS(TargetRA);
                double centerDec = CoordinateUtils.ParseDMS(TargetDec);
                var positions = CircularPatternCalculator.Calculate(
                    centerRA, centerDec, fovW, fovH,
                    RingPositions, RadiusPercent, true, IncludeCenter);

                Directory.CreateDirectory(subFrameDir);
                var capturedFiles = new List<string>();
                int total = positions.Count;

                // Build the visual map
                BuildMapPositions();

                Logger.Info($"SKW: Starting capture loop — {total} positions, subFrameDir={subFrameDir}");

                for (int i = 0; i < total; i++) {
                    ct.ThrowIfCancellationRequested();
                    var pos = positions[i];
                    int pctBase = 20;
                    int pctRange = 60;
                    Progress = pctBase + (i * pctRange / total);

                    // Update map
                    if (i < MapPositions.Count) MapPositions[i].State = PositionState.Active;
                    ProgressText = $"{i} / {total} positions";

                    var posCoords = new Coordinates(
                        Angle.ByHours(pos.RAHours),
                        Angle.ByDegree(pos.DecDegrees),
                        Epoch.J2000);

                    // Blind slew — telescope stays defocused, positions are intentionally off-center
                    StatusText = $"Slewing to {pos.Label} ({i + 1}/{total})...";
                    try {
                        await telescopeMediator.SlewToCoordinatesAsync(posCoords, ct);
                    } catch {
                        StatusText = $"Slew to {pos.Label} failed, skipping...";
                        if (i < MapPositions.Count) MapPositions[i].State = PositionState.Failed;
                        continue;
                    }

                    // Settle
                    if (SettleSeconds > 0) {
                        await Task.Delay(TimeSpan.FromSeconds(SettleSeconds), ct);
                    }

                    // Capture and save
                    StatusText = $"Exposing {ExposureTime}s at {pos.Label} ({i + 1}/{total})...";
                    try {
                        var captureSeq = new CaptureSequence(
                            ExposureTime,
                            CaptureSequence.ImageTypes.LIGHT,
                            FilterUtils.LookupFilterInfo(captureFilter, profileService),
                            new BinningMode((short)Binning, (short)Binning),
                            1) {
                            Gain = Gain,
                            Offset = Offset
                        };

                        var exposureData = await imagingMediator.CaptureImage(captureSeq, ct, progressReporter);
                        if (exposureData != null) {
                            var imageData = await exposureData.ToImageData(progressReporter, ct);
                            if (imageData != null) {
                                UpdatePreviewImage(imageData);

                                // Save directly as FITS — don't rely on NINA's file pattern
                                string posLabel = pos.Label.Replace(" ", "");
                                string fitsPath = Path.Combine(subFrameDir, $"SKW_{posLabel}_{i:D2}.fits");
                                try {
                                    // Write FITS directly using our own writer
                                    var px = imageData.Data.FlatArray;
                                    int imgW = imageData.Properties.Width;
                                    int imgH = imageData.Properties.Height;
                                    RawFitsWriter.Write(fitsPath, px, imgW, imgH, null);
                                    capturedFiles.Add(fitsPath);
                                    Logger.Info($"SKW: Saved sub-frame {fitsPath} ({imgW}x{imgH})");
                                    if (i < MapPositions.Count) MapPositions[i].State = PositionState.Done;
                                } catch (Exception writeEx) {
                                    Logger.Error($"SKW: Failed to write sub-frame {fitsPath}: {writeEx.Message}");
                                    if (i < MapPositions.Count) MapPositions[i].State = PositionState.Failed;
                                }
                            } else {
                                Logger.Warning($"SKW: ToImageData returned null for {pos.Label}");
                            }
                        } else {
                            Logger.Warning($"SKW: CaptureImage returned null for {pos.Label}");
                        }
                    } catch (Exception ex) {
                        Logger.Warning($"SKW: Capture at {pos.Label} failed: {ex.Message}");
                        if (i < MapPositions.Count) MapPositions[i].State = PositionState.Failed;
                    }
                }

                if (capturedFiles.Count < 2) {
                    StatusText = $"Error: Only {capturedFiles.Count} frames captured. Need at least 2.";
                    return false;
                }

                // Step 5: Integrate
                Progress = 85;

                // Verify captured files exist (NINA may adjust filenames/extensions)
                var existingFiles = capturedFiles.Where(f => File.Exists(f)).ToList();

                // If SaveToDisk returned paths that don't exist, scan the temp dir for FITS/XISF
                if (existingFiles.Count == 0 && Directory.Exists(subFrameDir)) {
                    existingFiles = Directory.GetFiles(subFrameDir, "*.*")
                        .Where(f => f.EndsWith(".fits", StringComparison.OrdinalIgnoreCase)
                                 || f.EndsWith(".fit", StringComparison.OrdinalIgnoreCase)
                                 || f.EndsWith(".xisf", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f).ToList();
                }

                if (existingFiles.Count < 2) {
                    StatusText = $"Error: Only {existingFiles.Count} files found for integration. Check temp dir: {subFrameDir}";
                    Logger.Warning($"SKW: Integration aborted — only {existingFiles.Count} files in {subFrameDir}. CapturedFiles: {string.Join(", ", capturedFiles)}");
                    return false;
                }

                StatusText = $"Integrating {existingFiles.Count} frames...";
                Logger.Info($"SKW: Integrating {existingFiles.Count} files: {string.Join(", ", existingFiles.Select(Path.GetFileName))}");

                // MAX stacking — each pixel keeps its maximum value across all frames
                // Stars are at different positions per frame, so MAX lets every donut shine through
                var firstImg = RawFitsReader.Read(existingFiles[0]);
                int width = firstImg.Width;
                int height = firstImg.Height;
                int pixelCount = width * height;
                double[] maxStack = new double[pixelCount];
                for (int p = 0; p < pixelCount; p++) maxStack[p] = firstImg.Pixels[p];

                int frameCount = 1;
                for (int f = 1; f < existingFiles.Count; f++) {
                    ct.ThrowIfCancellationRequested();
                    try {
                        var img = RawFitsReader.Read(existingFiles[f]);
                        if (img.Width == width && img.Height == height) {
                            for (int p = 0; p < pixelCount; p++) {
                                if (img.Pixels[p] > maxStack[p]) maxStack[p] = img.Pixels[p];
                            }
                            frameCount++;
                        } else {
                            Logger.Warning($"SKW: Skipping {existingFiles[f]} — size mismatch ({img.Width}x{img.Height} vs {width}x{height})");
                        }
                    } catch (Exception readEx) {
                        Logger.Warning($"SKW: Failed to read {existingFiles[f]}: {readEx.Message}");
                    }
                }
                var accumulated = maxStack;
                Logger.Info($"SKW: MAX-stacked {frameCount} frames ({width}x{height})");

                // Optional crop to ring pattern bounding box + 300px margin
                // (defocused stars on long sensor sides need extra safety distance)
                if (CropAfterStack) {
                    int cropMarginPx = 300;
                    int halfCropW = (int)(RadiusPercent / 100.0 * width / 2) + cropMarginPx;
                    int halfCropH = (int)(RadiusPercent / 100.0 * height / 2) + cropMarginPx;
                    int cropW = Math.Min(halfCropW * 2, width);
                    int cropH = Math.Min(halfCropH * 2, height);
                    int cropX = (width - cropW) / 2;
                    int cropY = (height - cropH) / 2;

                    if (cropW < width || cropH < height) {
                        var cropped = new double[cropW * cropH];
                        for (int row = 0; row < cropH; row++) {
                            Array.Copy(accumulated, (cropY + row) * width + cropX, cropped, row * cropW, cropW);
                        }
                        accumulated = cropped;
                        width = cropW;
                        height = cropH;
                        Logger.Info($"SKW: Cropped to {width}x{height} (ring pattern + {cropMarginPx}px margin)");
                    }
                }

                ushort[] pixelData = FitsAverager.ToUShort16(accumulated);

                double pixelSize = profileService?.ActiveProfile?.CameraSettings?.PixelSize ?? 3.76;
                var headers = FitsHeaderWriter.BuildHeaders(
                    fl, pixelSize, Binning, ExposureTime, -999, captureFilter);

                // Save integrated FITS directly in the output folder
                Directory.CreateDirectory(outputDir);
                string outputFile = $"SKW_Collimation_{sessionId}.fits";
                string outputPath = Path.Combine(outputDir, outputFile);

                RawFitsWriter.Write(outputPath, pixelData, width, height, headers);
                Logger.Info($"SKW: Integrated FITS saved to {outputPath} ({width}x{height}, {frameCount} frames)");

                Progress = 95;
                StatusText = $"Saved: {outputPath}";

                // Cleanup sub-frames
                if (AutoCleanSubFrames) {
                    try {
                        foreach (var f in existingFiles) {
                            if (File.Exists(f)) File.Delete(f);
                        }
                        if (Directory.Exists(subFrameDir) && !Directory.EnumerateFileSystemEntries(subFrameDir).Any()) {
                            Directory.Delete(subFrameDir);
                        }
                    } catch { }
                }

                Progress = 100;
                ProgressText = $"{capturedFiles.Count} / {total} positions done";
                success = true;
                successOutputFile = outputFile;
                return true;

            } catch (OperationCanceledException) {
                StatusText = "Cancelled by user";
                return false;
            } catch (Exception ex) {
                StatusText = $"Error: {ex.Message}";
                Logger.Error($"SKW Collimation failed: {ex}");
                return false;
            } finally {
                // Restore focuser — only undo defocus if it actually happened
                if (hasDefocused) {
                    try {
                        StatusText = $"Refocusing to original position...";
                        await focuserMediator.MoveFocuserRelative(-relativeDefocus, CancellationToken.None);
                        Logger.Info($"SKW: Focuser restored (moved {-relativeDefocus} steps back)");
                    } catch (Exception ex) {
                        Logger.Error($"SKW: Refocus failed! Original position was {originalFocuserPos}: {ex.Message}");
                    }
                }
                // Restore original filter if we switched away from it
                if (originalFilter != null) {
                    try {
                        await SwitchFilter(originalFilter, CancellationToken.None);
                        Logger.Info($"SKW: Filter restored to {originalFilter}");
                    } catch (Exception ex) {
                        Logger.Warning($"SKW: Filter restore to '{originalFilter}' failed: {ex.Message}");
                    }
                }
                IsRunning = false;
                runCts?.Dispose();
                runCts = null;

                // Show result after cleanup is done
                if (success) {
                    StatusText = $"Done! Output: {successOutputFile}";
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show(
                            $"Finished stacking!\n\nOutput: {successOutputFile}",
                            "SkyWave — Collimation Complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information));
                }
            }
        }

        // ── Settings Persistence ──

        private const string SETTINGS_PREFIX = "SKW_";

        private void SaveSettings() {
            try {
                var guid = new Guid("b7e3f1a2-9c4d-4e8b-a6f5-1d2c3b4a5e6f");
                var accessor = new NINA.Profile.PluginOptionsAccessor(profileService, guid);
                accessor.SetValueString(SETTINGS_PREFIX + "StarName", StarName);
                accessor.SetValueString(SETTINGS_PREFIX + "TargetRA", TargetRA);
                accessor.SetValueString(SETTINGS_PREFIX + "TargetDec", TargetDec);
                accessor.SetValueInt32(SETTINGS_PREFIX + "DefocusSteps", DefocusSteps);
                accessor.SetValueDouble(SETTINGS_PREFIX + "MicronsPerStep", MicronsPerStep);
                accessor.SetValueInt32(SETTINGS_PREFIX + "DefocusDirection", DefocusDirection);
                accessor.SetValueDouble(SETTINGS_PREFIX + "ExposureTime", ExposureTime);
                accessor.SetValueString(SETTINGS_PREFIX + "FilterName", FilterName);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Gain", Gain);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Offset", Offset);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Binning", Binning);
                accessor.SetValueInt32(SETTINGS_PREFIX + "RingPositions", RingPositions);
                accessor.SetValueInt32(SETTINGS_PREFIX + "RadiusPercent", RadiusPercent);
                accessor.SetValueInt32(SETTINGS_PREFIX + "SettleSeconds", SettleSeconds);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "IncludeCenter", IncludeCenter);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "RunAutofocus", RunAutofocus);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "CropAfterStack", CropAfterStack);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "AutoCleanSubFrames", AutoCleanSubFrames);
                accessor.SetValueString(SETTINGS_PREFIX + "SkyWaveOutputDirectory", skyWaveOutputDirectory);
            } catch (Exception ex) {
                Logger.Warning($"SKW: Failed to save settings: {ex.Message}");
            }
        }

        private void LoadSettings() {
            try {
                var guid = new Guid("b7e3f1a2-9c4d-4e8b-a6f5-1d2c3b4a5e6f");
                var accessor = new NINA.Profile.PluginOptionsAccessor(profileService, guid);
                starName = accessor.GetValueString(SETTINGS_PREFIX + "StarName", starName);
                Logger.Info($"SKW: Loaded settings — star={starName}, dir={skyWaveOutputDirectory}");
                targetRA = accessor.GetValueString(SETTINGS_PREFIX + "TargetRA", targetRA);
                targetDec = accessor.GetValueString(SETTINGS_PREFIX + "TargetDec", targetDec);
                defocusSteps = accessor.GetValueInt32(SETTINGS_PREFIX + "DefocusSteps", defocusSteps);
                micronsPerStep = accessor.GetValueDouble(SETTINGS_PREFIX + "MicronsPerStep", micronsPerStep);
                defocusDirection = accessor.GetValueInt32(SETTINGS_PREFIX + "DefocusDirection", defocusDirection);
                exposureTime = accessor.GetValueDouble(SETTINGS_PREFIX + "ExposureTime", exposureTime);
                filterName = accessor.GetValueString(SETTINGS_PREFIX + "FilterName", filterName);
                gain = accessor.GetValueInt32(SETTINGS_PREFIX + "Gain", gain);
                offset = accessor.GetValueInt32(SETTINGS_PREFIX + "Offset", offset);
                binning = accessor.GetValueInt32(SETTINGS_PREFIX + "Binning", binning);
                ringPositions = accessor.GetValueInt32(SETTINGS_PREFIX + "RingPositions", ringPositions);
                radiusPercent = accessor.GetValueInt32(SETTINGS_PREFIX + "RadiusPercent", radiusPercent);
                settleSeconds = accessor.GetValueInt32(SETTINGS_PREFIX + "SettleSeconds", settleSeconds);
                includeCenter = accessor.GetValueBoolean(SETTINGS_PREFIX + "IncludeCenter", includeCenter);
                runAutofocus = accessor.GetValueBoolean(SETTINGS_PREFIX + "RunAutofocus", runAutofocus);
                cropAfterStack = accessor.GetValueBoolean(SETTINGS_PREFIX + "CropAfterStack", cropAfterStack);
                autoCleanSubFrames = accessor.GetValueBoolean(SETTINGS_PREFIX + "AutoCleanSubFrames", autoCleanSubFrames);
                skyWaveOutputDirectory = accessor.GetValueString(SETTINGS_PREFIX + "SkyWaveOutputDirectory", skyWaveOutputDirectory);
                Logger.Info($"SKW: Settings loaded — star={starName}, filter={filterName}, dir={skyWaveOutputDirectory}");
            } catch (Exception ex) {
                Logger.Warning($"SKW: Failed to load settings: {ex.Message}");
            }
        }

        private void LoadFromProfile() {
            try {
                var p = profileService?.ActiveProfile;
                if (p == null) return;
                try { if (p.AstrometrySettings?.Latitude != 0) { /* lat/lon used in FindBestStar */ } } catch { }
            } catch { }
        }
    }
}
