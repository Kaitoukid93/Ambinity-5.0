using GalaSoft.MvvmLight;
using OpenRGB.NET.Models;
using System.Windows.Forms;

namespace adrilight.Util
{
    public class ColumnDataModel :ViewModelBase
    {
        public ColumnDataModel() { }
        public int Index { get; set; }
        public int Value { get; set; }
        public void SetValue(byte value)
        {
            Value = value;
            RaisePropertyChanged(nameof(Value));

           
        }
    }
}