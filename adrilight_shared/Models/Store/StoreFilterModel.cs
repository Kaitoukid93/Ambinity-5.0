using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Store
{
    public class StoreFilterModel : ViewModelBase
    {
        public StoreFilterModel(string name, StoreCategory catergory, string nameFilter)
        {
            Name = name;
            CatergoryFilter = catergory;
            NameFilter = nameFilter;
        }
        private int _pageIndex = 1;
        public StoreFilterModel()
        {

        }
        public string Name { get; set; }
        public int PageIndex { get => _pageIndex; set { Set(() => PageIndex, ref _pageIndex, value); } }
        public string DeviceTypeFilter { get; set; }
        public StoreCategory CatergoryFilter { get; set; }
        public string NameFilter { get; set; }
        public HomePageCarouselItem Carousel { get; set; }
    }
}
