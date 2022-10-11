

using System.Threading;

namespace adrilight
    {
        public interface IHWMonitor
        {
        bool IsRunning { get; }

        void Run(CancellationToken token);
        void Dispose();
        }


    }


