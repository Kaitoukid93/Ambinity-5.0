using adrilight.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    public interface IOnlineItemModel : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Owner { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        string Path { get; set; }
        BitmapImage Thumb { get; set; }
        List<BitmapImage> Screenshots { get; set; }
        string MarkDownDescription { get; set; }
        List<DeviceType> TargetDevices { get; set; }
        bool IsLocalExisted { get; set; }
        string Version { get; set; }
        bool IsDownloading { get; set; }

    }
}
