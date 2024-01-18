using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //load profile and playlist
            _collectionItemStore = store;
            _collectionItemStore.SelectedItemChanged += OnSelectedItemChanged;
            _collectionItemStore.SelectedItemsChanged += OnSelectedItemsChanged;
            _collectionItemStore.Navigated += OnCollectionViewNavigated;
            _collectionItemStore.ItemPinStatusChanged += OnCollectionPinStatusChanged;
            _collectionItemStore.ItemsRemoved += OnItemsRemove;
            _dialogService = service;
            LoadNonClientAreaData();
            LoadData();
            CommandSetup();
        }
        #endregion


        #region Events
        private void OnItemsRemove(List<IGenericCollectionItem> list)
        {
            //foreach (var item in list)
            //{
            //    DashboardPinnedItems.RemoveItems(item, false);
            //}
            //AvailableLightingProfiles.ResetSelectionStage();
            //AvailableLightingProfilePlaylists.ResetSelectionStage();
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
  
    

        private void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            //show button if needed
            var nonClientVm = NonClientAreaContent.DataContext as NonClientAreaContentViewModel;
            if (mode == DataViewMode.Detail)
            {
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
            }
            else
            {
                nonClientVm.ShowBackButton = false;
            }

        }
        #endregion


        #region Properties
        public NonClientArea NonClientAreaContent { get; set; }
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
        private LightingProfile _currentPlayingProfile;
        private LightingProfilePlaylist _currentRunningPlaylist;
        public LightingProfile CurrentPlayingProfile {
            get
            {
                return _currentPlayingProfile;
            }
            set
            {
                _currentPlayingProfile = value;
                RaisePropertyChanged();
            }
        }
        public LightingProfilePlaylist CurrentRunningPlaylist {
            get
            {
                return _currentRunningPlaylist;
            }
            set
            {
                _currentRunningPlaylist = value;
                RaisePropertyChanged();
            }
        }
        private int _currentProfileTime;
        public int CurrentProfileTime {
            get
            {
                return _currentProfileTime;
            }
            set
            {
                _currentProfileTime = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Methods
        private void CommandSetup()
        {
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
            foreach (var device in LoadDeviceIfExists())
            {
                AvailableDevices.AddItems(device);
            }

        }
      
        #endregion


        #region Commands
        public ICommand ChangePlaylistProfileDurationCommand { get; set; }
        public ICommand PlaySelectedItemCommand { get; set; }
        public ICommand StopSelectedItemCommand { get; set; }
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        #endregion





    }
}
