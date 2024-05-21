
using adrilight.ViewModel.Automation;
using adrilight_shared.Models;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.RelayCommand;
using System.Windows;
using System.Collections.Generic;
using adrilight.View;
using adrilight_shared.Models.ItemsCollection;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using adrilight_shared.Settings;
using adrilight_shared.Models.Device;

namespace adrilight.ViewModel.Dashboard
{
    public class AutomationManagerViewModel : ViewModelBase
    {
        #region Construct
        public AutomationManagerViewModel(
            DialogService service,
            AutomationManager automationManager,
            IList<ISelectablePage> availablePages,
            AutomationCollectionViewModel collectionViewModel,
            GeneralSettings generalSettings,
            DeviceManagerViewModel deviceManagerViewModel,
            AutomationExecutor executor,
            AutomationEditorViewModel automationEditingViewModel)
        {
            SelectablePages = availablePages;
            _automationEditorViewModel = automationEditingViewModel;
            _automationCollectionViewModel = collectionViewModel;
            _automationManager = automationManager;
            _generalSettings = generalSettings;
            _deviceManager = deviceManagerViewModel;
            _automationExecutor = executor;
            _deviceManager.NewDeviceAdded += OnNewDeviceAdded;
            _automationEditorViewModel.AutomationHotKeyChanged += OnAutomationHotKeyChanged;
            _automationCollectionViewModel.AutomationCardClicked += OnAutomationSelected;
            _automationManager.HotKeyPressed += OnHotKeyDetected;

            LoadNonClientAreaData("Adrilight  |  Automation Manager", "automationManager", false, null);
            LoadData();
            CommandSetup();
        }



        #endregion

        #region Events
        private async void OnHotKeyDetected(AutomationSettings automation)
        {
            await _automationExecutor.Execute(automation);
        }
        private void OnNewDeviceAdded(IDeviceSettings newDevice)
        {
            if (newDevice == null)
            {
                return;
            }

            var shutdownAutomation = _automationCollectionViewModel.GetShutdownAutomation();
            if (shutdownAutomation!=null)
            {
                shutdownAutomation.Actions.Add(_automationManager.CreateDeviceShutdownAction(newDevice));
            }
            var monitorSleepAutomation = _automationCollectionViewModel.GetMonitorSleepAutomation();
            if (monitorSleepAutomation != null)
            {
                monitorSleepAutomation.Actions.Add(_automationManager.CreateDeviceMonitorSleepAction(newDevice));
            }
            var monitorWakeupAutomation = _automationCollectionViewModel.GetMonitorWakeUpAutomation();
            if (monitorWakeupAutomation != null)
            {
                monitorWakeupAutomation.Actions.Add(_automationManager.CreateDeviceMonitorWakeupAction(newDevice));
            }
        }
        private void OnAutomationSelected(IGenericCollectionItem item) => GotoAutomationEditor(item as AutomationSettings);
        private void OnAutomationHotKeyChanged(IGenericCollectionItem automation)
        {
            _automationManager.Unregister();
            RegisterAllAutomation();


        }
        #endregion


        #region Properties
        //private
        private AutomationCollectionViewModel _automationCollectionViewModel;
        private AutomationEditorViewModel _automationEditorViewModel;
        private ISelectablePage _selectedPage;
        private AutomationManager _automationManager;
        private bool _isManagerWindowOpen;
        private GeneralSettings _generalSettings;
        private DeviceManagerViewModel _deviceManager;
        private AutomationExecutor _automationExecutor;
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
        private void GotoAutomationEditor(IGenericCollectionItem item)
        {
            if (item == null)
            {
                return;
            }
            var automation = item as AutomationSettings;
            _automationEditorViewModel.Init(automation);
            //show advance settings view
            var editorView = SelectablePages.Where(p => p.PageName == "Automation Editor").First();
            (editorView.Content as AutomationEditorView).DataContext = _automationEditorViewModel;
            SelectedPage = editorView;
            ICommand backButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BacktoCollectionView();
            }
            );
            LoadNonClientAreaData("Adrilight  |  Automation Manager | " + automation.Name, "automationManager", true, backButtonCommand);

        }
        private void BacktoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Automation Manager", "automationManager", false, null);
            var collectionView = SelectablePages.Where(p => p.PageName == "Automations Collection").First();
            (collectionView as AutomationCollectionView).DataContext = _automationCollectionViewModel;
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

            var automations = _automationManager.LoadAutomationIfExist();
            if (automations == null)
                return;
            _automationCollectionViewModel.Init(automations);
            if (_generalSettings.HotkeyEnable)
            {
                _automationManager.Start();
                RegisterAllAutomation();
            }

            BacktoCollectionView();
        }
        private void RegisterAllAutomation()
        {
            foreach (var item in _automationCollectionViewModel.AvailableAutomations.Items)
            {
                _automationManager.Register(item as AutomationSettings);
            }
        }
        #endregion

        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        #endregion



    }
}
