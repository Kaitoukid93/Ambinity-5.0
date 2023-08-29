using System.Collections.ObjectModel;

namespace adrilight_shared.Models.CompositionData
{
    public class Composition // for displaying motion at rainbow control panel
    {


        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Source { get; set; } // to load motion


        //timeline data item inheritance
        public ObservableCollection<MotionLayer> Layers { get; set; }
    }
}
