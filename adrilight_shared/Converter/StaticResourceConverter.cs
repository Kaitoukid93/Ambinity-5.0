﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace adrilight_shared.Converter
{
    public class StaticResourceConverter : MarkupExtension, IValueConverter
    {
        private Control _target;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == string.Empty)
                return _target?.FindResource("binary") ?? Application.Current.FindResource("binary");
            var resourceKey = (string)value;

            return _target?.FindResource(resourceKey) ?? Application.Current.FindResource(resourceKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootObjectProvider == null)
                return this;

            _target = rootObjectProvider.RootObject as Control;
            return this;
        }
    }
}