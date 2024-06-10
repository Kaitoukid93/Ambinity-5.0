using GalaSoft.MvvmLight;
using System.Windows.Media;

namespace adrilight.ViewModel
{
    public class ColorEditingObject : ViewModelBase
    {
        public ColorEditingObject()
        {

        }
        public ColorEditingObject(Color color)
        {
            Color = color;
            IsSelected = false;
        }
        private Color _color;
        private bool _isSelected;
        private bool _showPopTip;
        public Color Color { get => _color; set { Set(() => Color, ref _color, value); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public bool ShowPopTip { get => _showPopTip; set { Set(() => ShowPopTip, ref _showPopTip, value); } }
    }
}
