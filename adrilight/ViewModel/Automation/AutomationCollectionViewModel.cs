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

namespace adrilight.ViewModel.Automation
{
    public class AutomationCollectionViewModel : ViewModelBase
    {
        //raise when item get clicked by user
        public event Action<IGenericCollectionItem> AutomationCardClicked;
        #region Construct
        public AutomationCollectionViewModel()
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            AvailableAutomations = new ItemsCollection();
            CommandSetup();
        }

        #endregion



        #region Properties
        public ItemsCollection AvailableAutomations { get; set; }
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
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
        public void Init(List<AutomationSettings> automations)
        {
            AvailableAutomations.Items.Clear();
            foreach (var automation in automations)
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
        public AutomationSettings GetShutdownAutomation() => AvailableAutomations.Items.Where(
                a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
            && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.Shutdown)
                .FirstOrDefault()
                as AutomationSettings;
        public AutomationSettings GetMonitorSleepAutomation() => AvailableAutomations.Items.Where(
               a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
           && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorSleep)
               .FirstOrDefault()
               as AutomationSettings;
        public AutomationSettings GetMonitorWakeUpAutomation() => AvailableAutomations.Items.Where(
               a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
           && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorWakeup)
               .FirstOrDefault()
               as AutomationSettings;
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
        #endregion

    }
}

