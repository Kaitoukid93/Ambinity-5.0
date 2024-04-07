using adrilight_shared.Models.FrameData;
using System.ComponentModel;

namespace adrilight_shared.CaptureEngine
{
    public interface ICaptureEngine : INotifyPropertyChanged
    {
        ByteFrame[] Frames { get; set; }
        ByteFrame Frame { get; set; }
        void Stop();
        void RefreshCapturingState();
        object Lock { get; }
        int ServiceRequired { get; set; }
    }
}