using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adrilight.Spots
{
    public class LEDSetupHelpers
    {
        public  LEDSetup BuildLEDSetup(int matrixWidth, int matrixHeight, string name, double width, double height) // general settings is for compare each device setting
        {

            string owner = "Ambino";
            string description = "Default LED Setup for any device";
            string type = "ABRev2";

            var availableSpots = BuildMatrix(width, height, matrixWidth, matrixHeight);
            ObservableCollection<IDeviceSpot> reorderedActiveSpots = new ObservableCollection<IDeviceSpot>();

            foreach (var spot in availableSpots)
            {
                spot.SetVID(spot.Index);
                reorderedActiveSpots.Add(spot);
            }
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = Screen.PrimaryScreen.Bounds.Height;
            var scaleWidth = width / screenWidth;
            var scaleHeight = height / screenHeight;
            var ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, width, height, scaleWidth, scaleHeight);


            return ledSetup;
        }

        private static IDeviceSpot[] BuildMatrix(double rectwidth, double rectheight, int spotsX, int spotsY)
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
                    double scaleLeft = x / rectwidth;
                    double scaleTop = y / rectheight;
                    double scaleWidth = spotSize / rectwidth;
                    double scaleHeight = spotSize / rectheight;
                    spotSet[index] = new DeviceSpot( y, x, spotSize, spotSize, scaleTop, scaleLeft, scaleWidth, scaleHeight, index, index, i, index, j, false, "genericSquare");
                    counter++;

                }
            }

            return spotSet;

        }
    }
}
