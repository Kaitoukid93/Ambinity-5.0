using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class ActionSettings : ViewModelBase, IActionSettings
    {
        private string _targetDeviceUID;
        private string _targetDeviceType;
        private string _targetDeviceName;
        
        private string _actionType;
        
       
        //private List<IActionParameter> _allAvailableParameters; // available parameters
        private IActionParameter _actionParameter;
       

        

       




        public string TargetDeviceUID { get => _targetDeviceUID; set { Set(() => TargetDeviceUID, ref _targetDeviceUID, value); } }
        public string TargetDeviceType { get => _targetDeviceType; set { Set(() => TargetDeviceType, ref _targetDeviceType, value); } }
        public string TargetDeviceName { get => _targetDeviceName; set { Set(() => TargetDeviceName, ref _targetDeviceName, value); } }
        public string ActionType { get => _actionType; set { Set(() => ActionType, ref _actionType, value); } }
        //public List<IActionParameter> AllAvailableParameters { get => _allAvailableParameters; set { Set(() => AllAvailableParameters, ref _allAvailableParameters, value); } }
       
        public IActionParameter ActionParameter { get => _actionParameter; set { Set(() => ActionParameter, ref _actionParameter, value); } }
       
      

    }
}
//switch (value)
//{
//    case 0://profile activation  
//        AvailableParameters = AllAvailableParameters.Where(x => x.Type == "Profile" && x.TargetDeviceType == TargetDeviceType).ToList();
//        break;
//    case 1://Brightness control   
//        AvailableParameters = AllAvailableParameters.Where(x => x.Type == "Brightness").ToList();

//        break;
//}