using adrilight.View;
using adrilight.View.Screens.Mainview.ControlView.Parameters;
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
    public class ColorEditorViewModel : ViewModelBase
    {
        public event Action<ColorPalette> AddNewPalette;
        public ColorEditorViewModel()
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

        private bool _isSolidColorChecked = true;
        public bool IsSolidColorChecked {
            get
            {
                return _isSolidColorChecked;
            }
            set
            {
                _isSolidColorChecked = value; RaisePropertyChanged();
                if (value)
                {
                    if (ColorPicker == null)
                    {
                        ColorPicker = new SimpleColorPickerViewModel();
                    }
                    ColorPicker.ColorChanged += _colorPicker_ColorChanged;
                    ColorPicker.BackColorChanged += UpdateRecommendColorList;
                    ColorPicker.SelectedBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    ColorPicker.BackColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    SelectedColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    CurrentColorEditMode = new SolidColorEditView();
                    CurrentColorEditMode.DataContext = this;
                }
            }
        }
        private bool _isGradientColorChecked = false;
        public bool IsGradientColorChecked {
            get
            {
                return _isGradientColorChecked;
            }
            set
            {
                _isGradientColorChecked = value; RaisePropertyChanged();
                if (value)
                {
                    CurrentColorEditMode = new GradientEditView();
                    CurrentColorEditMode.DataContext = this;
                    SelectedColor = new LinearGradientBrush {
                        GradientStops = { new GradientStop(Color.FromRgb(0, 255, 0), 0) ,
                          new GradientStop(Color.FromRgb(255, 0, 0), 1) },

                    };
                }
            }
        }
        private UserControl _currentColorEditMode;
        public UserControl CurrentColorEditMode {
            get
            {
                return _currentColorEditMode;
            }
            set
            {
                _currentColorEditMode = value; RaisePropertyChanged();
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

        private Brush _selectedColor;
        public Brush SelectedColor {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                RaisePropertyChanged();
            }
        }

        private void CommandSetup()
        {

            PickerLostFocusCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                _pickerWindow?.Close();
            });


            SaveNewColorCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

            });
            SelectRecommendColorCommand = new RelayCommand<Color>((p) =>
            {
                return true;
            }, (p) =>
            {
                ColorPicker.SelectedBrush = new SolidColorBrush(p);
                SelectedColor = new SolidColorBrush(p);
            });
            GradientButtonClickCommand = new RelayCommand<MouseEventArgs>((p) =>
            {
                return true;
            }, (p) =>
            {

                if ((p.Source as Button).Name == "StartBtn")
                    OpenColorPickerWindow(p.Source as Button, (SelectedColor as LinearGradientBrush).GradientStops[0].Color);
                else if ((p.Source as Button).Name == "StopBtn")
                    OpenColorPickerWindow(p.Source as Button, (SelectedColor as LinearGradientBrush).GradientStops[1].Color);
            });

        }

        public void Init()
        {
            //add dummy palette
            UpdateRecommendColorList(Color.FromRgb(0, 255, 0));
            IsSolidColorChecked = true;
        }
        private void OpenColorPickerWindow(Button button, Color col)
        {
            _pickerWindow?.Close();
            ColorPicker = new SimpleColorPickerViewModel();
            ColorPicker.SelectedBrush = new SolidColorBrush(col);
            ColorPicker.BackColor = new SolidColorBrush(col);
            if (button.Name == "StartBtn")
                ColorPicker.ColorChanged += UpdateGradientStartColor;
            else if (button.Name == "StopBtn")
                ColorPicker.ColorChanged += UpdateGradientEndColor;
            var picker = SingleOpenHelper.CreateControl<SimpleColorPicker>();
            _pickerWindow = new PopupWindow {
                PopupElement = picker
            };
            _pickerWindow.DataContext = ColorPicker;
            _pickerWindow.Show(button, false);
        }
        private void _colorPicker_ColorChanged(Color color)
        {
            UpdateTargetColor(color);

        }
        private void UpdateGradientEndColor(Color color)
        {
            (SelectedColor as LinearGradientBrush).GradientStops[1].Color = color;
        }
        private void UpdateGradientStartColor(Color color)
        {
            (SelectedColor as LinearGradientBrush).GradientStops[0].Color = color;
        }
        private void UpdateTargetColor(Color color)
        {
            if (IsSolidColorChecked)
            {
                SelectedColor = new SolidColorBrush(color);
            }
            else
            {

            }
        }
        private void UpdateRecommendColorList(Color color)
        {
            RecommendColorList?.Clear();
            foreach (var c in CreateRecommendColorList(color))
            {
                RecommendColorList.Add(c);
            }
        }

        private List<Color> CreateRecommendColorList(Color color)
        {
            var list = new List<Color>();
            float step = 1.9f / 13f;
            float startFactor = -0.90f;
            for (int i = 0; i < 12; i++)
            {
                var newColor = ChangeColorBrightness(color, startFactor);
                list.Add(newColor);
                startFactor += step;
            }
            return list;
        }
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
        public ICommand PickerLostFocusCommand { get; set; }
        public ICommand SaveNewColorCommand { get; set; }
        public ICommand SelectRecommendColorCommand { get; set; }
        public ICommand GradientButtonClickCommand { get; set; }
    }
}
