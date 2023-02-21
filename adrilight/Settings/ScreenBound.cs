using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace adrilight.Settings
{
    public class ScreenBound : ViewModelBase, IDrawable
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
        private double _centerX;
        private double _centerY;
        private bool _isResizeable;
        private bool _isDeleteable;
        private int _index;
        public int Index { get => _index; set { Set(() => Index, ref _index, value); } }
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX { get => _centerX; set { Set(() => CenterX, ref _centerX, value); } }
        public double CenterY { get => _centerY; set { Set(() => CenterY, ref _centerY, value); } }
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

        public Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);

        public ScreenBound()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
            IsDraggable = false;
            IsSelectable = false;
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
        public void SetScale(double scale)
        {
            //keep left and top the same
            //scale width and height only
            var oldWidth = Width;
            var oldHeight = Height;
            Width = scale* oldWidth;
            Height = scale * oldHeight;
            RaisePropertyChanged(nameof(Width));
            RaisePropertyChanged(nameof(Height));
        }
      

      
    }
}
