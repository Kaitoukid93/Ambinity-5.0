﻿using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Rectangle = System.Drawing.Rectangle;
namespace adrilight.Settings
{
    internal class PWMMotorSlaveDevice : ViewModelBase,ISlaveDevice, IDrawable
    {
        /// <summary>
        /// info properties
        /// </summary>
        public string Name { get; set; }
        public string Owner { get; set; }
        public int ParrentID { get; set; }
        public string Thumbnail { get; set; }
        public DeviceTypeEnum DesiredParrent { get; set; }
        public SlaveDeviceTypeEnum DeviceType { get; set; }
        public DeviceTypeDataEnum TargetDeviceType { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Zone properties
        /// </summary>

        public ObservableCollection<IControlZone> ControlableZones { get; set; }
        [JsonIgnore]
        public Type DataType => typeof(PWMMotorSlaveDevice);





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
        public double ScaleTop { get => _scaleTop; set { Set(() => ScaleTop, ref _scaleTop, value); } }
        public double ScaleLeft { get => _scaleLeft; set { Set(() => ScaleLeft, ref _scaleLeft, value); } }
        public double ScaleWidth { get => _scaleWidth; set { Set(() => ScaleWidth, ref _scaleWidth, value); } }
        public double ScaleHeight { get => _scaleHeight; set { Set(() => ScaleHeight, ref _scaleHeight, value); } }

        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); OnPositionUpdate(); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); OnPositionUpdate(); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); OnPositionUpdate(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); OnPositionUpdate(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public System.Windows.Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        public Rectangle GetRect => new Rectangle((int)(Left), (int)(Top), (int)Width, (int)Height);
        public string Type { get; set; }
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
        private void OnPositionUpdate()
        {
            //update all child motor
            if(ControlableZones!=null)
            {
                foreach (var zone in ControlableZones)
                {
                    var motor = zone as FanMotor;
                    motor.Left = Left;
                    motor.Top = Top;
                    motor.Width = Width;
                    motor.Height = Height;
                }
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
            foreach (var fan in ControlableZones)
            {
                if (!(fan as IDrawable).SetScale(scaleX, scaleY, keepOrigin)) return false;
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