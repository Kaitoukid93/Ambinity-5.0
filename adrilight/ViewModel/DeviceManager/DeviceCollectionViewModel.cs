using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using System.Windows.Documents;
using System.Collections.Generic;
using System;

namespace adrilight.ViewModel.DeviceManager
{
    public class DeviceCollectionViewModel : ViewModelBase
    {
        //raise when item get clicked by user
        public event Action<IGenericCollectionItem> DeviceCardClicked;
        #region Construct
        public DeviceCollectionViewModel()
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            AvailableDevices = new ItemsCollection();
            CommandSetup();
        }

        #endregion



        #region Properties
        public ItemsCollection AvailableDevices { get; set; }
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
        public DeviceAdvanceSettingsViewModel CurrentDevice { get; set; }
        private string _warningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message;
        public string WarningMessage {
            get
            {
                return _warningMessage;
            }
            set
            {
                _warningMessage = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods
        public void Init(List<DeviceSettings> devices)
        {
            AvailableDevices.Items.Clear();
            foreach(var device in devices)
            {
                AvailableDevices.AddItem(device);
            }
        }
        private void CommandSetup()
        {
            CollectionItemToolCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                switch (p)
                {
                    case "delete":
                        AvailableDevices.RemoveSelectedItems(true);
                        break;
                }
                UpdateTools();
            });
            DeviceCardClickCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, async (p) =>
            {
                DeviceCardClicked?.Invoke(p);
            });
        }
        private void UpdateTools()
        {
            //clear Tool
            AvailableTools?.Clear();
            var selectedItems = AvailableDevices.Items.Where(d => d.IsSelected).ToList();
            if (selectedItems == null)
                return;
            if (selectedItems.Count == 0)
                return;
            AvailableTools.Add(DeleteTool());

        }
        private CollectionItemTool DeleteTool()
        {
            return new CollectionItemTool() {
                Name = "Delete",
                ToolTip = "Delete Selected Items",
                Geometry = "remove",
                CommandParameter = "delete"

            };
        }
        //posibility to add new device tools
        #endregion
        #region Command
        public ICommand CollectionItemToolCommand { get; set; }
        public ICommand DeviceCardClickCommand { get; set; }
        #endregion

    }
}

