using adrilight.Spots;
using MathNet.Numerics;
using NAudio.Gui;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TimeLineTool;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace adrilight.View
{
    public class LEDVisualizer
    {
        private readonly SolidColorBrush _fillBrush;
        private readonly Pen _pen;
        private readonly SolidColorBrush _penBrush;
        public Geometry? DisplayGeometry { get; private set; }
        public DeviceSpot? Spot { get; }
        public LEDVisualizer(DeviceSpot spot)
        {
            _fillBrush = new SolidColorBrush();
            _penBrush = new SolidColorBrush();
            _pen = new Pen(_penBrush, 1.0) { LineJoin = PenLineJoin.Round };
            Spot = spot;
            UpdateLED();
            //CreateLedGeometry();
            //PointerReleased += OnPointerReleased;
            //PropertyChanged += OnPropertyChanged;
        }

        public void RenderGeometry(DrawingContext drawingContext)
        {
            if (DisplayGeometry == null)
                return;

            byte r = Spot.Red;
            byte g = Spot.Green;
            byte b = Spot.Blue;
            _fillBrush.Color = Color.FromArgb(100, r, g, b);
            _penBrush.Color = Color.FromArgb(255, r, g, b);

            // Render the LED geometry
            drawingContext.DrawGeometry(_fillBrush, _pen, DisplayGeometry);
        }

        //public bool HitTest(Point position)
        //{
        //    return DisplayGeometry != null && DisplayGeometry.FillContains(position);
        //}
      
        private void UpdateLED()
        {

            try
            {
              
                double width = Spot.Width;
                double height = Spot.Height ;

                Geometry geometry = Spot.Geometry.Clone();
                geometry.Transform = new TransformGroup {
                    Children = new TransformCollection
                    {
                    new ScaleTransform(width/geometry.Bounds.Width, height/geometry.Bounds.Height),
                    new TranslateTransform(Spot.Left-geometry.Bounds.Left, Spot.Top-geometry.Bounds.Top)
                }
                };
                DisplayGeometry = geometry;
            }
            catch (Exception ex)
            {
                //CreateRectangleGeometry();
            }
        }



    }
}
