using adrilight.Services.CaptureEngine;
using adrilight.Ticker;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.TickData;
using adrilight_shared.Settings;
using MoreLinq;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Rectangle = System.Drawing.Rectangle;

namespace adrilight.Services.LightingEngine
{
    internal class Gifxelation : ILightingEngine
    {

        public static Bitmap WorkingBitmap { get; set; }
        public static Bitmap LoadedStillBitmap { get; set; }
        public ByteFrame[] LoadedGifImage { get; set; }
        public Gifxelation(IGeneralSettings generalSettings,
             IControlZone zone,
             RainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);


            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
        }




        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }

        private ICaptureEngine[] DesktopFrame { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private RainbowTicker RainbowTicker { get; }
        public LightingModeEnum Type { get; } = LightingModeEnum.Gifxelation;


        /// <summary>
        /// property changed event catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region PropertyChanged events
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
        private void OnCapturingRegionChanged(CapturingRegion region)
        {
            var left = region.ScaleX * LoadedGifImage[0].FrameWidth;
            var top = region.ScaleY * LoadedGifImage[0].FrameHeight;
            var width = region.ScaleWidth * LoadedGifImage[0].FrameWidth;
            var height = region.ScaleHeight * LoadedGifImage[0].FrameHeight;
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
        private void OnSelectedGifChanged(IParameterValue value)
        {
            //set palette

            if (value == null)
                return;
            var gif = value as Gif;
            lock (gif.Lock)
            {
                if (gif.Frames == null)
                {
                    if (gif.LocalPath == null)
                        return;
                    if (!File.Exists(gif.LocalPath))
                        return;
                    gif.LoadGifFromDisk(gif.LocalPath);

                }
            }

            lock (_lock)
            {
                LoadedGifImage = gif.Frames;
                UpdateTick(CurrentZone.IsInControlGroup);
            }
        }
        private void GetTick(bool isInControlGroup)
        {
            if (isInControlGroup)
            {
                //check if tick exist in rainbowticker
                lock (RainbowTicker.Lock)
                {
                    var frameTick = RainbowTicker.Ticks.Where(t => t.TickUID == CurrentZone.GroupID && t.TickType == TickEnum.FrameTick).FirstOrDefault();
                    if (frameTick == null)
                    {
                        //create new tick
                        var maxTick = LoadedGifImage != null ? LoadedGifImage.Length : 1024;
                        frameTick = RainbowTicker.MakeNewTick(maxTick, _speed, CurrentZone.GroupID, TickEnum.FrameTick);
                        frameTick.TickRate = 20;
                        frameTick.IsRunning = true;
                    }
                    _tick = frameTick;
                }

            }
            else
            {
                var frameTick = new Tick() {
                    MaxTick = LoadedGifImage != null ? LoadedGifImage.Length : 1024,
                    TickSpeed = _speed,
                    IsRunning = true
                };
                frameTick.TickRate = 20;
                _tick = frameTick;
            }
        }
        private void UpdateTick(bool isInControlGroup)
        {
            if (isInControlGroup)
            {
                lock (RainbowTicker.Lock)
                {
                    _tick.MaxTick = LoadedGifImage != null ? LoadedGifImage.Length : 1024;
                    _tick.TickSpeed = _speed;
                    _tick.CurrentTick = 0;
                }
            }
            else
            {
                _tick.MaxTick = LoadedGifImage != null ? LoadedGifImage.Length : 1024;
                _tick.TickSpeed = _speed;
                _tick.CurrentTick = 0;
            }
            // _pattern.Tick = _ticks[0];
        }
        private void OnSpeedChanged(int value)
        {
            _speed = value;
            UpdateTick(CurrentZone.IsInControlGroup);
        }
        #endregion
        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private double _brightness;
        private bool _isEnable;
        private int _smoothFactor;
        private int _frameRate = 60;
        private Tick _tick;
        private int _speed;
        private object _lock = new object();
        private Rectangle _currentCapturingRegion;
        private RunStateEnum _runState = RunStateEnum.Stop;
        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;

        private SliderParameter _brightnessControl;
        private SliderParameter _speedControl;
        private SliderParameter _smoothingControl;
        private CapturingRegionSelectionButtonParameter _regionControl;
        private ListSelectionParameter _gifControl;
        private ToggleParameter _enableControl;
        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            var isRunning = _cancellationTokenSource != null && _runState != RunStateEnum.Stop;

            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.Gifxelation &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true;
                //stop this engine when any surface or editor open because this could cause capturing fail
            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _runState = RunStateEnum.Pause;
                // Stop();

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
                //check if thread alive
                Start();
            }
            else if (isRunning && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
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
        public async void Init()
        {
            //get dependency properties from current lighting mode(based on screencapturing)
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_Enable_header, adrilight_shared.Properties.Resources.LightingEngine_Enable_description);
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);

            _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault() as SliderParameter;
            _speedControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SpeedControl_header, adrilight_shared.Properties.Resources.Rainbow_Init_SpeedControl_info);
            _speedControl.MaxValue = 10;
            _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);

            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_header, adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_info);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessPropertyChanged(_brightnessControl.Value);


            _gifControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
            _gifControl.Localize(adrilight_shared.Properties.Resources.GifControl_header, adrilight_shared.Properties.Resources.GifControl_info);
            _gifControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.ImportGif_header, "xx");
            _gifControl.SubParams[1].Localize(adrilight_shared.Properties.Resources.ExportGif_header, "xx");

            _smoothingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            _smoothingControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_header, adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_info);
            _smoothingControl.PropertyChanged += (_, __) => OnSmoothingPropertyChanged(_smoothingControl.Value);


            _regionControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.CapturingRegion).FirstOrDefault() as CapturingRegionSelectionButtonParameter;
            _regionControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_RegionControl_header, adrilight_shared.Properties.Resources.LightingEngine_RegionControl_info);
            _regionControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_regionControl.CapturingRegion):
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        break;
                    case nameof(_regionControl.CapturingSourceIndex):
                        //_gifControl.SelectedValue = _gifControl.AvailableValues[_regionControl.CapturingSourceIndex];
                        break;
                }
            };
            _gifControl.PropertyChanged += async (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_gifControl.SelectedValue):
                        await Task.Run(() => OnSelectedGifChanged(_gifControl.SelectedValue));
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        break;
                }
            };
           // _gifControl.LoadAvailableValues();
            if (_gifControl.SelectedValue == null)
            {
                //_gifControl.SelectedValue = _gifControl.AvailableValues.First();
            }
            EnableChanged(_enableControl.Value == 1 ? true : false);
            await Task.Run(() => OnSelectedGifChanged(_gifControl.SelectedValue));
            OnBrightnessPropertyChanged(_brightnessControl.Value);
            OnSmoothingPropertyChanged(_smoothingControl.Value);
            OnSpeedChanged(_speedControl.Value);

        }
        public void Run(CancellationToken token)
        {
            Bitmap image = null;
            var bitmapData = new BitmapData();
            IsRunning = true;
            //Log.Information("Gifxelation is running");
            int _idleCounter = 0;
            try
            {
                var updateIntervalCounter = 0;

                while (!token.IsCancellationRequested)
                {
                    if (_runState == RunStateEnum.Run)
                    {
                        
                        //this indicator that user is opening this device and we need raise event when color update on each spot
                        if (LoadedGifImage == null)
                            continue;
                        var frameTime = Stopwatch.StartNew();
                        var isPreviewRunning = false;
                        var newImage = _retryPolicy.Execute(() => GetNextFrame(image, (int)_tick.CurrentTick, isPreviewRunning));
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

                        catch (ArgumentException)
                        {
                            //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                            // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                            image = null;
                            continue;
                        }
                        NextTick();

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
                                    finalR = (byte)r;
                                    finalG = (byte)g;
                                    finalB = (byte)b;
                                }
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

                // Log.Information("Stopped Gifxelation Engine");
                IsRunning = false;
                GC.Collect();
            }
        }
        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.02)
                    _dimFactor -= 0.02;
            }
            else if (_dimMode == DimMode.Up)
            {
                if (_dimFactor <= 0.99)
                    _dimFactor += 0.01;
                //_dimMode = DimMode.Down;
            }
        }
        private void NextTick()
        {
            //stop ticking color if zone color source is solid or color use is static
            if (!CurrentZone.IsInControlGroup)
            {
                if (_tick.CurrentTick < _tick.MaxTick - _tick.TickSpeed)
                    _tick.CurrentTick += _tick.TickSpeed / _tick.TickRate;
                else
                {
                    _tick.CurrentTick = 0;
                }
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
                    //Log.Information("The gif size changed from {0}x{1} to {2}x{3}"
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
        private Bitmap GetNextFrame(Bitmap ReusableBitmap, int frameIndex, bool isPreviewRunning)
        {
            try
            {
                // get current working bitmap at frameIndex
                lock (_lock)
                {
                    if (frameIndex >= LoadedGifImage.Length)
                        return null;
                    Bitmap CurrentGifImage;
                    if (LoadedGifImage == null)
                    {
                        return null;
                    }
                    var currentFrame = LoadedGifImage[frameIndex];
                    if (isPreviewRunning)
                    {
                        // MainViewViewModel.DesktopsPreviewUpdate(currentFrame);
                    }
                    if (currentFrame == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (ReusableBitmap != null && ReusableBitmap.Width == currentFrame.FrameWidth && ReusableBitmap.Height == currentFrame.FrameHeight)
                        {
                            CurrentGifImage = ReusableBitmap;

                        }
                        else if (ReusableBitmap != null && (ReusableBitmap.Width != currentFrame.FrameWidth || ReusableBitmap.Height != currentFrame.FrameHeight))
                        {
                            CurrentGifImage = new Bitmap(currentFrame.FrameWidth, currentFrame.FrameHeight, PixelFormat.Format32bppRgb);
                        }
                        else //this is when app start
                        {
                            CurrentGifImage = new Bitmap(currentFrame.FrameWidth, currentFrame.FrameHeight, PixelFormat.Format32bppRgb);
                        }
                        var GifImageBitmapData = CurrentGifImage.LockBits(new Rectangle(0, 0, currentFrame.FrameWidth, currentFrame.FrameHeight), ImageLockMode.WriteOnly, CurrentGifImage.PixelFormat);
                        IntPtr pixelAddress = GifImageBitmapData.Scan0;
                        Marshal.Copy(currentFrame.Frame, 0, pixelAddress, currentFrame.Frame.Length);
                        CurrentGifImage.UnlockBits(GifImageBitmapData);
                        return CurrentGifImage;
                    }
                }

            }
            catch (Exception ex)
            {

                GC.Collect();
                return null;
            }
        }
        public void Start()
        {
            // Log.Information("Starting the Gifxelation Engine");
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
            GetTick(CurrentZone.IsInControlGroup);
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
            // Log.Information("Stop called for Gifxelation Engine");
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