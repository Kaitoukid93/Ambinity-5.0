using adrilight_shared.Models.RelayCommand;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.AdrilightStore
{
    public class SearchBarViewModel
    {
        public event Action<string> SearchContentCommited;
        public SearchBarViewModel()
        {
            SearchContentCommitedCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SearchContentCommited?.Invoke(p);

            });
        }
        public string SearchContent { get; set; }

        public ICommand SearchContentCommitedCommand { get; set; }
    }
}
