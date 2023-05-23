using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace adrilight_content_creator.Converter
{
    public class ThicknessToDoubleConverter : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ThicknessConverter tc = new ThicknessConverter();
            Thickness t = (Thickness)tc.ConvertFrom(value);
            return t;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness t = (Thickness)value;
            return t.Top;
        }
        #endregion
    }
}
