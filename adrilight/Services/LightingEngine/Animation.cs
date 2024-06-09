using adrilight.Ticker;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.DataSource;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.TickData;
using adrilight_shared.Settings;
using MoreLinq;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Color = System.Windows.Media.Color;


namespace adrilight.Services.LightingEngine
{
    internal class Animation : ILightingEngine
    {


        public Animation(
            IGeneralSettings generalSettings,
            IControlZone zone,
            IDeviceSettings device,
            RainbowTicker rainbowTicker,
            IList<IDataSource> dataSource
            )
        {
            _paletteDataSource = (ColorPaletteDataSource)dataSource.Where(d => d.Name == "ColorPalettes").FirstOrDefault();
            _vidDataSource = (VIDDataSource)dataSource.Where(d => d.Name == "VID").FirstOrDefault();
            _chasingPatternDataSource = (ChasingPatternDataSource)dataSource.Where(d => d.Name == "ChasingPatterns").FirstOrDefault();
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            CurrentDevice = device as DeviceSettings ?? throw new ArgumentNullException(nameof(device));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;

        }

        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }
        private ColorPaletteDataSource _paletteDataSource;
        private VIDDataSource _vidDataSource;
        private ChasingPatternDataSource _chasingPatternDataSource;
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private RainbowTicker RainbowTicker { get; }
        private DeviceSettings CurrentDevice { get; }
        public LightingModeEnum Type { get; } = LightingModeEnum.Animation;
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
            }
        }

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;

        private ListSelectionParameter _colorControl;
        private ListSelectionParameter _chasingPatternControl;
        private SliderParameter _brightnessControl;
        private SliderParameter _speedControl;
        private SliderParameter _smoothControl;
        private ToggleParameter _enableControl;
        private ListSelectionParameter _vidDataControl;

        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;
        private RunStateEnum _runState = RunStateEnum.Stop;
        private Color[] _colorBank;
        private double _brightness;
        private int _speed;
        private int _smoothFactor;
        private double _paletteSpeed;
        private Motion _motion;
        private int _frameWidth = 256;
        private bool _isEnable;
        private Frame[] _resizedFrames;
        private colorUseEnum _colorUse = colorUseEnum.MovingPalette;
        private int _paletteIntensity = 10;
        private int _frameIndex = 0;
        private Tick[] _ticks;
        private object _lock = new object();
        private bool _shouldBeMoving;
        private int _frameRate = 60;
        private enum colorUseEnum { StaticPalette, MovingPalette, CyclicPalette };
        private int _vidIntensity;
        private int _vidSpaceSize;

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
        private void OnSelectedPaletteChanged(IParameterValue value)
        {

            if (value == null)
                return;
            if (_colorControl.SelectedValue is ColorPalette)
            {
                //show sub params
                _colorControl.SubParams[0].IsEnabled = true;
                _colorControl.SubParams[1].IsEnabled = true;
                _colorControl.SubParams[2].IsEnabled = true;
                _shouldBeMoving = _colorControl.SubParams[0].Value == 1;

            }
            else if (_colorControl.SelectedValue is ColorCard)
            {
                _colorControl.SubParams[0].IsEnabled = false;
                _colorControl.SubParams[1].IsEnabled = false;
                _colorControl.SubParams[2].IsEnabled = false;
                _shouldBeMoving = false;

            }
            GetColorBank(value);
        }
        private void OnPaletteIntensityPropertyChanged(int value)
        {
            _paletteIntensity = value;
            GetColorBank(_colorControl.SelectedValue);
        }
        private void OnPaletteSpeedPropertyChanged(int value)
        {
            _paletteSpeed = value;
        }
        private void OnColorUsePropertyChanged(int value)
        {
            _colorUse = (colorUseEnum)value;
            _shouldBeMoving = _colorControl.SubParams[0].Value == 1;
            switch (value)
            {
                case 1:
                    _colorControl.SubParams[2].IsEnabled = true;
                    _colorControl.SubParams[1].IsEnabled = true;
                    GetColorBank(_colorControl.SelectedValue);
                    break;
                case 0:
                    _colorControl.SubParams[2].IsEnabled = false;
                    _colorControl.SubParams[1].IsEnabled = false;
                    GetColorBank(_colorControl.SelectedValue);
                    break;

            }

        }
        private void GetColorBank(IParameterValue value)
        {
            if (value is ColorCard)
            {
                var colors = value as ColorCard;
                _colorBank = GetColorGradient(colors.StartColor, colors.StopColor, CurrentZone.Spots.Count()).ToArray();
            }
            else if (value is ColorPalette)
            {
                var palette = value as ColorPalette;
                switch (_colorUse)
                {
                    case colorUseEnum.StaticPalette:
                        _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(palette.Colors, 2).ToArray();
                        break;
                    case colorUseEnum.MovingPalette:
                        _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(palette.Colors, _paletteIntensity).ToArray();
                        break;
                }
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
                    var colorTick = RainbowTicker.Ticks.Where(t => t.TickUID == CurrentZone.GroupID && t.TickType == TickEnum.ColorTick).FirstOrDefault();
                    if (frameTick == null)
                    {
                        //create new tick
                        var maxTick = _resizedFrames != null ? _resizedFrames.Length : 1024;
                        frameTick = RainbowTicker.MakeNewTick(maxTick, _speed, CurrentZone.GroupID, TickEnum.FrameTick);
                        frameTick.TickRate = 10;
                        frameTick.IsRunning = true;
                    }
                    if (colorTick == null)
                    {
                        //create new tick
                        var maxTick = _colorBank != null ? _colorBank.Length : 1024;
                        colorTick = RainbowTicker.MakeNewTick(maxTick, _paletteSpeed / 5d, CurrentZone.GroupID, TickEnum.ColorTick);
                    }
                    _ticks = new Tick[2];
                    _ticks[0] = frameTick;
                    _ticks[1] = colorTick;
                }

            }
            else
            {
                var frameTick = new Tick() {
                    MaxTick = _resizedFrames != null ? _resizedFrames.Length : 1024,
                    TickSpeed = _speed,
                    IsRunning = true
                };
                frameTick.TickRate = 10;
                var colorTick = new Tick() {
                    MaxTick = _colorBank != null ? _colorBank.Length : 1024,
                    TickSpeed = _paletteSpeed / 5d
                };
                _ticks = new Tick[2];
                _ticks[0] = frameTick;
                _ticks[1] = colorTick;
            }
        }
        private void UpdateTick(bool isInControlGroup)
        {
            if (isInControlGroup)
            {
                lock (RainbowTicker.Lock)
                {
                    _ticks[0].MaxTick = _resizedFrames != null ? _resizedFrames.Length : 1024;
                    _ticks[0].TickSpeed = _speed;
                    _ticks[0].CurrentTick = 0;
                    _ticks[1].MaxTick = _colorBank != null ? _colorBank.Length : 1024;
                    _ticks[1].TickSpeed = _paletteSpeed / 5d;
                    _ticks[1].CurrentTick = 0;
                }
            }
            else
            {
                _ticks[0].MaxTick = _resizedFrames != null ? _resizedFrames.Length : 1024;
                _ticks[0].TickSpeed = _speed;
                _ticks[0].CurrentTick = 0;
                _ticks[1].MaxTick = _colorBank != null ? _colorBank.Length : 1024;
                _ticks[1].TickSpeed = _paletteSpeed / 5d;
                _ticks[1].CurrentTick = 0;
            }
            // _pattern.Tick = _ticks[0];
        }
        private void OnSelectedChasingPatternChanged(IParameterValue value)
        {
            //resize motion here
            if (value == null)
                return;
            _frameIndex = 0;
            var pattern = value as ChasingPattern;
            if (pattern.LocalPath == null)
                return;
            if (!File.Exists(pattern.LocalPath))
                return;
            _motion = ReadMotionFromDisk(pattern.LocalPath);
            var numLED = CurrentZone.Spots.Count();
            var frameCount = 0;
            lock (_lock)
            {
                if (_motion == null)
                {
                    return;
                }
                _resizedFrames = new Frame[_motion.Frames.Length];
                UpdateTick(CurrentZone.IsInControlGroup);
                foreach (var frame in _motion.Frames)
                {
                    //scale each frame
                    //this is when VID comes to play
                    var resizedFrame = new Frame();
                    resizedFrame.BrightnessData = ResizeFrame(frame.BrightnessData, frame.BrightnessData.Length, _frameWidth);
                    _resizedFrames[frameCount++] = resizedFrame;

                }
            }
            //update tick

        }
        private void OnBrightnessValueChanged(int value)
        {
            _brightness = value / 100d;
        }
        private void OnSpeedChanged(int value)
        {
            _speed = value;
            UpdateTick(CurrentZone.IsInControlGroup);
        }
        private void OnSmoothChanged(int value)
        {
            _smoothFactor = value;
            //UpdateTick(CurrentZone.IsInControlGroup);
        }
        private void OnSelectedVIDDataChanged(IParameterValue value)
        {
            if (value == null)
                return;
            var vid = value as VIDDataModel;
            if (_motion == null)
                return;


            if (vid.ExecutionType == VIDType.PositonGeneratedID)
            {
                _vidDataControl.SubParams[0].IsEnabled = false;
                _vidDataControl.SubParams[1].IsEnabled = true;
               // _vidDataControl.WarningMessageVisible = false;
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
                            _frameWidth = group.GenerateVID(value, 1, 1, CurrentDevice);
                            if (_frameWidth < CurrentZone.Spots.Count)
                            {
                                //display warning message
                                _frameWidth = CurrentZone.Spots.Count;
                               // _vidDataControl.WarningMessage = "Hình dạng sản phẩm có thể không phù hợp cho chiều chạy này ";
                               // _vidDataControl.WarningMessageVisible = true;

                            }

                        }
                        finally
                        {
                            Monitor.Exit(group.IDGeneratingLock);
                        }
                    }

                }
                else
                {
                    _frameWidth = CurrentZone.Spots.Count();
                    var val = CurrentZone.GenerateVID(0, value, 1, 1);
                    if (val < CurrentZone.Spots.Count)
                    {
                        //display warning message
                        _frameWidth = CurrentZone.Spots.Count;
                       // _vidDataControl.WarningMessage = "Hình dạng sản phẩm có thể không phù hợp cho chiều chạy này ";
                        //_vidDataControl.WarningMessageVisible = true;

                    }
                }


                var frameCount = 0;
                foreach (var frame in _motion.Frames)
                {
                    //scale each frame
                    //this is when VID comes to play
                    var resizedFrame = new Frame();
                    resizedFrame.BrightnessData = ResizeFrame(frame.BrightnessData, frame.BrightnessData.Length, _frameWidth);
                    _resizedFrames[frameCount++] = resizedFrame;
                }
            }
            else
            {
                _vidDataControl.SubParams[0].IsEnabled = false;
                _vidDataControl.SubParams[1].IsEnabled = true;
                var frameCount = 0;
                var minVID = vid.DrawingPath.MinBy(b => b.ID).First().ID;
                var maxVID = vid.DrawingPath.MaxBy(b => b.ID).First().ID;
                _frameWidth = maxVID - minVID + 1;
                CurrentZone.ApplyPredefinedVID(value, minVID);
                foreach (var frame in _motion.Frames)
                {
                    //scale each frame
                    //this is when VID comes to play
                    var resizedFrame = new Frame();
                    resizedFrame.BrightnessData = ResizeFrame(frame.BrightnessData, frame.BrightnessData.Length, _frameWidth);
                    _resizedFrames[frameCount++] = resizedFrame;
                }
            }


        }
        private void OnVIDIntensityValueChanged(int value)
        {
            _vidIntensity = value;
            var currentVID = _vidDataControl.SelectedValue as VIDDataModel;
            //if (currentVID.ExecutionType == VIDType.PositonGeneratedID)
            // GenerateVID(_vidDataControl.SelectedValue as VIDDataModel);
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
                _currentLightingMode.BasedOn == LightingModeEnum.Animation &&
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
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_Enable_header, adrilight_shared.Properties.Resources.LightingEngine_Enable_description);
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);


            _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault() as SliderParameter;
            _speedControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SpeedControl_header, adrilight_shared.Properties.Resources.Rainbow_Init_SpeedControl_info);
            _speedControl.MaxValue = 20;


            _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.MixedColor).FirstOrDefault() as ListSelectionParameter;
            _colorControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControl_header, adrilight_shared.Properties.Resources.LightingEngine_ColorControl_info);
            _colorControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.ColorControl_ColorPaletteUse_header, "xx");
            _colorControl.SubParams[1].Localize(adrilight_shared.Properties.Resources.ColorControl_ColorPaletteSpped_header, "xx");
            _colorControl.SubParams[2].Localize(adrilight_shared.Properties.Resources.ColorControl_ColorPaletteIntensity_header, "xx");

            _smoothControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            if (_smoothControl == null)
            {
                var controlModeHelper = new ControlModeHelpers();
                _smoothControl = controlModeHelper.GenericSmoothParameter as SliderParameter;
                _currentLightingMode.Parameters.Add(_smoothControl);
            }
            _smoothControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_header, adrilight_shared.Properties.Resources.LightingEngine_SmoothControl_info);
            _smoothControl.MaxValue = 7;
            _smoothControl.MinValue = 0;
            _colorControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_colorControl.SelectedValue):
                        OnSelectedPaletteChanged(_colorControl.SelectedValue);
                        break;
                }
            };
            _chasingPatternControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.ChasingPattern).FirstOrDefault() as ListSelectionParameter;
            _chasingPatternControl.Localize(adrilight_shared.Properties.Resources.ChasingPatternControl_header, adrilight_shared.Properties.Resources.ChasingPatternControl_info);
            _chasingPatternControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.ChasingPatternControl_Import_header, "xx");
            _chasingPatternControl.SubParams[1].Localize(adrilight_shared.Properties.Resources.ChasingPatternControl_Export_header, "xx");
            _chasingPatternControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_chasingPatternControl.SelectedValue):
                        OnSelectedChasingPatternChanged(_chasingPatternControl.SelectedValue);
                        break;
                }
            };
            _vidDataControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.VID).FirstOrDefault() as ListSelectionParameter;
            if (_vidDataControl == null)
            {
                var controlModeHelper = new ControlModeHelpers();
                _vidDataControl = controlModeHelper.GenericVIDSelectParameter as ListSelectionParameter;
                _currentLightingMode.Parameters.Add(_vidDataControl);
            }
            _vidDataControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_VIDDataControl_header, adrilight_shared.Properties.Resources.LightingEngine_VIDDataControl_info);
            _vidDataControl.SubParams[0].Localize(adrilight_shared.Properties.Resources.LightingEngine_ColorControl_SubParam_Intensity_Content, "xx");
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
            _colorControl.SubParams[0].PropertyChanged += (_, __) => OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            _colorControl.SubParams[2].PropertyChanged += (_, __) => OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            _colorControl.SubParams[1].PropertyChanged += (_, __) => OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);


            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.Localize(adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_header, adrilight_shared.Properties.Resources.LightingEngine_BrightnessControl_info);
            _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);
            _smoothControl.PropertyChanged += (_, __) => OnSmoothChanged(_smoothControl.Value);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
            #endregion
            //safety check
            if (_colorControl.SelectedValue == null)
            {
                _colorControl.SelectedValue = (ColorPalette)_paletteDataSource.Items.First();
            }
            OnSelectedPaletteChanged(_colorControl.SelectedValue);
            if (_chasingPatternControl.SelectedValue == null)
            {
                //init default value
                _chasingPatternControl.SelectedValue = (ChasingPattern)_chasingPatternDataSource.Items.First();
            }
            if (_vidDataControl.SelectedValue == null)
            {
                //init default value
                _vidDataControl.SelectedValue = (VIDDataModel)_vidDataSource.Items.First();
            }
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnSelectedChasingPatternChanged(_chasingPatternControl.SelectedValue);
            OnSelectedVIDDataChanged(_vidDataControl.SelectedValue);
            OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
            OnVIDIntensityValueChanged(_vidDataControl.SubParams[0].Value);
            OnSpeedChanged(_speedControl.Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
        }
        public void Run(CancellationToken token)
        {
            IsRunning = true;
            //Log.Information("Animation engine is running");
            int _idleCounter = 0;
            try
            {

                var updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    if (_runState == RunStateEnum.Run)
                    {
                      
                        if (_resizedFrames == null)
                            continue;

                        NextTick();
                        lock (CurrentZone.Lock)
                        {
                            DimLED();
                            foreach (var spot in CurrentZone.Spots)
                            {
                                var position = 0;
                                position = Convert.ToInt32(Math.Floor(_ticks[1].CurrentTick + spot.Index));
                                position %= _colorBank.Length;
                                lock (_lock)
                                {
                                    //var index = spot.VID - startVID;
                                    //if (index >= _resizedFrames[(int)_ticks[0].CurrentTick].BrightnessData.Length)
                                    //{
                                    //    index = 0;
                                    //}
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
                                        int index = spot.VID;
                                        if (spot.VID >= _resizedFrames[(int)_ticks[0].CurrentTick].BrightnessData.Length)
                                            index = 0;
                                        var brightness = _resizedFrames[(int)_ticks[0].CurrentTick].BrightnessData[index] * (float)_brightness / 255;
                                        ApplySmoothing((float)(brightness * colorR * _dimFactor), (float)(brightness * colorG * _dimFactor), (float)(brightness * colorB * _dimFactor), out var FinalR, out var FinalG, out var FinalB, spot.Red, spot.Green, spot.Blue);
                                        spot.SetColor(FinalR, FinalG, FinalB, false);
                                    }
                                    else
                                    {
                                        spot.SetColor(0, 0, 0, false);
                                    }

                                }
                            }

                        }
                        var sleepTime = 1000 / _frameRate;
                        Thread.Sleep(sleepTime);
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
                //Log.Information("Stopped the Animation Engine");
                IsRunning = false;
                GC.Collect();
            }
        }
        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.01)
                    _dimFactor -= 0.01;
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
                if (_ticks[0].CurrentTick < _ticks[0].MaxTick - _ticks[0].TickSpeed)
                    _ticks[0].CurrentTick += _ticks[0].TickSpeed / _ticks[0].TickRate;
                else
                {
                    _ticks[0].CurrentTick = 0;
                }
                if (!_shouldBeMoving)
                {
                    _ticks[1].CurrentTick = 0;
                }
                else
                {
                    if (_ticks[1].CurrentTick < _ticks[1].MaxTick - _ticks[1].TickSpeed)
                        _ticks[1].CurrentTick += _ticks[1].TickSpeed;
                    else
                    {
                        _ticks[1].CurrentTick = 0;
                    }
                }

            }
            else
            {
                if (!_shouldBeMoving)
                {
                    _ticks[1].IsRunning = false;
                }
                else
                {
                    _ticks[1].IsRunning = true;
                }
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
                     (byte)c(from.A, stepA),
                     (byte)c(from.R, stepR),
                     (byte)c(from.G, stepG),
                     (byte)c(from.B, stepB)));

                int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }
            colorList.Add(to);
            colorList.Insert(0, from);
            return colorList;

        }
        private byte[] ResizeFrame(byte[] pixels, int w1, int w2)
        {
            var temp = new byte[w2];
            var x_ratio = (w1 << 16) / w2 + 1;
            var y_ratio = 1;
            int x2, y2;
            for (var i = 0; i < 1; i++)
            {
                for (var j = 0; j < w2; j++)
                {
                    x2 = j * x_ratio >> 16;
                    y2 = i * y_ratio >> 16;
                    temp[i * w2 + j] = pixels[y2 * w1 + x2];
                }
            }
            return temp;
        }
        public static IEnumerable<Color> GetColorGradientfromPalette(Color[] colorCollection, int colorNum)
        {
            var colors = new List<Color>();
            var colorPerGap = colorNum / (colorCollection.Count() - 1);

            for (var i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            var remainTick = colorNum - colors.Count();
            colors = colors.Concat(colors.Take(remainTick).ToList()).ToList();
            return colors;


            // new update, create free amount of color????
        }
        public static IEnumerable<Color> GetColorGradientfromPaletteWithFixedColorPerGap(Color[] colorCollection, int colorPerGap)
        {
            var colors = new List<Color>();


            for (var i = 0; i < colorCollection.Length - 1; i++)
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
            if (_smoothFactor > 7 || _smoothFactor < 0)
                _smoothFactor = 2;
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
        private static Motion ReadMotionFromDisk(string path)
        {
            var motion = new Motion(256);
            try
            {
                var json = File.ReadAllText(path);

                motion = JsonConvert.DeserializeObject<Motion>(json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Read Motion From Disk");
                HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "Animation Import", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return motion;
        }
        public void Start()
        {
            //start it
            // Log.Information("starting the Animation Color Engine");
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
            GetTick(CurrentZone.IsInControlGroup);
            Init();
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "Animation"
            };
            _workerThread.Start();
        }
        public void Stop()
        {
            // Log.Information("Stop called for Animation Engine");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}