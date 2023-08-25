using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace adrilight_shared.Models.Drawable
{
    public interface IDrawable : INotifyPropertyChanged
    {
        string Name { get; set; }
        Type DataType { get; }

        double Top { get; set; }
        double Left { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double Angle { get; set; }
        double CenterX { get; }
        double CenterY { get; }
        Rect GetRect { get; }

        bool IsSelected { get; set; }
        bool IsResizeable { get; set; }
        bool IsDeleteable { get; set; }
        bool IsSelectable { get; set; }
        bool IsDraggable { get; set; }
        VisualProperties VisualProperties { get; set; }

        bool SetScale(double scaleX, double scaleY, bool keepOrigin);

        ICommand LeftChangedCommand { get; }
        ICommand TopChangedCommand { get; }

    }
}
