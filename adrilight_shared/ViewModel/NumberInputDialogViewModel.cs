using GalaSoft.MvvmLight;

namespace adrilight_shared.ViewModel
{
    public class NumberInputDialogViewModel : ViewModelBase
    {
        public NumberInputDialogViewModel(string header, int value, string content, string geometry)
        {
            Header = header;
            Value = value;
            Geometry = geometry;
            Content = content;
        }
        public int Value { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public string Geometry { get; set; } = "rename";
    }
}
