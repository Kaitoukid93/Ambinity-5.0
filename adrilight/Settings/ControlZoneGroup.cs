using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace adrilight.Settings
{
    public class ControlZoneGroup : ViewModelBase
    {
        public ControlZoneGroup()
        {
            ZoneUIDCollection = new List<string>();
        }
        private Border _border;
        public string Name { get; set; }
        public Border Border { get => _border; set { Set(() => Border, ref _border, value); } }
        public ControllerTypeEnum Type { get; set; }
        public List<string> ZoneUIDCollection { get; set; }
        public string GroupUID { get; set; }
        public IControlZone MaskedControlZone { get; set; }
        public ISlaveDevice MaskedSlaveDevice { get; set; }
    }
}
