using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace adrilight_shared.Models.Drawable
{
    public class ImageVisual : ViewModelBase, IDrawable
    {


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

        private bool _isResizeable;
        private string _imagePath;
        private bool _isDeleteable;
        private double _offsetX;
        private double _offsetY;
        public double OffsetX { get => _offsetX; set { Set(() => OffsetX, ref _offsetX, value); } }
        public double OffsetY { get => _offsetY; set { Set(() => OffsetY, ref _offsetY, value); } }

        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public string ImagePath { get => _imagePath; set { Set(() => ImagePath, ref _imagePath, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }
        [JsonIgnore]
        public Type DataType => typeof(ImageVisual);
        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }
        [JsonIgnore]
        public Rect GetRect => new Rect(Left + OffsetX, Top + OffsetY, Width, Height);
        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        [JsonIgnore]
        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);
        [JsonIgnore]
        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);

        public ImageVisual()
        {
            VisualProperties = new VisualProperties();
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
            IsDraggable = true;
            IsSelectable = true;
        }


        protected virtual void OnLeftChanged(double delta)
        {

        }
        protected virtual void OnTopChanged(double delta)
        {

        }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {

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



    }
}
