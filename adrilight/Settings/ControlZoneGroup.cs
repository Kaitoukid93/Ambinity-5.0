using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class ControlZoneGroup : ViewModelBase
    {
        public ControlZoneGroup()
        {
            ZoneUIDCollection = new List<string>();
        }
        public string Name { get; set; }
        public Border Border { get; set; }
        public List<string> ZoneUIDCollection { get; set; }
        public IControlZone MaskedControlZone { get; set; }
        public ISlaveDevice MaskedSlaveDevice { get; set; }
    }
}
