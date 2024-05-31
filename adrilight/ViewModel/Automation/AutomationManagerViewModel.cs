
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
using adrilight.Manager;
using static adrilight.View.AutomationCollectionView;
using static adrilight.View.AutomationEditorView;

namespace adrilight.ViewModel.Dashboard
{
    public class AutomationManagerViewModel : ItemsManagerViewModelBase
    {
        #region Construct
        public AutomationManagerViewModel(
            IList<ISelectablePage> availablePages,
            AutomationEditorViewModel editorViewModel,
            AutomationCollectionViewModel collectionViewModel,
            AutomationManager automationManager)
        {
            SelectablePages = availablePages;
            _automationManager = automationManager;
            _automationEditorViewModel = editorViewModel;
            _automationCollectionViewModel = collectionViewModel;
            _automationManager.NewAutomationAdded += OnNewAutomationAdded;
            _automationCollectionViewModel.AutomationCardClicked += OnAutomationSelected;
            CommandSetup();
        }
        #endregion

        #region Events
        private void OnAutomationSelected(IGenericCollectionItem item) => GotoAutomationEditor(item as AutomationSettings);
        private void OnAutomationHotKeyChanged(IGenericCollectionItem automation)
        {
            _automationManager.Refresh();
            
        }
        private void OnNewAutomationAdded(AutomationSettings automation)
        {
            _automationCollectionViewModel.Init();
        }
        #endregion


        #region Properties
        //private
        private AutomationCollectionViewModel _automationCollectionViewModel;
        private ISelectablePage _selectedPage;
        private AutomationManager _automationManager;
        private AutomationEditorViewModel _automationEditorViewModel;
        private bool _isManagerWindowOpen;

        //public
        private NonClientArea _nonClientAreaContent;
        //public
        public NonClientArea NonClientAreaContent {
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
            var editorView = SelectablePages.Where(p => p is AutomationEditorViewPage).First();
            SelectedPage = editorView;
            ICommand backButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BacktoCollectionView();
            }
            );
            LoadNonClientAreaData("Adrilight  |  Automation Manager | " + automation.Name, "auto", true, backButtonCommand);

        }
        private void BacktoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Automation Manager", "auto", false, null);
            _automationCollectionViewModel.Init();
            var collectionView = SelectablePages.Where(p => p is AutomationCollectionViewPage).First();
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
        public override void LoadData()
        {
            BacktoCollectionView();
        }

        #endregion

        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        #endregion



    }
}
