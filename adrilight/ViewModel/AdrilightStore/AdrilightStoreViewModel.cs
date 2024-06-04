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
        private async void OnSelectedCategoryChanged(StoreCategory catergory)
        {
            _itemsCollection.CurrentFilter = new StoreFilterModel();
            if (catergory.Name == "Home")
            {
                GotoToHomePageView();
                return;
            }
            ProgressBarVisibility = true;
            GotoCollectionView();
            _itemsCollection.CurrentFilter.CatergoryFilter = catergory;
            await Task.Run(() => _itemsCollection.UpdateCollectionView(0, _progress));
            await Task.Delay(500);
            ProgressBarVisibility = false;
        }
        private void OnItemClicked(OnlineItemModel item)
        {
            GotoItemDetailView(item);
        }
        private async void OnSearchContentCommited(string content)
        {
            //change nonclient area text
            ProgressBarVisibility = true;
            GotoCollectionView();
            _itemsCollection.CurrentFilter.NameFilter = content;
            await Task.Run(() => _itemsCollection.UpdateCollectionView(0, _progress));
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
        public bool ProgressBarVisibility {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
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
            base.Dispose();
        }
        public override async void LoadData()
        {
            ProgressBarVisibility = true;
            GotoToHomePageView();
            await _storeCategories.Init();
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
        private void GotoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Store  |  Items", "onlineStore", true, new RelayCommand<string>((p) =>
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
