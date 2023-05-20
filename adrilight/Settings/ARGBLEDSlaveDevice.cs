using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using HandyControl.Tools.Extension;
using MoreLinq;
using NAudio.Gui;
using Newtonsoft.Json;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace adrilight.Settings
{
    /// <summary>
    /// this class holding LED Zones that current slave device has
    /// </summary>
    internal class ARGBLEDSlaveDevice : ViewModelBase, ISlaveDevice, IDrawable
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string SupportedSlaveDeviceFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string deviceDirectory => Directory.Exists(Path.Combine(SupportedSlaveDeviceFolderPath, Name))? Path.Combine(SupportedSlaveDeviceFolderPath, Name): Path.Combine(SupportedSlaveDeviceFolderPath, "GenericDevice");
        public string Name { get; set; }
        public string Owner { get; set; }
        public int ParrentID { get; set; }
        [JsonIgnore]
        public string Thumbnail => Path.Combine(deviceDirectory, "thumbnail.png");
        public SlaveDeviceTypeEnum DeviceType { get; set; }
        public DeviceTypeEnum DesiredParrent { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Zone properties
        /// </summary>
        private ObservableCollection<IControlZone> _controlableZones;
        public ObservableCollection<IControlZone> ControlableZones { get => _controlableZones; set { Set(() => ControlableZones, ref _controlableZones, value); } }
        [JsonIgnore]
        public Type DataType => typeof(ARGBLEDSlaveDevice);


        public ARGBLEDSlaveDevice()
        {
            VisualProperties = new VisualProperties();
            Scale = new System.Windows.Point(1, 1);
        }




        /// <summary>
        /// drawable properties
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

        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }
        public int LEDCount => GetLEDsCount();
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
        public Rectangle GetRect => new Rectangle((int)(Left), (int)(Top), (int)Width, (int)Height);
        public DeviceType TargetDeviceType { get; set; }
        private DrawableHelpers DrawableHlprs;
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
        
        public void UpdateSizeByChild(bool withPoint)
        {

            var boundRct = GetDeviceRectBound(ControlableZones.ToArray());
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
            var newBound = GetDeviceRectBound(ControlableZones.ToArray());
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
            var center = new Point(Width / 2, Height / 2);
            var devicePos = new Point(Left, (Top + Height));
            var newCenter = new Point(CenterX, CenterY);
            foreach (var zone in ControlableZones)
            {
                (zone as LEDSetup).RotateLEDSetup(90.0, center);
                var pos = new Point((zone as IDrawable).Left, (zone as IDrawable).Top + (zone as IDrawable).Height);
                var width = (zone as IDrawable).Width;
                var height = (zone as IDrawable).Height;
                (zone as IDrawable).Left = RotatePoint(pos, center, 90.0).X;
                (zone as IDrawable).Top = RotatePoint(pos, center, 90.0).Y;
                (zone as IDrawable).Width = height;
                (zone as IDrawable).Height = width;

            }
            var newBound = GetDeviceRectBound(ControlableZones.ToArray());
            foreach (var zone in ControlableZones)
            {
                (zone as IDrawable).Left -= newBound.Left;
                (zone as IDrawable).Top -= newBound.Top;
            }
            UpdateSizeByChild(false);
            
            
            Left = RotatePoint(devicePos, newCenter, 90.0).X;
            Top = RotatePoint(devicePos, newCenter, 90.0).Y;


        }
        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        private static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public Rect GetDeviceRectBound(IControlZone[] zones)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();


            return DrawableHlprs.GetBound(zones);


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
        protected virtual void OnLeftChanged(double delta) { }

        protected virtual void OnTopChanged(double delta) { }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

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

        public virtual void OnDrawingEnded(Action<object> callback = default) { }
    }
}
