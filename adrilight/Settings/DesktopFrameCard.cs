using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace adrilight.Settings
{
    public class DesktopFrameCard
    {
        public DesktopFrameCard() { }
        public string Name { get; set; }
        public string Size { get; set; }
        public WriteableBitmap Bitmap { get; set; }
    }
}
