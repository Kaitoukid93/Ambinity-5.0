using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class StoreCategoriesViewModel : ViewModelBase
    {
        public event Action<StoreCategory> SelectedCatergoryChanged;
        public StoreCategoriesViewModel(AdrilightStoreSFTPClient client)
        {
            CommandSetup();
            _client = client;
            AvailableCatergories = new ObservableCollection<StoreCategory>();
        }
        private ObservableCollection<StoreCategory> _availableCatergories;
        public ObservableCollection<StoreCategory> AvailableCatergories {
            get
            {
                return _availableCatergories;
            }
            set
            {
                _availableCatergories = value;
                RaisePropertyChanged();
            }
        }
        private AdrilightStoreSFTPClient _client;

        public async Task Init()
        {
            if (!_client.IsInit)
            {
                await Task.Run(() => _client.Init());
            }
            AvailableCatergories?.Clear();
            foreach (var category in _client.GetStoreCategories())
            {
                AvailableCatergories.Add(category);
            }
        }
        private void CommandSetup()
        {
            CatergoryClickedCommand = new RelayCommand<StoreCategory>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SelectedCatergoryChanged?.Invoke(p);

            });
        }
        public ICommand CatergoryClickedCommand { get; set; }
    }
}
