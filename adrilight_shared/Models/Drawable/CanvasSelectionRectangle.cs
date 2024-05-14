using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Drawable
{
    public class CanvasSelectionRectangle :ViewModelBase
    {
        public CanvasSelectionRectangle() { }
        private double _strokeThickness = 1.0;
        private int _strokeDashArray = 2;
        System.Windows.Media.Color _strokeColor = System.Windows.Media.Color.FromRgb(255,255,255);
        System.Windows.Media.Color _fillColor = System.Windows.Media.Color.FromArgb(64,255, 255, 255);
        public double StrokeThickness { get => _strokeThickness; set { Set(() => StrokeThickness, ref _strokeThickness, value); } }
        public int StrokeDashArray { get => _strokeDashArray; set { Set(() => StrokeDashArray, ref _strokeDashArray, value); } }
        public System.Windows.Media.Color StrokeColor { get => _strokeColor; set { Set(() => StrokeColor, ref _strokeColor, value); } }
        public System.Windows.Media.Color FillColor { get => _fillColor; set { Set(() => FillColor, ref _fillColor, value); } }
    }
}
