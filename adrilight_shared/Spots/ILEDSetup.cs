using System.Collections.ObjectModel;
using System.ComponentModel;

namespace adrilight_shared.Spots
{
    public interface ILEDSetup : INotifyPropertyChanged
    {



        string Name { get; set; }
        string Geometry { get; set; }
        string Owner { get; set; }
        string Thumbnail { get; set; }
        ObservableCollection<IDeviceSpot> Spots { get; set; }
        string TargetType { get; set; }
        string Description { get; set; }
        object Lock { get; }
        RGBLEDOrderEnum RGBLEDOrder { get; set; }
        void DimLED(float dimFactor);
        public double ScaleTop { get; set; }
        public double ScaleLeft { get; set; }
        public double ScaleWidth { get; set; }
        public double ScaleHeight { get; set; }
        int OutputSelectedDisplay { get; set; }
        bool IsScreenCaptureEnabled { get; set; }
        void OnResolutionChanged(double scaleX, double scaleY);
        void RefreshSizeAndPosition();
    }

}

