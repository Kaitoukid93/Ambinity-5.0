using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.Store
{
    public class HomePageCarouselItem : ViewModelBase // this for displaying on the store
    {

        public HomePageCarouselItem()
        {

        }
        private ObservableCollection<OnlineItemModel> _carouselItems;
        public int Order { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> EmbeddedURL { get; set; }
        public string Path { get; set; }
        public int TemplateSelector { get; set; }
        [JsonIgnore]
        public ObservableCollection<OnlineItemModel> CarouselItem { get => _carouselItems; set { Set(() => CarouselItem, ref _carouselItems, value); } }
    }
}
