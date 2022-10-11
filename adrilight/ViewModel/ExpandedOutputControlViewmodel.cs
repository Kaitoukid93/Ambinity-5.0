using adrilight.Spots;

using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class ExpandedOutputControlViewmodel : ViewModelBase
    {

        private string _outputName;
        public string OutputName {
            get { return _outputName; }
            set { _outputName=value; }
        }
    }
}
