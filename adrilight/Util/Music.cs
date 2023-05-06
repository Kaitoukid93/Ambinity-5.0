using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using adrilight.DesktopDuplication;
using NLog;
using Polly;
using System.Linq;
using System.Windows.Media.Imaging;
using adrilight.ViewModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using adrilight.Resources;
using adrilight.Util;
using adrilight.Spots;
using System.Windows;
using adrilight.Helpers;
using adrilight.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using NAudio.SoundFont;
using Color = System.Windows.Media.Color;
using System.Net;
using MathNet.Numerics.Distributions;
using ColorPalette = adrilight.Util.ColorPalette;
using SixLabors.ImageSharp;
using HandyControl.Tools.Extension;
using adrilight_effect_analyzer.Model;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Automation.Peers;
using Rectangle = System.Drawing.Rectangle;
using MoreLinq;
using SharpDX.WIC;

namespace adrilight
{
    internal class Music : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public Music(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            ICaptureEngine[] audioFrame,
            IControlZone zone,
            IRainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            AudioFrame = audioFrame.Where(c => c is AudioFrame).FirstOrDefault();
            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
            Refresh();
            _log.Info($"DesktopDuplicatorReader created.");
        }

        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private ICaptureEngine AudioFrame { get; set; }

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
                case nameof(CurrentZone.IsInControlGroup):
                case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                case nameof(MainViewViewModel.IsRegisteringGroup):
                case nameof(_colorControl):
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
        private IModeParameter _colorControl;
        private IModeParameter _brightnessControl;
        private IParameterValue _selectedColorSource;
        private Color[] _colorBank;
        private double _brightness;
        private int _speed;
        private double _paletteSpeed;
        private colorUseEnum _colorUse = colorUseEnum.MovingPalette;
        private int _paletteIntensity = 10;
        private Tick[] _ticks;
        private object _lock = new object();
        private bool _shouldBeMoving;
        private IModeParameter _midDataControl;
        private MIDDataModel _midPath;
        private int _dancingMode;
        private int _vuMode;
        private enum colorUseEnum { StaticPalette, MovingPalette, CyclicPalette };

