using adrilight.DesktopDuplication;
using adrilight.Extensions;
using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace adrilight
{
    public class LEDSetup : ViewModelBase, IControlZone, IDrawable
    {

        public LEDSetup(string name, string owner, string type, string description, ObservableCollection<IDeviceSpot> spots, double pixelWidth, double pixelHeight, double scaleWidth, double scaleHeight)
        {
            Name = name;
            Owner = owner;
            TargetType = type;
            Description = description;
            Spots = spots;
            Width = pixelWidth;
            Height = pixelHeight;
            ScaleWidth = scaleWidth;
            ScaleHeight = scaleHeight;
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
            AvailableControlMode = new List<IControlMode>();
        }

        public BitmapImage Thumb { get; set; }

        private int _currentActiveControlModeIndex;
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// this list contains all available control mode ex: auto-manual,music-rainbow-capturing...
        /// </summary>
        public List<IControlMode> AvailableControlMode { get; set; }
        [JsonIgnore]
        public IControlMode CurrentActiveControlMode { get; set; }

        public int CurrentActiveControlModeIndex { get => _currentActiveControlModeIndex; set { if (value >= 0) Set(() => CurrentActiveControlModeIndex, ref _currentActiveControlModeIndex, value); OnActiveControlModeChanged(); } }

        private void OnActiveControlModeChanged()
        {
            if (CurrentActiveControlModeIndex >= 0)
            {
                CurrentActiveControlMode = AvailableControlMode[CurrentActiveControlModeIndex];
                RaisePropertyChanged(nameof(CurrentActiveControlMode));
            }
        }

        private ObservableCollection<IDeviceSpot> _spots;
        public string Owner { get; set; }
        public string Geometry { get; set; }
        public string TargetType { get; set; } // Tartget Type of the spotset (keyboard, strips, ...
        public ObservableCollection<IDeviceSpot> Spots { get => _spots; set { Set(() => Spots, ref _spots, value); RaisePropertyChanged(); } }
        public object Lock { get; } = new object();
        public string Thumbnail { get; set; }
        public void DimLED(float dimFactor)
        {
            foreach (var spot in Spots)
            {
                spot.DimLED(dimFactor);
            }
        }

        /// <summary>
        /// Idrawable Implementation
        /// </summary>
        /// 
        public LEDSetup()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
            AvailableControlMode = new List<IControlMode>();
        }
        private bool _isScreenCaptureEnabled = true;
        private int _outputSelectedDisplay;
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
        private double _centerX;
        private double _centerY;
        private bool _isDeleteable;
        private bool _isResizeable;
        private double _scaleTop;
        private double _scaleLeft;
        private double _scaleWidth = 1;
        private double _scaleHeight = 1;
        public int OutputSelectedDisplay { get => _outputSelectedDisplay; set { Set(() => OutputSelectedDisplay, ref _outputSelectedDisplay, value); } }
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX { get => _centerX; set { Set(() => CenterX, ref _centerX, value); } }
        public double CenterY { get => _centerY; set { Set(() => CenterY, ref _centerY, value); } }
        public bool IsScreenCaptureEnabled { get => _isScreenCaptureEnabled; set { Set(() => IsScreenCaptureEnabled, ref _isScreenCaptureEnabled, value); } }
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

        public void SetScale(double scale)
        {
            //keep left and top the same
            //scale width and height only
            Width *= scale;
            Height *= scale;
            ScaleHeight *= scale;
            ScaleHeight *= scale;// these value is for setting new rectangle and it's position when parrents size is not stored( app startup)
                                 //SetRectangle(new Rectangle(OutputRectangle.Left, OutputRectangle.Top, (int)Width, (int)Height));
                                 //we need to change ledsetup width and height too
            foreach (var deviceSpot in Spots)
            {
                (deviceSpot as DeviceSpot).Width *= scale;
                (deviceSpot as DeviceSpot).Height *= scale;
                (deviceSpot as DeviceSpot).Left *= scale;
                (deviceSpot as DeviceSpot).Top *= scale;

            }
            RaisePropertyChanged(nameof(Width));
            RaisePropertyChanged(nameof(Height));
        }
        public void OnResolutionChanged(double scaleX, double scaleY)
        {
            Width *= scaleX;
            Height *= scaleY;
            Left *= scaleX;
            Top *= scaleY;
            foreach (var deviceSpot in Spots)
            {
                (deviceSpot as DeviceSpot).Width *= scaleX;
                (deviceSpot as DeviceSpot).Height *= scaleY;
                (deviceSpot as DeviceSpot).Left *= scaleX;
                (deviceSpot as DeviceSpot).Top *= scaleY;

            }
        }
        public void RefreshSizeAndPosition()
        {
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Width = screenWidth * ScaleWidth;
            Height = screenHeight * ScaleHeight;
            Left = screenWidth * ScaleLeft;
            Top = screenHeight * ScaleTop;
            foreach (var spot in Spots)
            {
                spot.RebuildSpot(Width, Height);
            }
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
