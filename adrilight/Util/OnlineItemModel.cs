using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class OnlineItemModel : ViewModelBase,IOnlineItemModel // this for displaying on the store
    {

        public OnlineItemModel()
        {

        }

        public string Name { get; set; }
        public string Owner { get; set; } // the name of creator
        public string Type { get; set; } // ledsetup or color palette
        public string Description { get; set; }
        public string Path { get; set; }
        public BitmapImage Thumb { get; set; }
    }
}
