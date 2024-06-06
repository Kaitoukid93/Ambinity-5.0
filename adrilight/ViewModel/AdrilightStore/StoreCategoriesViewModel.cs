using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using GalaSoft.MvvmLight;
using NLog.Filters;
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
        public event Action<StoreFilterModel> FilterChipClicked;
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
        private StoreCategory _selectedCategory;
        public StoreCategory SelectedCategory {
            get
            {
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged();
                if (_selectedCategory != null && _selectedCategory.Name != "Home")
                {
                    _selectedCategory.DefaultFilters = new ObservableCollection<StoreFilterModel>();
                    var downloadedFilter = _client.GetCatergoryFilter(_selectedCategory.OnlineFolderPath + "/filters.json").Result;
                    if (downloadedFilter != null)
                    {
                        foreach (var item in downloadedFilter)
                        {
                            _selectedCategory.DefaultFilters.Add(item);
                        }
                    }
                }

            }
        }
        public void Init()
        {
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
            CatergoryFilterChipClickedCommand = new RelayCommand<StoreFilterModel>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                FilterChipClicked?.Invoke(p);

            });
        }
        public ICommand CatergoryClickedCommand { get; set; }
        public ICommand CatergoryFilterChipClickedCommand { get; set; }
    }
}
