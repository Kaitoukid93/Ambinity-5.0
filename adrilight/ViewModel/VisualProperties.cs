using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.ViewModel
{
    public class VisualProperties : ObservableObject
    {
        // #MD ColorPicker bug when Color binded to non-colors??
        private Color _fillColor = Colors.Red;
        private Color _borderColor = Colors.Red;

        public Color FillColor {
            get { return _fillColor; }
            set { _fillColor = value; }
        }
        public Color BorderColor {
            get { return _borderColor; }
            set { _borderColor = value; }
        }
    }
}
