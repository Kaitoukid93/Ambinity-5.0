using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace adrilight.Settings
{
    /// <summary>
    /// this class holding LED Zones that current slave device has
    /// </summary>
    public class ARGBLEDSlaveDevice : ViewModelBase, ISlaveDevice, IDrawable
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string SupportedSlaveDeviceFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string deviceDirectory => Directory.Exists(Path.Combine(SupportedSlaveDeviceFolderPath, Name)) ? Path.Combine(SupportedSlaveDeviceFolderPath, Name) : Path.Combine(SupportedSlaveDeviceFolderPath, "GenericDevice");
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Vendor { get; set; }
        public int ParrentID { get; set; }

        [JsonIgnore]
        public string Thumbnail => Path.Combine(deviceDirectory, "thumbnail.png");

        public SlaveDeviceTypeEnum DeviceType { get; set; }
        public DeviceTypeEnum DesiredParrent { get; set; }
        public string Description { get; set; }
        public ImageVisual Image { get; set; }

        /// <summary>
        /// Zone properties
        /// </summary>
        private ObservableCollection<IControlZone> _controlableZones;

        [ProfileIgnore]
        public ObservableCollection<IControlZone> ControlableZones { get => _controlableZones; set { Set(() => ControlableZones, ref _controlableZones, value); } }

        [JsonIgnore]
        public Type DataType => typeof(ARGBLEDSlaveDevice);

        public ARGBLEDSlaveDevice()
        {
            VisualProperties = new VisualProperties();
            Scale = 1.0;
            ControlableZones = new ObservableCollection<IControlZone>();
        }

        /// <summary>
        /// drawable properties
        /// </summary>

        private double _top = 0;
        private double _left = 0;
        private bool _isSelected;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _actualWidth;
        private double _actualHeight;
        private double _width;
        private double _height;
        private VisualProperties _visualProperties;
        private bool _shouldBringIntoView;
        private System.Windows.Point _directionPoint;
        private RelayCommand<double> leftChangedCommand;
        private RelayCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;
        private bool _isDeleteable;
        private bool _isResizeable;
        private double _scale = 1.0;
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }

        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }

        public double CenterX => Width / 2 + Left;

        public double CenterY => Height / 2 + Top;

        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }

        public double Top { get => _top; set { Set(() => Top, ref _top, value); UpdateChildOffSet(); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); UpdateChildOffSet(); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public double ActualWidth { get => _actualWidth; set { Set(() => ActualWidth, ref _actualWidth, value); } }

        public double ActualHeight { get => _actualHeight; set { Set(() => ActualHeight, ref _actualHeight, value); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public int LEDCount => GetLEDsCount();

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public double Scale { get => _scale; set { Set(() => Scale, ref _scale, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);

        public Rect GetRect => new Rect(Left, Top, Width, Height);
        private string _version = "1.0.0";
        public string Version { get => _version; set { Set(() => Version, ref _version, value); } }
        public DeviceType TargetDeviceType { get; set; }

        private DrawableHelpers DrawableHlprs = new DrawableHelpers();

        private int GetLEDsCount()
        {
            int ledCount = 0;
            if (ControlableZones != null)
            {
                foreach (var zone in ControlableZones)
                {
                    ledCount += (zone as LEDSetup).Spots.Count();
                }
            }

            return ledCount;
        }

        private void UpdateChildOffSet()
        {
            foreach (var zone in ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                ledZone.OffsetX = Left;
                ledZone.OffsetY = Top;
            }
            if (Image != null)
            {
                Image.OffsetX = Left;
                Image.OffsetY = Top;
            }
        }

        public void UpdateSizeByChild(bool withPoint)
        {
            List<IDrawable> children = new List<IDrawable>();
            foreach (var zone in ControlableZones)
            { children.Add(zone as IDrawable); }
            children.Add(Image as IDrawable);
            var boundRct = GetDeviceRectBound();
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

        public void ReflectLEDSetupVertical()
        {
            var center = Width; // reflect using right edge
            foreach (var zone in ControlableZones)
            {
                (zone as LEDSetup).ReflectLEDSetupVertical(center);
                var pos = new Point((zone as IDrawable).Left + (zone as IDrawable).Width, (zone as IDrawable).Top);
                (zone as IDrawable).Left = ReflectPointVertical(pos, center).X;
                (zone as IDrawable).Top = ReflectPointVertical(pos, center).Y;
            }
            var newBound = GetDeviceRectBound();
            foreach (var zone in ControlableZones)
            {
                (zone as IDrawable).Left -= newBound.Left;
                (zone as IDrawable).Top -= newBound.Top;
            }
            UpdateSizeByChild(false);
            Left += Width;
        }

        public void RotateLEDSetup(double angleInDegrees)
        {
            var deltaAngle = angleInDegrees - Angle;
            var center = new Point(Left + Width / 2, Top + Height / 2);
            foreach (var zone in ControlableZones)
            {
                (zone as LEDSetup).RotateLEDSetup(deltaAngle, center);
            }
            //rotate background image,
            if (Image != null)
            {
                Image.Angle += deltaAngle;
                if (Image.Angle == 360)
                {
                    Image.Angle = 0;
                }
            }
            var newBound = GetDeviceRectBound();
            foreach (var zone in ControlableZones)
            {
                (zone as IDrawable).Left -= newBound.Left;
                (zone as IDrawable).Top -= newBound.Top;
            }
            if (Image != null)
            {
                Image.Left += (newBound.Width - Width) / 2;
                Image.Top += (newBound.Height - Height) / 2;
            }
            // UpdateSizeByChild(false);
            Left = newBound.Left;
            Top = newBound.Top;
            Width = newBound.Width;
            Height = newBound.Height;
            Angle = angleInDegrees;
        }

        public Rect GetDeviceRectBound()
        {
            List<IDrawable> children = new List<IDrawable>();
            foreach (var zone in ControlableZones)
            { children.Add(zone as IDrawable); }
            if (Image != null)
            {
                //get image rotation bounding box
                var rect = new Rect(Image.Left + Image.OffsetX, Image.Top + Image.OffsetY, Image.Width, Image.Height);
                var rotatedImageBoundingBox = DrawableHlprs.RotateRectangle(rect, new Point(CenterX, CenterY), Image.Angle);
                var r = new Drawable(rotatedImageBoundingBox.Top, rotatedImageBoundingBox.Left, rotatedImageBoundingBox.Width, rotatedImageBoundingBox.Height);
                children.Add(r);
            }

            return DrawableHlprs.GetBound(children);
        }

        public bool ApplyScale(double scale)
        {
            var scaleFactor = scale / Scale;
            foreach (var zone in ControlableZones)
            {
                if (!(zone as IDrawable).SetScale(scaleFactor, scaleFactor, false)) return false;
            }
            var width = Width * scaleFactor;
            var height = Height * scaleFactor;
            if (width < 10 || height < 10)
            {
                return false;
            }
            else
            {
                Width *= scaleFactor;
                Height *= scaleFactor;
            }

            Image?.SetScale(scaleFactor, scaleFactor, false);
            Scale = scale;
            return true;
        }

        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {
            foreach (var zone in ControlableZones)
            {
                if (!(zone as IDrawable).SetScale(scaleX, scaleY, keepOrigin)) return false;
            }
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
            //change child offset
            foreach (var zone in ControlableZones)
            {
                (zone as LEDSetup).OffsetX = Left;
                (zone as LEDSetup).OffsetY = Top;
            }
            return true;
        }

        /// <summary>
        /// tell zones to update size and position after system or local change of resolution
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>

        private void MoveChildX(double delta)
        {
            foreach (var ledZone in ControlableZones)
            {
                (ledZone as LEDSetup).Left += delta;
            }
        }

        private void MoveChildY(double delta)
        {
            foreach (var ledZone in ControlableZones)
            {
                (ledZone as LEDSetup).Top += delta;
            }
        }

        protected virtual void OnLeftChanged(double delta)
        { }

        protected virtual void OnTopChanged(double delta)
        { }

        protected virtual void OnWidthUpdated()
        { }

        protected virtual void OnHeightUpdated()
        { }

        protected virtual void OnRotationChanged()
        { }

        protected virtual void OnIsSelectedChanged(bool value)
        {
            if (ControlableZones != null)
            {
                switch (value)
                {
                    case true:
                        foreach (var zone in ControlableZones)
                        {
                            foreach (var spot in (zone as LEDSetup).Spots)
                            {
                                if (spot.Index == 0)
                                    spot.SetColor(0, 0, 255, true);
                                else
                                {
                                    spot.SetColor(0, 0, 0, true);
                                }
                            }
                        }
                        break;

                    case false:
                        foreach (var zone in ControlableZones)
                        {
                            foreach (var spot in (zone as LEDSetup).Spots)
                            {
                                spot.SetColor(0, 0, 0, true);
                            }
                        }
                        break;
                }
            }
        }

        public virtual void OnDrawingEnded(Action<object> callback = default)
        { }
    }
}