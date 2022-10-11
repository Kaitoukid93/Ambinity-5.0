﻿using adrilight.Spots;
using adrilight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace adrilight.Settings
{
    internal class DefaulOutputCollection
    {
        public static List<IOutputSettings> AvailableDefaultOutputs {
            get
            {
                return new List<IOutputSettings> { AmbinoBasic(0, 11, 7, "LED Màn hình 24", true, "24inch"),
                                                   AmbinoBasic(0, 13, 7, "LED Màn hình 27", true, "27inch"),
                                                   AmbinoBasic(0, 14, 7, "LED Màn hình 29", true, "29inch"),
                                                   AmbinoBasic(0, 15, 8, "LED Màn hình 32", true, "32inch"),
                                                   AmbinoBasic(0, 17, 7, "LED Màn hình 34", true, "34inch"),
                                                   AmbinoEdge(0, 24, "LED Cạnh Bàn 1m2", 1, true, "ledstrip"),
                                                   AmbinoEdge(0, 20, "LED Cạnh Bàn 2m", 2, true, "ledstrip")
        };
            }
        }
        public static OutputSettings AmbinoBasic(int id, int numLEDX, int numLEDY, string name, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                Geometry = geometry,
                OutputID = id,
                OutputType = "Frame",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerLED = 2,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8), "Frame")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }

        public static OutputSettings AmbinoEdge(int id, int numLED, string name, int ledPerSpot, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                Geometry = geometry,
                OutputID = id,
                OutputType = "Strip",
                OutputNumLED = 100,
                OutputNumLEDX = numLED,
                OutputNumLEDY = 1,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Width / (8 * numLED)),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerSpot = ledPerSpot,
                LEDPerLED = 2,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLED, 1, "Strip", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Width / (8 * numLED)), "Strip")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }

        public static OutputSettings GenericLEDStrip(int id, int numLED, string name, int ledPerSpot, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                Geometry = geometry,
                OutputType = "Strip",
                OutputNumLED = 100,
                OutputNumLEDX = numLED,
                OutputNumLEDY = 1,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Width / (8 * numLED)),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 200,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerSpot = ledPerSpot,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLED, 1, "Strip", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Width / (8 * numLED)), "Strip")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericLEDMatrix(int id, int numLEDX, int numLEDY, string name, int ledPerSpot, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                Geometry = geometry,
                OutputID = id,
                OutputType = "Matrix",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerSpot = ledPerSpot,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Matrix", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8), "Matrix")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericRectangle(string name, int id, int numLEDX, int numLEDY)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                OutputType = "Frame",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = true,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8), "Frame")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericFan(string name, int id, int numLEDX, int numLEDY, bool isEnabled)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                OutputType = "Frame",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputRectangle = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                Geometry = "Fan",
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 8, Screen.PrimaryScreen.Bounds.Height / 8), "Frame")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        private static ILEDSetup BuildLEDSetup(int matrixWidth, int matrixHeight, string name, int setupID, System.Drawing.Rectangle rectangle, string outputType) // general settings is for compare each device setting
        {

            string owner = "Ambino";
            string description = "Default LED Setup for Ambino Basic Rev 2";
            string type = "ABRev2";



            List<IDeviceSpot> reorderedSpots = new List<IDeviceSpot>();

            //Create default spot
            var availableSpots = BuildMatrix(rectangle.Width, rectangle.Height, matrixWidth, matrixHeight);
            int counter = 0;


            switch (outputType)
            {
                case "Frame":
                    for (var i = 0; i < matrixHeight; i++) // bottom right ( default ambino basic start point) go up to top right
                    {
                        var spot = availableSpots[availableSpots.Length - 1 - matrixWidth * i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 1; i++) // top right go left to top left
                    {
                        var spot = availableSpots[matrixWidth - 2 - i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixHeight - 1; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (i + 1)];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 2; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (matrixHeight - 1) + i + 1];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);

                    }
                    break;
                case "Matrix":
                    foreach (var spot in availableSpots)
                    {
                        spot.IsActivated = true;
                        reorderedSpots.Add(spot);
                    }
                    break;
                case "Strip":
                    foreach (var spot in availableSpots)
                    {
                        if (spot.YIndex == 0)
                            spot.IsActivated = true;
                        
                        reorderedSpots.Add(spot);
                    }
                    break;


            }

            counter = 0;

            IDeviceSpot[] reorderedActiveSpots = new DeviceSpot[reorderedSpots.Count];

            foreach (var spot in reorderedSpots)
            {
                spot.SetVID(spot.VID * (256 / reorderedSpots.Count()));
                reorderedActiveSpots[counter++] = spot;
            }

            ILEDSetup ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, matrixWidth, matrixHeight, setupID, rectangle.Width, rectangle.Height);




            //if (availableLEDSetups != null)
            //{
            //    foreach (var ledsetup in availableLEDSetups)
            //    {
            //        if (ledsetup.SetupID == outputSettings.OutputUniqueID)//found match
            //            ledSetup = ledsetup;

            //    }
            //}



            return ledSetup;
        }

        private static IDeviceSpot[] BuildMatrix(int rectwidth, int rectheight, int spotsX, int spotsY)
        {
            int spacing = 1;
            if (spotsX == 0)
                spotsX = 1;
            if (spotsY == 0)
                spotsY = 1;
            IDeviceSpot[] spotSet = new DeviceSpot[spotsX * spotsY];
            var compareWidth = (rectwidth - (spacing * (spotsX + 1))) / spotsX;
            var compareHeight = (rectheight - (spacing * (spotsY + 1))) / spotsY;
            var spotSize = Math.Min(compareWidth, compareHeight);


            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;




            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing * i + (rectwidth - (spotsX * spotSize) - spacing * (spotsX - 1)) / 2 + i * spotSize;
                    var y = spacing * j + (rectheight - (spotsY * spotSize) - spacing * (spotsY - 1)) / 2 + j * spotSize;
                    var index = counter;

                    spotSet[index] = new DeviceSpot(i, j, x, y, spotSize, spotSize, index, index, i, index, j, false, false);
                    counter++;

                }
            }

            return spotSet;

        }
    }
}

