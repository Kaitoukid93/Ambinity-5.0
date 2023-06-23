using adrilight.Util;
using adrilight.Util.ModeParameters;
using adrilight.ViewModel;
using MoreLinq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Color = System.Windows.Media.Color;
using ColorPalette = adrilight.Util.ColorPalette;

namespace adrilight
{
    internal class Rainbow : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public Rainbow(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            IControlZone zone,
            IRainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
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
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
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
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = true);
            }
            else
            {
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
            var palette = value as ColorPalette;
            _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(palette.Colors, 64).ToArray();
        }
        private void OnSelectedVIDDataChanged(IParameterValue value)
        {

            var vid = value as VIDDataModel;
            if (vid.ExecutionType == VIDType.PositonGeneratedID)
            {
                _vidDataControl.SubParams[0].IsEnabled = true;
                _vidDataControl.SubParams[1].IsEnabled = false;
                //generate VID for this zone here
                GenerateVID(value);
            }
            else
            {
                _vidDataControl.SubParams[0].IsEnabled = false;
                _vidDataControl.SubParams[1].IsEnabled = true;
            }


        }
        private void OnVIDIntensityValueChanged(int value)
        {
            _vidIntensity = value;
            GenerateVID(_vidDataControl.SelectedValue as VIDDataModel);
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
            var isRunning = _cancellationTokenSource != null;

            _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.Rainbow &&
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
                _log.Debug("stopping the Rainbow Engine");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Init();
                _log.Debug("starting the Static Color Engine");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "Rainbow"
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
            _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            _vidDataControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.VID).FirstOrDefault() as ListSelectionParameter;
            _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault() as SliderParameter;
            _colorControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_colorControl.SelectedValue):
                        OnSelectedPaletteChanged(_colorControl.SelectedValue);
                        break;
                }
            };
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
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
            _systemSyncControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.IsSystemSync).FirstOrDefault() as ToggleParameter;
            _systemSyncControl.PropertyChanged += (_, __) => OnSystemSyncValueChanged(_systemSyncControl.Value == 1 ? true : false);
            _systemSyncControl.SubParams[0].PropertyChanged += (_, __) => OnSystemSyncSpeedValueChanged(_systemSyncControl.SubParams[0].Value);
            _colorControl.LoadAvailableValues();
            _vidDataControl.LoadAvailableValues();
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
            OnSystemSyncValueChanged(_systemSyncControl.Value == 1 ? true : false);
        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Rainbow engine.");
            IsRunning = true;
            try
            {
                double StartIndex = 0d;

                while (!token.IsCancellationRequested)
                {
                    var startPID = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
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


                        foreach (var spot in CurrentZone.Spots)
                        {
                            int position = 0;
                            position = Convert.ToInt32(Math.Floor(StartIndex + spot.VID));
                            position %= _colorBank.Length;
                            if (spot.HasVID && _isEnable)
                            {
                                ApplySmoothing((float)_brightness * _colorBank[position].R, (float)_brightness * _colorBank[position].G, (float)_brightness * _colorBank[position].B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
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
            }
            finally
            {

                _log.Debug("Stopped the Rainbow Engine");
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
        private void GenerateVID(IParameterValue value)
        {
            var vid = value as VIDDataModel;
            if (vid.ExecutionType == VIDType.PredefinedID)
                return;
            Rect vidSpace = new Rect();
            if (CurrentZone.IsInControlGroup)
            {
                vidSpace = CurrentZone.GroupRect;
            }
            else
            {
                vidSpace = new Rect(CurrentZone.GetRect.Left, CurrentZone.GetRect.Top, CurrentZone.Width, CurrentZone.Height);
            }
            //get brush size
            //brush rect will move as the dirrection, so intersect size increase
            //this is moving left to right

            double vidSpaceWidth;
            double vidSpaceHeight;
            double zoneOffSetLeft;
            double zoneOffSetTop;
            int VIDCount;
            CurrentZone.ResetVIDStage();
            switch (vid.Dirrection)
            {
                case VIDDirrection.left2right:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = CurrentZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = CurrentZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)zoneOffSetLeft;
                    for (int x = 0; x < vidSpaceWidth; x += 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, 0 - zoneOffSetTop, x, vidSpaceHeight);
                        foreach (var spot in CurrentZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += _vidIntensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.right2left:
                    vidSpaceWidth = (int)vidSpace.Width;
                    vidSpaceHeight = (int)vidSpace.Height;
                    zoneOffSetLeft = CurrentZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = CurrentZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)(vidSpaceWidth - zoneOffSetLeft);
                    for (int x = (int)vidSpaceWidth; x > 0; x -= 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(x - zoneOffSetLeft, 0 - zoneOffSetTop, vidSpaceWidth - x, vidSpaceHeight);
                        foreach (var spot in CurrentZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += _vidIntensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.bot2top:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = CurrentZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = CurrentZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)(vidSpaceHeight - zoneOffSetTop);
                    for (int y = (int)vidSpaceHeight; y > 0; y -= 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, y - zoneOffSetTop, vidSpaceWidth, vidSpaceHeight - y);
                        foreach (var spot in CurrentZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += _vidIntensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.top2bot:
                    vidSpaceWidth = vidSpace.Width;
                    vidSpaceHeight = vidSpace.Height;
                    zoneOffSetLeft = CurrentZone.GetRect.Left - vidSpace.Left;
                    zoneOffSetTop = CurrentZone.GetRect.Top - vidSpace.Top;
                    VIDCount = (int)zoneOffSetTop;
                    for (int y = 0; y < (int)vidSpaceHeight; y += 5)
                    {
                        int settedVIDCount = 0;
                        var brush = new Rect(0 - zoneOffSetLeft, 0 - zoneOffSetTop, vidSpaceWidth, y);
                        foreach (var spot in CurrentZone.Spots)
                        {
                            if (spot.GetVIDIfNeeded(VIDCount, brush, 0))
                                settedVIDCount++;
                        }
                        if (settedVIDCount > 0)
                        {
                            VIDCount += _vidIntensity;
                        }
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    break;
                case VIDDirrection.linear:
                    foreach (var spot in CurrentZone.Spots)
                    {
                        spot.SetVID(spot.Index * _vidIntensity);
                        spot.HasVID = true;
                    }
                    break;


            }

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