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
        public class ComPortObject
        {
            public ComPortObject(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
            public string Port { get; set; }
            public bool IsConnected { get; set; }
            public bool IsChecked { get; set; }

        }
        public List<ComPortObject> GetPorts()
        {
            var ports = new List<ComPortObject>();
            foreach(var device in Devices)
            {
                var port = new ComPortObject(device.OutputPort);
                port.Port = device.OutputPort;
                port.IsConnected = device.IsTransferActive;
                ports.Add(port);
            }
            return ports;
        }
        public void Connect()
        {
            if(Devices == null)
            {
                return;
            }
            foreach (var dev in Devices)
            {
                dev.IsTransferActive = true;
            }
        }
        public void Disconnect()
        {
            if (Devices == null)
            {
                return;
            }
            foreach (var dev in Devices)
            {
                dev.IsTransferActive = false;
            }
        }
        public string HUBSerial { get => _hubSerial; set { Set(() => HUBSerial, ref _hubSerial, value); } }
    }
}
