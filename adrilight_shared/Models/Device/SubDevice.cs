using adrilight_shared.Models.DashboardItem;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Device
{
    public class SubDevice : ViewModelBase
    {
        public SubDevice() { }
        public SubDevice(string name, int portIndex, string firmwareVersion, string hardwareVersion, string serial, string comPort)
        {
            Name = name;
            PortIndex = portIndex;
            FirmwareVersion = firmwareVersion;
            HardwareVersion = hardwareVersion;
            Serial = serial;
            ComPort = comPort;
        }

        public string Name { get; set; }
       public int PortIndex { get; set; }
       public string FirmwareVersion { get; set; }  
        public string HardwareVersion { get; set; }
        public string Serial { get; set; }
        public string ComPort { get; set; }
        public string HUBSerial { get; set; }
        public int MaxLEDOutput { get; set; }
    }
}
