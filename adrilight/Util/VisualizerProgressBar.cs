using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class VisualizerProgressBar : ViewModelBase, IVisualizerProgressBar
    {
        public VisualizerProgressBar(float value, Color ondemandcolors)
        {
            Value = value;
            OndemandColor = ondemandcolors;

        }
        public float Value { get; set; }
        public Color OndemandColor { get; set; }
        public void SetColor(Color color)
        {
            OndemandColor = color;


            RaisePropertyChanged(nameof(OndemandColor));


        }
        public void SetValue(float value)
        {
            Value = value;


            RaisePropertyChanged(nameof(Value));


        }
    }
}
