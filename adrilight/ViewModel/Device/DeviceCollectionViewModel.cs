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
using adrilight.Manager;

namespace adrilight.ViewModel
{
    public class DeviceCollectionViewModel : ViewModelBase
    {
        //raise when item get clicked by user
        public event Action<IGenericCollectionItem> DeviceCardClicked;
        #region Construct
        public DeviceCollectionViewModel(DeviceManager deviceManager)
        {

            _devicemanager = deviceManager;
            CommandSetup();
        }

        #endregion
        ~DeviceCollectionViewModel() { 
        
        
        }

        #region Events
        private void OnItemCheckStatusChanged(IGenericCollectionItem item)
        {
            UpdateTools();
        }
        #endregion

        #region Properties
        public ItemsCollection AvailableDevices { get; set; }
        private DeviceManager _devicemanager;
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
        private string _warningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message;
        public bool ShowToolBar => AvailableTools.Count > 0;
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
        public void Init()
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            AvailableDevices = new ItemsCollection();
            foreach (DeviceSettings device in _devicemanager.AvailableDevices)
            {
                AvailableDevices.AddItem(device);
            }
            AvailableDevices.ItemCheckStatusChanged += OnItemCheckStatusChanged;
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
                       
                        break;
                }
                UpdateTools();
            });
            DeviceCardClickCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            },  (p) =>
            {
                DeviceCardClicked?.Invoke(p);
            });
        }
        private void UpdateTools()
        {
            //clear Tool
            AvailableTools?.Clear();
            var selectedItems = AvailableDevices.Items.Where(d => d.IsChecked).ToList();
            if(selectedItems!=null&& selectedItems.Count>0)
            AvailableTools.Add(DeleteTool());
            RaisePropertyChanged(nameof(ShowToolBar));

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
        public void Dispose()
        {
            AvailableDevices = null;
            AvailableTools = null;
        }
        //posibility to add new device tools
        #endregion
        #region Command
        public ICommand CollectionItemToolCommand { get; set; }
        public ICommand DeviceCardClickCommand { get; set; }
        #endregion

    }
}

