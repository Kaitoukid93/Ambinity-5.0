using OpenRGB.NET;
using System.Collections.Generic;

namespace adrilight
{
    public interface IOpenRGBStream
    {
        bool IsRunning { get; }
        bool IsInitialized { get; set; }
        void Start();
        void Stop();
        //bool IsValid();
        void DFU();
        void RefreshTransferState();
        List<OpenRGB.NET.Models.Device> ScanNewDevice();
        OpenRGBClient AmbinityClient { get; set; }
        System.Diagnostics.Process ORGBProcess { get; set; }
        void Dispose();


    }
}