using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class GifCard : IGifCard
    {
       

        public string Name { get; set; }
        public string Owner { get; set; }

        public string Type { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
    }
}
