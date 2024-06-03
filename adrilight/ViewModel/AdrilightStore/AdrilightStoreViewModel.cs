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
            StoreCategoriesViewModel categories)
        {
            _searchBar = searchBar;
            _itemsCollection = itemsCollection;
            _storeCategories = categories;
            _homePage = homePage;
            SelectablePages = availablePages;
            _searchBar.SearchContentCommited += OnSearchContentCommited;
            _storeCategories.SelectedCatergoryChanged += OnSelectedCategoryChanged;
            _itemsCollection.ItemClicked += OnItemClicked;
        }
        #region Events
        private void OnSelectedCategoryChanged(StoreCategory catergory)
        {
            GotoCollectionView();
        }
        private void OnItemClicked(OnlineItemModel item)
        {
            GotoItemDetailView(item);
        }
        private void OnSearchContentCommited(string content)
        {
            //change nonclient area text
            GotoCollectionView();
        }
        #endregion
        private SearchBarViewModel _searchBar;
        private StoreCategoriesViewModel _storeCategories;
        private AdrilightStoreItemsCollectionViewModel _itemsCollection;
        private AdrilightStoreHomePageViewModel _homePage;
        private StoreNonClientArea _nonClientAreaContent;
        private ISelectablePage _selectedPage;
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
            base.Dispose();
        }
        public override async void LoadData()
        {
            GotoToHomePageView();
            await _storeCategories.Init();
            //_itemsCollection.Init();
            await _homePage.Init();
        }
        private void GotoToHomePageView()
        {
            LoadNonClientAreaData("Adrilight  |  Store | Home", "onlineStore", false, null);
            var homePageView = SelectablePages.Where(p => p is StoreHomePageViewPage).First();
            //init homepage view?
            SelectedPage = homePageView;
        }
        private void GotoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Store | Items", "onlineStore", true, new RelayCommand<string>((p) =>
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
            LoadNonClientAreaData("Adrilight  |  Store | " + item.Name, "onlineStore", true, new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GotoCollectionView();
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
