using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.TickData;
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
            MainViewViewModel mainViewViewModel,
            IControlZone zone,
            RainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
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
        private ToggleParameter _enableControl;

        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;

        private Color[] _colorBank;
        private double _brightness;
        private int _speed;
        private double _paletteSpeed;
        private Motion _motion;
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
                    var resizedFrame = new Frame();
                    resizedFrame.BrightnessData = ResizeFrame(frame.BrightnessData, frame.BrightnessData.Length, numLED);
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
        #endregion

        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            var isRunning = _cancellationTokenSource != null;

            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;
            GetTick(CurrentZone.IsInControlGroup);
            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.Animation &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false;
            ////registering group shoud be done
            //MainViewViewModel.IsRegisteringGroup == false;

            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                Log.Information("stopping the Animation Engine");
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Log.Information("starting the Animation Color Engine");
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                Init();
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "Animation"
                };
                _workerThread.Start();
            }
            else if (isRunning && shouldBeRunning)
            {
                Init();
            }


        }
        public void Init()
        {
            #region registering params
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
            _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault() as SliderParameter;
            _speedControl.MaxValue = 20;
            _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.MixedColor).FirstOrDefault() as ListSelectionParameter;
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
            _chasingPatternControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_chasingPatternControl.SelectedValue):
                        OnSelectedChasingPatternChanged(_chasingPatternControl.SelectedValue);
                        break;
                }
            };
            _colorControl.SubParams[0].PropertyChanged += (_, __) => OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            _colorControl.SubParams[2].PropertyChanged += (_, __) => OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            _colorControl.SubParams[1].PropertyChanged += (_, __) => OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
            _colorControl.LoadAvailableValues();
            _chasingPatternControl.LoadAvailableValues();
            #endregion
            //safety check
            if (_colorControl.SelectedValue == null)
            {
                _colorControl.SelectedValue = _colorControl.AvailableValues.First();
            }
            OnSelectedPaletteChanged(_colorControl.SelectedValue);
            if (_chasingPatternControl.SelectedValue == null)
            {
                _chasingPatternControl.SelectedValue = _chasingPatternControl.AvailableValues.First();
            }
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnSelectedChasingPatternChanged(_chasingPatternControl.SelectedValue);
            OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
            OnSpeedChanged(_speedControl.Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
        }
        public void Run(CancellationToken token)
        {
            IsRunning = true;
            Log.Information("Animation engine is running");
            try
            {

                var updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    if (MainViewViewModel.IsRichCanvasWindowOpen)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    if (_resizedFrames == null)
                        continue;
                    var startPID = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
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
                                var index = spot.Index - startPID;
                                if (index >= _resizedFrames[(int)_ticks[0].CurrentTick].BrightnessData.Length)
                                {
                                    index = 0;
                                }
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
                                var brightness = _resizedFrames[(int)_ticks[0].CurrentTick].BrightnessData[index] * (float)_brightness / 255;
                                ApplySmoothing((float)(brightness * colorR * _dimFactor), (float)(brightness * colorG * _dimFactor), (float)(brightness * colorB * _dimFactor), out var FinalR, out var FinalG, out var FinalB, spot.Red, spot.Green, spot.Blue);
                                spot.SetColor(FinalR, FinalG, FinalB, false);


                            }
                        }

                    }
                    var sleepTime = 1000 / _frameRate;
                    Thread.Sleep(sleepTime);
                    updateIntervalCounter++;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ToString());
            }
            finally
            {
                Log.Information("Stopped the Animation Engine");
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
            var lastGradient = GetColorGradient(colorCollection[15], colorCollection[0], colorPerGap);
            colors = colors.Concat(lastGradient).ToList();
            return colors;

        }

        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
           byte lastColorR, byte lastColorG, byte lastColorB)
        {
            var smoothingFactor = 7;
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
        private static Motion ReadMotionFromDisk(string path)
        {
            var motion = new Motion(256);
            try
            {
                var json = File.ReadAllText(path);

                motion = JsonConvert.DeserializeObject<Motion>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Read Motion From Disk");
                HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "Animation Import", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return motion;
        }
        public void Stop()
        {
            Log.Information("Stop called for Animation Engine");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}