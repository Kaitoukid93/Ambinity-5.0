using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace adrilight_shared.Spots
{
    public interface IDeviceSpot : INotifyPropertyChanged
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }

        byte SentryRed { get; }
        byte SentryGreen { get; }
        byte SentryBlue { get; }
        bool IsEnabled { get; set; }
        bool HasVID { get; set; }
        Color OnDemandColor { get; }
        Color SentryColor { get; }



        int Index { get; set; }
        int VID { get; set; }
        int MID { get; set; }
        int BackupID { get; set; }
        int CID { get; set; } //ffs this is column ID, soudn stupid but it represent the freuency position but in VU metter mode

        bool IsActivated { get; set; }


        Geometry Geometry { get; set; }
        void DimLED(float dimFactor);
        void SetColor(byte red, byte green, byte blue, bool raiseEvents);
        void SetSentryColor(byte red, byte green, byte blue);
        void SetVID(int vid);
        void SetID(int id);
        void SetMID(int mid);
        void SetCID(int cid);

        bool GetVIDIfNeeded(int vid, Rect rect, int mode);
        void UpdateView();
    }
}
