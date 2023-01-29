using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public abstract class Drawable : ObservableObject
    {
        private double _top;
        private double _left;
        private bool _isSelected;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _width;
        private double _height;
        private VisualProperties _visualProperties;
        private bool _shouldBringIntoView;
        private Point _directionPoint;
        private RelayCommand<double> leftChangedCommand;
        private RelayCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;

        public double Angle {
            get { return _angle; }
            set { _angle = value; }
        }

        public double Top {
            get { return _top; }
            set { _top = value; }
        }

        public double Left {
            get { return _left; }
            set { _left = value; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnIsSelectedChanged(value);
            }
        }

        public double Width {
            get { return _width; }
            set
            {
                _width = value;
                OnWidthUpdated();
            }
        }

        public double Height {
            get { return _height;}
            set
            {
                _height = value;
                OnHeightUpdated();
            }
        }

        public VisualProperties VisualProperties {
            get { return _visualProperties; }
            set { _visualProperties = value; }
        }

        public bool IsSelectable {
            get { return _isSelectable; }
            set { _isSelectable = value; }
        }

        public bool IsDraggable {
            get { return _isDraggable; }
            set { _isDraggable = value; }
        }

        public bool HasCustomBehavior {
            get { return _hasCustomBehavior; }
            set { _hasCustomBehavior = value; }
        }

        public bool ShouldBringIntoView {
            get { return _shouldBringIntoView; }
            set { _shouldBringIntoView = value; }
        }

        public Point Scale {
            get { return _directionPoint; }
            set { _directionPoint = value; }
        }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);

        public Drawable()
        {
            VisualProperties = new VisualProperties();
            Scale = new Point(1, 1);
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
