﻿
using System.Collections.ObjectModel;
using System.Threading;

namespace adrilight
{
    public interface IDeviceDiscovery
    {
         ObservableCollection<IDeviceSettings> AvailableOpenRGBDevices { get; set; }
         ObservableCollection<IDeviceSettings> AvailableWLEDDevices { get; set; }
         ObservableCollection<IDeviceSettings> AvailableSerialDevices { get; set; }
        void StartThread();
    }


}

