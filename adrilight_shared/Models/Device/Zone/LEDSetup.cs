using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Geometry = System.Windows.Media.Geometry;
using Point = System.Windows.Point;

namespace adrilight_shared.Models.Device.Zone
{
    public class LEDSetup : ViewModelBase, IControlZone, IDrawable
    {

        public LEDSetup(string name, string owner, string type, string description, ObservableCollection<IDeviceSpot> spots, double pixelWidth, double pixelHeight)
        {
            Name = name;
            TargetType = type;
            Description = description;
            Spots = spots;
            Width = pixelWidth;
            Height = pixelHeight;
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
            AvailableControlMode = new List<IControlMode>();
        }

        /// <summary>
        /// control Zone properties
        /// </summary>
        public BitmapImage Thumb { get; set; }

        //private int _currentActiveControlModeIndex;
        private IControlMode _currentActiveControlMode;
        [JsonIgnore]
        public Type DataType => typeof(LEDSetup);
        private string _name;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get; set; }

        /// <summary>
        /// this list contains all available control mode ex: auto-manual,music-rainbow-capturing...
        /// </summary>
        public List<IControlMode> AvailableControlMode { get; set; }
        // [JsonIgnore]
        //public IControlMode CurrentActiveControlMode => IsInControlGroup ? MaskedControlMode : AvailableControlMode[CurrentActiveControlModeIndex >= 0 ? CurrentActiveControlModeIndex : 0];
        public IControlMode CurrentActiveControlMode { get => _currentActiveControlMode; set { Set(() => CurrentActiveControlMode, ref _currentActiveControlMode, value); } }
        //public int CurrentActiveControlModeIndex { get => _currentActiveControlModeIndex; set { if (value >= 0) Set(() => CurrentActiveControlModeIndex, ref _currentActiveControlModeIndex, value); RaisePropertyChanged(nameof(CurrentActiveControlMode)); } }
        private bool _isEnabled = true;
        private bool _isInsideScreen = true;
        private bool _isInControlGroup;
        public bool IsInControlGroup { get => _isInControlGroup; set { Set(() => IsInControlGroup, ref _isInControlGroup, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        //private IControlMode _maskedControlMode;
        //public IControlMode MaskedControlMode { get => _maskedControlMode; set { Set(() => MaskedControlMode, ref _maskedControlMode, value); if (IsInControlGroup) RaisePropertyChanged(nameof(CurrentActiveControlMode)); } }


        //LED Setup properties
        private ObservableCollection<IDeviceSpot> _spots;
        public string Geometry { get; set; }
        public bool IsInsideScreen { get => _isInsideScreen; set { Set(() => IsInsideScreen, ref _isInsideScreen, value); } }
        public string TargetType { get; set; } // Tartget Type of the spotset (keyboard, strips, ...
        public ObservableCollection<IDeviceSpot> Spots { get => _spots; set { Set(() => Spots, ref _spots, value); } }
        [JsonIgnore]
        public object Lock { get; } = new object();
        public string Thumbnail { get; set; }
        public void DimLED(float dimFactor)
        {
            foreach (var spot in Spots)
            {
                spot.DimLED(dimFactor);
            }
        }
        private DancingModeEnum _dancingMode = DancingModeEnum.Brightness;
        DancingModeEnum DancingMode { get => _dancingMode; set { Set(() => DancingMode, ref _dancingMode, value); } }
        /// <summary>
        /// this is the value of Offset, because left and with only show absolute position with SlaveDevice, not the screen
        /// </summary>

        private double _offsetX;
        private double _offsetY;
        public double OffsetX { get => _offsetX; set { Set(() => OffsetX, ref _offsetX, value); } }
        public double OffsetY { get => _offsetY; set { Set(() => OffsetY, ref _offsetY, value); } }

        /// <summary>
        /// Idrawable Properties
        /// </summary>
        /// 
        public LEDSetup()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
            AvailableControlMode = new List<IControlMode>();
            Spots = new ObservableCollection<IDeviceSpot>();

        }
        private bool _isScreenCaptureEnabled = true;
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
        private Rect _groupRect;
        private bool _isDeleteable;
        private bool _isResizeable;
        private string _zoneWarningText = string.Empty;
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public bool IsScreenCaptureEnabled { get => _isScreenCaptureEnabled; set { Set(() => IsScreenCaptureEnabled, ref _isScreenCaptureEnabled, value); } }
        public string ZoneUID { get; set; }
        public string GroupID { get; set; }
        public Rect GroupRect { get => _groupRect; set { Set(() => GroupRect, ref _groupRect, value); } }
        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string ZoneWarningText { get => _zoneWarningText; set { Set(() => ZoneWarningText, ref _zoneWarningText, value); } }
        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }


        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _shouldBringIntoView, value); } }

        public Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }
        [JsonIgnore]
        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);
        [JsonIgnore]
        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        [JsonIgnore]
        public Rect GetRect => new Rect(Left + OffsetX, Top + OffsetY, Width, Height);
        public string Type { get; set; }
        private DrawableHelpers DrawableHlprs = new DrawableHelpers();
        #region Lighting Related Method

        public List<ColorCard> GetStaticColorDataSource()
        {
            var colors = new List<ColorCard>();
            var staticColorControlMode = AvailableControlMode.Where(m => (m as LightingMode).BasedOn == LightingModeEnum.StaticColor).FirstOrDefault() as LightingMode;
            (staticColorControlMode.ColorParameter as ListSelectionParameter).LoadAvailableValues();
            (staticColorControlMode.ColorParameter as ListSelectionParameter).AvailableValues.ForEach(c => colors.Add(c as ColorCard));
            return colors;
        }
        public void BrightnessUp(int value)
        {
            var currentLightingMode = CurrentActiveControlMode as LightingMode;
            if (currentLightingMode != null)
            {
                var currentBrightness = currentLightingMode.GetBrightness();
                if (currentBrightness + value <= currentLightingMode.MaxBrightness)
                {
                    currentLightingMode.SetBrightness(currentBrightness + value);
                }
                else
                {
                    currentLightingMode.SetBrightness(currentLightingMode.MaxBrightness);
                }

            }
        }
        public void BrightnessDown(int value)
        {
            var currentLightingMode = CurrentActiveControlMode as LightingMode;
            if (currentLightingMode != null)
            {
                var currentBrightness = currentLightingMode.GetBrightness();
                if (currentBrightness - value >= currentLightingMode.MinBrightness)
                {
                    currentLightingMode.SetBrightness(currentBrightness - value);
                }
                else
                {
                    currentLightingMode.SetBrightness(currentLightingMode.MinBrightness);
                }

            }
        }
        public void TurnOffLED()
        {
            var currentLightingMode = CurrentActiveControlMode as LightingMode;
            if (currentLightingMode != null)
            {
                currentLightingMode.Disable();
            }
        }
        public void TurnOnLED()
        {
            var currentLightingMode = CurrentActiveControlMode as LightingMode;
            if (currentLightingMode != null)
            {
                currentLightingMode.Enable();
            }
        }
        #endregion
        #region Graphic Related Method
        public void ResetVIDStage()
        {
            foreach (var spot in Spots)
            {
                spot.HasVID = false;
                spot.SetVID(0);
            }
        }
        public void UpdateSizeByChild(bool withPoint)
        {
            //get all child and set size
            var boundRct = GetDeviceRectBound(Spots.ToList());
            Width = boundRct.Width;
            Height = boundRct.Height;
            if (withPoint)
            {
                Left = boundRct.Left;
                Top = boundRct.Top;
            }


        }
        private static Point ReflectPointVertical(Point pointToReflect, double center)
        {
            double distance = pointToReflect.X - center;
            return new Point(center - distance, pointToReflect.Y);
        }
        public void ReflectLEDSetupVertical(double center)
        {
            foreach (var spot in Spots)
            {
                var translatedCenterPoint = center - Left;
                var pos = new Point((spot as IDrawable).Left + (spot as IDrawable).Width, (spot as IDrawable).Top); //topRight  will become new topleft
                (spot as IDrawable).Left = ReflectPointVertical(pos, translatedCenterPoint).X;
                (spot as IDrawable).Top = ReflectPointVertical(pos, translatedCenterPoint).Y;
            }
            var newBound = GetDeviceRectBound(Spots.ToList());
            foreach (var spot in Spots)
            {
                (spot as IDrawable).Left -= newBound.Left;
                (spot as IDrawable).Top -= newBound.Top;
            }
        }
        private Geometry ScaleGeometry(Geometry inputGeometry, double scaleX, double scaleY)
        {
            //rotate the geometry
            var inputGeometryClone = inputGeometry.Clone(); // we need a clone since in order to
                                                            // apply a Transform and geometry might be readonly
            inputGeometryClone.Transform = new ScaleTransform(scaleX, scaleY);// applying some transform to it
            var result = inputGeometryClone.GetFlattenedPathGeometry();
            result.Freeze();
            return result;
        }
        public void RotateLEDSetup(double angleInDegrees, Point centerPoint)
        {

            foreach (var spot in Spots)
            {
                (spot as DeviceSpot).RotateSpot(angleInDegrees, centerPoint, GetRect.Left, GetRect.Top);
            }
            var newBound = GetDeviceRectBound(Spots.ToList());
            foreach (var spot in Spots)
            {
                (spot as IDrawable).Left -= newBound.Left;
                (spot as IDrawable).Top -= newBound.Top;
            }
            Left = newBound.Left;
            Top = newBound.Top;
            Width = newBound.Width;
            Height = newBound.Height;
        }
        public Rect GetDeviceRectBound(List<IDeviceSpot> spots)
        {
            var listDrawable = new List<IDrawable>();
            foreach (var spot in spots)
            {
                listDrawable.Add(spot as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);


        }
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {
            foreach (var spot in Spots)
            {
                if (!(spot as IDrawable).SetScale(scaleX, scaleY, keepOrigin)) return false;
                spot.Geometry = ScaleGeometry(spot.Geometry, scaleX, scaleY);
            }
            var width = Width * scaleX;
            var height = Height * scaleY;
            if (width < 1 || height < 1)
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
        public void FillSpotsColor(Color color)
        {
            foreach (var spot in Spots)
            {
                spot.SetColor(color.R, color.G, color.B, true);
            }
        }
        public void BackupSpots()
        {
            foreach (var spot in Spots)
            {
                spot.BackupID = spot.Index;
                spot.IsEnabled = false;
            }
        }
        public void RestoreSpots()
        {
            foreach (var spot in Spots)
            {
                spot.IsEnabled = true;
                spot.Index = spot.BackupID;
            }
        }
        #endregion
        protected virtual void OnLeftChanged(double delta) { }

        protected virtual void OnTopChanged(double delta) { }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }

    }


}
