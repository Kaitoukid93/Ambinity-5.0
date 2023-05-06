using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public class VisualizerDataModel : ViewModelBase
    {
        public VisualizerDataModel(int numFreq)
        {
            Columns = new ObservableCollection<ColumnDataModel>();
            for(int i=0;i<numFreq;i++)
            {
                Columns.Add(new ColumnDataModel() { Index = i, Value = 0 });
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<ColumnDataModel> Columns { get; set; }

    }
}
