using OpenRGB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using adrilight.ViewModel;
using System.Threading;
using NLog;
using System.Threading.Tasks;
using System.Diagnostics;
using adrilight.Spots;

namespace adrilight
{
    internal class Rainbow : IRainbow
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Rainbow(IOutputSettings outputSettings, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            // OutputSpotSet = outputSpotSet ?? throw new ArgumentException(nameof(outputSpotSet));


            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;
            RefreshColorState();
            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }

        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }
        // private IDeviceSpotSet OutputSpotSet { get; }

        private Color[] colorBank = new Color[256];
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(OutputSettings.OutputParrentIsEnable):
                case nameof(OutputSettings.IsInSpotEditWizard):
                    RefreshColorState();
                    break;
                case nameof(OutputSettings.OutputCurrentActivePalette):
               
                case nameof(OutputSettings.OutputIsSystemSync):


                    ColorPaletteChanged();
                    break;


            }
        }


        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 1 && OutputSettings.IsInSpotEditWizard == false;

            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Rainbow Color");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Rainbow Color");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "RainbowColorCreator" + OutputSettings.OutputUniqueID
                };
                thread.Start();
            }
        }
        private void ColorPaletteChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 1 && OutputSettings.IsInSpotEditWizard == false;


            if (isRunning && shouldBeRunning)
            {
                // rainbow is running and we need to change the color bank
                colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(OutputSettings.OutputCurrentActivePalette.Colors, 18).ToArray();
                inSync = OutputSettings.OutputIsSystemSync;
                //if(isInEditWizard)
                //    colorBank = GetColorGradientfromPalette(DefaultColorCollection.black).ToArray();
            }

        }





        public void Run(CancellationToken token)

        {

            if (IsRunning) throw new Exception(" Rainbow Color is already running!");

            IsRunning = true;

            _log.Debug("Started Rainbow Color.");

            try
            {



                var numLED = OutputSettings.OutputLEDSetup.Spots.Count * OutputSettings.LEDPerSpot * OutputSettings.LEDPerLED;
                var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;
                var effectSpeed = OutputSettings.OutputPaletteSpeed;
                var frequency = OutputSettings.OutputPaletteBlendStep;
                var colorNum = GeneralSettings.SystemRainbowMaxTick;
                Color[] paletteSource = OutputSettings.OutputCurrentActivePalette.Colors;
                colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(paletteSource, 18).ToArray();
                double StartIndex = 0d;
                int OutputStartIndex = 0;


                while (!token.IsCancellationRequested)
                {
                    bool outputIsSelected = false;
                    var currentOutput = MainViewViewModel.CurrentOutput;
                    if (currentOutput != null && currentOutput.OutputUniqueID == OutputSettings.OutputUniqueID)
                        outputIsSelected = true;
                    bool isPreviewRunning = (MainViewViewModel.IsSplitLightingWindowOpen && outputIsSelected)||MainViewViewModel.IsRichCanvasWindowOpen;
                    double speed = OutputSettings.OutputPaletteSpeed / 5d;
                    StartIndex += speed;
                    if (StartIndex > GeneralSettings.SystemRainbowMaxTick)
                    {
                        StartIndex = 0;
                    }
                    if (inSync)
                    {
                        OutputStartIndex = (int)RainbowTicker.RainbowStartIndex;
                    }
                    else
                    {
                        OutputStartIndex = (int)StartIndex;
                    }

                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {

                        int position = 0;
                        foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                        {

                            //caculate the overlap 

                            position = OutputStartIndex + spot.VID;
                            int n = 0;
                            if (position >= colorBank.Length)
                                //position = Math.Abs(colorBank.Length - position);
                                n = position / colorBank.Length;
                            position -= n * colorBank.Length; // run with VID


                            var brightness = OutputSettings.OutputBrightness / 100d;
                            var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                            var outputColor = Brightness.applyBrightness(newColor, brightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                            ApplySmoothing(outputColor.R, outputColor.G, outputColor.B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                            if((OutputSettings as OutputSettings).IsSelected)
                            {
                                spot.SetColor(21, 0, 255, isPreviewRunning);
                            }
                            else
                            {
                                if (!OutputSettings.IsInSpotEditWizard)
                                {
                                    if (spot.IsEnabled)
                                        spot.SetColor(FinalR, FinalG, FinalB, isPreviewRunning);
                                    else
                                    {
                                        spot.SetColor(0, 0, 0, isPreviewRunning);
                                    }

                                }
                            }
                           


                        }


                    }
                    Thread.Sleep(10);


                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");


            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                _log.Debug("Stopped Rainbow Color Creator.");
                IsRunning = false;

            }


        }



        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
      byte lastColorR, byte lastColorG, byte lastColorB)
        {
            ;

            semifinalR = (byte)((r + 7 * lastColorR) / (7 + 1));
            semifinalG = (byte)((g + 7 * lastColorG) / (7 + 1));
            semifinalB = (byte)((b + 7 * lastColorB) / (7 + 1));
        }


        [Obsolete]
        private static Color GetColorByOffset(GradientStopCollection collection, double position)
        {
            double offset = position / 1000.0;

            GradientStop[] stops = collection.OrderBy(x => x.Offset).ToArray();
            if (offset <= 0) return stops[0].Color;
            if (offset >= 1) return stops[stops.Length - 1].Color;
            GradientStop left = stops[0], right = null;
            foreach (GradientStop stop in stops)
            {
                if (stop.Offset >= offset)
                {
                    right = stop;
                    break;
                }
                left = stop;
            }
            Debug.Assert(right != null);
            offset = Math.Round((offset - left.Offset) / (right.Offset - left.Offset), 2);

            byte r = (byte)((right.Color.R - left.Color.R) * offset + left.Color.R);
            byte g = (byte)((right.Color.G - left.Color.G) * offset + left.Color.G);
            byte b = (byte)((right.Color.B - left.Color.B) * offset + left.Color.B);
            return Color.FromRgb(r, g, b);
        }
        [Obsolete]
        public GradientStopCollection GradientPaletteColor(Color[] ColorCollection)
        {

            GradientStopCollection gradientPalette = new GradientStopCollection(16);
            gradientPalette.Add(new GradientStop(ColorCollection[0], 0.00));
            gradientPalette.Add(new GradientStop(ColorCollection[1], 0.066));
            gradientPalette.Add(new GradientStop(ColorCollection[2], 0.133));
            gradientPalette.Add(new GradientStop(ColorCollection[3], 0.199));
            gradientPalette.Add(new GradientStop(ColorCollection[4], 0.265));
            gradientPalette.Add(new GradientStop(ColorCollection[5], 0.331));
            gradientPalette.Add(new GradientStop(ColorCollection[6], 0.397));
            gradientPalette.Add(new GradientStop(ColorCollection[7], 0.464));
            gradientPalette.Add(new GradientStop(ColorCollection[8], 0.529));
            gradientPalette.Add(new GradientStop(ColorCollection[9], 0.595));
            gradientPalette.Add(new GradientStop(ColorCollection[10], 0.661));
            gradientPalette.Add(new GradientStop(ColorCollection[11], 0.727));
            gradientPalette.Add(new GradientStop(ColorCollection[12], 0.793));
            gradientPalette.Add(new GradientStop(ColorCollection[13], 0.859));
            gradientPalette.Add(new GradientStop(ColorCollection[14], 0.925));
            gradientPalette.Add(new GradientStop(ColorCollection[15], 1));

            return gradientPalette;
        }


        public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
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
            return colorList;

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


    }
}
