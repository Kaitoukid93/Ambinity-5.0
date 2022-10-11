
using System.Threading;

namespace adrilight
    {
    public interface IRainbowTicker
    {
        bool IsRunning { get; }

        void Run(CancellationToken token);
        double RainbowStartIndex { get; }
        double MusicStartIndex { get; }
        double BreathingBrightnessValue { get; }

    }
    }


