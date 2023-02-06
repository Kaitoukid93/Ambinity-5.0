using adrilight.DesktopDuplication;
using adrilight.Extensions;
using adrilight.Spots;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace adrilight
{
    internal class LEDSetup : ViewModelBase, ILEDSetup
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonLEDSetupFileNameAndPath => Path.Combine(JsonPath, "adrilight-LEDSetups.json");
        public LEDSetup(string name, string owner, string type, string description, ObservableCollection<IDeviceSpot> spots, int matrixWidth, int matrixHeight, int setupID, double pixelWidth, double pixelHeight)
        {
            Name = name;
            Owner = owner;
            TargetType = type;
            Description = description;
            Spots = spots;
            MatrixWidth = matrixWidth;
            MatrixHeight = matrixHeight;
            SetupID = setupID;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;

        }
        public LEDSetup()
        {

        }
        private ObservableCollection<IDeviceSpot> _spots;
        public string Name { get; set; }
        public string Owner { get; set; }
        public string TargetType { get; set; } // Tartget Type of the spotset (keyboard, strips, ...
        public string Description { get; set; }
        public ObservableCollection<IDeviceSpot> Spots { get => _spots; set { Set(() => Spots, ref _spots, value); RaisePropertyChanged(); } }
        public int MatrixWidth { get; set; }
        public int MatrixHeight { get; set; }
        public double PixelWidth { get; set; }
        public double PixelHeight { get; set; }
        public object Lock { get; } = new object();
        public int SetupID { get; set; }    // to match with device ID

        public void DimLED(float dimFactor)
        {
            foreach (var spot in Spots)
            {
                spot.DimLED(dimFactor);
            }
        }
    }


}
