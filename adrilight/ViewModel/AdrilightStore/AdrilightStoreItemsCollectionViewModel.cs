using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using GalaSoft.MvvmLight;
using HandyControl.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreItemsCollectionViewModel:ViewModelBase
    {
        public event Action<OnlineItemModel> ItemClicked;
        #region Construct
        public AdrilightStoreItemsCollectionViewModel(AdrilightStoreSFTPClient client, StoreCategoriesViewModel catergories, SearchBarViewModel searchBar)
        {
            _client = client;
            _catergoriesViewModel = catergories;
            _catergoriesViewModel.SelectedCatergoryChanged += OnSelectedCatergoryChanged;
            _searchBarViewModel = searchBar;
            _searchBarViewModel.SearchContentCommited += OnSearchContentCommited;
            CurrentFilter = new StoreFilterModel();
            Items = new ObservableCollection<OnlineItemModel>();
        }
        #endregion
        #region Events
        private async void OnSelectedCatergoryChanged(StoreCategory selectedCatergory)
        {
            CurrentFilter.CatergoryFilter = selectedCatergory;
            await UpdateCollectionView(0);
            //init new filter and update
        }
        private async void OnSearchContentCommited(string content)
        {
            CurrentFilter.NameFilter = content;
            await (UpdateCollectionView(0));
        }
        #endregion
        #region Properties
        private AdrilightStoreSFTPClient _client;
        private StoreCategoriesViewModel _catergoriesViewModel;
        private SearchBarViewModel _searchBarViewModel;
        private int _itemsPerPage = 12;
        public int ItemsPerPage {
            get
            {
                return _itemsPerPage;
            }
            set
            {
                _itemsPerPage = value;
                RaisePropertyChanged();
            }
        }
        public string NullValueText { get; set; }
        public StoreFilterModel CurrentFilter { get; set; }
        public ObservableCollection<OnlineItemModel> Items { get; set; }
        #endregion
        #region Methods
        public async Task Init(StoreFilterModel filter)
        {
            if (filter == null)
                return;
            CurrentFilter = filter;
            await UpdateCollectionView(0);

        }
        private void CommandSetup()
        {
            PaginationUpdateCommand = new RelayCommand<FunctionEventArgs<int>>((p) =>
            {
                return p != null;
            }, async (p) =>
            {
                await UpdateCollectionView(p.Info);

            });
            FilterChipClickCommand = new RelayCommand<StoreFilterModel>((p) =>
            {
                return p != null;
            }, async (p) =>
            {
                CurrentFilter = p;
                await UpdateCollectionView(0);

            });
            ItemCardClickCommand = new RelayCommand<OnlineItemModel>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ItemClicked?.Invoke(p);

            });
        }
        public async Task UpdateCollectionView(int pageIndex)
        {
            //show loading until done
            Items?.Clear();
            NullValueText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            var offSet = (pageIndex - 1) * ItemsPerPage;
            var items = await _client.GetStoreItems(CurrentFilter, offSet, ItemsPerPage);
            foreach (var item in items)
            {
                Items.Add(item);
            }
            //get thumb for items

        }
        #endregion
        #region Icommand
        public ICommand FilterChipClickCommand { get; set; }
        public ICommand PaginationUpdateCommand { get; set; }
        public ICommand ItemCardClickCommand { get; set; }
        #endregion

    }
}
