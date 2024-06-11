using adrilight.View;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using adrilight_shared.View.CustomControls;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace adrilight.ViewModel.DeviceControl
{
    public class ColorPaletteCreatorViewModel : ViewModelBase
    {
        public event Action<ColorPalette> AddNewPalette;
        public ColorPaletteCreatorViewModel()
        {
            Init();
            CommandSetup();

        }
        private SimpleColorPickerViewModel _colorPicker;
        public SimpleColorPickerViewModel ColorPicker {
            get
            {
                return _colorPicker;
            }
            set
            {
                _colorPicker = value;
                RaisePropertyChanged();
            }
        }
        private Color _eyeDroperColor;
        public Color EyeDropperColor {
            get
            {
                return _eyeDroperColor;
            }
            set
            {
                _eyeDroperColor = value;
                _colorPicker_ColorChanged(_eyeDroperColor);
            }
        }
        private PopupWindow _pickerWindow;
        private int _numColor = 16;
        public int NumColor {
            get
            {
                return _numColor;
            }
            set
            {
                _numColor = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ColorEditingObject> _colors = new ObservableCollection<ColorEditingObject>();
        public ObservableCollection<ColorEditingObject> Colors {
            get
            {
                return _colors;
            }
            set
            {
                _colors = value;
                RaisePropertyChanged();
            }
        }
        private int _paletteWidth = 400;
        public int PaletteWidth {
            get
            {
                return _paletteWidth;
            }
            set
            {
                _paletteWidth = value;
                RaisePropertyChanged();
            }
        }
        private int _tileWidth = 50;
        public int TileWidth {
            get
            {
                return _tileWidth;
            }
            set
            {
                _tileWidth = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<Color> _recommendColorList = new ObservableCollection<Color>();
        public ObservableCollection<Color> RecommendColorList {
            get
            {
                return _recommendColorList;

            }
            set
            {
                _recommendColorList = value;
                RaisePropertyChanged();
            }
        }
  
        private SolidColorBrush _selectedColor;
        public SolidColorBrush SelectedColor {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                RaisePropertyChanged();
            }
        }

        private void CommandSetup()
        {
            ColorPaletteMouseUpCommand = new RelayCommand<MouseButtonEventArgs>((p) =>
            {
                return true;
            }, (p) =>
            {

                var color = ((p.Source as Border).DataContext as ColorEditingObject).Color;
                _colorPicker_ColorChanged(color);
                OpenColorPickerWindow(p.Source as Border, color);
            });
            ColorPaletteMouseDownCommand = new RelayCommand<ColorEditingObject>((p) =>
            {
                return true;
            }, (p) =>
            {
                _pickerWindow?.Close();

            });
            ColorPaletteLostFocusCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                _pickerWindow?.Close();
            });
            SelectRecommendColorCommand = new RelayCommand<Color>((p) =>
            {
                return true;
            }, (p) =>
            {
                UpdateSelectedTilesColor(p);
            });
            UpdateColorFromStringCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if(p==null|| p== string.Empty) return;
                var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(p);
                _colorPicker_ColorChanged(color);
            });
            UpdateColorNumberCommand = new RelayCommand<double>((p) =>
            {
                return p >= 4;
            }, (p) =>
            {
                NumColor = (int)p;
                Init();
            });
            SaveNewPaletteCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                AddNewPalette?.Invoke(CreateNewPaletteFromCurrentSelectedColor());
            });
        }
        private ColorPalette CreateNewPaletteFromCurrentSelectedColor()
        {
            var pal = new ColorPalette(Colors.Count);
            var listCol = new List<Color>();
            foreach (var color in Colors)
            {
                listCol.Add(color.Color);
            }
            pal.Colors = listCol.ToArray();
            return pal;
        }
        public void Init()
        {
            //add dummy palette
            Colors?.Clear();
            foreach (var color in GetHueRainbow(NumColor))
            {
                //double each color
                Colors.Add(new ColorEditingObject(color));
            }
            NumColor = Colors.Count;
            Colors.First().IsSelected = true;
            UpdateRecommendColorList(Colors.First().Color);
            SelectedColor = new SolidColorBrush(Colors.First().Color);
            CalculatePaletteWidth();

        }
        private void OpenColorPickerWindow(Border border, Color col)
        {
            ColorPicker = new SimpleColorPickerViewModel();
            ColorPicker.SelectedBrush = new SolidColorBrush(col);
            ColorPicker.BackColor = new SolidColorBrush(col);
            ColorPicker.ColorChanged += _colorPicker_ColorChanged;
            var picker = SingleOpenHelper.CreateControl<SimpleColorPicker>();
            _pickerWindow = new PopupWindow {
                PopupElement = picker
            };
            _pickerWindow.DataContext = ColorPicker;
            _pickerWindow.Show(border, false);

        }

        private void _colorPicker_ColorChanged(Color color)
        {
            UpdateSelectedTilesColor(color);
            UpdateRecommendColorList(color);
        }
        private void UpdateRecommendColorList(Color color)
        {
            RecommendColorList?.Clear();
            foreach (var c in CreateRecommendColorList(color))
            {
                RecommendColorList.Add(c);
            }
        }
        private void UpdateSelectedTilesColor(Color color)
        {
            SelectedColor = new SolidColorBrush(color);
            foreach (var c in Colors.Where(c => c.IsSelected))
            {
                c.Color = color;
            }
        }
        private List<Color> CreateRecommendColorList(Color color)
        {
            var list = new List<Color>();
            float step = 1.8f / 6f;
            float startFactor = -0.8f;
            for (int i = 0; i < 5; i++)
            {
                var newColor = ChangeColorBrightness(color, startFactor);
                list.Add(newColor);
                startFactor += step;
            }
            return list;
        }
        public static Color FromHsv(double hue, double saturation, double value)
        {
            if (saturation is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (value is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            while (hue < 0) { hue += 360; }
            while (hue >= 360) { hue -= 360; }

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return Color.FromRgb(v, t, p);
                case 1:
                    return Color.FromRgb(q, v, p);
                case 2:
                    return Color.FromRgb(p, v, t);
                case 3:
                    return Color.FromRgb(p, q, v);
                case 4:
                    return Color.FromRgb(t, p, v);
                default:
                    return Color.FromRgb(v, p, q);
            }
        }
        public static IEnumerable<Color> GetHueRainbow(int amount, double hueStart = 0, double huePercent = 1.0,
      double saturation = 1.0, double value = 1.0)
        {
            return Enumerable.Range(0, amount)
                .Select(i => FromHsv(hueStart + 360.0d * huePercent / amount * i, saturation, value));
        }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }
        private void CalculatePaletteWidth()
        {
            if (NumColor > 0)
            {
                TileWidth = 50;
            }
            if (NumColor > 16)
            {
                TileWidth = 25;
            }
            if (NumColor > 32)
            {
                TileWidth = 20;
            }
            PaletteWidth = NumColor * (TileWidth + 4);
        }
        public ICommand ColorPaletteMouseUpCommand { get; set; }
        public ICommand ColorPaletteMouseDownCommand { get; set; }
        public ICommand ColorPaletteLostFocusCommand { get; set; }
        public ICommand SelectRecommendColorCommand { get; set; }
        public ICommand UpdateColorFromStringCommand { get; set; }
        public ICommand UpdateColorNumberCommand { get; set; }
        public ICommand SaveNewPaletteCommand { get; set; }
    }
}
