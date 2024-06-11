using HandyControl.Controls;
using System;
using System.Collections.Generic;
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

namespace adrilight_shared.View.CustomControls
{
    /// <summary>
    /// Interaction logic for SimpleColorPicker.xaml
    /// </summary>
    public partial class SimpleColorDropper : UserControl, ISingleOpen
    {
        public SimpleColorDropper()
        {
            InitializeComponent();
        }

        public bool CanDispose => true;

        public void Dispose()
        {
            
        }

    }
}
