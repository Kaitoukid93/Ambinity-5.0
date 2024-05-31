using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel.Splash
{
    public class SplashScreenViewModel : ViewModelBase
    {
        public SplashScreenViewModel()
        {

        }
        private int _progress;
        private string _description;
        public string Title { get; set; }
        public string Description {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
        }
        public string Icon { get; set; }
        public int Progress {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }
    }
}
