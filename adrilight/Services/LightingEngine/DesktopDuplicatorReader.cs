using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Settings;
using MoreLinq;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Windows.Foundation;
using Color = System.Windows.Media.Color;
using Rect = System.Windows.Rect;
using Rectangle = System.Drawing.Rectangle;

namespace adrilight.Services.LightingEngine
{
    internal class DesktopDuplicatorReader : ILightingEngine
    {
        public DesktopDuplicatorReader(IGeneralSettings generalSettings,
            ICaptureEngine[] desktopFrame,
             MainViewViewModel mainViewViewModel,
             IControlZone zone
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            if (desktopFrame != null && desktopFrame.Count() > 0 && desktopFrame.Any(d => d is DesktopFrame || d is DesktopFrameDXGI))
            {
                var frame = desktopFrame.Where(d => d is DesktopFrame || d is DesktopFrameDXGI).First();
                DesktopFrame = desktopFrame.Where(d => d is DesktopFrame || d is DesktopFrameDXGI).First() ?? throw new ArgumentNullException(nameof(desktopFrame));
            }

            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);

            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;

        }




        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }

        private ICaptureEngine DesktopFrame { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private RunStateEnum _runState = RunStateEnum.Stop;
        private MainViewViewModel MainViewViewModel { get; }
        public LightingModeEnum Type { get; } = LightingModeEnum.ScreenCapturing;


        /// <summary>
        /// property changed event catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region PropertyChanged events
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //which property that require this engine to refresh
                case nameof(CurrentZone.CurrentActiveControlMode):
                    var isRunning = _cancellationTokenSource != null;
                    if (isRunning || (CurrentZone.CurrentActiveControlMode as LightingMode).BasedOn == Type)
                        Refresh();
                    break;
                //case nameof(CurrentZone.MaskedControlMode):
                case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                    // case nameof(MainViewViewModel.IsRegisteringGroup):
                    Refresh();
                    break;

            }
        }
        private void EnableChanged(bool value)
        {
            _isEnable = value;
            if (value)
            {
                _dimMode = DimMode.Up;
                _dimFactor = 0.00;
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = true);
            }
            else
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = false);
            }
        }
        private void OnBrightnessPropertyChanged(int value)
        {
            _brightness = value / 100d;

        }
        private void OnSmoothingPropertyChanged(int value)
        {
            _smoothFactor = value;
        }
        private void OnUseLinearLightingPropertyChanged(bool value)
        {
            _useLinearLighting = value;

        }
        private void OnCapturingRegionChanged(CapturingRegion region)
        {
            if (DesktopFrame.Frames[(int)_currentScreenIndex] == null || DesktopFrame.Frames[(int)_currentScreenIndex].FrameWidth == 0 || DesktopFrame.Frames[(int)_currentScreenIndex].FrameHeight == 0)
            {
                Log.Error("DesktopFrame is null");
                return;
            }


            var left = region.ScaleX * _currentColoredRegion.Width + _currentColoredRegion.Left;
            var top = region.ScaleY * _currentColoredRegion.Height + _currentColoredRegion.Top;
            var width = region.ScaleWidth * _currentColoredRegion.Width;
            var height = region.ScaleHeight * _currentColoredRegion.Height;
            var zoneParrent = new Rect();
            if (CurrentZone.IsInControlGroup)
            {
                zoneParrent = CurrentZone.GroupRect;

            }
            else
            {
                zoneParrent = new Rect(CurrentZone.GetRect.Left, CurrentZone.GetRect.Top, CurrentZone.Width, CurrentZone.Height);
            }
            var zoneLeft = CurrentZone.GetRect.Left - zoneParrent.Left;
            var zoneTop = CurrentZone.GetRect.Top - zoneParrent.Top;
            var zoneRegion = new CapturingRegion(zoneLeft / zoneParrent.Width, zoneTop / zoneParrent.Height, CurrentZone.Width / zoneParrent.Width, CurrentZone.Height / zoneParrent.Height);
            var regionLeft = Math.Max((int)(zoneRegion.ScaleX * width + left), 0);
            var regionTop = Math.Max((int)(zoneRegion.ScaleY * height + top), 0);
            var regionWidth = Math.Max((int)(zoneRegion.ScaleWidth * width), 1);
            var regionHeight = Math.Max((int)(zoneRegion.ScaleHeight * height), 1);
            _currentCapturingRegion = new Rectangle(regionLeft, regionTop, regionWidth, regionHeight);
        }
        private void OnCapturingSourceChanged(int sourceIndex)
        {
            if (sourceIndex >= DesktopFrame.Frames.Length)
                _regionControl.CapturingSourceIndex = 0;
            _currentScreenIndex = _regionControl.CapturingSourceIndex;
        }
        #endregion
        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private int? _currentScreenIndex;
        private bool _useLinearLighting;
        private bool _isEnable;
        private double _brightness;
        private int _smoothFactor;
        private int _frameRate = 60;
        private enum VideoRatio { Normal, LeterBox }
        private VideoRatio _currentVideoRatio;
        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;
        private Rectangle _currentCapturingRegion;
        private Rect _currentColoredRegion;//for blackbar detection
        private Rect _lastBorder;
        private byte[] cropPixelData;
        private int _consistantBorderCnt;
        private int _inconsistantBorderCnt;

        private SliderParameter _brightnessControl;
        private SliderParameter _smoothingControl;
        private ToggleParameter _useLinearLightingControl;
        private ToggleParameter _enableControl;
        private CapturingRegionSelectionButtonParameter _regionControl;
        public void Refresh()
        {
            if (DesktopFrame == null)
                return;
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;
            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.ScreenCapturing &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false &&
                //stop this engine when this zone is outside or atleast there is 1 pixel outside of the screen region
                CurrentZone.IsInsideScreen == true;
            ////registering group shoud be done
            //MainViewViewModel.IsRegisteringGroup == false;

            // this is stop sign by one or some of the reason above
            if (_runState == RunStateEnum.Run && !shouldBeRunning)
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _runState = RunStateEnum.Pause;
                if (DesktopFrame.ServiceRequired > 0)
                    DesktopFrame.ServiceRequired--;
                //Stop();
            }
            // this is start sign
            else if (_runState == RunStateEnum.Stop && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
                DesktopFrame.ServiceRequired++;
                Start();
            }
            else if (_runState == RunStateEnum.Pause && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
                DesktopFrame.ServiceRequired++;
                Init();
            }
        }
        private readonly Policy _retryPolicy;
        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {
                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming
                // DeviceSpotSet.IndicateMissingValues();
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }
        public void Init()
        {
            //get dependency properties from current lighting mode(based on screencapturing)

            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_Enable_header, adrilight_shared.Properties.Resources.LightingEngine_Enable_description);
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
            /// brightness.///
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_header, adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_info);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessPropertyChanged(_brightnessControl.Value);

            /// smooth ///
            _smoothingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            _smoothingControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_header, adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_info);
            _smoothingControl.PropertyChanged += (_, __) => OnSmoothingPropertyChanged(_smoothingControl.Value);

            /// linear lighting///
            _useLinearLightingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.LinearLighting).FirstOrDefault() as ToggleParameter;
            _useLinearLightingControl.Localize(adrilight_shared.Properties.Resources.DesktopDuplicatorReader_LinearLightingControl_header, adrilight_shared.Properties.Resources.DesktopDuplicatorReader_Init_Xx);
            _useLinearLightingControl.PropertyChanged += (_, __) => OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);

            /// region data control///
            _regionControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.CapturingRegion).FirstOrDefault() as CapturingRegionSelectionButtonParameter;
            _regionControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_RegionControl_header, adrilight_shared.Properties.Resources.LightingEngine_RegionControl_info);
            _regionControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_regionControl.CapturingSourceIndex):
                        OnCapturingSourceChanged(_regionControl.CapturingSourceIndex);
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        break;
                    case nameof(_regionControl.CapturingRegion):
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        break;
                }
            };

            ///activate these value///
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnBrightnessPropertyChanged(_brightnessControl.Value);
            OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);
            OnSmoothingPropertyChanged(_smoothingControl.Value);
            OnCapturingSourceChanged(_regionControl.CapturingSourceIndex);

        }
        public Color GetPixel(int x, int y, byte[] frame, int width, int height)
        {
            var clr = Color.FromRgb(0, 0, 0);

            // Get color components count
            var cCount = 32 / 8;

            // Get start index of the specified pixel
            var i = (y * (width + 1) + x) * cCount;

            if (i > frame.Length - cCount)
                throw new IndexOutOfRangeException();


            var b = frame[i];
            var g = frame[i + 1];
            var r = frame[i + 2];
            var a = frame[i + 3]; // a
            //clr = Color.FromArgb(a, r, g, b);
            clr = Color.FromArgb(255, r, g, b);


            return clr;
        }
        public void Run(CancellationToken token)
        {

            // Log.Information("Desktop Duplicator Reader is Running");
            Bitmap image = null;
            var bitmapData = new BitmapData();
            IsRunning = true;
            try
            {
                //get current zone size and position respect to screen size scaled 8.0
                //var screenLeft = Screen.AllScreens[(int)_currentScreenIndex].Bounds.Left;
                //var screenTop = Screen.AllScreens[(int)_currentScreenIndex].Bounds.Top;
                //var x = (int)((CurrentZone.Left + CurrentZone.OffsetX - screenLeft) / 8.0);
                //var y = (int)((CurrentZone.Top + CurrentZone.OffsetY - screenTop) / 8.0);
                //var zoneWidth = (int)(CurrentZone.Width / 8.0) >= 1 ? (int)(CurrentZone.Width / 8.0) : 1;
                //var zoneHeight = (int)(CurrentZone.Height / 8.0) >= 1 ? (int)(CurrentZone.Height / 8.0) : 1;
                var updateIntervalCounter = 0;
                int _idleCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    //this indicator that user is opening this device and we need raise event when color update on each spot
                    if (_runState == RunStateEnum.Run)
                    {

                        if (MainViewViewModel.IsRichCanvasWindowOpen)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var frameTime = Stopwatch.StartNew();
                        var newImage = _retryPolicy.Execute(() => GetNextFrame(image));
                        //TraceFrameDetails(newImage);
                        if (newImage == null)
                        {
                            //there was a timeout before there was the next frame, simply retry!
                            continue;
                        }

                        image = newImage;
                        try
                        {
                            //calculate current capturing region blackbar

                            image.LockBits(_currentCapturingRegion, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb, bitmapData);
                        }

                        catch (ArgumentException)
                        {
                            //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                            // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                            image = null;
                            continue;
                        }



                        lock (CurrentZone.Lock)
                        {
                            DimLED();
                            foreach (var spot in CurrentZone.Spots)
                            {
                                var left = (spot as DeviceSpot).Left / CurrentZone.Width * _currentCapturingRegion.Width;
                                var top = (spot as DeviceSpot).Top / CurrentZone.Height * _currentCapturingRegion.Height;
                                var width = Math.Max(1, (spot as DeviceSpot).Width / CurrentZone.Width * _currentCapturingRegion.Width);
                                var height = Math.Max(1, (spot as DeviceSpot).Height / CurrentZone.Height * _currentCapturingRegion.Height);
                                const int numberOfSteps = 15;
                                var stepx = Math.Max(1, (int)width / numberOfSteps);
                                var stepy = Math.Max(1, (int)height / numberOfSteps);
                                var actualRectangle = new Rectangle(
                                    (int)left,
                                    (int)top,
                                    (int)width,
                                    (int)height);
                                GetAverageColorOfRectangularRegion(actualRectangle, stepy, stepx, bitmapData,
                                    out var sumR, out var sumG, out var sumB, out var count);

                                var countInverse = 1f / count;
                                var r = sumR * countInverse;
                                var g = sumG * countInverse;
                                var b = sumB * countInverse;
                                byte finalR = 0;
                                byte finalG = 0;
                                byte finalB = 0;
                                if (_dimMode == DimMode.Down)
                                {
                                    //keep same last color
                                    finalR = spot.Red;
                                    finalG = spot.Green;
                                    finalB = spot.Blue;
                                }
                                else if (_dimMode == DimMode.Up)
                                {
                                    if (!_useLinearLighting)
                                    {
                                        finalR = FadeNonLinear(r);
                                        finalG = FadeNonLinear(g);
                                        finalB = FadeNonLinear(b);
                                    }
                                    else
                                    {
                                        finalR = (byte)r;
                                        finalG = (byte)g;
                                        finalB = (byte)b;
                                    }
                                }

                                var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);
                                ApplySmoothing(
                                    (float)(finalR * _brightness * _dimFactor),
                                     (float)(finalG * _brightness * _dimFactor),
                                     (float)(finalB * _brightness * _dimFactor),
                                    out var RealfinalR,
                                    out var RealfinalG,
                                    out var RealfinalB,
                                 spot.Red,
                                 spot.Green,
                                 spot.Blue);
                                spot.SetColor(RealfinalR, RealfinalG, RealfinalB, false);
                            }

                        }

                        image.UnlockBits(bitmapData);
                        var minFrameTimeInMs = 1000 / _frameRate;
                        var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                        if (elapsedMs < minFrameTimeInMs)
                        {
                            Thread.Sleep(minFrameTimeInMs - elapsedMs);
                        }
                        updateIntervalCounter++;
                    }
                    else
                    {
                        Thread.Sleep(10);
                        _idleCounter++;
                        if (_idleCounter >= 1000)
                        {
                            _runState = RunStateEnum.Stop;
                            break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ToString());
            }
            finally
            {
                image?.Dispose();

                //Log.Information("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();
            }
        }
        private int? _lastObservedHeight;
        private int? _lastObservedWidth;
        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.1)
                    _dimFactor -= 0.1;
            }
            else if (_dimMode == DimMode.Up)
            {
                if (_dimFactor <= 0.99)
                    _dimFactor += 0.01;
                //_dimMode = DimMode.Down;
            }
        }
        private void TraceFrameDetails(Bitmap image)
        {
            //there are many frames per second and we need to extract useful information and only log those!
            if (image == null)
            {
                //if the frame is null, this can mean two things. the timeout from the desktop duplication api was reached
                //before the monitor content changed or there was some other error.
            }
            else
            {
                if (_lastObservedHeight != null && _lastObservedWidth != null
                    && (_lastObservedHeight != image.Height || _lastObservedWidth != image.Width))
                {
                    //Log.Information("The desktop size changed from {0}x{1} to {2}x{3}"
                    //    , _lastObservedWidth, _lastObservedHeight
                    //    , image.Width, image.Height);

                }
                _lastObservedWidth = image.Width;
                _lastObservedHeight = image.Height;
            }
        }
        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
           byte lastColorR, byte lastColorG, byte lastColorB)
        {

            semifinalR = (byte)((r + _smoothFactor * lastColorR) / (_smoothFactor + 1));
            semifinalG = (byte)((g + _smoothFactor * lastColorG) / (_smoothFactor + 1));
            semifinalB = (byte)((b + _smoothFactor * lastColorB) / (_smoothFactor + 1));
        }

        private readonly byte[] _nonLinearFadingCache = Enumerable.Range(0, 2560)
            .Select(n => FadeNonLinearUncached(n / 10f))
            .ToArray();

        private byte FadeNonLinear(float color)
        {
            var cacheIndex = (int)(color * 10);
            return _nonLinearFadingCache[Math.Min(2560 - 1, Math.Max(0, cacheIndex))];
        }
        private static byte FadeNonLinearUncached(float color)
        {
            const float factor = 80f;
            return (byte)(256f * ((float)Math.Pow(factor, color / 256f) - 1f) / (factor - 1));
        }
        private int _blackborderThreshold = 2;
        private bool isBlack(Color color)
        {
            return color.R < _blackborderThreshold && color.G < _blackborderThreshold && color.B < _blackborderThreshold;
        }
        /// <summary>
        /// Algorithm from Hyperion github
        /// </summary>
        private Rect GetCurrentColoredRegion(byte[] frame, int frameWidth, int frameHeight)
        {
            if (!GeneralSettings.IsBlackBarDetectionEnabled)
            {
                return new Rect(0, 0, frameWidth, frameHeight);
            }
            Rect newRegion = new Rect(0, 0, frameWidth, frameHeight);
            // find X position at height33 and height66 we check from the left side, Ycenter will check from right side
            // then we try to find a pixel at this X position from top and bottom and right side from top
            var width33percent = frameWidth / 3;
            var height33percent = frameHeight / 3;
            var height66percent = height33percent * 2;
            var yCenter = frameHeight / 2;
            var width25percent = frameWidth / 4;
            var width75percent = frameWidth * 3 / 4;

            var firstNonBlackXPixelIndex = -1;
            var firstNonBlackYPixelIndex = -1;

            frameWidth--; // remove 1 pixel to get end pixel index
            frameHeight--;

            // find first X pixel of the image
            int x;
            for (x = 0; x < width33percent; ++x)
            {
                if (!isBlack(GetPixel(frameWidth - x, yCenter, frame, frameWidth, frameHeight))
                    || !isBlack(GetPixel(x, height33percent, frame, frameWidth, frameHeight))
                    || !isBlack(GetPixel(x, height66percent, frame, frameWidth, frameHeight)))
                {
                    firstNonBlackXPixelIndex = x;
                    break;
                }
            }

            // find first Y pixel of the image
            for (var y = 0; y < height33percent; ++y)
            {
                // left side top + left side bottom + right side top  +  right side bottom
                if (!isBlack(GetPixel(width25percent, y, frame, frameWidth, frameHeight))
                  || !isBlack(GetPixel(width25percent, frameHeight - y, frame, frameWidth, frameHeight))
                  || !isBlack(GetPixel(width75percent, y, frame, frameWidth, frameHeight))
                  || !isBlack(GetPixel(width75percent, frameHeight - y, frame, frameWidth, frameHeight)))
                {
                    firstNonBlackYPixelIndex = y;
                    break;
                }
            }
            if (firstNonBlackXPixelIndex < 10) // sensitivity
                firstNonBlackXPixelIndex = 0;
            if (firstNonBlackYPixelIndex < 10) // sensitivity
                firstNonBlackYPixelIndex = 0;
            if (firstNonBlackYPixelIndex != 0 || firstNonBlackXPixelIndex != 0)
            {
                if (firstNonBlackXPixelIndex == -1)
                    firstNonBlackXPixelIndex = 0;
                if (firstNonBlackYPixelIndex == -1)
                    firstNonBlackYPixelIndex = 0;
                newRegion = new Rect(firstNonBlackXPixelIndex, firstNonBlackYPixelIndex, frameWidth - 2 * firstNonBlackXPixelIndex, frameHeight - 2 * firstNonBlackYPixelIndex);

            }
            else
            {
                frameWidth++;
                frameHeight++;
                newRegion = new Rect(0, 0, frameWidth, frameHeight);

            }
            return newRegion;
            //set area

        }
        private Bitmap GetNextFrame(Bitmap ReusableBitmap)
        {

            try
            {
                ByteFrame CurrentFrame = null;
                Bitmap DesktopImage;
                bool borderChanged = false;
                if (_currentScreenIndex >= DesktopFrame.Frames.Length)
                {
                    HandyControl.Controls.MessageBox.Show("màn hình không khả dụng", "Sáng theo màn hình", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentScreenIndex = 0;
                }
                //var currentDesktop = ;
                if (DesktopFrame.Frames[(int)_currentScreenIndex] == null)
                    return null;
                lock (DesktopFrame.Lock)
                {
                    CurrentFrame = DesktopFrame.Frames[(int)_currentScreenIndex];

                    if (CurrentFrame == null || CurrentFrame.FrameWidth == 0 || CurrentFrame.FrameHeight == 0)
                    {
                        return null;
                    }
                    else
                    {
                        //calculate colored area
                        if (_currentColoredRegion.Width == 0 || _currentColoredRegion.Height == 0)
                        {
                            _currentColoredRegion = new Rect(0, 0, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight);
                            OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        }
                        var newColoredRegion = GetCurrentColoredRegion(CurrentFrame.Frame, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight);

                        //if new colored region is the same as current colored region. Ignore the image processing

                        var currentBorder = new Rect(0, 0, (CurrentFrame.FrameWidth - newColoredRegion.Width) / 2, (CurrentFrame.FrameHeight - newColoredRegion.Height) / 2);
                        if (!Rect.Equals(currentBorder, _lastBorder))
                        {
                            ++_inconsistantBorderCnt;
                            if (_inconsistantBorderCnt <= 10)
                            {
                                borderChanged = false;
                            }
                            _lastBorder = currentBorder;
                            _consistantBorderCnt = 0;

                        }
                        else
                        {
                            ++_consistantBorderCnt;
                            _inconsistantBorderCnt = 0;
                        }
                        if (Rect.Equals(_currentColoredRegion, newColoredRegion))
                        {
                            // No change required
                            _inconsistantBorderCnt = 0;
                            borderChanged = false;
                        }
                        //else
                        //{
                        //    borderChanged = true;
                        //}
                        if (_consistantBorderCnt == GeneralSettings.BlackBarDetectionDelayTime * 60)
                        {
                            borderChanged = true;
                        }
                        if (borderChanged)
                        {
                            _currentColoredRegion = newColoredRegion;
                            OnCapturingRegionChanged(_regionControl.CapturingRegion);
                            //borderChanged = false;
                        }
                        if (ReusableBitmap != null && ReusableBitmap.Width == CurrentFrame.FrameWidth && ReusableBitmap.Height == CurrentFrame.FrameHeight)
                        {
                            DesktopImage = ReusableBitmap;

                        }
                        else if (ReusableBitmap != null && (ReusableBitmap.Width != CurrentFrame.FrameWidth || ReusableBitmap.Height != CurrentFrame.FrameHeight))
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, PixelFormat.Format32bppRgb);
                            _currentColoredRegion = newColoredRegion;
                            OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        }
                        else //this is when app start
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, PixelFormat.Format32bppRgb);
                        }
                        var DesktopImageBitmapData = DesktopImage.LockBits(new Rectangle(0, 0, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight), ImageLockMode.WriteOnly, DesktopImage.PixelFormat);
                        IntPtr pixelAddress = DesktopImageBitmapData.Scan0;
                        //if (CurrentFrame.FrameWidth == _currentColoredRegion.Width && CurrentFrame.FrameHeight == _currentColoredRegion.Height)
                        //{
                        //    cropPixelData = CurrentFrame.Frame;
                        //}
                        //else
                        //{
                        //    cropPixelData = CropImageArray(CurrentFrame.Frame, CurrentFrame.FrameWidth, 32, _currentColoredRegion);
                        //}
                        Marshal.Copy(CurrentFrame.Frame, 0, pixelAddress, CurrentFrame.Frame.Length);
                        //}

                        //else
                        //{
                        //    Marshal.Copy(CurrentFrame.Frame, 0, pixelAddress, CurrentFrame.Frame.Length);
                        //}

                        DesktopImage.UnlockBits(DesktopImageBitmapData);
                        return DesktopImage;
                    }
                }

            }
            catch (Exception ex)
            {
                GC.Collect();
                return null;
            }
        }
        public static byte[] CropImageArray(byte[] pixels, int sourceWidth, int bitsPerPixel, Rectangle rect)
        {
            var blockSize = bitsPerPixel / 8;
            var outputPixels = new byte[rect.Width * rect.Height * blockSize];

            //Create the array of bytes.
            for (var line = 0; line <= rect.Height - 1; line++)
            {
                var sourceIndex = ((rect.Y + line) * sourceWidth + rect.X) * blockSize;
                var destinationIndex = line * rect.Width * blockSize;

                Array.Copy(pixels, sourceIndex, outputPixels, destinationIndex, rect.Width * blockSize);
            }

            return outputPixels;
        }
        public void Start()
        {
            //start it
            // Log.Information("starting the capturing");
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
            Init();
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "DesktopDuplicatorReader"
            };
            _workerThread.Start();
        }
        public void Stop()
        {
            // Log.Information("Stop called for Desktop Duplicator Reader");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


        private unsafe void GetAverageColorOfRectangularRegion(Rectangle spotRectangle, int stepy, int stepx, BitmapData bitmapData, out int sumR, out int sumG,
            out int sumB, out int count)
        {
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            var stepCount = spotRectangle.Width / stepx;
            var stepxTimes4 = stepx * 4;
            for (var y = spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
            {
                var pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (var i = 0; i < stepCount; i++)
                {
                    sumB += pointer[0];
                    sumG += pointer[1];
                    sumR += pointer[2];

                    pointer += stepxTimes4;
                }
                count += stepCount;
            }
        }

    }
}