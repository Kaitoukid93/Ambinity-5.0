using GalaSoft.MvvmLight;
using System.Windows.Media.Imaging;

namespace adrilight.Settings
{
    public class DesktopFrameCard : ViewModelBase
    {
        private WriteableBitmap _bitmap;
        public DesktopFrameCard() { }
        public string Name { get; set; }
        public string Size { get; set; }
        public WriteableBitmap Bitmap { get => _bitmap; set { Set(() => Bitmap, ref _bitmap, value); } }
    }
}
