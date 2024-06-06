using adrilight_shared.Models.RelayCommand;
using GalaSoft.MvvmLight;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class SearchBarViewModel : ViewModelBase
    {
        public event Action<string> SearchContentCommited;
        public SearchBarViewModel()
        {
            SearchContentCommitedCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (p == string.Empty)
                    return;
                SearchContentCommited?.Invoke(p);

            });
        }
        private bool _isEnabled = true;
        public string SearchContent { get; set; }
        public bool IsEnabled {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }
        public ICommand SearchContentCommitedCommand { get; set; }
    }
}
