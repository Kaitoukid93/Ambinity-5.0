using adrilight.View;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Stores;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel.Dashboard
{
    public class DashboardViewModel:ViewModelBase
    {
        #region Construct
        public DashboardViewModel(NavigationEvent navigation, DeviceManagerViewModel deviceManager, LightingProfileManagerViewModel lightingProfileManagerViewModel = null)
        {

            _navigation = navigation;
            DeviceManagerViewModel = deviceManager;
            LightingProfileManagerViewModel = lightingProfileManagerViewModel;
        }
        #endregion



        #region Properties
        //private
        private NavigationEvent _navigation;
        //public
        public DeviceManagerViewModel DeviceManagerViewModel { get; set; }
        public LightingProfileManagerViewModel LightingProfileManagerViewModel { get; set; }
        #endregion

        #region Methods
        public void GotoDeviceControl(IDeviceSettings selectedDevice)
        {
            Log.Information("Navigating to Device Control");    
            _navigation.NavigateToDeviceControl(selectedDevice);
            //mainviewmodel will take the rest actions
        }
        #endregion


        #region Commands
        #endregion
    }
}
