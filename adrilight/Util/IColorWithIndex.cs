using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IColorWithIndex
    {
        int Index { get; set; }
        Color Color { get; set; }
    }
}
