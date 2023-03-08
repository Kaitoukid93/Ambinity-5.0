using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    }
}
