using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public class PaletteCardContextMenu
    {
        public string Name { get; set; }
        public List<PaletteCardContextMenu> MenuItem { get; set; }

        public PaletteCardContextMenu(string name)
        {
            Name = name;
        }
    }
}
