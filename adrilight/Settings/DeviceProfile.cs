using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DeviceProfile : ViewModelBase, IDeviceProfile
    {
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string ProfileUID { get; set; }
        public IOutputSettings UnionOutput { get; set; }
        public IOutputSettings[] OutputSettings { get; set; }
        public void SaveProfile(IOutputSettings[] availableOutputs)
        {
            if (OutputSettings == null)
            {
                OutputSettings = new OutputSettings[availableOutputs.Length];
                for(int i = 0; i < availableOutputs.Length; i++)
                {
                    OutputSettings[i] = new OutputSettings();
                }
            }    
               
            for (var i = 0; i < OutputSettings.Length; i++)
            {


                foreach (PropertyInfo property in OutputSettings[i].GetType().GetProperties())
                {

                    if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
                        property.SetValue(OutputSettings[i], property.GetValue(availableOutputs[i], null), null);
                }


            }
           


        }
    } 
}
