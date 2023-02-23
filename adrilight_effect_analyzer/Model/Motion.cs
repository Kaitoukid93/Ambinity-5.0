using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_effect_analyzer.Model
{
    public class Motion
    {
        public Motion()
        {
            Frames = new ObservableCollection<Frame>();
        }
        public ObservableCollection<Frame> Frames { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }

    }
}
