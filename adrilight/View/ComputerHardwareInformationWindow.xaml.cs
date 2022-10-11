using adrilight.Util;
using adrilight.ViewModel;
using HandyControl.Data;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ComputerHardwareInformationWindow
    {
        static System.Windows.Forms.Timer UpdateTicker = new System.Windows.Forms.Timer();
        public ComputerHardwareInformationWindow()
        {
            InitializeComponent();
            UpdateTicker.Tick += new EventHandler(Timer_Tick);

            // Sets the timer interval to 5 seconds.
            UpdateTicker.Interval = 1000;
            UpdateTicker.Start();
            //ViewModel.PropertyChanged += UpdateList;
            Init();

        }

      

        Util.IComputer thisComputer { get; set; }
        LibreHardwareMonitor.Hardware.Computer computer { get; set; }
        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private UpdateVisitor updateVisitor = new UpdateVisitor();
        public void Init()
        {
            computer = new LibreHardwareMonitor.Hardware.Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true

            };

            computer.Open();
            computer.Accept(updateVisitor);
            thisComputer = new Util.Computer();
            thisComputer.Processor = new List<IHardware>(); // init cpu list
            thisComputer.MotherBoard = new List<IHardware>(); // init mb list
            thisComputer.Ram = new List<IHardware>(); // init mb list
            thisComputer.GraphicCard = new List<IHardware>(); // init mb list
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                    thisComputer.Processor.Add(hardware);
                if (hardware.HardwareType == HardwareType.Motherboard)
                    thisComputer.MotherBoard.Add(hardware);
                if (hardware.HardwareType == HardwareType.Memory)
                    thisComputer.Ram.Add(hardware);
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                    thisComputer.GraphicCard.Add(hardware);
            }
            cpulist.ItemsSource = thisComputer.Processor;   
            

        }
        private void Timer_Tick(object sender, EventArgs e)
        {

            cpulist.Items.Refresh();

            computer.Accept(updateVisitor);
        }

        public void Dispose()
        {
            computer.Close();
        }



    }
}
