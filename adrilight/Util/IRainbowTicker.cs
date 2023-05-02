
using adrilight.Util;
using System.Collections.ObjectModel;
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
        int FrameIndex { get; set; }
        object Lock { get; }
        ObservableCollection<Tick> Ticks { get; }
        Tick MakeNewTick(int maxTick, double tickSpeed,string tickUID,TickEnum tickType);

    }
    }


