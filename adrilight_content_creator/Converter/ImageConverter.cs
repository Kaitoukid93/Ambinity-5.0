using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace adrilight_content_creator.Converter
{
    public class ImageConverter : IValueConverter
    {
        public ImageConverter()
        {
            this.DecodeHeight = -1;
            this.DecodeWidth = -1;
        }

        public int DecodeWidth { get; set; }
        public int DecodeHeight { get; set; }

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var uri = value as Uri;
            if (uri != null)
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.UriSource = uri;
                if (this.DecodeWidth >= 0)
                    source.DecodePixelWidth = this.DecodeWidth;
                if (this.DecodeHeight >= 0)
                    source.DecodePixelHeight = this.DecodeHeight;
                source.EndInit();
                return source;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
