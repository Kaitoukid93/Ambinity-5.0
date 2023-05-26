using adrilight.Helpers;
using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Defaults;
using NAudio.Gui;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace adrilight.Settings
{
    public class FanMotor : ViewModelBase, IControlZone, IDrawable
    {



        /// <summary>
        /// Fan Motor Porperties
        /// </summary>
        public BitmapImage Thumb { get; set; }

        public string FanSpiningAnimationPath { get; set; }

        public FanMotor()
        {

            VisualProperties = new VisualProperties();
            Scale = new System.Windows.Point(1, 1);
            AvailableControlMode = new List<IControlMode>();

        }
        private int _currentActiveControlModeIndex;
        private bool _isInControlGroup;
        private double _actualWidth;
        private double _actualHeight;
        public double ActualWidth { get => _actualWidth; set { Set(() => ActualWidth, ref _actualWidth, value); } }

        public double ActualHeight { get => _actualHeight; set { Set(() => ActualHeight, ref _actualHeight, value); } }
        public bool IsInControlGroup { get => _isInControlGroup; set { Set(() => IsInControlGroup, ref _isInControlGroup, value); } }
        public string Name { get; set; }
        [JsonIgnore]
        public Type DataType => typeof(FanMotor);
        public string Description { get; set; }
        public List<IControlMode> AvailableControlMode { get; set; }
        [JsonIgnore]
        public IControlMode CurrentActiveControlMode => IsInControlGroup ? MaskedControlMode : AvailableControlMode[CurrentActiveControlModeIndex >= 0 ? CurrentActiveControlModeIndex : 0];
        public int CurrentActiveControlModeIndex { get => _currentActiveControlModeIndex; set { if (value >= 0) Set(() => CurrentActiveControlModeIndex, ref _currentActiveControlModeIndex, value); RaisePropertyChanged(nameof(CurrentActiveControlMode)); } }
        private IControlMode _maskedControlMode;
        public IControlMode MaskedControlMode { get => _maskedControlMode; set { Set(() => MaskedControlMode, ref _maskedControlMode, value); if (IsInControlGroup) RaisePropertyChanged(nameof(CurrentActiveControlMode)); } }
        public string ZoneUID { get; set; }
        public string GroupID { get; set; }
        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        private ChartValues<ObservableValue> _lineValues;
        public ChartValues<ObservableValue> LineValues { get => _lineValues; set { Set(() => LineValues, ref _lineValues, value); } }
        private int _currentPWMValue;
        public int CurrentPWMValue { get => _currentPWMValue; set { Set(() => CurrentPWMValue, ref _currentPWMValue, value); } }
        /// <summary>
        /// Drawable Properties
        /// </summary>
        private double _top = 0;
        private double _left = 0;
        private bool _isSelected;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _width = 100;
        private double _height = 100;
        private VisualProperties _visualProperties;
        private bool _shouldBringIntoView;
        private System.Windows.Point _directionPoint;
        private RelayCommand<double> leftChangedCommand;
        private RelayCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;
        private string _name;

        private bool _isDeleteable;
        private bool _isResizeable;
        private double _scaleTop;
        private double _scaleLeft;
        private double _scaleWidth = 1;
        private double _scaleHeight = 1;
        public Rect GetRect => new Rect(Left, Top, Width, Height);
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public double ScaleTop { get => _scaleTop; set { Set(() => ScaleTop, ref _scaleTop, value); } }
        public double ScaleLeft { get => _scaleLeft; set { Set(() => ScaleLeft, ref _scaleLeft, value); } }
        public double ScaleWidth { get => _scaleWidth; set { Set(() => ScaleWidth, ref _scaleWidth, value); } }
        public double ScaleHeight { get => _scaleHeight; set { Set(() => ScaleHeight, ref _scaleHeight, value); } }

        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public System.Windows.Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);

        public string Type { get; set; }

        private DrawableHelpers DrawableHlprs;
        public void UpdateSizeByChild(bool withPoint)
        {
            //get all child and set size
            //var boundRct = GetDeviceRectBound(Spots.ToList());
            //Width = boundRct.Width;
            //Height = boundRct.Height;
        }
        public Rect GetDeviceRectBound(IControlZone[] zones)

        {
            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var listDrawable = new List<IDrawable>();
            foreach (var zone in zones)
            {
                listDrawable.Add(zone as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);
        }




        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {
            var width = Width * scaleX;
            var height = Height * scaleY;
            if (width < 10 || height < 10)
            {
                return false;
            }
            else
            {
                Width *= scaleX;
                Height *= scaleY;
                if (!keepOrigin)
                {
                    Left *= scaleX;
                    Top *= scaleY;
                }
            }
            return true;

        }


        protected virtual void OnLeftChanged(double delta) { }

        protected virtual void OnTopChanged(double delta) { }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }
    }
}
