using adrilight.Services.DeviceDiscoveryServices;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using adrilight.Manager;
using Task = System.Threading.Tasks.Task;
using adrilight.View;

namespace adrilight.ViewModel
{
    /// <summary>
    /// device manager viewmodel contains device collection  and device advance settings
    /// hence contains DeviceCollectionViewModel and DeviceAdvanceSettingsViewModel
    /// </summary>
    public class DeviceManagerViewModel : ItemsManagerViewModelBase
    {
        #region Construct
        public DeviceManagerViewModel(
            IList<ISelectablePage> availablePages,
            DeviceCollectionViewModel collectionViewModel,
            DeviceManager deviceManager, 
            DeviceDBManager dBManager,
            DeviceAdvanceSettingsViewModel advanceSettingsViewModel)
        {
            SelectablePages = availablePages;
            _deviceAdvanceSettingsViewModel = advanceSettingsViewModel;
            _deviceCollectionViewModel = collectionViewModel;
            _deviceManager = deviceManager;
            _deviceDBManager = dBManager;   
            _deviceHlprs = new DeviceHelpers(); 
            LoadData();
            CommandSetup();
        }

        #endregion

        #region Events
        private async void OnDeviceSelected(IGenericCollectionItem item)
        {
            await Task.Run(() => GotoDeviceSettings(item as DeviceSettings));
        }
        private void OnNewDeviceAdded(IDeviceSettings device)
        {
            _deviceCollectionViewModel.Init();
        }

        #endregion
        #region Properties
        //private
        private DeviceHelpers _deviceHlprs;
        private DeviceCollectionViewModel _deviceCollectionViewModel;
        private DeviceAdvanceSettingsViewModel _deviceAdvanceSettingsViewModel;
        private ISelectablePage _selectedPage;
        private DeviceManager _deviceManager;
        private DeviceDBManager _deviceDBManager;
        private bool _isManagerWindowOpen;
        private NonClientArea _nonClientAreaContent;
        //public
        public NonClientArea NonClientAreaContent {
            get
            {
                return _nonClientAreaContent;
            }
            set
            {
                _nonClientAreaContent = value;
                RaisePropertyChanged();
            }
        }
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
            var loadingScreen = SelectablePages.Where(p => p is DeviceLoadingViewPage).First();
            SelectedPage = loadingScreen;
            //load device info
            await _deviceAdvanceSettingsViewModel.Init(device);
            //show advance settings view
            var advanceView = SelectablePages.Where(p => p is DeviceAdvanceSettingsViewPage).First();
            //(advanceView as DeviceControlView).DataContext = _deviceAdvanceSettingsViewModel;
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
            var collectionView = SelectablePages.Where(p => p is DeviceCollectionViewPage).First();
            _deviceAdvanceSettingsViewModel?.Dispose();
            _deviceCollectionViewModel.Init();
            //(collectionView as DeviceCollectionView).DataContext = _deviceCollectionViewModel;
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
        public override void LoadData()
        {
            //stop discovery service
            _deviceManager.StopDiscoveryService();
            _deviceManager.NewDeviceAdded += OnNewDeviceAdded;
            _deviceCollectionViewModel.DeviceCardClicked += OnDeviceSelected;
            _deviceCollectionViewModel.Init();
            BacktoCollectionView();
        }
        public override void Dispose()
        {
            _deviceManager.NewDeviceAdded -= OnNewDeviceAdded;
            _deviceManager.StartDiscoveryService();
            _deviceCollectionViewModel.DeviceCardClicked -= OnDeviceSelected;
            _deviceCollectionViewModel?.Dispose();
            _deviceAdvanceSettingsViewModel?.Dispose();
            SelectedPage = null;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        public ICommand OpenDevicesFolderCommand { get; set; }
        #endregion





    }
}
