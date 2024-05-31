using adrilight.Manager;
using adrilight.View;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace adrilight.ViewModel.Dashboard
{
    
    public class DashboardViewModel : ViewModelBase
    {
        #region Construct
        public event Action<IDeviceSettings> DeviceClicked;
        public event Action<string> ManagerButtonClicked;
        public DashboardViewModel(DeviceManager deviceManager, LightingProfileManager profileManager, AutomationManager automationManager)
        {

            _deviceManager = deviceManager;
            _profileManager = profileManager;
            _automationManager = automationManager;
            PinnedDevices = new ObservableCollection<IGenericCollectionItem>();
            PinnedProfileAndPlaylist = new ObservableCollection<IGenericCollectionItem>();
            PinnedAutomation = new ObservableCollection<IGenericCollectionItem>();
            Init();
            CommandSetup();
        }
        #endregion

        #region Events
        #endregion
        #region Properties
        //private
        private DeviceManager _deviceManager;
        private LightingProfileManager _profileManager;
        private AutomationManager _automationManager;
        //public
        public ObservableCollection<IGenericCollectionItem> PinnedDevices { get; set; }
        public ObservableCollection<IGenericCollectionItem> PinnedProfileAndPlaylist { get; set; }
        public ObservableCollection<IGenericCollectionItem> PinnedAutomation { get; set; }

        #endregion

        #region Methods
        public void Init()
        {
            PinnedDevices.Clear();
            PinnedProfileAndPlaylist.Clear();
            PinnedAutomation.Clear();
            foreach (DeviceSettings item in _deviceManager.AvailableDevices)
            {
                    PinnedDevices.Add(item);
            }
            foreach (LightingProfile item in _profileManager.AvailableProfiles)
            {
                if (item.IsPinned)
                    PinnedProfileAndPlaylist.Add(item);
            }
            foreach (LightingProfilePlaylist item in _profileManager.AvailablePlaylists)
            {
                if (item.IsPinned)
                    PinnedProfileAndPlaylist.Add(item);
            }
            foreach (var item in _automationManager.AvailableAutomations)
            {
                if (item.IsPinned)
                    PinnedAutomation.Add(item);
            }
        }
        private void CommandSetup()
        {
            ItemClickCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (p is DeviceSettings)
                {
                    DeviceClicked?.Invoke(p as DeviceSettings);
                }
                else if (p is LightingProfilePlaylist)
                {

                }
                else if (p is LightingProfile)
                {
                    _profileManager.ActivateProfile(p as LightingProfile);
                }
                else if (p is AutomationSettings)
                {
                    _profileManager.ActivatePlaylist(p as LightingProfilePlaylist);
                }


            });
            MangerButtonClickCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ManagerButtonClicked?.Invoke(p);

            });
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Commands
        public ICommand ItemClickCommand { get; set; }
        public ICommand MangerButtonClickCommand { get; set; }
        #endregion
    }
}
