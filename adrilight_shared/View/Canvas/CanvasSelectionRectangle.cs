using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.View.Canvas
{
    public class CanvasSelectionRectangle :ViewModelBase
    {
        public CanvasSelectionRectangle() { }
        private double _strokeThickness = 1.0;
        private int _strokeDashArray = 2;
        Color _strokeColor = Color.White;
        Color _fillColor = Color.Transparent;
        public double StrokeThickness { get => _strokeThickness; set { Set(() => StrokeThickness, ref _strokeThickness, value); } }
        public int StrokeDashArray { get => _strokeDashArray; set { Set(() => StrokeDashArray, ref _strokeDashArray, value); } }
        public Color StrokeColor { get => _strokeColor; set { Set(() => StrokeColor, ref _strokeColor, value); } }
        public Color FillColor { get => _fillColor; set { Set(() => FillColor, ref _fillColor, value); } }
    }
}
