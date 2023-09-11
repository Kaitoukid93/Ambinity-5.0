using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.Lighting
{
    public class LightingProfile : ViewModelBase
    {
        public LightingProfile()
        {

        }
        private bool _isSelected;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string ProfileUID { get; set; }
        public LightingMode ControlMode { get; set; }
        public List<string> TargetDevicesUID { get; set; }
        [JsonIgnore]
        public ObservableCollection<IDeviceSettings> TargetDevices { get; set; }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
    }
}
