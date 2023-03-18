using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IColorPalette
    {
        string Name { get; set; }
        string Owner { get; set; }
        System.Windows.Media.Color[] Colors { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        string FormatVersion { get; set; }
        void SetColor(int index, Color color);
    }
}
