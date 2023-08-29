using adrilight_shared.Models.Device.Zone.Spot;
using System;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace adrilight_shared.Models.DataVisualizer
{
    public class LEDVisualizer
    {
        private readonly SolidColorBrush _fillBrush;
        private readonly Pen _pen;
        private readonly SolidColorBrush _penBrush;
        public Geometry DisplayGeometry { get; private set; }
        public DeviceSpot Spot { get; }

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
            _fillBrush.Color = Color.FromArgb(160, r, g, b);
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
                double height = Spot.Height;

                Geometry geometry = Spot.Geometry.Clone();
                var boundsLeft = geometry.Bounds.Left;
                var boundsTop = geometry.Bounds.Top;
                var scaleX = width / geometry.Bounds.Width;
                var scaleY = height / geometry.Bounds.Height;
                geometry.Transform = new TransformGroup
                {
                    Children = new TransformCollection
                    {
                         new ScaleTransform(scaleX, scaleY),
                        new TranslateTransform(Spot.Left-boundsLeft*scaleX, Spot.Top-boundsTop*scaleY)

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