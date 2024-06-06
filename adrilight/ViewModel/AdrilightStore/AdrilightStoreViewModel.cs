using adrilight.View;
using adrilight.View.Screens.Store;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using static adrilight.View.OnlineItemDetailView;
using static adrilight.View.Screens.Store.StoreItemsCollectionView;
using static adrilight.View.StoreHomePageView;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreViewModel : ItemsManagerViewModelBase
    {
        #region Construct

        #endregion
        public AdrilightStoreViewModel(IList<ISelectablePage> availablePages,
            SearchBarViewModel searchBar,
            AdrilightStoreHomePageViewModel homePage,
            AdrilightStoreItemsCollectionViewModel itemsCollection,
            AdrilightStoreSFTPClient client,
            StoreCategoriesViewModel categories)
        {
            _searchBar = searchBar;
            _itemsCollection = itemsCollection;
            _storeCategories = categories;
            _homePage = homePage;
            SelectablePages = availablePages;
            _client = client;
        }




        #region Events
        private async void OnFilterChipClicked(StoreFilterModel filter)
        {
            ProgressBarVisibility = true;
            GotoCollectionView("Search " + "[" + filter.Name + "]");
            _itemsCollection.CurrentFilter = filter;
            _itemsCollection.PaginationIndex = 1;
            await Task.Run(() => _itemsCollection.UpdateCollectionView(_progress));
            await Task.Delay(500);
            ProgressBarVisibility = false;
        }
        private void _itemsCollection_UpdateCollectionComplete()
        {
            IsBusy = false;
            _searchBar.IsEnabled = true;
        }
        private void _itemsCollection_StartUpdatingCollection()
        {
            IsBusy = true;
            _searchBar.IsEnabled = false;
        }
        private async void _homePage_SeeAllButtonClicked(HomePageCarouselItem carousel)
        {
            ProgressBarVisibility = true;
            GotoCollectionView(carousel.Name);
            if (!_client.IsInit)
                _client.Init();
            try
            {
                await Task.Run(() => _itemsCollection.UpdateCollectionView(carousel, _progress));
            }
            catch (Exception ex)
            {
                _client.Dispose();
            }
            await Task.Delay(500);
            ProgressBarVisibility = false;
        }
        private async void OnPaginationUpdated(int pageIndex)
        {
            ProgressBarVisibility = true;
            // _itemsCollection.PaginationIndex = pageIndex;
            if (!_client.IsInit)
                _client.Init();
            try
            {
                await Task.Run(() => _itemsCollection.UpdateCollectionView(_progress));
            }
            catch (Exception ex)
            {
                _client.Dispose();
            }
            await Task.Delay(500);
            ProgressBarVisibility = false;
        }
        private void OnSelectedCategoryChanged(StoreCategory catergory)
        {
            _itemsCollection.CurrentFilter = new StoreFilterModel();
            if (catergory.Name == "Home")
            {
                GotoToHomePageView();
                return;
            }
            GotoCollectionView(catergory.Name);
            _itemsCollection.CurrentFilter.CatergoryFilter = catergory;
            _itemsCollection.PaginationIndex = 1;

        }
        private void OnItemClicked(OnlineItemModel item)
        {
            GotoItemDetailView(item);
        }
        private async void OnSearchContentCommited(string content)
        {
            //change nonclient area text
            ProgressBarVisibility = true;
            GotoCollectionView("Search " + "[" + content + "]");
            _itemsCollection.CurrentFilter.NameFilter = content;
            _itemsCollection.PaginationIndex = 1;
            await Task.Run(() => _itemsCollection.UpdateCollectionView(_progress));
            await Task.Delay(500);
            ProgressBarVisibility = false;
        }
        #endregion
        private SearchBarViewModel _searchBar;
        private StoreCategoriesViewModel _storeCategories;
        private AdrilightStoreItemsCollectionViewModel _itemsCollection;
        private AdrilightStoreHomePageViewModel _homePage;
        private StoreNonClientArea _nonClientAreaContent;
        private ISelectablePage _selectedPage;
        private IProgress<int> _progress;
        private bool _progressBarVisibility;
        private AdrilightStoreSFTPClient _client;
        private bool _isBusy;
        public bool IsBusy {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }
        public bool ProgressBarVisibility {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                if (!value)
                {
                    CurrentProgressBarValue = 0;
                }
                RaisePropertyChanged();
            }
        }
        private int _currentProgressBarValue;
        public int CurrentProgressBarValue {
            get
            {
                return _currentProgressBarValue;
            }
            set
            {
                _currentProgressBarValue = value;
                RaisePropertyChanged();
            }
        }
        public IList<ISelectablePage> SelectablePages { get; set; }
        //public
        public ISelectablePage SelectedPage {
            get => _selectedPage;

            set
            {
                Set(ref _selectedPage, value);
            }
        }

        public StoreNonClientArea NonClientAreaContent {
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
        public override void Dispose()
        {
            _searchBar.SearchContentCommited -= OnSearchContentCommited;
            _storeCategories.SelectedCatergoryChanged -= OnSelectedCategoryChanged;
            _itemsCollection.ItemClicked -= OnItemClicked;
            _itemsCollection.PaginationUpdated -= OnPaginationUpdated;
            _homePage.SeeAllButtonClicked -= _homePage_SeeAllButtonClicked;
            _homePage.ItemClicked -= OnItemClicked;
            _client.Dispose();
            base.Dispose();
        }
        public override async void LoadData()
        {
            _searchBar.SearchContentCommited += OnSearchContentCommited;
            _storeCategories.SelectedCatergoryChanged += OnSelectedCategoryChanged;
            _storeCategories.FilterChipClicked += OnFilterChipClicked;
            _itemsCollection.ItemClicked += OnItemClicked;
            _itemsCollection.PaginationUpdated += OnPaginationUpdated;
            _itemsCollection.StartUpdatingCollection += _itemsCollection_StartUpdatingCollection;
            _itemsCollection.UpdateCollectionComplete += _itemsCollection_UpdateCollectionComplete;
            _homePage.SeeAllButtonClicked += _homePage_SeeAllButtonClicked;
            _homePage.ItemClicked += OnItemClicked;
            ProgressBarVisibility = true;
            GotoToHomePageView();
            if (!_client.IsInit)
            {
                await Task.Run(() => _client.Init());
            }
            _storeCategories.Init();
            _storeCategories.SelectedCategory = _storeCategories.AvailableCatergories.First();
            //_itemsCollection.Init();
            _progress = new Progress<int>(percent =>
            {
                CurrentProgressBarValue = percent;
            });
            await _homePage.Init(_progress);
            await Task.Delay(500);
            ProgressBarVisibility = false;

        }
        private void GotoToHomePageView()
        {
            LoadNonClientAreaData("Adrilight  |  Store  |  Home", "onlineStore", false, null);
            var homePageView = SelectablePages.Where(p => p is StoreHomePageViewPage).First();
            //init homepage view?
            SelectedPage = homePageView;
        }
        private void GotoCollectionView(string currentHeader)
        {
            LoadNonClientAreaData("Adrilight  |  Store  |  " + currentHeader, "onlineStore", true, new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GotoToHomePageView();
            }));
            var collectionView = SelectablePages.Where(p => p is StoreItemsCollectionViewPage).First();
            //init homepage view?
            SelectedPage = collectionView;
        }
        private void GotoItemDetailView(OnlineItemModel item)
        {
            LoadNonClientAreaData("Adrilight  |  Store  |  " + item.Name, "onlineStore", true, new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GotoCollectionView("Items");
            }));
            var detailView = SelectablePages.Where(p => p is OnlineItemDetailViewPage).First();
            //init detailView
            SelectedPage = detailView;
        }
        private void LoadNonClientAreaData(string content, string geometry, bool showBackButton, ICommand buttonCommand)
        {
            var vm = new NonClientAreaContentViewModel(content, geometry, showBackButton, buttonCommand);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new StoreNonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }

    }
}
