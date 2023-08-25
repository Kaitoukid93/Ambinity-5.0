﻿
using adrilight.Models.ControlMode.Enum;
using System.Threading;

namespace adrilight
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


