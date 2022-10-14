using OpenRGB.NET;
using System.Collections.Generic;

namespace adrilight
{
    public interface IAmbinityClient
    {
        bool IsRunning { get; }
        bool IsInitialized { get; set; }

        void RefreshOpenRGBDeviceState( bool init);
        void RefreshTransferState();
        List<OpenRGB.NET.Models.Device> ScanNewDevice();
        List<IDeviceSettings> AvailableDevices { get; set; }
        OpenRGBClient Client { get; set; }
        System.Diagnostics.Process ORGBProcess { get; set; }
        void Dispose();
        object Lock { get;}


    }
}