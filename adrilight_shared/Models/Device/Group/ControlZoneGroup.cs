using adrilight_shared.Enums;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Device.Group
{
    public class ControlZoneGroup : ViewModelBase
    {
        public ControlZoneGroup()
        {

        }
        private Border _border;
        public string Name { get; set; }
        public Border Border { get => _border; set { Set(() => Border, ref _border, value); } }
        public ControllerTypeEnum Type { get; set; }
        public string GroupUID { get; set; }
        public IControlZone MaskedControlZone { get; set; }
        public ISlaveDevice MaskedSlaveDevice { get; set; }
    }
}
