using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Language
{
    public class LangModel
    {
        public LangModel() { }
        public LangModel(CultureInfo culture, string name, string geometry)
        {
            Name = name;
            FlagGeometry = geometry;
            Culture = culture;

        }
        public string Name { get; set; }
        public CultureInfo Culture { get; set; }
        public string FlagGeometry { get; set; }
    }
}
