using adrilight_shared.View.NonClientAreaContent;
using GalaSoft.MvvmLight;
using System.Net.Mime;
using System.Windows;
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
        public NonClientAreaContentViewModel(string content, string geometry, bool showBackButton, ICommand buttonCommand)
        {
            Header = content;
            Geometry = geometry;
            ShowBackButton = showBackButton;
            if (ShowBackButton)
                BackButtonCommand = buttonCommand;
        }
        public string Geometry { get; set; }
        private string _header;
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                RaisePropertyChanged();
            }
        }
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
