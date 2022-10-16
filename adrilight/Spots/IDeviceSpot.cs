using System.Drawing;
using Color = System.Windows.Media.Color;

namespace adrilight.Spots
{
    public interface IDeviceSpot
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }

        byte SentryRed { get; }
        byte SentryGreen { get; }
        byte SentryBlue { get; }
        bool IsEnabled { get; set; }
        Color OnDemandColor { get; }
        Color SentryColor { get; }
        Rectangle Rectangle { get; set; }
        bool IsFirst { get; set; }
        int RadiusX { get; set; }
        int RadiusY { get; set; }
        string ID { get; set; }
        int id { get; set; }
        int VID { get; set; }
        int MID { get; set; }
        int CID { get; set; } //ffs this is column ID, soudn stupid but it represent the freuency position but in VU metter mode
        int YIndex { get; set; }
        int XIndex { get; set; }
        bool IsActivated { get; set; }
        double BorderThickness { get; set; }
        bool IsIDVissible { get; set; }

        void DimLED(float dimFactor);
        void SetColor(byte red, byte green, byte blue, bool raiseEvents);
        void SetSentryColor(byte red, byte green, byte blue);
        void SetVID(int vid);
        void SetID(int id);
        void SetMID(int mid);
        void SetCID(int cid);
        void SetStroke(double strokeThickness);
        void SetIDVissible (bool isIDVissible);
        void SetRectangle(Rectangle rectangle);


    }
}
