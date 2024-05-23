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
using adrilight_shared.Models.Automation;
using adrilight_shared.ViewModel;
using adrilight_shared.Services;
using adrilight.Manager;

namespace adrilight.ViewModel.Automation
{
    public class AutomationCollectionViewModel : ViewModelBase
    {
        //raise when item get clicked by user
        public event Action<IGenericCollectionItem> AutomationCardClicked;
        #region Construct
        public AutomationCollectionViewModel(DialogService dialogSerivce, AutomationManager manager)
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            _dialogService = dialogSerivce;
            _automationManager = manager;
            AvailableAutomations = new ItemsCollection();
            CommandSetup();
        }
        #endregion

        #region Events

        #endregion


        #region Properties
        public ItemsCollection AvailableAutomations { get; set; }
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
        private DialogService _dialogService;
        private AutomationManager _automationManager;
        private string _warningMessage;
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
            AvailableAutomations.Items.Clear();
            foreach (var automation in _automationManager.AvailableAutomations)
            {
                AvailableAutomations.AddItem(automation);
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
                        AvailableAutomations.RemoveSelectedItems(true);
                        break;
                }
                UpdateTools();
            });
            AutomationCardClickCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, async (p) =>
            {
                AutomationCardClicked?.Invoke(p);
            });
            OpenCreateNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var vm = new AddNewDialogViewModel(adrilight_shared.Properties.Resources.AddNew, "New Playlist", null);
                _dialogService.ShowDialog<AddNewDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        //tell automation manager to add new automation
                        _automationManager.AddNewAutomation(vm.Content);
                    }

                }, vm);
            });
        }
        private void UpdateTools()
        {
            //clear Tool
            AvailableTools?.Clear();
            var selectedItems = AvailableAutomations.Items.Where(d => d.IsSelected).ToList();
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
        public ICommand AutomationCardClickCommand { get; set; }
        public ICommand OpenCreateNewAutomationCommand { get; set; }
        #endregion

    }
}

