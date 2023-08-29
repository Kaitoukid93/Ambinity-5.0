using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Audio
{
    public class ColumnDataModel : ViewModelBase
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