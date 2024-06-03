using adrilight_shared.Models.Store;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreHomePageViewModel
    {
        public AdrilightStoreHomePageViewModel(AdrilightStoreSFTPClient client)
        {
            _client = client;
            AvailableCarousels = new ObservableCollection<HomePageCarouselItem>();
        }
        private AdrilightStoreSFTPClient _client;
        public ObservableCollection<HomePageCarouselItem> AvailableCarousels { get; set; }
        public async Task Init()
        {
            if (!_client.IsInit)
            {
                await Task.Run(() => _client.Init());
            }
            var carousels = await _client.GetCarousel();
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                AvailableCarousels?.Clear();
                foreach (var item in carousels) { AvailableCarousels.Add(item); }

            });

            foreach (var item in AvailableCarousels)
            {
                await Task.Run(() => LoadCarouselItems(item));
            }

        }
        public async Task LoadCarouselItems(HomePageCarouselItem carousel)
        {

            carousel.CarouselItem = new ObservableCollection<OnlineItemModel>();
            var listItem = await _client.GetStoreItems(carousel.EmbeddedURL.Take(5).ToList());
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                listItem.ForEach(i => carousel.CarouselItem.Add(i));
            });
            foreach (var item in listItem)
            {
                var thumbPath = item.Path + "/thumb.png";
                item.Thumb = _client.GetThumb(thumbPath).Result;
            }

        }
    }
}
