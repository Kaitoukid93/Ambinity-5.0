using OpenRGB.NET;

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
        OpenRGB.NET.Models.Device[] GetDevices { get; }
        OpenRGBClient AmbinityClient { get; set; }


    }
}