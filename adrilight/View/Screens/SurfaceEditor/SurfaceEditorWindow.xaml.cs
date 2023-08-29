using adrilight.Settings;
using adrilight.View.Adorners;
using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
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
    public partial class SurfaceEditorWindow
    {
        public SurfaceEditorWindow()
        {
            
            InitializeComponent();
           
        }
    
        private void OnScrolling(object sender, RoutedEventArgs e)
        { 
            AttachedAdorner.OnScrolling();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);


            NonClientAreaContent = new SurfaceEditorNonClientAreaContent();

        }

    }
}
