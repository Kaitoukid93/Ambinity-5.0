﻿using adrilight.Helpers;
using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class DeviceManagerViewModel : ViewModelBase
    {
        #region Construct
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        public DeviceManagerViewModel(CollectionItemStore store, DialogService service)
        {

            _collectionItemStore = store;
            _collectionItemStore.SelectedItemChanged += OnSelectedItemChanged;
            _collectionItemStore.SelectedItemsChanged += OnSelectedItemsChanged;
            _collectionItemStore.Navigated += OnCollectionViewNavigated;
            _collectionItemStore.ItemPinStatusChanged += OnCollectionPinStatusChanged;
            _collectionItemStore.ItemsRemoved += OnItemsRemove;
            _collectionItemStore.ItemAdded += OnItemAdded;
            _dialogService = service;
            _deviceHlprs = new DeviceHelpers();
            LoadNonClientAreaData();
            LoadData();
            CommandSetup();
        }
        #endregion


        #region Events
        private void OnItemAdded(IGenericCollectionItem item)
        {

            var newDevice = item as DeviceSettings;
            if (item == null)
                return;
            // create new IDeviceSettings with new Name
            //Get ID:
            
                var iD = newDevice.DeviceUID.ToString();
                //now inject
                InjectingDevice(kernel, device);
                device.DeviceEnableChanged();
                var playlistDecoder = kernel.Get<PlaylistDecoder>();
                playlistDecoder.AvailableDevices.Add(device);
                //since openRGBStream is single instance, we need to manually add device then refresh
        }
        private void OnItemsRemove(List<IGenericCollectionItem> list)
        {
            foreach (var item in list)
            {
                var deviceToRemove = item as DeviceSettings;
                deviceToRemove.IsTransferActive = false;
                _deviceHlprs.RemoveDeviceLocalData(item as DeviceSettings);
            }

            //foreach (var item in list)
            //{
            //    DashboardPinnedItems.RemoveItems(item, false);
            //}
            //AvailableLightingProfiles.ResetSelectionStage();
            //AvailableLightingProfilePlaylists.ResetSelectionStage();

            var removedDevice = e.OldItems;
            foreach (IDeviceSettings device in removedDevice) // when an item got removed, simply stop dependencies service from running
            {
                UnInjectingDevice(kernel, device);
            }




        }
        private void OnCollectionPinStatusChanged(IGenericCollectionItem item)
        {
            // RefreshDashboardItems();

        }
        private void OnSelectedItemsChanged(List<IGenericCollectionItem> list, string target)
        {

        }
        private void OnSelectedItemChanged(IGenericCollectionItem item)
        {
            //if (item is LightingProfilePlaylist)
            //{
            //    _player.Play(item as LightingProfilePlaylist);
            //}
            //else if (item is LightingProfile)
            //{
            //    _player.Play(item as LightingProfile);
            //}
        }



        private async void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            //show button if needed
            if (item == null)
            {
                return;
            }
            if (CurrentDevice == null)
                CurrentDevice = new DeviceAdvanceSettingsViewModel(_dialogService, item as DeviceSettings);
            var nonClientVm = NonClientAreaContent.DataContext as NonClientAreaContentViewModel;
            if (mode == DataViewMode.Loading)
            {
                //aquire device advance info
                await CurrentDevice.RefreshDeviceHardwareInfo();
                IsApplyingDeviceHardwareSettings = false;
                //show button
                nonClientVm.ShowBackButton = true;
                nonClientVm.BackButtonCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, (p) =>
                {
                    _collectionItemStore.BackToCollectionView(item);
                }
                );
                AvailableDevices.GotoCurrentItemDetailViewCommand.Execute(CurrentDevice.Device);
            }
            else if (mode == DataViewMode.Detail)
            {
                nonClientVm.ShowBackButton = true;
            }
            else
            {
                nonClientVm.ShowBackButton = false;
                CurrentDevice = null;
            }

        }
        #endregion


        #region Properties
        public NonClientArea NonClientAreaContent { get; set; }
        private DeviceHelpers _deviceHlprs;
        public DeviceAdvanceSettingsViewModel CurrentDevice { get; set; }
        private bool isApplyingDeviceHardwareSettings;
        public bool IsApplyingDeviceHardwareSettings {
            get
            {
                return isApplyingDeviceHardwareSettings;
            }
            set
            {
                isApplyingDeviceHardwareSettings = value;
                RaisePropertyChanged();
            }
        }
        private readonly CollectionItemStore _collectionItemStore;
        public List<DeviceSettings> LoadDeviceIfExists()
        {
            var devices = new List<DeviceSettings>();
            if (!Directory.Exists(DevicesCollectionFolderPath)) return null; // no device has been added

            foreach (var folder in Directory.GetDirectories(DevicesCollectionFolderPath))
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(folder, "config.json"));
                    var device = JsonConvert.DeserializeObject<DeviceSettings>(json);
                    device.AvailableControllers = new List<IDeviceController>();
                    //read slave device info
                    //check if this device contains lighting controller
                    var lightingoutputDir = Path.Combine(Path.Combine(folder, "LightingOutputs"));
                    var pwmoutputDir = Path.Combine(Path.Combine(folder, "PWMOutputs"));
                    DeserializeChild<ARGBLEDSlaveDevice>(lightingoutputDir, device, OutputTypeEnum.ARGBLEDOutput);
                    DeserializeChild<PWMMotorSlaveDevice>(pwmoutputDir, device, OutputTypeEnum.PWMOutput);
                    devices.Add(device);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, folder);
                    continue;
                }
            }
            return devices;
        }
        private void DeserializeChild<T>(string outputDir, IDeviceSettings device, OutputTypeEnum outputType)
        {
            if (Directory.Exists(outputDir))
            {
                //add controller to this device

                var controller = new DeviceController();
                switch (outputType)
                {
                    case OutputTypeEnum.PWMOutput:
                        controller.Geometry = "fanSpeedController";
                        controller.Name = "Fan";
                        controller.Type = ControllerTypeEnum.PWMController;
                        break;
                    case OutputTypeEnum.ARGBLEDOutput:
                        controller.Geometry = "brightness";
                        controller.Name = "Lighting";
                        controller.Type = ControllerTypeEnum.LightingController;
                        break;
                }


                foreach (var subfolder in Directory.GetDirectories(outputDir)) // each subfolder contains 1 slave device
                {
                    //read slave device info
                    var outputJson = File.ReadAllText(Path.Combine(subfolder, "config.json"));
                    var output = JsonConvert.DeserializeObject<OutputSettings>(outputJson);
                    var slaveDeviceJson = File.ReadAllText(Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "config.json"));
                    var slaveDevice = JsonConvert.DeserializeObject<T>(slaveDeviceJson);

                    if (slaveDevice == null)//somehow data corrupted
                        continue;
                    else
                    {
                        if (!File.Exists((slaveDevice as ISlaveDevice).Thumbnail))
                        {
                            //(slaveDevice as ISlaveDevice).Thumbnail = Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "thumbnail.png");
                        }
                    }


                    output.SlaveDevice = slaveDevice as ISlaveDevice;
                    controller.Outputs.Add(output);
                    //each slave device attach to one output so we need to create output
                    //lightin

                }
                device.AvailableControllers.Add(controller);
            }
        }
        private void RefreshDashboardItems()
        {
            if (DashboardPinnedItems == null)
                DashboardPinnedItems = new DataCollection("Dashboard Items", _dialogService, "profile", _collectionItemStore);
            DashboardPinnedItems.Items.Clear();
            foreach (var item in AvailableDevices.Items)
            {
                if (item.IsPinned)
                    DashboardPinnedItems.AddItems(item);
            }
            foreach (var item in AvailableDevices.Items)
            {
                if (item.IsPinned)
                    DashboardPinnedItems.AddItems(item);
            }
        }
        public DataCollection DashboardPinnedItems { get; set; }
        public DataCollection AvailableDevices { get; set; }
        private DialogService _dialogService;

        #endregion


        #region Methods
        private void CommandSetup()
        {
            OpenDevicesFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (Directory.Exists(DevicesCollectionFolderPath))
                    Process.Start("explorer.exe", DevicesCollectionFolderPath);
            });
            WindowClosing = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //Player.WindowsStatusChanged(false);

            });
            WindowOpen = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //Player.WindowsStatusChanged(true);
            });
        }
        public void SaveData()
        {

        }
        private void LoadNonClientAreaData()
        {
            var vm = new NonClientAreaContentViewModel("Adrilight  |  Device Manager", "profileManager");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }
        public void LoadData()
        {
            //RefreshDashboardItems();
            AvailableDevices = new DataCollection("All Devices", _dialogService, "profile", _collectionItemStore);
            AvailableDevices.PropertyChanged += AvailableDevicesPropertyChanged;
            AvailableDevices.WarningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message;
            var devices = LoadDeviceIfExists();
            if (devices == null)
                return;
            foreach (var device in devices)
            {
                AvailableDevices.AddItems(device);
            }

        }
        private void AvailableDevicesPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //which property that require this engine to refresh
                case nameof(AvailableDevices.ShowManagerToolBar):
                    if (AvailableDevices.ShowManagerToolBar)
                    {
                        AvailableDevices.WarningMessage = adrilight_shared.Properties.Resources.DeviceManager_Delete_Warning_Message;
                    }
                    else
                    {
                        AvailableDevices.WarningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message; ;
                    }
                    break;
            }
        }
        #endregion


        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        public ICommand OpenDevicesFolderCommand { get; set; }
        #endregion





    }
}
