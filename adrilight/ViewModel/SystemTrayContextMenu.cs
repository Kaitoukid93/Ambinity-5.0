using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class SystemTrayContextMenu : ViewModelBase
    {
        public string Header { get; set; }
        public ICommand Action { get; set; }
        public string Geometry { get; set; }
        public ObservableCollection<SystemTrayContextMenu> SubMenus { get; set; } = new ObservableCollection<SystemTrayContextMenu>();
    }
}
