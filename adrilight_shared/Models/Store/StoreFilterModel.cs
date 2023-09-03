using adrilight_shared.Enums;

namespace adrilight_shared.Models.Store
{
    public class StoreFilterModel
    {
        public string Name { get; set; }
        public DeviceTypeEnum DeviceTypeFilter { get; set; } // this could be a string or device type...
        public string DataTypeFilter { get; set; }
        public string NameFilter { get; set; }
    }
}
