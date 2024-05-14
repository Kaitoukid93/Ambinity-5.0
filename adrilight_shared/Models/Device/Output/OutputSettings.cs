using adrilight_shared.Enums;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace adrilight_shared.Models.Device.Output
{
    public class OutputSettings : ViewModelBase, IOutputSettings, IDrawable
    {

        private string _outputName;
        private OutputTypeEnum _outputType;
        private int _outputID;
        private bool _isVissible = true;
        private bool _isLocked;
        private string _outputDescription;
        private string _targetDevice;


        private string _outputUniqueID;
        private string _outputRGBLEDOrder;
        private bool _outputIsVisible;
        private int _outputPowerVoltage;
        private int _outputPowerMiliamps;
        private bool _outputIsSelected = false;
        private bool _isEnabled = true;
        private ISlaveDevice _slaveDevice;
        private IControlZone[] _controlableZone;
        private string _geometry = "generaldevice";
        private bool _isMouseOver;
        private bool _isVisible = true;
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsMouseOver { get => _isMouseOver; set { Set(() => IsMouseOver, ref _isMouseOver, value); } }
        [JsonIgnore]
        public bool IsLoadingProfile { get; set; }
        public OutputSettings()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
        }

        public string OutputName { get => _outputName; set { Set(() => OutputName, ref _outputName, value); } }
        public string TargetDevice { get => _targetDevice; set { Set(() => TargetDevice, ref _targetDevice, value); } }
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        [JsonIgnore]
        public int DisplayOutputID => OutputID + 1;
        public OutputTypeEnum OutputType { get => _outputType; set { Set(() => OutputType, ref _outputType, value); } }
        public string OutputDescription { get => _outputDescription; set { Set(() => OutputDescription, ref _outputDescription, value); } }
        public bool IsVissible { get => _isVissible; set { Set(() => IsVissible, ref _isVissible, value); } }
        public bool IsLocked { get => _isLocked; set { Set(() => IsLocked, ref _isLocked, value); } }
        public bool OutputIsSelected { get => _outputIsSelected; set { Set(() => OutputIsSelected, ref _outputIsSelected, value); } }
        public Rectangle Rectangle { get; set; }
        public string OutputUniqueID { get => _outputUniqueID; set { Set(() => OutputUniqueID, ref _outputUniqueID, value); } }
        public bool OutputIsVisible { get => _outputIsVisible; set { Set(() => OutputIsVisible, ref _outputIsVisible, value); } }
        public int OutputPowerVoltage { get => _outputPowerVoltage; set { Set(() => OutputPowerVoltage, ref _outputPowerVoltage, value); } }
        public int OutputPowerMiliamps { get => _outputPowerMiliamps; set { Set(() => OutputPowerMiliamps, ref _outputPowerMiliamps, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public string OutputInterface { get; set; }
        public ISlaveDevice SlaveDevice { get => _slaveDevice; set { Set(() => SlaveDevice, ref _slaveDevice, value); } }

        public bool ShouldSerializeAvailableControllers()
        {
            return IsLoadingProfile;
        }

        /// <summary>
        /// Idrawable implementation
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
        private Point _directionPoint;
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

        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public string Name { get; set; }
        public Type DataType => typeof(OutputSettings);
        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }
        [JsonIgnore]
        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);
        [JsonIgnore]
        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        [JsonIgnore]
        public Rect GetRect => new Rect(Left, Top, Width, Height);
        protected virtual void OnLeftChanged(double delta) { }

        protected virtual void OnTopChanged(double delta) { }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }
        public bool ShouldSerializeSlaveDevice()
        {
            return IsLoadingProfile;
        }
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin) => throw new NotImplementedException();
    }
}
