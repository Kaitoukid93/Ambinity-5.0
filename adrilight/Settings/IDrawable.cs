using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using System.Windows.Input;
using adrilight.ViewModel;
using System.Windows;

namespace adrilight
{
    public interface IDrawable : INotifyPropertyChanged 
    {
        //bool Autostart { get; set; }



        double Angle { get; set; }
        double CenterX { get; set; }
        double CenterY { get; set; }

        double Top { get; set; }
        string Name { get; set; }
        Type DataType { get; }

        double Left { get; set; }


        bool IsSelected { get; set; }

        bool IsResizeable { get; set; }
        bool IsDeleteable { get; set; }
        double Width { get; set; }


        double Height { get; set; }


        VisualProperties VisualProperties { get; set; }


        bool IsSelectable { get; set; }

        bool IsDraggable { get; set; }


        bool HasCustomBehavior { get; set; }


        bool ShouldBringIntoView { get; set; }

        void SetScale(double scale);
        Point Scale { get; set; }
        ICommand LeftChangedCommand { get; }

        ICommand TopChangedCommand { get; }

        //public OutputSettings()
        //{
        //    VisualProperties = new VisualProperties();
        //    Scale = new Point(1, 1);
        //}




    }
}
