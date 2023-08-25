using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Preview
{
    public class GifCard : ViewModelBase
    {
        private string _path;
        public GifCard() { }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Path { get => _path; set { Set(() => Path, ref _path, value); } }
    }
}
