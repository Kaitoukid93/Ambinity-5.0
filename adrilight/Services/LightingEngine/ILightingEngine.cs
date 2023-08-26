using adrilight_shared.Enums;
using System.Threading;

namespace adrilight.Services.LightingEngine
{
    public interface ILightingEngine
    {
        bool IsRunning { get; }
        void Run(CancellationToken token);
        void Refresh();
        void Stop();
        LightingModeEnum Type { get; }

    }


}


