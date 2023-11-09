using GalaSoft.MvvmLight;
using System.Windows.Input;

namespace adrilight_shared.ViewModel
{
    public class NonClientAreaContentViewModel : ViewModelBase
    {
        public NonClientAreaContentViewModel(string header, string geometry)
        {
            Geometry = geometry;
            Header = header;
        }
        public string Geometry { get; set; }
        public string Header { get; set; }
        private bool _showBackButton;
        private ICommand _backButtonCommand;
        public ICommand BackButtonCommand
        {
            get { return _backButtonCommand; }
            set
            {
                _backButtonCommand = value;
                RaisePropertyChanged();
            }
        }
        public bool ShowBackButton
        {
            get { return _showBackButton; }
            set { _showBackButton = value; RaisePropertyChanged(); }
        }
    }
}
