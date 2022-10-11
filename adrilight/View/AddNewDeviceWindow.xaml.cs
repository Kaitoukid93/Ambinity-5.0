using adrilight.Settings;
using adrilight.Util;
using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AddNewDeviceWindow

    {
        public event EventHandler<DeviceCreatedEventArgs> DeviceCreated;
        private bool discoveryMode = false;
        private int devicesFoundCount = 0;
        private bool addTypeManual = false;

        public AddNewDeviceWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }

        private void OnDiscoveryButtonClicked(object sender, EventArgs e)
        {
            discoveryMode = !discoveryMode;
            Button b = sender as Button;
            if (b == null) return;
            var discovery = DeviceDiscovery.GetInstance();
            if (discoveryMode)
            {
                //Start mDNS discovery

                devicesFoundCount = 0;
                discovery.ValidDeviceFound += OnDeviceCreated;


                discovery.StartDiscovery();
            }
            else
            {
                //Stop mDNS discovery
                discovery.StopDiscovery();
                discovery.ValidDeviceFound -= OnDeviceCreated;

            }
        }
        private void OnDeviceCreated(object sender, DeviceCreatedEventArgs e)
        {
            //this method only gets called by mDNS search, display found devices
            devicesFoundCount++;
            e.CreatedDevice.Geometry = "powerLED";

            ViewModel.AvailableWLEDDevices.Add(e.CreatedDevice);

            ////create new device based on created wled device
            //IDeviceSettings wirelessDevice = new DeviceSettings();
            //wirelessDevice.DeviceName = e.CreatedDevice.Name;
            //wirelessDevice.OutputPort = e.CreatedDevice.NetworkAddress;
            //wirelessDevice.DeviceType = "WLED";
            //wirelessDevice.AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0,64)};
            //wirelessDevice.DeviceID = ViewModel.AvailableDevices.Count() + 1;
            ////add device to available devices
            //ViewModel.AvailableDevices.Add(wirelessDevice);

        }
        void Window_Closing(object sender, CancelEventArgs e)
        {
            //stop discovery if running
            if (discoveryMode)
            {
                var discovery = DeviceDiscovery.GetInstance();
                discovery.StopDiscovery();
                discovery.ValidDeviceFound -= OnDeviceCreated;

            }

        }


        private void Tab_Changed(object sender, SelectionChangedEventArgs e)
        {


        }
        public class DeviceCreatedEventArgs
        {
            public WLEDDevice CreatedDevice { get; }
            public bool RefreshRequired { get; } = true;

            public DeviceCreatedEventArgs(WLEDDevice created, bool refresh = true)
            {
                CreatedDevice = created;

                //DeviceDiscovery already made an API request to confirm that the new device is a WLED light,
                //so a refresh is only required for manually added devices
                RefreshRequired = refresh;
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {

            discoveryMode = !discoveryMode;


            var discovery = DeviceDiscovery.GetInstance();
            if (discoveryMode)
            {
                ViewModel.AvailableWLEDDevices = new System.Collections.ObjectModel.ObservableCollection<WLEDDevice>();
                //Start mDNS discovery
                LoadingLine.Visibility = Visibility.Visible;
                RefreshButton.Content = "Stop";
                devicesFoundCount = 0;
                discovery.ValidDeviceFound += OnDeviceCreated;

                discovery.StartDiscovery();
            }
            else
            {
                //Stop mDNS discovery
                LoadingLine.Visibility = Visibility.Collapsed;
                RefreshButton.Content = "Refresh";
                discovery.StopDiscovery();
                discovery.ValidDeviceFound -= OnDeviceCreated;

            }

        }

        private void ScanWiredDevice(object sender, RoutedEventArgs e)
        {
            discoveryMode = !discoveryMode;



            if (discoveryMode)
            {

                wiredLoadingLine.Visibility = Visibility.Visible;
                ScanButton.Content = "Stop";
                ScanButton.CommandParameter = "stop";
            }
            else
            {
                wiredLoadingLine.Visibility = Visibility.Collapsed;
                ScanButton.Content = "Scan now";
                ScanButton.CommandParameter = "scan";
            }
        }

        private void AddManually(object sender, RoutedEventArgs e)
        {
            addTypeManual = !addTypeManual;
            if(addTypeManual)
            {

                AddManual.Content = "Auto scan";
                autowired.Visibility = Visibility.Collapsed;
                manualwired.Visibility = Visibility.Visible;
                AddManuallyGrid.Visibility = Visibility.Visible;
                AddAllButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddManual.Content = "Add manually";
                autowired.Visibility = Visibility.Visible;
                manualwired.Visibility = Visibility.Collapsed;
                AddManuallyGrid.Visibility = Visibility.Collapsed;
                AddAllButton.Visibility = Visibility.Visible;
            }
            
        }
    }
}
