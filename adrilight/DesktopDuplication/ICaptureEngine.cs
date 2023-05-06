using adrilight.DesktopDuplication;
using System.ComponentModel;

namespace adrilight
{
    public interface ICaptureEngine : INotifyPropertyChanged
    {
        ByteFrame Frame { get; set; }
        void Stop();
        void RefreshCapturingState();
        string DeviceName { get; set; }
        object Lock { get; }
    }
}