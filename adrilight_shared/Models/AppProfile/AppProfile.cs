using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace adrilight_shared.Models.AppProfile
{
    public class AppProfile : ViewModelBase
    {
        public AppProfile() { DeviceProfiles = new List<DeviceProfile>(); }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Geometry { get; set; } = "profile";
        public List<DeviceProfile> DeviceProfiles { get; set; }
        public bool IsActivated { get; set; }



    }
}
