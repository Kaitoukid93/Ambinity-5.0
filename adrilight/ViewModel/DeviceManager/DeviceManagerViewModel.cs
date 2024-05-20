using adrilight.Services.DeviceDiscoveryServices;
using adrilight.View;
using adrilight.ViewModel.DeviceManager;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Microsoft.Win32.TaskScheduler;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace adrilight.ViewModel
{
    /// <summary>
    /// device manager viewmodel contains device collection  and device advance settings
    /// hence contains DeviceCollectionViewModel and DeviceAdvanceSettingsViewModel
    /// </summary>
    public class DeviceManagerViewModel : ViewModelBase
    {
        #region Construct
        public event Action<IDeviceSettings> NewDeviceAdded;
        public DeviceManagerViewModel(
            DialogService service,
            DeviceDBManager deviceDBManager,
            IList<ISelectablePage> availablePages,
            DeviceCollectionViewModel collectionViewModel,
            DeviceAdvanceSettingsViewModel advanceSettingsViewModel,
            DeviceDiscovery deviceDiscovery)
        {
            SelectablePages = availablePages;
            _deviceAdvanceSettingsViewModel = advanceSettingsViewModel;
            _deviceCollectionViewModel = collectionViewModel;
            _deviceHlprs = new DeviceHelpers();
            _deviceDBManager = deviceDBManager;
            _deviceDiscovery = deviceDiscovery;
            _deviceCollectionViewModel.DeviceCardClicked += OnDeviceSelected;
            _deviceDiscovery.SerialDevicesScanComplete += OnSerialDevicesScanComplete;
            LoadNonClientAreaData("Adrilight  |  Device Manager", "profileManager", false, null);
            LoadData();
            CommandSetup();
        }

        #endregion

        #region Events
        private async void OnDeviceSelected(IGenericCollectionItem item)
        {
            await GotoDeviceSettings(item as DeviceSettings);
        }
        private async void OnOpenRGBDevicesScanComplete(List<Device> availableDevices)
        {
            //check if theres any old device existed
            //if true, turn on serial stream for each device
            //check if any new device
            //if true, show searching screen
            //run device construct and device downloader as part of the process, loading bar always exist
        }
        private async void OnSerialDevicesScanComplete(List<string> availableDevices)
        {
            //check if theres any old device existed
            //if true, turn on serial stream for each device
            //check if any new device
            //if true, show searching screen
            //run device construct and device downloader as part of the process, loading bar always exist
            //add new device to collection by calling add items method on devicecollectionviewmodel
            //foreach new devices, invoke new device found action










            //if (newDevices != null && newDevices.Count > 0)
            //{
            //    IsDeviceDiscoveryInit = false;
            //    foreach (var newDevice in newDevices)
            //    {
            //        var device = newDevice as IDeviceSettings;
            //        SetSearchingScreenHeaderText("New device found: " + device.DeviceName, true);
            //        //download device info
            //        SetSearchingScreenHeaderText("Downloading device modules: " + device.DeviceName, true);
            //        var result = await DownloadDeviceInfo(device);
            //        if (!result)
            //        {
            //            SetSearchingScreenHeaderText("Using Default: " + device.DeviceName, true);
            //        }
            //        else
            //        {
            //            SetSearchingScreenHeaderText("Device modules downloaded: " + device.DeviceName, true);
            //            var downloadedDevice = ImportDevice(Path.Combine(CacheFolderPath, device.DeviceName + ".zip"));
            //            if (downloadedDevice != null)
            //            {
            //                //transplant
            //                downloadedDevice.OutputPort = device.OutputPort;
            //                downloadedDevice.FirmwareVersion = device.FirmwareVersion;
            //                downloadedDevice.HardwareVersion = device.HardwareVersion;
            //                downloadedDevice.DeviceSerial = device.DeviceSerial;
            //                downloadedDevice.DeviceType.ConnectionTypeEnum = device.DeviceType.ConnectionTypeEnum;
            //                device = downloadedDevice;
            //            }
            //        }
            //        device.IsTransferActive = true;

            //        if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3)
            //        {
            //            GeneralSettings.IsOpenRGBEnabled = true;
            //        }
            //        SetSearchingScreenProgressText("Writing device information...");

            //        //await Task.Delay(TimeSpan.FromSeconds(2));
            //        lock (device)
            //        {
            //            WriteDeviceInfo(device);
            //        }


            //        lock (AvailableDeviceLock)
            //        {
            //            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            //            {
            //                AvailableDevices.Insert(0, device);
            //            });
            //        }
            //        lock (DeviceManagerViewModel.AvailableDevices)
            //        {
            //            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            //            {
            //                DeviceManagerViewModel.AvailableDevices.AddItems(device as DeviceSettings);
            //            });

            //        }
            //        await Task.Delay(TimeSpan.FromSeconds(2));
            //    }
            //    SearchingForDevices = false;
            //    IsDeviceDiscoveryInit = true;
            //    await Task.Delay(TimeSpan.FromSeconds(1));
            //}
            //else
            //{
            //    if (_noDeviceDetectedCounter < 3)
            //        _noDeviceDetectedCounter++;
            //    if (_noDeviceDetectedCounter >= 3)
            //    {
            //        SearchingForDevices = false;
            //        IsDeviceDiscoveryInit = true;
            //    }
            //}
        }
        #endregion


        #region Properties
        //private
        private DeviceHelpers _deviceHlprs;
        private DeviceCollectionViewModel _deviceCollectionViewModel;
        private DeviceAdvanceSettingsViewModel _deviceAdvanceSettingsViewModel;
        private ISelectablePage _selectedPage;
        private DevicesManager _deviceManager;
        private DeviceDBManager _deviceDBManager;
        private bool _isManagerWindowOpen;
        private DeviceDiscovery _deviceDiscovery;
        //public
        public NonClientArea NonClientAreaContent { get; set; }
        public IList<ISelectablePage> SelectablePages { get; set; }
        public ISelectablePage SelectedPage {
            get => _selectedPage;

            set
            {
                Set(ref _selectedPage, value);
            }
        }
        public bool IsManagerWindowOpen {
            get
            {
                return _isManagerWindowOpen;
            }
            set
            {
                _isManagerWindowOpen = value;
                RaisePropertyChanged();
            }
        }



        #endregion


        #region Methods
        private void CommandSetup()
        {
            OpenDevicesFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                _deviceDBManager.OpenDevicesFolder();
            });

            WindowClosing = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                IsManagerWindowOpen = false;

            });
            WindowOpen = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                IsManagerWindowOpen = true;
            });
        }
        private async Task GotoDeviceSettings(IGenericCollectionItem item)
        {
            if (item == null)
            {
                return;
            }
            var device = item as DeviceSettings;
            //show loading screen, in the mean time, load device hardware info
            //show loading screen
            var loadingScreen = SelectablePages.Where(p => p.PageName == "Loading").First();
            SelectedPage = loadingScreen;
            //load device info
            await _deviceAdvanceSettingsViewModel.Init(device);
            //show advance settings view
            var advanceView = SelectablePages.Where(p => p.PageName == "Devices Advance Settings").First();
            (advanceView as DeviceControlView).DataContext = _deviceAdvanceSettingsViewModel;
            SelectedPage = advanceView;
            ICommand backButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BacktoCollectionView();
            }
            );
            LoadNonClientAreaData("Adrilight  |  Device Manager | " + device.DeviceName, "profileManager", true, backButtonCommand);

        }
        private void BacktoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Device Manager", "profileManager", false, null);
            var collectionView = SelectablePages.Where(p => p.PageName == "Devices Collection").First();
            (collectionView as DeviceCollectionView).DataContext = _deviceCollectionViewModel;
            SelectedPage = collectionView;

        }

        private void LoadNonClientAreaData(string content, string geometry, bool showBackButton, ICommand buttonCommand)
        {
            var vm = new NonClientAreaContentViewModel(content, geometry, showBackButton, buttonCommand);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }
        public void LoadData()
        {

            _deviceManager = new DevicesManager();
            var devices = _deviceManager.LoadDeviceIfExists();
            if (devices == null)
                return;
            _deviceCollectionViewModel.Init(devices);
            BacktoCollectionView();
        }
        #endregion

        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        public ICommand OpenDevicesFolderCommand { get; set; }
        #endregion





    }
}
