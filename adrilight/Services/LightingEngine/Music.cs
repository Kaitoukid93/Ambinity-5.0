using adrilight.Services.CaptureEngine;
using adrilight.Ticker;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.TickData;
using adrilight_shared.Settings;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Color = System.Windows.Media.Color;
using ColorPalette = adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues.ColorPalette;

namespace adrilight.Services.LightingEngine
{
    internal class Music : ILightingEngine
    {
        public Music(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            ICaptureEngine[] audioFrame,
            IControlZone zone,
            RainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            if (audioFrame != null && audioFrame.Count() > 0)
            {
                AudioFrame = audioFrame.Where(c => c is AudioFrame).FirstOrDefault();
            }
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
        private ICaptureEngine AudioFrame { get; set; }

        public LightingModeEnum Type { get; } = LightingModeEnum.MusicCapturing;

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
        private SliderParameter _brightnessControl;
        private ListSelectionParameter _midDataControl;
        private ToggleParameter _enableControl;
        private RunStateEnum _runState = RunStateEnum.Stop;
        private AudioDeviceSelectionButtonParameter _audioDeviceSelectionControl;
        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;

        private Color[] _colorBank;
        private double _brightness;
        private int _speed;
        private bool _isEnable;
        private double _paletteSpeed;
        private colorUseEnum _colorUse = colorUseEnum.MovingPalette;
        private int _paletteIntensity = 10;
        private Tick[] _ticks;
        private object _lock = new object();
        private bool _shouldBeMoving;
        private int _dancingMode;
        private int _vuMode;
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
        private void OnSelectedMIDDataChanged(IParameterValue value)
        {
            if (value == null)
                return;
            var mid = value as MIDDataModel;
            if (mid.ExecutionType == VIDType.PositonGeneratedID)
            {
                _midDataControl.SubParams[0].IsEnabled = false;
                //generate VID for this zone here
                GenerateMID(mid.Frequency);
            }
            else
            {
                _midDataControl.SubParams[0].IsEnabled = true;

            }


        }
        private void OnDancingModePropertyChanged(int value)
        {
            _dancingMode = value;
            switch (value)
            {
                case 0:
                    _midDataControl.SubParams[2].IsEnabled = false;
                    break;
                case 1:
                    _midDataControl.SubParams[2].IsEnabled = true;
                    break;
            }
        }
        private void OnVUModePropertyChanged(int value)
        {
            _vuMode = value;
        }
        private void OnSelectedPaletteChanged(IParameterValue value)
        {
            if (value == null)
                return;
            if (value is ColorPalette)
            {
                //show sub params
                _colorControl.SubParams[0].IsEnabled = true;
                _colorControl.SubParams[1].IsEnabled = true;
                _colorControl.SubParams[2].IsEnabled = true;
                _shouldBeMoving = _colorControl.SubParams[0].Value == 1;

            }
            else if (value is ColorCard)
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
            _ticks[0].TickSpeed = _paletteSpeed / 5d;
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

                    var colorTick = RainbowTicker.Ticks.Where(t => t.TickUID == CurrentZone.GroupID && t.TickType == TickEnum.ColorTick).FirstOrDefault();

                    if (colorTick == null)
                    {
                        //create new tick
                        var maxTick = _colorBank != null ? _colorBank.Length : 1024;
                        colorTick = RainbowTicker.MakeNewTick(maxTick, _paletteSpeed / 5d, CurrentZone.GroupID, TickEnum.ColorTick);
                    }
                    _ticks = new Tick[1];
                    _ticks[0] = colorTick;
                }

            }
            else
            {

                var colorTick = new Tick() {
                    MaxTick = _colorBank != null ? _colorBank.Length : 1024,
                    TickSpeed = _paletteSpeed / 5d
                };
                _ticks = new Tick[1];
                _ticks[0] = colorTick;
            }
        }
        private void UpdateTick(bool isInControlGroup)
        {
            if (isInControlGroup)
            {
                lock (RainbowTicker.Lock)
                {

                    _ticks[0].MaxTick = _colorBank != null ? _colorBank.Length : 1024;
                    _ticks[0].TickSpeed = _paletteSpeed / 5d;
                    _ticks[0].CurrentTick = 0;

                }
            }
            else
            {
                _ticks[0].MaxTick = _colorBank != null ? _colorBank.Length : 1024;
                _ticks[0].TickSpeed = _paletteSpeed / 5d;
                _ticks[0].CurrentTick = 0;
            }
        }

