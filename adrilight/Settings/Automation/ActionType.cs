using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings.Automation
{
    public class ActionType
    {
        public string LinkText { get; set; }
        public string ToResultText { get; set; }
        public string Name { get; set; }
        public bool IsValueDisplayed { get; set; }
         public bool IsTargetDeviceDisplayed { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
    }
}
