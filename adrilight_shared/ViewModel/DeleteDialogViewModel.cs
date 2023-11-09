using GalaSoft.MvvmLight;

namespace adrilight_shared.ViewModel
{
    public class DeleteDialogViewModel : ViewModelBase
    {
        public DeleteDialogViewModel(string header, string content)
        {
            Header = header;
            Content = content;
        }
        public string Content { get; set; }
        public string Header { get; set; }
    }
}
