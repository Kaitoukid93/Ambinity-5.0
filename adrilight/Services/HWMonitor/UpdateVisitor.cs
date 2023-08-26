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
