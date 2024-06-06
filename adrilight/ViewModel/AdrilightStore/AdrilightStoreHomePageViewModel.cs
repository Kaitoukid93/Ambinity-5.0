using adrilight_shared.Enums;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreHomePageViewModel
    {
        public event Action<OnlineItemModel> ItemClicked;
        public event Action<HomePageCarouselItem> SeeAllButtonClicked;
        public event Action<OnlineItemModel> ItemDownloadButtonClicked;
        public AdrilightStoreHomePageViewModel(AdrilightStoreSFTPClient client)
        {
            _client = client;
            AvailableCarousels = new ObservableCollection<HomePageCarouselItem>();
            CommandSetup();
        }
        private AdrilightStoreSFTPClient _client;
        public ObservableCollection<HomePageCarouselItem> AvailableCarousels { get; set; }
        public async Task Init(IProgress<int> progress = null)
        {
            int value = 0;
            if (!_client.IsInit)
            {
                await Task.Run(() => _client.Init());
            }
            value = 20;
            progress.Report(value);
            var carousels = await _client.GetCarousel();
            value = 40;
            progress.Report(value);
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                AvailableCarousels?.Clear();
                foreach (var item in carousels) { AvailableCarousels.Add(item); }

            });

            foreach (var item in AvailableCarousels)
            {
                value += (60 / AvailableCarousels.Count);
                await Task.Run(() => LoadCarouselItems(item, progress));
                progress.Report(value);
            }

        }
        public async Task LoadCarouselItems(HomePageCarouselItem carousel, IProgress<int> progress = null)
        {

            carousel.CarouselItem = new ObservableCollection<OnlineItemModel>();
            var listItem = await _client.GetStoreItems(carousel.EmbeddedURL.Take(5).ToList());
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                listItem.ForEach(i => carousel.CarouselItem.Add(i));
            });
            foreach (var item in listItem)
            {
                if (item.AvatarType == OnlineItemAvatarTypeEnum.Image)
                {
                    var thumbPath = item.Path + "/thumb.png";
                    item.Thumb = _client.GetThumb(thumbPath).Result;
                }
            }

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
            SeAllButtonClickCommand = new RelayCommand<HomePageCarouselItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SeeAllButtonClicked?.Invoke(p);

            });
            ItemDownloadButtonClickCommand = new RelayCommand<OnlineItemModel>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ItemDownloadButtonClicked?.Invoke(p);

            });
        }
        public ICommand ItemCardClickCommand { get; set; }
        public ICommand ItemDownloadButtonClickCommand { get; set; }
        public ICommand SeAllButtonClickCommand { get; set; }
    }
}
