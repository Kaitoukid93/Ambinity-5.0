using adrilight.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
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
        List<DeviceTypeDataEnum> TargetDevices { get; set; }

    }
}