        private void OnBrightnessValueChanged(int value)
        {
            _brightness = value / 100d;
        }
        private void OnCapturingSourceChanged(int sourceIndex)
        {

            if (sourceIndex >= AudioFrame.Frames.Length)
                _audioDeviceSelectionControl.CapturingSourceIndex = 0;

        }
        #endregion

        public void Refresh()
        {
            if (AudioFrame == null)
                return;
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;
            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.MusicCapturing &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false;
            ////registering group shoud be done
            //MainViewViewModel.
            //== false;

            // this is stop sign by one or some of the reason above
            if (_runState == RunStateEnum.Run && !shouldBeRunning)
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _runState = RunStateEnum.Pause;
                if (AudioFrame.ServiceRequired > 0)
                    AudioFrame.ServiceRequired--;
                //Stop();
            }
            // this is start sign
            else if (_runState == RunStateEnum.Stop && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
                AudioFrame.ServiceRequired++;
                Start();
            }
            else if (_runState == RunStateEnum.Pause && shouldBeRunning)
            {
                _runState = RunStateEnum.Run;
                AudioFrame.ServiceRequired++;
                Init();
            }


        }
        public void Init()
        {
            #region registering params
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
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
            _midDataControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.MID).FirstOrDefault() as ListSelectionParameter;
            _midDataControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_midDataControl.SelectedValue):
                        OnSelectedMIDDataChanged(_midDataControl.SelectedValue);
                        break;
                }
            };
            _colorControl.SubParams[0].PropertyChanged += (_, __) => OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            _colorControl.SubParams[2].PropertyChanged += (_, __) => OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            _colorControl.SubParams[1].PropertyChanged += (_, __) => OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
            _midDataControl.SubParams[1].PropertyChanged += (_, __) => OnDancingModePropertyChanged(_midDataControl.SubParams[1].Value);
            _midDataControl.SubParams[2].PropertyChanged += (_, __) => OnVUModePropertyChanged(_midDataControl.SubParams[2].Value);
            _colorControl.LoadAvailableValues();
            _midDataControl.LoadAvailableValues();
            _audioDeviceSelectionControl = _currentLightingMode.Parameters.Where(p => p is AudioDeviceSelectionButtonParameter).FirstOrDefault() as AudioDeviceSelectionButtonParameter;
            _audioDeviceSelectionControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_audioDeviceSelectionControl.CapturingSourceIndex):
                        OnCapturingSourceChanged(_audioDeviceSelectionControl.CapturingSourceIndex);
                        break;
                }
            };
            #endregion
            //safety check
            if (_colorControl.SelectedValue == null)
            {
                _colorControl.SelectedValue = _colorControl.AvailableValues.First();
            }
            OnSelectedPaletteChanged(_colorControl.SelectedValue);
            if (_midDataControl.SelectedValue == null)
            {
                _midDataControl.SelectedValue = _midDataControl.AvailableValues.First();
            }
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnSelectedMIDDataChanged(_midDataControl.SelectedValue);
            OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
            OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
            OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
            OnDancingModePropertyChanged(_midDataControl.SubParams[1].Value);
            OnVUModePropertyChanged(_midDataControl.SubParams[2].Value);
        }
        public void Run(CancellationToken token)
        {

            IsRunning = true;
            Log.Information("Music Engine is running.");
            int _idleCounter = 0;
            try
            {

                var updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    if (_runState == RunStateEnum.Run)
                    {
                        if (MainViewViewModel.IsRichCanvasWindowOpen)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var startPID = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                        NextTick();
                        var ledCount = CurrentZone.Spots.Count();
                        var offset = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                        var id = CurrentZone.Spots[0].MID;
                        if (id > 31)
                            id = 0;
                        var currentFrame = BrightnessMapCreator(id, CurrentZone.Spots.Count());
                        if (currentFrame == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        lock (CurrentZone.Lock)
                        {
                            DimLED();
                            foreach (var spot in CurrentZone.Spots)
                            {
                                var position = 0;
                                var translatedIndex = spot.Index - offset;
                                position = Convert.ToInt32(Math.Floor(_ticks[0].CurrentTick + spot.Index));
                                position %= _colorBank.Length;
                                //get brightness using MID and audioFrame
                                var columnIndex = spot.MID;
                                if (columnIndex >= 32)
                                    columnIndex = 0;
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
                                var brightness = (float)_brightness * (currentFrame[translatedIndex] / 255f) * (float)_dimFactor;
                                ApplySmoothing(brightness * colorR, brightness * colorG, brightness * colorB, out var FinalR, out var FinalG, out var FinalB, spot.Red, spot.Green, spot.Blue);
                                spot.SetColor(FinalR, FinalG, FinalB, false);

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
                Log.Information("Stopped the Music Engine");
                IsRunning = false;
                GC.Collect();
            }
        }
        private void NextTick()
        {
            //stop ticking color if zone color source is solid or color use is static
            if (!CurrentZone.IsInControlGroup)
            {

                if (!_shouldBeMoving)
                {
                    _ticks[0].CurrentTick = 0;
                }
                else
                {
                    if (_ticks[0].CurrentTick < _ticks[0].MaxTick - _ticks[0].TickSpeed)
                        _ticks[0].CurrentTick += _ticks[0].TickSpeed;
                    else
                    {
                        _ticks[0].CurrentTick = 0;
                    }
                }

            }
            else
            {
                if (!_shouldBeMoving)
                {
                    _ticks[0].IsRunning = false;
                }
                else
                {
                    _ticks[0].IsRunning = true;
                }
            }

        }
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
        private void GenerateMID(MIDFrequency frequency)
        {
            var freq = 0;
            switch (frequency)
            {
                case MIDFrequency.Low:
                    freq = 2;
                    break;
                case MIDFrequency.Middle:
                    freq = 8;
                    break;
                case MIDFrequency.High:
                    freq = 15;
                    break;
            }
            foreach (var spot in CurrentZone.Spots)
            {
                spot.SetMID(freq);
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
        public byte[] BrightnessMapCreator(int index, int maxHeight)//create brightnessmap based on input fft or volume
        {
            if (_audioDeviceSelectionControl.CapturingSourceIndex >= AudioFrame.Frames.Length)
                if (AudioFrame.Frames.Length == 0)
                {
                    // _audioDeviceSelectionControl.CapturingSourceIndex = -1;
                    return null;
                }
                else if (AudioFrame.Frames.Length == 1)
                {
                    _audioDeviceSelectionControl.CapturingSourceIndex = 0;
                }
            var currentFrame = AudioFrame.Frames[_audioDeviceSelectionControl.CapturingSourceIndex];
            if (currentFrame == null)
                return null;
            var brightnessMap = new byte[maxHeight];

            lock (AudioFrame.Frames[_audioDeviceSelectionControl.CapturingSourceIndex])
            {
                switch (_dancingMode)
                {
                    case 0:
                        for (var i = 0; i < brightnessMap.Length; i++)
                        {
                            brightnessMap[i] = currentFrame.Frame[index];
                        }
                        break;
                    case 1:
                        var height = currentFrame.Frame[index] / 255f;
                        var actualHeight = (int)(height * maxHeight);
                        switch (_vuMode)
                        {
                            case 0://normal VU                  

                                for (var i = 0; i < maxHeight; i++)
                                {
                                    if (i < actualHeight)
                                        brightnessMap[i] = 255;
                                    else
                                        brightnessMap[i] = 0;

                                }
                                break;
                            case 1://normal VU inverse
                                for (var i = 0; i < maxHeight; i++)
                                {
                                    if (i < maxHeight - actualHeight)
                                        brightnessMap[i] = 0;
                                    else
                                        brightnessMap[i] = 255;

                                }


                                break;
                            case 2://floating vu


                                for (var i = 0; i < maxHeight / 2; i++)
                                {
                                    if (Math.Abs(0 - i) <= actualHeight)
                                        brightnessMap[i] = 0;
                                    else
                                        brightnessMap[i] = 255;

                                }


                                for (var i = maxHeight / 2; i < maxHeight; i++)
                                {
                                    if (Math.Abs(maxHeight / 2 - i) <= actualHeight)
                                        brightnessMap[i] = 255;
                                    else
                                        brightnessMap[i] = 0;

                                }



                                break;
                        }
                        break;
                }
            }







            return brightnessMap;

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
        public void Start()
        {
            //start it
            Log.Information("starting the Music Engine");
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
            GetTick(CurrentZone.IsInControlGroup);
            Init();
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "Music"
            };
            _workerThread.Start();
        }
        public void Stop()
        {
            Log.Information("Stop called for Music engine");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}