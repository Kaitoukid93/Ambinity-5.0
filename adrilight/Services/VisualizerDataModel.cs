using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight.Util
{
    public class VisualizerDataModel : ViewModelBase
    {
        public VisualizerDataModel(int numFreq, string name)
        {
            Columns = new ObservableCollection<ColumnDataModel>();
            for (int i = 0; i < numFreq; i++)
            {
                Columns.Add(new ColumnDataModel() { Index = i, Value = 0 });
            }
            Name = name;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<ColumnDataModel> Columns { get; set; }

    }
}
