using adrilight_shared.Models.DashboardItem;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Device
{
    /// <summary>
    /// this represent a HUB like device ( contains more than one usb port)
    /// </summary>
    public class DeviceHUB : ViewModelBase, IDashboardItem, IGenericCollectionItem
    {
        public DeviceHUB()
        {

        }
        public DeviceHUB(string name)
        {
            Name = name;
            Devices = new ObservableCollection<IDeviceSettings>();
        }
        private bool _isPinned = false;
        private bool _isChecked = false;
        private bool _isSelected = false;
        private string _hubSerial;
        public bool IsPinned { get => _isPinned; set { Set(() => IsPinned, ref _isPinned, value); } }
        public string Name { get; set; }
        [JsonIgnore]
        public bool IsEditing { get; set; }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
        public string InfoPath { get; set; }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public ObservableCollection<IDeviceSettings> Devices { get; set; }
        public List<string> GetPorts()
        {
            var ports = new List<string>();
            foreach(var device in Devices)
            {
                ports.Add(device.OutputPort.ToString());
            }
            return ports;
        }
        public string HUBSerial { get => _hubSerial; set { Set(() => HUBSerial, ref _hubSerial, value); } }
    }
}
