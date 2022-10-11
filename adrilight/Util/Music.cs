using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using System.Threading;
using NLog;
using System.Windows;
using System.Diagnostics;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;

namespace adrilight
{
    internal class Music : IMusic
    {



        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Music(IOutputSettings outputSettings, IAudioFrame audioFrames, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            AudioFrames = audioFrames ?? throw new ArgumentException(nameof(audioFrames));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;

            RefreshAudioState();
            _log.Info($"MusicColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }
        private IAudioFrame AudioFrames { get; set; }
        private MainViewViewModel MainViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }


        private Color[] colorBank = new Color[256];

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(MainViewModel.IsVisualizerWindowOpen):
                case nameof(OutputSettings.OutputParrentIsEnable):
                    RefreshAudioState();
                    break;
                case nameof(OutputSettings.OutputCurrentActivePalette):
                case nameof(OutputSettings.IsInSpotEditWizard):
                    ColorPaletteChanged();
                    break;



            }

        }
        private void RefreshAudioState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 2 && OutputSettings.IsInSpotEditWizard == false;


            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Music Color");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                // Free();
            }

            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Music Color");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "MusicColorCreator"
                };
                thread.Start();
            }
        }

        private void ColorPaletteChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 2 && OutputSettings.IsInSpotEditWizard == false;

            if (isRunning && shouldBeRunning)
            {
                // rainbow is running and we need to change the color bank
                colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(OutputSettings.OutputCurrentActivePalette.Colors, 18).ToArray();
                inSync = OutputSettings.OutputIsSystemSync;
            }

        }




        public void Run(CancellationToken token)

        {


            if (IsRunning) throw new Exception(" Music Color is already running!");

            IsRunning = true;

            _log.Debug("Started Music Color.");


            try
            {


                var colorNum = GeneralSettings.SystemRainbowMaxTick;
                Color[] paletteSource = OutputSettings.OutputCurrentActivePalette.Colors;
                colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(paletteSource, 18).ToArray();
                int musicMode = OutputSettings.OutputSelectedMusicMode;
                var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;
                var numLED = OutputSettings.OutputLEDSetup.Spots.Length * OutputSettings.LEDPerSpot * OutputSettings.LEDPerLED;


                while (!token.IsCancellationRequested)
                {

                    bool outputIsSelected = false;
                    var currentOutput = MainViewModel.CurrentOutput;
                    if (currentOutput != null && currentOutput.OutputUniqueID == OutputSettings.OutputUniqueID)
                        outputIsSelected = true;
                    bool isPreviewRunning = MainViewModel.IsSplitLightingWindowOpen && outputIsSelected;
                    var fft = new float[32];
                    if (AudioFrames.FFT != null)
                        fft = AudioFrames.FFT;
                    
                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {
                        int position = 0;
                        foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                        {
                            position = (int)RainbowTicker.MusicStartIndex + spot.VID;
                            int n = 0;
                            if (position >= colorBank.Length)
                                n = position / colorBank.Length;
                            position = position - n * colorBank.Length; // run with VID

                            
                            //var brightness = 0.5;/*brightnessMap[spot.VID];*/
                            var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                            switch(OutputSettings.OutputMusicDancingMode)
                            {
                                case 0: // equalizer mode
                                    var brightnessMap = SpectrumCreator(fft, 0, 1, 1, 0, 32);// get brightness map based on spectrum data
                                    var freq = spot.MID;
                                    var actualFreq = 32 * ((double)freq / 1023d);
                                    var brightnessCap = OutputSettings.OutputBrightness / 100d;
                                    var actualBrightness = brightnessMap[(int)actualFreq] * brightnessCap;
                                    var outputColor = Brightness.applyBrightness(newColor, actualBrightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                                    ApplySmoothing(outputColor.R, outputColor.G, outputColor.B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                                    if (!OutputSettings.IsInSpotEditWizard)
                                        spot.SetColor(FinalR, FinalG, FinalB, isPreviewRunning);
                                    break;
                                case 1:
                                    var orientation = OutputSettings.VUOrientation;
                                    var brightnessMapVU = SpectrumVUCreator(fft, 0, 0, orientation);
                                    var column = spot.CID;
                                    var actualVUBrightness = 0d;
                                    switch (orientation)
                                    {
                                        case 0:
                                             actualVUBrightness = brightnessMapVU[column][spot.XIndex] / 255d;
                                            break;
                                        case 1:
                                             actualVUBrightness = brightnessMapVU[column][spot.YIndex] / 255d;
                                            break;
                                    }
                                    
                                    var outputColor1 = Brightness.applyBrightness(newColor, actualVUBrightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                                    ApplySmoothing(outputColor1.R, outputColor1.G, outputColor1.B, out byte FinalR1, out byte FinalG1, out byte FinalB1, spot.Red, spot.Green, spot.Blue);
                                    if (!OutputSettings.IsInSpotEditWizard)
                                        spot.SetColor(FinalR1, FinalG1, FinalB1, isPreviewRunning);
                                    break;
                            }
                           

                        }
                    }
                    Thread.Sleep(10);


                }
            }

            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                // return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");



                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {


                _log.Debug("Stopped MusicColor Color Creator.");
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


        public double[] SpectrumCreator(float[] fft, int sensitivity, double levelLeft, double levelRight, int musicMode, int numLED)//create brightnessmap based on input fft or volume
        {

            int counter = 0;
            int factor = numLED / fft.Length;
            byte maxbrightness = 255;
            double[] brightnessMap = new double[numLED];

            //this function take the input as frequency and output the color but the brightness change as the frequency band's value
            //equalizer mode, each block of LED is respond to 1 band of frequency spectrum


            for (int i = 0; i < fft.Length; i++)
            {

                brightnessMap[counter++] = (double)fft[i] / 255.0;

            }




            return brightnessMap;

        }
        public double[][] SpectrumVUCreator(float[] fft, int sensitivity, int vuMode, int orientation)//create brightnessmap based on input fft or volume
        {

            double[][] brightnessMap = new double[fft.Length][];
            int maxHeight=0;
            switch(orientation)
            {
                case 0://horizontal
                    maxHeight = OutputSettings.OutputNumLEDX;
                    break;
                case 1://vertical
                    maxHeight = OutputSettings.OutputNumLEDY;
                    break;
            }


            //this function take the input as frequency and output the color but the brightness change as the frequency band's value

            //Vu Metter mode , each block of led represent level of a frequency band (CID)

            switch (OutputSettings.VUMode)
            {
                case 0://normal VU
                    for (var i = 0; i < fft.Length; i++)
                    {
                        //create column
                        // get max position(could be X or Y base on settings)
                        //create a column represent the level of current fft value,

                        var height = fft[i] / 255f;
                        var actualHeight = (int)(height * maxHeight);
                        double[] brightnessColumn = new double[maxHeight];
                        for (int j = 0; j < maxHeight; j++)
                        {
                            if (j < actualHeight)
                                brightnessColumn[j] = 255;
                            else
                                brightnessColumn[j] = 0;

                        }
                        brightnessMap[i] = brightnessColumn;
                    }

                    break;
                case 1://normal VU inverse
                    for (var i = 0; i < fft.Length; i++)
                    {
                        //create column
                        // get max position(could be X or Y base on settings)
                        //create a column represent the level of current fft value,

                        var height = fft[i] / 255f;
                        var actualHeight = (int)(height * maxHeight);
                        double[] brightnessColumn = new double[maxHeight];
                        for (int j = 0; j < maxHeight; j++)
                        {
                            if (j < maxHeight-actualHeight)
                                brightnessColumn[j] = 0;
                            else
                                brightnessColumn[j] = 255;

                        }
                        brightnessMap[i] = brightnessColumn;
                    }

                    break;
                case 2://floating vu
                    for (var i = 0; i < fft.Length; i++)
                    {

                        var height = fft[i] / 255f;
                        var actualHeight = (int)(height * maxHeight);
                        double[] brightnessColumn = new double[maxHeight];

                        for (var j = 0; j < maxHeight / 2; j++)
                        {
                            if (Math.Abs(0 - j) <= actualHeight)
                                brightnessColumn[j] = 0.0;
                            else
                                brightnessColumn[j] = 255.0;

                        }


                        for (var j = maxHeight / 2; j < maxHeight; j++)
                        {
                            if (Math.Abs(maxHeight / 2 - j) <= actualHeight)
                                brightnessColumn[j] = 255.0;
                            else
                                brightnessColumn[j] = 0.0;

                        }
                        brightnessMap[i] = brightnessColumn;
                    }

                    break;
            }




            return brightnessMap;

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

    }
}
