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

        public ColorPaletteCreatorViewModel()
        {
            Init();
            CommandSetup();

        }
        private SimpleColorPickerViewModel _colorPicker;
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
        private bool _colorPickerIsOpen;
        public bool ColorPickerIsOpen {
            get
            {
                return _colorPickerIsOpen;
            }
            set
            {
                _colorPickerIsOpen = value;
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
        private ObservableCollection<ColorEditingObject> _selectedColors = new ObservableCollection<ColorEditingObject>();
        public ObservableCollection<ColorEditingObject> SelectedColors {
            get
            {
                return _selectedColors;

            }
            set
            {
                _selectedColors = value;
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
                var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(p);
                _colorPicker_ColorChanged(color);



            });
        }
        public void Init()
        {
            //add dummy palette
            NumColor = 16;
            foreach (var color in CreateRainbow())
            {
                //double each color
                Colors.Add(new ColorEditingObject(Color.FromRgb(color.R, color.G, color.B)));
                Colors.Add(new ColorEditingObject(Color.FromRgb(color.R, color.G, color.B)));
            }
            NumColor = Colors.Count;
            Colors.First().IsSelected = true;
            foreach (var color in CreateRecommendColorList(Colors.First().Color))
            {
                RecommendColorList.Add(color);
            }
            SelectedColor = new SolidColorBrush(Colors.First().Color);
            CalculatePaletteWidth();

        }
        private void OpenColorPickerWindow(Border border,Color col)
        {
            _colorPicker = new SimpleColorPickerViewModel();
            _colorPicker.SelectedBrush = new SolidColorBrush(col);
            _colorPicker.ColorChanged += _colorPicker_ColorChanged;
            var picker = SingleOpenHelper.CreateControl<SimpleColorPicker>();
            _pickerWindow = new PopupWindow {
                PopupElement = picker
            };
            _pickerWindow.DataContext = _colorPicker;
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
        private List<System.Drawing.Color> CreateRainbow()
        {
            var colorList = new List<System.Drawing.Color>
             {
                System.Drawing.Color.LightSkyBlue,
                System.Drawing.Color.Red,
                System.Drawing.Color.Yellow,
                System.Drawing.Color.Purple,
                System.Drawing.Color.Orange,
                System.Drawing.Color.Blue,
                System.Drawing.Color.Green
             };

            var orderedColorList = colorList
                .OrderBy(color => color.GetHue())
                .ThenBy(o => o.R * 3 + o.G * 2 + o.B * 1).ToList();
            return orderedColorList;
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
            PaletteWidth = NumColor * 54;
        }
        public ICommand ColorPaletteMouseUpCommand { get; set; }
        public ICommand ColorPaletteMouseDownCommand { get; set; }
        public ICommand ColorPaletteLostFocusCommand { get; set; }
        public ICommand SelectRecommendColorCommand { get; set; }
        public ICommand UpdateColorFromStringCommand { get; set; }
    }
}
