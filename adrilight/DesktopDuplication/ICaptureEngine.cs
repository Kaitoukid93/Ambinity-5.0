using adrilight.DesktopDuplication;
using System.ComponentModel;

namespace adrilight
{
    public interface ICaptureEngine : INotifyPropertyChanged
    {
        ByteFrame[] Frames { get; set; }
        ByteFrame Frame { get; set; }
        void Stop();
        void RefreshCapturingState();
        object Lock { get; }
    }
}