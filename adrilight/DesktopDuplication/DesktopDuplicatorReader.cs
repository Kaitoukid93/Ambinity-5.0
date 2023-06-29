using adrilight.DesktopDuplication;
using adrilight.Spots;
using adrilight.Util;
using adrilight.Util.ModeParameters;
using adrilight.ViewModel;
using MoreLinq;
using NLog;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace adrilight
{
    internal class DesktopDuplicatorReader : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopDuplicatorReader(IGeneralSettings generalSettings,
            ICaptureEngine[] desktopFrame,
             MainViewViewModel mainViewViewModel,
             IControlZone zone
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            DesktopFrame = desktopFrame.Where(d => d is DesktopFrame).ToArray() ?? throw new ArgumentNullException(nameof(desktopFrame));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);


            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;


            // Refresh();

            _log.Info($"DesktopDuplicatorReader created.");
        }




        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }

        private ICaptureEngine[] DesktopFrame { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }

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
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = true);
            }
            else
            {
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
            if (DesktopFrame[(int)_currentScreenIndex].Frame == null || DesktopFrame[(int)_currentScreenIndex].Frame.FrameWidth == 0 || DesktopFrame[(int)_currentScreenIndex].Frame.FrameHeight == 0)
                _log.Error("DesktopFrame is null");
            return;

            var left = region.ScaleX * DesktopFrame[(int)_currentScreenIndex].Frame.FrameWidth;
            var top = region.ScaleY * DesktopFrame[(int)_currentScreenIndex].Frame.FrameHeight;
            var width = region.ScaleWidth * DesktopFrame[(int)_currentScreenIndex].Frame.FrameWidth;
            var height = region.ScaleHeight * DesktopFrame[(int)_currentScreenIndex].Frame.FrameHeight;
            Rect zoneParrent = new Rect();
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
            if (sourceIndex >= DesktopFrame.Length)
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
        private Rectangle _currentCapturingRegion;

        private SliderParameter _brightnessControl;
        private SliderParameter _smoothingControl;
        private ToggleParameter _useLinearLightingControl;
        private ToggleParameter _enableControl;
        private CapturingRegionSelectionButtonParameter _regionControl;
        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            var isRunning = _cancellationTokenSource != null;

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
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the capturing");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                //get current lighting mode confirm that based on desktop duplicator reader engine
                Init();
                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();
            }
            else if (isRunning && shouldBeRunning)
            {
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
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessPropertyChanged(_brightnessControl.Value);
            _smoothingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            _smoothingControl.PropertyChanged += (_, __) => OnSmoothingPropertyChanged(_smoothingControl.Value);
            _useLinearLightingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.LinearLighting).FirstOrDefault() as ToggleParameter;
            _useLinearLightingControl.PropertyChanged += (_, __) => OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);
            _regionControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.CapturingRegion).FirstOrDefault() as CapturingRegionSelectionButtonParameter;
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
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnBrightnessPropertyChanged(_brightnessControl.Value);
            OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);
            OnSmoothingPropertyChanged(_smoothingControl.Value);
            OnCapturingSourceChanged(_regionControl.CapturingSourceIndex);

        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
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
                int updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {

                    //this indicator that user is opening this device and we need raise event when color update on each spot

                    if (MainViewViewModel.IsRichCanvasWindowOpen)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image));
                    TraceFrameDetails(newImage);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    if (image != null && (newImage.Width != image.Width || newImage.Height != image.Height))
                    {
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                    }
                    else
                    {
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                    }

                    image = newImage;
                    try
                    {
                        image.LockBits(_currentCapturingRegion, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb, bitmapData);
                    }

                    catch (System.ArgumentException)
                    {
                        //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                        // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                        image = null;
                        continue;
                    }


                    lock (CurrentZone.Lock)
                    {



                        foreach (var spot in CurrentZone.Spots)
                        {
                            var left = ((spot as DeviceSpot).Left / CurrentZone.Width) * _currentCapturingRegion.Width;
                            var top = ((spot as DeviceSpot).Top / CurrentZone.Height) * _currentCapturingRegion.Height;
                            var width = Math.Max(1, ((spot as DeviceSpot).Width / CurrentZone.Width) * _currentCapturingRegion.Width);
                            var height = Math.Max(1, ((spot as DeviceSpot).Height / CurrentZone.Height) * _currentCapturingRegion.Height);
                            const int numberOfSteps = 15;
                            int stepx = Math.Max(1, (int)(width) / numberOfSteps);
                            int stepy = Math.Max(1, (int)(height) / numberOfSteps);
                            Rectangle actualRectangle = new Rectangle(
                                (int)(left),
                                (int)(top),
                                (int)(width),
                                (int)(height));
                            GetAverageColorOfRectangularRegion(actualRectangle, stepy, stepx, bitmapData,
                                out int sumR, out int sumG, out int sumB, out int count);

                            var countInverse = 1f / count;
                            var r = sumR * countInverse;
                            var g = sumG * countInverse;
                            var b = sumB * countInverse;
                            var finalR = (byte)r;
                            var finalG = (byte)g;
                            var finalB = (byte)b;
                            if (!_useLinearLighting)
                            {
                                finalR = FadeNonLinear(r);
                                finalG = FadeNonLinear(g);
                                finalB = FadeNonLinear(b);
                            }
                            var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);
                            ApplySmoothing(
                                spotColor.R,
                                spotColor.G,
                                spotColor.B,
                                out byte RealfinalR,
                                out byte RealfinalG,
                                out byte RealfinalB,
                             spot.Red,
                             spot.Green,
                             spot.Blue);
                            if (_isEnable)
                                spot.SetColor((byte)(RealfinalR * _brightness), (byte)(RealfinalG * _brightness), (byte)(RealfinalB * _brightness), false);
                            else
                            {
                                spot.SetColor(0, 0, 0, false);
                            }

                        }

                    }

                    image.UnlockBits(bitmapData);
                    int minFrameTimeInMs = 1000 / _frameRate;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                    updateIntervalCounter++;
                }
            }
            finally
            {
                image?.Dispose();

                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();
            }
        }
        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

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
                    _log.Debug("The frame size changed from {0}x{1} to {2}x{3}"
                        , _lastObservedWidth, _lastObservedHeight
                        , image.Width, image.Height);

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
        private Bitmap GetNextFrame(Bitmap ReusableBitmap)
        {

            try
            {
                ByteFrame CurrentFrame = null;
                Bitmap DesktopImage;
                if (_currentScreenIndex >= DesktopFrame.Length)
                {
                    HandyControl.Controls.MessageBox.Show("màn hình không khả dụng", "Sáng theo màn hình", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentScreenIndex = 0;
                }
                var currentDesktop = DesktopFrame[(int)_currentScreenIndex];
                lock (currentDesktop.Lock)
                {
                    CurrentFrame = currentDesktop.Frame;

                    if (CurrentFrame == null || CurrentFrame.FrameWidth == 0 || CurrentFrame.FrameHeight == 0)
                    {
                        return null;
                    }
                    else
                    {
                        if (ReusableBitmap != null && ReusableBitmap.Width == CurrentFrame.FrameWidth && ReusableBitmap.Height == CurrentFrame.FrameHeight)
                        {
                            DesktopImage = ReusableBitmap;

                        }
                        else if (ReusableBitmap != null && (ReusableBitmap.Width != CurrentFrame.FrameWidth || ReusableBitmap.Height != CurrentFrame.FrameHeight))
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        }
                        else //this is when app start
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        }
                        var DesktopImageBitmapData = DesktopImage.LockBits(new Rectangle(0, 0, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight), ImageLockMode.WriteOnly, DesktopImage.PixelFormat);
                        IntPtr pixelAddress = DesktopImageBitmapData.Scan0;
                        Marshal.Copy(CurrentFrame.Frame, 0, pixelAddress, CurrentFrame.Frame.Length);
                        DesktopImage.UnlockBits(DesktopImageBitmapData);
                        return DesktopImage;
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message != "_outputDuplication is null" && ex.Message != "Access Lost, resolution might be changed" && ex.Message != "Invalid call, might be retrying" && ex.Message != "Failed to release frame.")
                {
                    _log.Error(ex.Message, "GetNextFrame() failed.");

                    // throw;
                }
                else if (ex.Message == "Access Lost, resolution might be changed")
                {
                    _log.Error(ex, "Access Lost, retrying");

                }
                else if (ex.Message == "Invalid call, might be retrying")
                {
                    _log.Error(ex, "Invalid Call Lost, retrying");
                }
                else if (ex.Message == "Failed to release frame.")
                {
                    _log.Error(ex, "Failed to release frame.");
                }
                else
                {
                    throw new DesktopDuplicationException("Unknown Device Error", ex);
                }

                GC.Collect();
                return null;
            }
        }

        public void Stop()
        {
            _log.Debug("Stop called.");
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
                byte* pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (int i = 0; i < stepCount; i++)
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