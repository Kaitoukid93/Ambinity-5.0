using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace adrilight_shared.Converter
{
    public class ImageConverter : IValueConverter
    {
        public ImageConverter()
        {
            DecodeHeight = -1;
            DecodeWidth = -1;
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
                if (DecodeWidth >= 0)
                    source.DecodePixelWidth = DecodeWidth;
                if (DecodeHeight >= 0)
                    source.DecodePixelHeight = DecodeHeight;
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
