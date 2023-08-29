﻿using GalaSoft.MvvmLight;
using System.Windows.Media;

namespace adrilight_shared.Models.Drawable
{
    public class VisualProperties : ObservableObject
    {
        // #MD ColorPicker bug when Color binded to non-colors??
        private Color _fillColor = Colors.Red;
        private Color _borderColor = Colors.Red;

        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }
    }
}