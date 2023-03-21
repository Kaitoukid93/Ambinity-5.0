using adrilight.Settings;
using adrilight.Spots;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight
{
    public interface ISlaveDevice
    {
        string Name { get; set; }
        string Owner { get; set; }
        string Thumbnail { get; set; }
        DeviceTypeEnum DesiredParrent { get; set; } // which hub or controller this slave device can be attached to
        string Description { get; set; }
        double ScaleTop { get; set; }
        double ScaleLeft { get; set; }
        double ScaleWidth { get; set; }
        double ScaleHeight { get; set; }
        List<IControlZone> ControlableZones { get; set; }
        void OnResolutionChanged(double scaleX, double scaleY);
        void UpdateSizeByChild();
        void RefreshSizeAndPosition();
    }
}