
using System.Threading;

namespace adrilight
    {
        public interface ICompositionPlayer
        {
        bool IsRunning { get; }

        void Run(CancellationToken token);
        
    }


    }


