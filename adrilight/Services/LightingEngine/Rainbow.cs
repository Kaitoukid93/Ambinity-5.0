using adrilight.Services.LightingEngine;
using adrilight.Ticker;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Settings;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class Rainbow : ILightingEngine
    {

        public Rainbow(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            IControlZone zone,
            IDeviceSettings device,
            RainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            CurrentDevice = device as DeviceSettings ?? throw new ArgumentNullException(nameof(device));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
        }

        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private RainbowTicker RainbowTicker { get; }
        private DeviceSettings CurrentDevice { get; }
        private RunStateEnum _runState = RunStateEnum.Stop;
        public LightingModeEnum Type { get; } = LightingModeEnum.Rainbow;
        /// <summary>
        /// property changed event catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

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
                //case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                //    // case nameof(MainViewViewModel.IsRegisteringGroup):
                //    //case nameof(_colorControl):
                //    Refresh();
                //break;
                case nameof(GeneralSettings.SystemRainbowSpeed):
                    OnSystemSpeedChanged(GeneralSettings.SystemRainbowSpeed);
                    break;
            }
        }

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private ListSelectionParameter _colorControl;
        private SliderParameter _brightnessControl;
        private SliderParameter _speedControl;
        private ToggleParameter _systemSyncControl;
        private ListSelectionParameter _vidDataControl;
        private ToggleParameter _enableControl;
        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;
        private Color[] _colorBank;
        private bool _isSystemSync;
        private double _brightness;
        private bool _isEnable;
        private double _speed;
        private int _vidIntensity;
        private int _frameRate = 60;


        #region Properties changed event handler 
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
        private void OnSystemSpeedChanged(int value)
        {
            if (_systemSyncControl != null)
                _systemSyncControl.SubParams[0].Value = value;

        }

        private void OnSystemSyncValueChanged(bool value)
        {
            _isSystemSync = value;
            if (value)
            {
                //hide native speed control
                _speedControl.IsEnabled = false;
                _systemSyncControl.SubParams[0].IsEnabled = true;

            }
            else
            {
                _speedControl.IsEnabled = true;
                _systemSyncControl.SubParams[0].IsEnabled = false;
            }
        }
        private void OnSelectedPaletteChanged(IParameterValue value)
        {
            //set palette
            if (value == null)
                return;
            var palette = value as ColorPalette;
            _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(palette.Colors).ToArray();
        }
        private void OnSelectedVIDDataChanged(IParameterValue value)
        {
            if (value == null)
                return;
            var vid = value as VIDDataModel;
            if (vid.ExecutionType == VIDType.PositonGeneratedID)
            {
                _vidDataControl.SubParams[0].IsEnabled = true;
                if (CurrentZone.IsInControlGroup)
                {
                    //acquire this group this zone belongs to
                    var group = CurrentDevice.ControlZoneGroups.Where(g => g.GroupUID == CurrentZone.GroupID).FirstOrDefault();
                    if (group == null)
                        return;
                    if (Monitor.TryEnter(group.IDGeneratingLock))
                    {
                        try
                        {
                            group.GenerateVID(value, _vidIntensity, 5, CurrentDevice);
                        }
                        finally
                        {
                            Monitor.Exit(group.IDGeneratingLock);
                        }
                    }

                }
                else
                    CurrentZone.GenerateVID(0, value, _vidIntensity, 5);
            }
            else
            {
                _vidDataControl.SubParams[0].IsEnabled = false;
                // _vidDataControl.SubParams[1].IsEnabled = true;
                CurrentZone.ApplyPredefinedVID(value, 0);
            }


        }
        private void OnVIDIntensityValueChanged(int value)
        {
            _vidIntensity = value;
            var currentVID = _vidDataControl.SelectedValue as VIDDataModel;
            if (CurrentZone.IsInControlGroup)
            {
                //acquire this ground this zone belongs to
                var group = CurrentDevice.ControlZoneGroups.Where(g => g.GroupUID == CurrentZone.GroupID).FirstOrDefault();
                if (group == null)
                    return;
                if (Monitor.TryEnter(group.IDGeneratingLock))
                {
                    try
                    {
                        group.GenerateVID(_vidDataControl.SelectedValue as VIDDataModel, _vidIntensity, 5, CurrentDevice);
                    }
                    finally
                    {
                        Monitor.Exit(group.IDGeneratingLock);
                    }
                }

            }
            else
                CurrentZone.GenerateVID(0, _vidDataControl.SelectedValue as VIDDataModel, _vidIntensity, 5);
        }
        private void OnBrightnessValueChanged(int value)
        {
            _brightness = value / 100d;
        }
        private void OnSpeedChanged(int value)
        {
            _speed = value / 5d;
        }
        private void OnSystemSyncSpeedValueChanged(int value)
        {
            GeneralSettings.SystemRainbowSpeed = value;
        }
        #endregion

        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            var isRunning = _cancellationTokenSource != null && _runState != RunStateEnum.Stop;

            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.Rainbow &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false;
            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _runState = RunStateEnum.Stop;
                //Stop();
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
        public void Init()
        {
            #region registering params
            ///enable control//
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_Enable_header, adrilight_shared.Properties.Resources.LightingEngine_Enable_description);
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
            ///color control//
            _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            _colorControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControl_header, adrilight_shared.Properties.Resources.LightingEngine_ColorControl_info);
            _colorControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControlSubParam_CreateNewPalette_Content, "xx");
            _colorControl.SubParams[1].Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControlSubParam_ExportPalette_Content, "xx");
            _colorControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_colorControl.SelectedValue):
                        OnSelectedPaletteChanged(_colorControl.SelectedValue);
                        break;
                }
            };
            ///vid data control
            _vidDataControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.VID).FirstOrDefault() as ListSelectionParameter;
            _vidDataControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_VIDDataControl_header, adrilight_shared.Properties.Resources.LightingEngine_VIDDataControl_info);
            _vidDataControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControl_SubParam_Intensity_Content,"xx");
            _vidDataControl.SubParams[1].Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControl_SubParam_AddNewVID_Content, "xx");
            _vidDataControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_vidDataControl.SelectedValue):
                        OnSelectedVIDDataChanged(_vidDataControl.SelectedValue);
                        break;
                }
            };
            _vidDataControl.SubParams[0].PropertyChanged += (_, __) => OnVIDIntensityValueChanged(_vidDataControl.SubParams[0].Value);
            ///speed control///
            _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault() as SliderParameter;
            _speedControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SpeedControl_header, adrilight_shared.Properties.Resources.Rainbow_Init_SpeedControl_info);
            _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);
            ///brightness control///
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_header, adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_info);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);


            _systemSyncControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.IsSystemSync).FirstOrDefault() as ToggleParameter;
            _systemSyncControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SystemSyncControl_header, adrilight_shared.Properties.Resources.LightingEngine_SystemSyncControl_info);
            _systemSyncControl.PropertyChanged += (_, __) => OnSystemSyncValueChanged(_systemSyncControl.Value == 1 ? true : false);
            _systemSyncControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.LightingEngine_SystemSync_Speed_header, "xx");
            _systemSyncControl.SubParams[0].PropertyChanged += (_, __) => OnSystemSyncSpeedValueChanged(_systemSyncControl.SubParams[0].Value);

            ///activate these value///
           // _colorControl.LoadAvailableValues();
           // _vidDataControl.LoadAvailableValues();
            #endregion
            //safety check
            if (_colorControl.SelectedValue == null)
            {
                _colorControl.SelectedValue = _colorControl.AvailableValues.First();
            }
            OnSelectedPaletteChanged(_colorControl.SelectedValue);
            if (_vidDataControl.SelectedValue == null)
            {
                _vidDataControl.SelectedValue = _vidDataControl.AvailableValues.Last();
            }
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnSelectedVIDDataChanged(_vidDataControl.SelectedValue);
            OnVIDIntensityValueChanged(_vidDataControl.SubParams[0].Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
            OnSpeedChanged(_speedControl.Value);
            OnSystemSyncSpeedValueChanged(_systemSyncControl.SubParams[0].Value);
            OnSystemSyncValueChanged(_systemSyncControl.Value == 1 ? true : false);
        }
        public void Run(CancellationToken token)
        {
            Thread.Sleep(500);
            //Log.Information("Rainbow Engine Is Running");
            IsRunning = true;
            try
            {
                double StartIndex = 0d;
                var startPID = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                int _idleCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    if (_runState == RunStateEnum.Run)
                    {
                        if (MainViewViewModel.IsRichCanvasWindowOpen)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        StartIndex -= _speed;
                        if (StartIndex < 0)
                        {
                            StartIndex = _colorBank.Length - 1;
                        }
                        if (_isSystemSync)
                        {
                            lock (RainbowTicker.Lock)
                            {
                                StartIndex = RainbowTicker.RainbowStartIndex;
                            }

                        }
                        lock (CurrentZone.Lock)
                        {
                            DimLED();
                            foreach (var spot in CurrentZone.Spots)
                            {
                                int position = 0;
                                position = Convert.ToInt32(Math.Floor(StartIndex + spot.VID));
                                position %= _colorBank.Length;
                                var brightness = _brightness * _dimFactor;
                                byte colorR = 0;
                                byte colorG = 0;
                                byte colorB = 0;
                                if (_dimMode == DimMode.Down)
                                {
                                    //keep same last color
                                    colorR = spot.Red;
                                    colorG = spot.Green;
                                    colorB = spot.Blue;
                                }
                                else if (_dimMode == DimMode.Up)
                                {
                                    colorR = _colorBank[position].R;
                                    colorG = _colorBank[position].G;
                                    colorB = _colorBank[position].B;
                                }
                                if (spot.HasVID)
                                {
                                    ApplySmoothing((float)brightness * colorR, (float)brightness * colorG, (float)brightness * colorB, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                                    spot.SetColor(FinalR, FinalG, FinalB, false);
                                }
                                else
                                {
                                    spot.SetColor(0, 0, 0, false);
                                }
                            }



                        }
                        var sleepTime = 1000 / _frameRate;
                        Thread.Sleep(sleepTime);
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10));
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
                Log.Error(ex, this.ToString());
            }
            finally
            {

                // Log.Information("Stopped the Rainbow Engine");
                IsRunning = false;
                GC.Collect();
            }
        }


        public static List<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
        {
            if (totalNumberOfColors < 2)
            {
                throw new ArgumentException("Gradient cannot have less than two colors.", nameof(totalNumberOfColors));
            }
            var colorList = new List<Color>();
            double diffA = to.A - from.A;
            double diffR = to.R - from.R;
            double diffG = to.G - from.G;
            double diffB = to.B - from.B;

            var steps = totalNumberOfColors - 1;

            var stepA = diffA / steps;
            var stepR = diffR / steps;
            var stepG = diffG / steps;
            var stepB = diffB / steps;



            for (var i = 1; i < steps; ++i)
            {
                colorList.Add(Color.FromArgb(
                     (byte)(c(from.A, stepA)),
                     (byte)(c(from.R, stepR)),
                     (byte)(c(from.G, stepG)),
                     (byte)(c(from.B, stepB))));

                int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }
            colorList.Add(to);
            colorList.Insert(0, from);
            return colorList;

        }
        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.1)
                    _dimFactor -= 0.1;
                // if (_dimFactor < 0.1)
                //  _dimMode = DimMode.Up;
            }
            else if (_dimMode == DimMode.Up)
            {
                if (_dimFactor <= 0.99)
                    _dimFactor += 0.01;
                //_dimMode = DimMode.Down;
            }
        }
        private byte[] ResizeFrame(byte[] pixels, int w1, int w2)
        {
            byte[] temp = new byte[w2];
            int x_ratio = (int)((w1 << 16) / w2) + 1;
            int y_ratio = 1;
            int x2, y2;
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < w2; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * w2) + j] = pixels[(y2 * w1) + x2];
                }
            }
            return temp;
        }
        public static IEnumerable<Color> GetColorGradientfromPalette(Color[] colorCollection, int colorNum)
        {
            var colors = new List<Color>();
            int colorPerGap = colorNum / (colorCollection.Count() - 1);

            for (int i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            int remainTick = colorNum - colors.Count();
            colors = colors.Concat(colors.Take(remainTick).ToList()).ToList();
            return colors;
            // new update, create free amount of color????
        }

        public static IEnumerable<Color> GetColorGradientfromPaletteWithFixedColorPerGap(Color[] colorCollection)
        {
            var colors = new List<Color>();
            var colorPerGap = (int)(1024 / colorCollection.Length);

            for (int i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            var lastGradient = GetColorGradient(colorCollection[colorCollection.Length - 1], colorCollection[0], colorPerGap);
            colors = colors.Concat(lastGradient).ToList();
            return colors;

        }
        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
           byte lastColorR, byte lastColorG, byte lastColorB)
        {
            int smoothingFactor = 7;
            semifinalR = (byte)((r + smoothingFactor * lastColorR) / (smoothingFactor + 1));
            semifinalG = (byte)((g + smoothingFactor * lastColorG) / (smoothingFactor + 1));
            semifinalB = (byte)((b + smoothingFactor * lastColorB) / (smoothingFactor + 1));
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

        public void Start()
        {
            //start it
            //Log.Information("starting the Static Color Engine");
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
            Init();
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "Rainbow"
            };
            _workerThread.Start();
        }
        public void Stop()
        {
            //Log.Information("Stop called for Rainbow Engine");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}