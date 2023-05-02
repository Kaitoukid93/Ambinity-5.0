using adrilight.ViewModel;
using HandyControl.Data;
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
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PaletteEditWindow 
    { private string _mode;
        public PaletteEditWindow(string mode)
        {
            InitializeComponent();
            _mode = mode;
        }
        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;

        }

        private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            //switch (_mode)
            //{
            //    case "overwrite":
            //        this.Close();
            //        break;
            //    case "createnew":
            //        ViewModel.OpenCreateNewDialog();
            //        this.Close();
            //        break;
                    
            //}

            
        }

        private void mColorPicker_Canceled(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
