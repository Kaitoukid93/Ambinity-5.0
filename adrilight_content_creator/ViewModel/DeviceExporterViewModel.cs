using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Drawable;
using adrilight_shared.ViewModel;
using ExcelDataReader.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight_content_creator.ViewModel
{
    public class DeviceExporterViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        #region Construct
        public DeviceExporterViewModel(DeviceDBManager dbManager,OutputMappingViewModel outputmap)
        {
            CommandSetup();
            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            _dbManager =  dbManager;
            _outputMapViewModel = outputmap;
            Init();
        }
        #endregion
        #region Properties
        private DeviceDBManager _dbManager;
        private IDeviceSettings _selectedDevice;
        private OutputMappingViewModel _outputMapViewModel;
        public IDeviceSettings SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }
            set
            {
                _selectedDevice = value;
                _outputMapViewModel.Init(_selectedDevice);
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<IDeviceSettings> AvailableDevices { get; set; }
        #endregion
        #region Methods
        public async void Init()
        {
            //load available devices in database
            var devices = await _dbManager.LoadDeviceFromFolder(DevicesCollectionFolderPath);
            foreach(var device in devices) {
                AvailableDevices.Add(device);
            }
        }

        private void CommandSetup()
        {

            AddNewBlankDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AvailableDevices.Add(new DeviceSettings() { DeviceName = "New Device"});

            });
        }
        public void Dispose()
        {

        }
        #endregion
        #region Icommand
        public ICommand AddNewBlankDeviceCommand { get; set; }
        
        #endregion



    }
}
