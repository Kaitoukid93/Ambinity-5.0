using adrilight.Util;
using adrilight.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeLineTool;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for ScreenCapturingControl.xaml
    /// </summary>
    public partial class CompositionSidePanel : UserControl
    {


        public CompositionSidePanel()
        {
            InitializeComponent();

            var bouncing = new TempDataType() {
                Name = "Bouncing",
                StartFrame = 5,
                EndFrame = 125,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 120
            };
           // ViewModel.AvailableMotions.Add(bouncing);
        }
      

    }
}
