using adrilight_shared.Helpers;
using adrilight_shared.Model.VerticalMenu;
using adrilight_shared.Models;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.Stores;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class VerticalMenuControlViewModel : ViewModelBase
    {
        #region Construct
        public VerticalMenuControlViewModel()
        {
            Items = new ObservableCollection<IVerticalMenuItem>();
        }
        #endregion


        #region Properties
        private int _selectedIndex;
        public ObservableCollection<IVerticalMenuItem> Items { get; set; }
        public DeviceControlEvent DeviceControlEvent { get; set; }
        public int SelectedIndex {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                DeviceControlEvent.ChangeSelectedVerticalMenuIndex(value);
                RaisePropertyChanged();
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
