using adrilight_shared.Models.ControlMode.ModeParameters;
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

namespace adrilight.View.Template
{
    /// <summary>
    /// Interaction logic for ListSelectionParamTemplate.xaml
    /// </summary>
    public partial class ButtonParamTemplate : UserControl
    {
        public ButtonParamTemplate()
        {
            InitializeComponent();
        }
        private void GroupBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grBx = (Border)sender;
            var dataCntx = grBx.DataContext;
            var dataSource = (ListSelectionParameter)dataCntx;
            if (dataSource != null)
            {
                if (dataSource.ShowMore)
                    dataSource.ShowMore = false;
                else
                    dataSource.ShowMore = true;
            }
        }
    }
}
