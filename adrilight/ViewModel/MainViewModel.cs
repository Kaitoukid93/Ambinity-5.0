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

namespace adrilight.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Construct
        public MainViewModel(IList<ISelectableViewPart> pages, DashboardViewModel dashboardViewModel, DeviceControlViewModel controlViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _dashboardViewModel.DeviceClicked += OnDeviceClicked;
            _deviceControlViewModel = controlViewModel;
            _deviceControlViewModel.BackToDashboardEvent += BackToDashboard;
            SelectableViewParts = pages;
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
        #endregion

        #region Properties
        private DashboardViewModel _dashboardViewModel;
        private DeviceControlViewModel _deviceControlViewModel;
        private ISelectableViewPart _selectedViewPart;
        public IList<ISelectableViewPart> SelectableViewParts{ get; set; }
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
               

            });
          
        }
        private void Init()
        {
            //show loading screen
            //get default viewpart in general settings
            //show dashboard or map control
            SelectedViewPart = SelectableViewParts.Where(v => v is DashboardViewSelectableViewPart).First();
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

        #endregion

        #region Icommand
        public ICommand WindowClosingCommand { get; set; }
        #endregion

    }
}
