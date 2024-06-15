using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.Drawable;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel.DeviceControl
{
    partial class CapturingRegionSelectionViewModel : ViewModelBase
    {
        public CapturingRegionSelectionViewModel()
        {

        }
        public void Init(CapturingRegionSelectionButtonParameter parameter)
        {
            _sourceParameter = parameter;
        }
        //we borrow Idrawable for this task
        private Border _regionSelectionRect;
        public Border RegionSelectionRect {
            get { return _regionSelectionRect; }
            set
            {
                _regionSelectionRect = value;
                RaisePropertyChanged();
            }
        }
        private CapturingRegionSelectionButtonParameter _sourceParameter;
    }
}
