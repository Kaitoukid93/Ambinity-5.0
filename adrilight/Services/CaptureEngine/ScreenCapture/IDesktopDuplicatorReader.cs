using System.Threading;

namespace adrilight.Services.CaptureEngine.ScreenCapture
{
    public interface IDesktopDuplicatorReader
    {
        //bool IsRunning { get; }

        void Run(CancellationToken token);
        void Stop();
        void Refresh();
    }
}