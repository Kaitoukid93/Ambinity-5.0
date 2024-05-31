using adrilight_content_creator.View.Adorners;
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

namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for OutputMapingCanvansView.xaml
    /// </summary>
    public partial class OutputMapingCanvansView : UserControl
    {
        public OutputMapingCanvansView()
        {
            InitializeComponent();
        }
        private void OnScrolling(object sender, RoutedEventArgs e)
        {
            AttachedAdorner.OnScrolling();

        }
    }
}
