using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace adrilight.Extensions
{
    static class BitmapExtension
    {
        public static WriteableBitmap Crop(this WriteableBitmap bmp, Rect region)
        {
            return bmp.Crop((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
        }
    }
}