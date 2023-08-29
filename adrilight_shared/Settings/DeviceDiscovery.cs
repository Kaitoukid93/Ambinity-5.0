using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.MDns;
using static adrilight.View.AddNewDeviceWindow;

namespace adrilight.Settings
{
    class DeviceDiscovery
    {
        private static DeviceDiscovery Instance;
        private ServiceBrowser serviceBrowser;
        public event EventHandler<DeviceCreatedEventArgs> ValidDeviceFound;

        private DeviceDiscovery()
        {
            serviceBrowser = new ServiceBrowser();
            serviceBrowser.ServiceAdded += OnServiceAdded;
        }

        public void StartDiscovery()
        {
            serviceBrowser.StartBrowse("_http._tcp");
        }

        public void StopDiscovery()
        {
            serviceBrowser.StopBrowse();
        }

        private async void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            WLEDDevice toAdd = new WLEDDevice();
            foreach (var addr in e.Announcement.Addresses)
            {
                toAdd.NetworkAddress = addr.ToString(); break; //only get first address
            }
            toAdd.Name = e.Announcement.Hostname;
            toAdd.NameIsCustom = false;
            if (await toAdd.Refresh()) //check if the service is a valid WLED light
            {
                OnValidDeviceFound(new DeviceCreatedEventArgs(toAdd, false));
            }
        }

        public static DeviceDiscovery GetInstance()
        {
            if (Instance == null) Instance = new DeviceDiscovery();
            return Instance;
        }

        protected virtual void OnValidDeviceFound(DeviceCreatedEventArgs e)
        {
            ValidDeviceFound?.Invoke(this, e);
        }
    }
}
