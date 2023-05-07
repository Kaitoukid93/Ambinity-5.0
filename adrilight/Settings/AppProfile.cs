using adrilight.Helpers;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
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
