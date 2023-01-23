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
    /// Interaction logic for ActionmanagerWindow.xaml
    /// </summary>
    public partial class TargetDeviceSelectionWindow
    {
        public TargetDeviceSelectionWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender,EventArgs e)
        {
            base.OnContentRendered(e);
            MoveCenterOfWindowToMousePosition();
        }

        private void MoveCenterOfWindowToMousePosition()
        {
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(GetMousePosition());
            Left = mouse.X - ActualWidth/2;
            Top = mouse.Y - ActualHeight/2;
        }

        public System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        

    }
}