        #region Properties changed event handler 
        private void OnSelectedMIDDataChanged(IParameterValue value)
        {
            _midPath = value as MIDDataModel;
            if (_midPath.ExecutionType == VIDType.PositonGeneratedID)
            {
                _midDataControl.SubParams[0].IsEnabled = false;
                //generate VID for this zone here
                GenerateMID(_midPath.Frequency);
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
            _selectedColorSource = value;
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
            GetColorBank(_selectedColorSource);
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
                    GetColorBank(_selectedColorSource);
                    break;
                case 0:
                    _colorControl.SubParams[2].IsEnabled = false;
                    _colorControl.SubParams[1].IsEnabled = false;
                    GetColorBank(_selectedColorSource);
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
        #endregion

        public void Refresh()
        {

            var isRunning = _cancellationTokenSource != null;

            var currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;
            GetTick(CurrentZone.IsInControlGroup);
            var shouldBeRunning =
                currentLightingMode.BasedOn == LightingModeEnum.MusicCapturing &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false &&
                //registering group shoud be done
                MainViewViewModel.IsRegisteringGroup == false;

            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Animation Engine");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                #region registering params
                _currentLightingMode = currentLightingMode;
                _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.MixedColor).FirstOrDefault();
                _colorControl.PropertyChanged += (_, __) =>
                {
                    switch (__.PropertyName)
                    {
                        case nameof(_colorControl.SelectedValue):
                            OnSelectedPaletteChanged(_colorControl.SelectedValue);
                            break;
                    }
                };

                _colorControl.SubParams[0].PropertyChanged += (_, __) => OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
                _colorControl.SubParams[2].PropertyChanged += (_, __) => OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
                _colorControl.SubParams[1].PropertyChanged += (_, __) => OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
                _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault();
                _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
                _midDataControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.MID).FirstOrDefault();
                _midDataControl.PropertyChanged += (_, __) => OnSelectedMIDDataChanged(_midDataControl.SelectedValue);
                _midDataControl.SubParams[1].PropertyChanged += (_, __) => OnDancingModePropertyChanged(_midDataControl.SubParams[1].Value);
                _midDataControl.SubParams[2].PropertyChanged += (_, __) => OnVUModePropertyChanged(_midDataControl.SubParams[2].Value);
                #endregion
                _log.Debug("starting the Static Color Engine");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "Animation"
                };
                _workerThread.Start();
            }


        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Animation engine.");


            try
            {
                //get properties value for first time running

                //color param
                _selectedColorSource = _colorControl.SelectedValue;
                OnSelectedPaletteChanged(_selectedColorSource);
                OnColorUsePropertyChanged(_colorControl.SubParams[0].Value);
                OnPaletteIntensityPropertyChanged(_colorControl.SubParams[2].Value);
                OnPaletteSpeedPropertyChanged(_colorControl.SubParams[1].Value);
                OnBrightnessValueChanged(_brightnessControl.Value);
                OnSelectedMIDDataChanged(_midDataControl.SelectedValue);
                OnDancingModePropertyChanged(_midDataControl.SubParams[1].Value);
                OnVUModePropertyChanged(_midDataControl.SubParams[2].Value);
                var startPID = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                while (!token.IsCancellationRequested)
                {
                    bool isPreviewRunning = MainViewViewModel.IsLiveViewOpen && MainViewViewModel.IsAppActivated;
                    NextTick();
                    var ledCount = CurrentZone.Spots.Count();
                    var offset = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                    var currentFrame = BrightnessMapCreator(CurrentZone.Spots[0].MID, CurrentZone.Spots.Count());
                    lock (CurrentZone.Lock)
                    {
                        foreach (var spot in CurrentZone.Spots)
                        {
                            int position = 0;
                            int translatedIndex = spot.Index - offset;
                            position = Convert.ToInt32(Math.Floor(_ticks[0].CurrentTick + spot.Index));
                            position %= _colorBank.Length;
                            //get brightness using MID and audioFrame
                            var columnIndex = spot.MID;
                            if (columnIndex > AudioFrame.Frame.Frame.Length)
                                columnIndex = 0;
                            var brightness = (float)_brightness*(currentFrame[translatedIndex] / 255f);
                            ApplySmoothing(brightness * _colorBank[position].R, brightness * _colorBank[position].G, brightness * _colorBank[position].B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                            spot.SetColor(FinalR, FinalG, FinalB, isPreviewRunning);

                        }

                    }
                    Thread.Sleep(10);
                }
            }
            finally
            {
                _log.Debug("Stopped the Animation Engine");
                //IsRunning = false;
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
        private void GenerateMID(MIDFrequency frequency)
        {
            int freq = 0;
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
        public static IEnumerable<Color> GetColorGradientfromPaletteWithFixedColorPerGap(Color[] colorCollection, int colorPerGap)
        {
            var colors = new List<Color>();


            for (int i = 0; i < colorCollection.Length - 1; i++)
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
            var currentFrame = AudioFrame.Frame;
            byte[] brightnessMap = new byte[maxHeight];

            lock(AudioFrame.Lock)
            {
                switch (_dancingMode)
                {
                    case 0:
                        for (int i = 0; i < brightnessMap.Length; i++)
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

                                for (int i = 0; i < maxHeight; i++)
                                {
                                    if (i < actualHeight)
                                        brightnessMap[i] = 255;
                                    else
                                        brightnessMap[i] = 0;

                                }
                                break;
                            case 1://normal VU inverse
                                for (int i = 0; i < maxHeight; i++)
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
        private void ApplyColorCorrections(float r, float g, float b, out byte finalR, out byte finalG, out byte finalB, bool useLinearLighting, byte saturationTreshold
         , byte lastColorR, byte lastColorG, byte lastColorB)
        {
            if (lastColorR == 0 && lastColorG == 0 && lastColorB == 0)
            {
                //if the color was black the last time, we increase the saturationThreshold to make flickering more unlikely
                saturationTreshold += 2;
            }
            if (r <= saturationTreshold && g <= saturationTreshold && b <= saturationTreshold)
            {
                //black
                finalR = finalG = finalB = 0;
                return;
            }

            //"white" on wall was 66,68,77 without white balance
            //white balance
            //todo: introduce settings for white balance adjustments
            r *= 100 / 100f;
            g *= 100 / 100f;
            b *= 100 / 100f;

            if (!useLinearLighting)
            {
                //apply non linear LED fading ( http://www.mikrocontroller.net/articles/LED-Fading )
                finalR = FadeNonLinear(r);
                finalG = FadeNonLinear(g);
                finalB = FadeNonLinear(b);
            }
            else
            {
                //output
                finalR = (byte)r;
                finalG = (byte)g;
                finalB = (byte)b;
            }
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

        public void Stop()
        {
            _log.Debug("Stop called.");
            CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}