
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

namespace adrilight.ViewModel.Dashboard
{
    public class AutomationManagerViewModel : ViewModelBase
    {
        #region Construct
        public AutomationManagerViewModel(CollectionItemStore store, DialogService service)
        {

            _collectionItemStore = store;
            _dialogService = service;
            LoadNonClientAreaData();
            CommandSetup();
        }
        #endregion



        #region Properties
        //private
        private CollectionItemStore _collectionItemStore;
        private DialogService _dialogService;
        //public
        public DeviceManagerViewModel DeviceManagerViewModel { get; set; }
        public NonClientArea NonClientAreaContent { get; set; }
        public LightingProfileManagerViewModel LightingProfileManagerViewModel { get; set; }
        public AutomationEditingViewModel CurrentAutomation { get; set; }
        public DataCollection AvailableAutomations { get; set; }
        #endregion

        #region Methods
        private void CommandSetup()
        {

        }
        private void LoadNonClientAreaData()
        {
            var vm = new NonClientAreaContentViewModel("Adrilight  |  Automation Manager", "automation");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }
        private async void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            //show button if needed
            if (item == null)
            {
                return;
            }
            if (CurrentAutomation == null)
                CurrentAutomation = new AutomationEditingViewModel(_dialogService, item as AutomationSettings);
            var nonClientVm = NonClientAreaContent.DataContext as NonClientAreaContentViewModel;
            if (mode == DataViewMode.Loading)
            {
                nonClientVm.ShowBackButton = true;
                nonClientVm.BackButtonCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, (p) =>
                {
                    _collectionItemStore.BackToCollectionView(item);
                }
                );
                AvailableAutomations.GotoCurrentItemDetailViewCommand.Execute(CurrentAutomation);
                nonClientVm.Header = "Adrilight  |  Device Manager | " + CurrentDevice.Device.DeviceName;
            }
            else if (mode == DataViewMode.Detail)
            {
                nonClientVm.ShowBackButton = true;
            }
            else
            {
                nonClientVm.ShowBackButton = false;
                nonClientVm.Header = "Adrilight  |  Device Manager";
                CurrentDevice = null;
            }

        }
        #endregion


        #region Commands
        #endregion
    }
}
