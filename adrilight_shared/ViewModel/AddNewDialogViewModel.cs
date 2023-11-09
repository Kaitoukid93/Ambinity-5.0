using GalaSoft.MvvmLight;

namespace adrilight_shared.ViewModel
{
    public class AddNewDialogViewModel : ViewModelBase
    {
        public AddNewDialogViewModel(string header, string content, string geometry)
        {
            Header = header;
            Content = content;
            Geometry = geometry;
        }
        public string Geometry { get; set; }
        public string Content { get; set; }
        public string Header { get; set; }
    }
}
