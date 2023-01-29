using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace adrilight.Converter
{
    public class BoolToBorderThicknessConverter : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness t = new Thickness(0.0);
            if((bool)value)
            {
                t = new Thickness(5.0);
            }
             return t;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness t = new Thickness(0.0);
            if ((Thickness)value == t )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
