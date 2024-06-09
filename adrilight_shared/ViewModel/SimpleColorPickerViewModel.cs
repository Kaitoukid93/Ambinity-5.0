using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.RelayCommand;

namespace adrilight_shared.ViewModel
{
    public class SimpleColorPickerViewModel:ViewModelBase
    {
        private readonly List<ColorRange> _colorRangeList = new List<ColorRange>
    {
        new ColorRange
        {
            Start = Color.FromRgb(byte.MaxValue, 0, 0),
            End = Color.FromRgb(byte.MaxValue, 0, byte.MaxValue)
        },
        new ColorRange
        {
            Start = Color.FromRgb(byte.MaxValue, 0, byte.MaxValue),
            End = Color.FromRgb(0, 0, byte.MaxValue)
        },
        new ColorRange
        {
            Start = Color.FromRgb(0, 0, byte.MaxValue),
            End = Color.FromRgb(0, byte.MaxValue, byte.MaxValue)
        },
        new ColorRange
        {
            Start = Color.FromRgb(0, byte.MaxValue, byte.MaxValue),
            End = Color.FromRgb(0, byte.MaxValue, 0)
        },
        new ColorRange
        {
            Start = Color.FromRgb(0, byte.MaxValue, 0),
            End = Color.FromRgb(byte.MaxValue, byte.MaxValue, 0)
        },
        new ColorRange
        {
            Start = Color.FromRgb(byte.MaxValue, byte.MaxValue, 0),
            End = Color.FromRgb(byte.MaxValue, 0, 0)
        }
    };
        public SimpleColorPickerViewModel()
        {
            CommandSetup();
        }
        private Brush _backColor = Brushes.Red;
        public Brush BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                RaisePropertyChanged();
            }
        }
        private void CommandSetup()
        {
            ColorPickerSliderValueChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<double>>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                UpdatebackColor(p);

            });

        }
        private void UpdatebackColor(RoutedPropertyChangedEventArgs<double> e)
        {
            int num = Math.Min(5, (int)Math.Floor(e.NewValue));
            double range = e.NewValue - (double)num;
            Color color = _colorRangeList[num].GetColor(range);
            BackColor = new SolidColorBrush(color);
        }
        public ICommand ColorPickerSliderValueChangedCommand { get; set; }
    }
}
