using adrilight.View;
using adrilight.ViewModel.Dashboard;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Stores;
using GalaSoft.MvvmLight;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using adrilight_shared.Models.RelayCommand;
using static adrilight.View.DashboardView;
using static adrilight.View.DeviceControlView;
using System;
using adrilight.Manager;
using HandyControl.Controls;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using adrilight_shared.Settings;
using System.Threading.Tasks;
using adrilight.Services.DeviceDiscoveryServices;

namespace adrilight.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Construct
        public MainViewModel(IList<ISelectableViewPart> pages,
            GeneralSettings generalSettings,
            DashboardViewModel dashboardViewModel,
            DeviceDiscovery deviceDiscoveryService,
            DeviceControlViewModel controlViewModel)
        {
            _generalSettings = generalSettings;
            _dashboardViewModel = dashboardViewModel;
            _dashboardViewModel.DeviceClicked += OnDeviceClicked;
            _dashboardViewModel.ManagerButtonClicked += OnManageButtonClicked;
            _deviceControlViewModel = controlViewModel;
            _deviceControlViewModel.BackToDashboardEvent += BackToDashboard;
            _deviceDiscoveryService = deviceDiscoveryService;
            SelectableViewParts = pages;
            LoadNonClientAreaData();
            CommandSetup();
            Init();
        }
        #endregion

        ~MainViewModel()
        {

        }
        #region Events
        private void OnDeviceClicked(IDeviceSettings device)
        {
            //go to device
            GoToDevieControl(device);
        }
        private void ManagerWindow_Closed(object sender, EventArgs e)
        {
            var wndw = (Window)sender;
            var dtcntx = wndw.DataContext as ItemsManagerViewModelBase;
            dtcntx?.Dispose();
        }

        private void OnManageButtonClicked(string value)
        {
            switch (value)
            {
                case "deviceManager":
                     OpenManagerWindow(new DeviceManagerWindow());
                    break;
                case "profileManager":
                     OpenManagerWindow(new LightingProfileManagerWindow());
                    break;
                case "automationManager":
                     OpenManagerWindow(new AutomationManagerWindow());
                    break;
            }
        }
        #endregion

        #region Properties
        private DashboardViewModel _dashboardViewModel;
        private DeviceControlViewModel _deviceControlViewModel;
        private ISelectableViewPart _selectedViewPart;
        private GeneralSettings _generalSettings;
        private NonClientAreaContent _nonClientAreaContent;
        private DeviceDiscovery _deviceDiscoveryService;
        //public
        public NonClientAreaContent NonClientAreaContent {
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
        public IList<ISelectableViewPart> SelectableViewParts { get; set; }
        public ISelectableViewPart SelectedViewPart {
            get => _selectedViewPart;

            set
            {
                Set(ref _selectedViewPart, value);
            }
        }
        #endregion

        #region Methods
        private void CommandSetup()
        {
            WindowClosingCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SelectedViewPart = null;

            });
            OpenMainWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                Init();

            });
            OpenAdrilightStoreWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                 OpenManagerWindow(new AmbinoOnlineStoreView());

            });
        }
        private void Init()
        {
            //show loading screen
            //get default viewpart in general settings
            //show dashboard or map control
            SelectedViewPart = SelectableViewParts.Where(v => v is DashboardViewSelectableViewPart).First();
            _deviceDiscoveryService?.Start();
        }
        private void BackToDashboard()
        {
            _deviceControlViewModel?.Dispose();
            SelectedViewPart = SelectableViewParts.Where(v => v is DashboardViewSelectableViewPart).First();
        }
        private void GoToDevieControl(IDeviceSettings device)
        {
            Log.Information("Navigating to Device Control");
            _deviceControlViewModel?.Init(device);
            SelectedViewPart = SelectableViewParts.Where(v => v is DeviceControlViewSelectableViewPart).First();
        }
        public void Dispose()
        {
            _deviceControlViewModel?.Dispose();
            _dashboardViewModel?.Dispose();
        }
        private void OpenManagerWindow(Window window)
        {
            //stop discovery service
            window.Owner = App.Current.MainWindow;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            window.Closed += ManagerWindow_Closed;
            window.Show();
            var dtcntx = window.DataContext as ItemsManagerViewModelBase;
            dtcntx?.LoadData();
            
        }
        private void LoadNonClientAreaData()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientAreaContent(_generalSettings.AppCulture.Culture);
            });

        }
        #endregion

        #region Icommand
        public ICommand WindowClosingCommand { get; set; }
        public ICommand OpenMainWindowCommand { get; set; }
        public ICommand OpenAdrilightStoreWindowCommand { get; set; }
        #endregion

    }
}
