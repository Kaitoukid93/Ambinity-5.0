using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace adrilight_shared.Models.Store
{
    public class HomePageCarouselItem : ViewModelBase // this for displaying on the store
    {

        public HomePageCarouselItem()
        {

        }
        private BitmapImage _image;
        public string Name { get; set; }
        public string Description { get; set; }
        public string EmbeddedURL { get; set; }
        public string Path { get; set; }
        [JsonIgnore]
        public BitmapImage Image { get => _image; set { Set(() => Image, ref _image, value); } }
    }
}
