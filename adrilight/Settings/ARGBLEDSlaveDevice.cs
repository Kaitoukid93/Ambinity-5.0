using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace adrilight.Settings
{
    /// <summary>
    /// this class holding LED Zones that current slave device has
    /// </summary>
    internal class ARGBLEDSlaveDevice : ViewModelBase, ISlaveDevice, IDrawable
    {
        /// <summary>
        /// info properties
        /// </summary>
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Thumbnail { get; set; }
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
        public DeviceTypeDataEnum TargetDeviceType { get; set; }
        private DrawableHelpers DrawableHlprs;
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
            foreach(var zone in ControlableZones)
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

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }
    }
}
