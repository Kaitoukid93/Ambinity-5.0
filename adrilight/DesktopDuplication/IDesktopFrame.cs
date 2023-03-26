using adrilight.DesktopDuplication;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace adrilight
{
    public interface IDesktopFrame : INotifyPropertyChanged
    {
        ByteFrame Frame { get; set; }
        void Stop();
        void RefreshCapturingState();
        string ScreenToCapture { get; set; }
    }
}