using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device.Zone.Spot;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Media;

namespace adrilight_shared.Models.Device.Zone
{
    public class LEDSetupHelpers
    {
        private static ControlModeHelpers CtrlHlprs { get; set; }

        public LEDSetup BuildLEDSetup(string name, int left, int top, int matrixWidth, int matrixHeight, double width, double height, int indexOffset) // general settings is for compare each device setting
        {
            if (CtrlHlprs == null)
            {
                CtrlHlprs = new ControlModeHelpers();
            }

            string owner = "Ambino";
            string description = "Default LED Setup for any device";
            string type = "ABRev2";

            var availableSpots = BuildMatrix(width, height, matrixWidth, matrixHeight, indexOffset);
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
            var ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, width, height);
            ledSetup.Left = left;
            ledSetup.Top = top;
            ledSetup.ZoneUID = Guid.NewGuid().ToString();
            ledSetup.Name = name;
            CtrlHlprs.MakeZoneControlable(ledSetup);
            return ledSetup;
        }

        private static IDeviceSpot[] BuildMatrix(double rectwidth, double rectheight, int spotsX, int spotsY, int indexOffset)
        {
            int spacing = 1;
            if (spotsX == 0)
                spotsX = 1;
            if (spotsY == 0)
                spotsY = 1;
            IDeviceSpot[] spotSet = new DeviceSpot[spotsX * spotsY];
            var compareWidth = (rectwidth - spacing * (spotsX + 1)) / spotsX;
            var compareHeight = (rectheight - spacing * (spotsY + 1)) / spotsY;
            var spotSize = Math.Min(compareWidth, compareHeight);

            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;
            var offSet = indexOffset;

            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing * i + (rectwidth - spotsX * spotSize - spacing * (spotsX - 1)) / 2 + i * spotSize;
                    var y = spacing * j + (rectheight - spotsY * spotSize - spacing * (spotsY - 1)) / 2 + j * spotSize;
                    var index = counter;
                    double scaleLeft = x / rectwidth;
                    double scaleTop = y / rectheight;
                    double scaleWidth = spotSize / rectwidth;
                    double scaleHeight = spotSize / rectheight;
                    var geometry = Geometry.Parse("M0 0H100V100H0V0Z").Clone();
                    geometry.Transform = new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                         new ScaleTransform(spotSize/100, spotSize/100),
                        }
                    };
                    var result = geometry.GetFlattenedPathGeometry();
                    result.Freeze();
                    spotSet[index] = new DeviceSpot(y, x, spotSize, spotSize, scaleTop, scaleLeft, scaleWidth, scaleHeight, index + offSet, index + offSet, i, index + offSet, j, false, result);
                    counter++;
                }
            }

            return spotSet;
        }
    }
}