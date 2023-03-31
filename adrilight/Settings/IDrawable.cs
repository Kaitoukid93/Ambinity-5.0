﻿using adrilight.Spots;
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
using SharpDX;
using System.Drawing;

namespace adrilight
{
    public interface IDrawable : INotifyPropertyChanged
    {
        //bool Autostart { get; set; }



        double Angle { get; set; }
        double CenterX { get; }
        double CenterY { get; }

        double Top { get; set; }
        string Name { get; set; }
        Type DataType { get; }

        double Left { get; set; }


        bool IsSelected { get; set; }

        bool IsResizeable { get; set; }
        bool IsDeleteable { get; set; }
        double Width { get; set; }

        System.Drawing.Rectangle GetRect { get;}
        double Height { get; set; }


        VisualProperties VisualProperties { get; set; }


        bool IsSelectable { get; set; }

        bool IsDraggable { get; set; }


        bool HasCustomBehavior { get; set; }


        bool ShouldBringIntoView { get; set; }

        bool SetScale(double scaleX, double scaleY, bool keepOrigin);
        System.Windows.Point Scale { get; set; }
        ICommand LeftChangedCommand { get; }

        ICommand TopChangedCommand { get; }

        //public OutputSettings()
        //{
        //    VisualProperties = new VisualProperties();
        //    Scale = new Point(1, 1);
        //}




    }
}
