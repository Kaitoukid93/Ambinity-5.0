using OpenRGB.NET;
using System.Collections.Generic;

namespace adrilight.Services.OpenRGBService
{
    public interface IAmbinityClient
    {
        bool IsInitialized { get; set; }
        List<OpenRGB.NET.Models.Device> ScanNewDevice();

        OpenRGBClient Client { get; set; }
        System.Diagnostics.Process ORGBProcess { get; set; }
        void Dispose();
        object Lock { get; }

    }
}