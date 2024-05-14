using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight_shared.Models.ItemsCollection
{
    //data model for tool button to interact with item(s) in collection
    public class CollectionItemTool
    {
        public CollectionItemTool() { }
        public string Name { get; set; }
        public string Geometry { get; set; }
        public string ToolTip { get; set; }
        public string CommandParameter { get; set; }
        public ICommand Command { get; set; }
    }
}
