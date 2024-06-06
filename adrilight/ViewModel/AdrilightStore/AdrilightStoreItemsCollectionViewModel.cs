using adrilight_shared.Enums;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using GalaSoft.MvvmLight;
using HandyControl.Data;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreItemsCollectionViewModel : ViewModelBase
    {
        public event Action<OnlineItemModel> ItemClicked;
        public event Action<int> PaginationUpdated;
        public event Action StartUpdatingCollection;
        public event Action UpdateCollectionComplete;
        #region Construct
        public AdrilightStoreItemsCollectionViewModel(AdrilightStoreSFTPClient client)
        {
            _client = client;
            CurrentFilter = new StoreFilterModel();
            Items = new ObservableCollection<OnlineItemModel>();
            CommandSetup();
        }
        #endregion
        #region Events
        #endregion
        #region Properties
        private AdrilightStoreSFTPClient _client;
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
        public string NullValueText {
            get
            {
                return _nullValueText;
            }
            set
            {
                _nullValueText = value;
                RaisePropertyChanged();
            }
        }
        private string _nullValueText;
        public StoreFilterModel CurrentFilter { get; set; }
        public ObservableCollection<OnlineItemModel> Items { get; set; }
        private int _paginationCount;
        public int PaginationCount {
            get
            {
                return _paginationCount;
            }
            set
            {
                _paginationCount = value;
                RaisePropertyChanged();
            }
        }
        private int _paginationIndex;
        public int PaginationIndex {
            get
            {
                return _paginationIndex;
            }
            set
            {
                _paginationIndex = value;
                RaisePropertyChanged();
                PaginationUpdated?.Invoke(PaginationIndex);
            }
        }
        private ObservableCollection<StoreFilterModel> _filters;
        public ObservableCollection<StoreFilterModel> Filters {
            get
            {
                return _filters;
            }
            set
            {
                _filters = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region Methods
        public async Task Init(StoreFilterModel filter)
        {
            if (filter == null)
                return;
            CurrentFilter = filter;
            // await UpdateCollectionView(0);

        }
        private void CommandSetup()
        {
            ItemCardClickCommand = new RelayCommand<OnlineItemModel>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ItemClicked?.Invoke(p);

            });
        }
        public async Task UpdateCollectionView(IProgress<int> progress = null)
        {
            //show loading until done
            NullValueText = "";
            StartUpdatingCollection?.Invoke();
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Items?.Clear();
            });
            int pageIndex = PaginationIndex;
            if (pageIndex < 1)
                pageIndex = 1;

            var offSet = (pageIndex - 1) * ItemsPerPage;
            progress.Report(20);
            var (items, totalItems) = await _client.GetStoreItems(CurrentFilter, offSet, ItemsPerPage);
            PaginationCount = totalItems / ItemsPerPage + (totalItems % ItemsPerPage > 0 ? 1 : 0);
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            });
            progress.Report(60);
            foreach (var item in items)
            {
                if (item.AvatarType == OnlineItemAvatarTypeEnum.Image)
                {
                    var thumbPath = item.Path + "/thumb.png";
                    item.Thumb = _client.GetThumb(thumbPath).Result;
                }
            }
            progress.Report(100);
            UpdateCollectionComplete?.Invoke();
            if (items.Count == 0)
                NullValueText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";

        }
        public async Task UpdateCollectionView(HomePageCarouselItem carousel, IProgress<int> progress = null)
        {
            NullValueText = "";
            StartUpdatingCollection?.Invoke();
            //show loading until done
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Items?.Clear();
            });           
            progress.Report(20);
            var items = await _client.GetStoreItems(carousel.EmbeddedURL);
            PaginationCount = 1;
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            });
            progress.Report(60);
            foreach (var item in items)
            {
                if (item.AvatarType == OnlineItemAvatarTypeEnum.Image)
                {
                    var thumbPath = item.Path + "/thumb.png";
                    item.Thumb = _client.GetThumb(thumbPath).Result;
                }
            }
            progress.Report(100);
            UpdateCollectionComplete?.Invoke();
            if (items.Count == 0)
                NullValueText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            //get thumb for items

        }
        #endregion
        #region Icommand
        public ICommand ItemCardClickCommand { get; set; }
        #endregion

    }
}
