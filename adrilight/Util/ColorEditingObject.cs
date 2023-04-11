using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public class ColorEditingObject :ViewModelBase
    {
        private Color _color;
        private bool _isSelected;
        public Color Color { get => _color; set { Set(() => Color, ref _color, value); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
    }
}
