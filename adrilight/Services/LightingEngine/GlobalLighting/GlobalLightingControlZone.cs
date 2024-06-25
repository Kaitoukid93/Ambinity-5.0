using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using adrilight_shared.Models.RelayCommand;
using System.Windows.Input;
using adrilight_shared.Models.Device.Zone;

namespace adrilight.Services.LightingEngine.GlobalLighting
{
    public class GlobalLightingControlZone : ViewModelBase, IDrawable
    {

        public GlobalLightingControlZone()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);

        }
        #region IDrawableImplement
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
        private bool _isMouseOver;
        private bool _isVisible = true;
        public string Name { get; set; }

        public string Description { get; set; }
        public Type DataType => typeof(GlobalLightingControlZone);
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsMouseOver { get => _isMouseOver; set { Set(() => IsMouseOver, ref _isMouseOver, value); } }
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
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string ZoneWarningText { get => _zoneWarningText; set { Set(() => ZoneWarningText, ref _zoneWarningText, value); } }
        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }
        [JsonIgnore]
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
        public Rect GetRect => new Rect(Left, Top, Width, Height);
        public string Type { get; set; }
        #endregion



        private IControlMode _currentActiveControlMode;
        public IControlMode CurrentActiveControlMode { get => _currentActiveControlMode; set { Set(() => CurrentActiveControlMode, ref _currentActiveControlMode, value); } }
        
        #region Lighting Related Method
 
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

        #region IDrawable Methods
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {

            Width *= scaleX;
            Height *= scaleY;
            if (!keepOrigin)
            {
                Left *= scaleX;
                Top *= scaleY;
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
        #endregion

    }
}
