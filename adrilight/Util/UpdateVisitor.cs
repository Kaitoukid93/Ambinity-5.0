using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using Castle.Core.Logging;
using NLog;
using adrilight.ViewModel;
using System.Diagnostics;
using adrilight.Spots;
using LibreHardwareMonitor.Hardware;



namespace adrilight.Util
{
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(LibreHardwareMonitor.Hardware.IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(LibreHardwareMonitor.Hardware.IHardware hardware)
        {
            hardware.Update();
            foreach (LibreHardwareMonitor.Hardware.IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
