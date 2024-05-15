using adrilight.View;
using adrilight.ViewModel.DeviceManager;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Microsoft.Win32.TaskScheduler;
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
        public DeviceManagerViewModel(
            DialogService service,
            DeviceDBManager deviceDBManager,
            IList<ISelectablePage> availablePages,
            DeviceCollectionViewModel collectionViewModel,
            DeviceAdvanceSettingsViewModel advanceSettingsViewModel)
        {
            SelectablePages = availablePages;
            _deviceAdvanceSettingsViewModel = advanceSettingsViewModel;
            _deviceCollectionViewModel = collectionViewModel;
            _deviceHlprs = new DeviceHelpers();
            _deviceDBManager = deviceDBManager;

            _deviceCollectionViewModel.DeviceCardClicked += OnDeviceSelected;
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
