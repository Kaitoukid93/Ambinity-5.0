using System.Windows;

namespace adrilight.View
{

    public partial class DeviceSearchingDialog
    {
        public DeviceSearchingDialog()
        {
            InitializeComponent();
        }
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            var window = this.Parent as Window;
            window.DialogResult = true;
            //this.Close();
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            var window = this.Parent as Window;
            window.DialogResult = false;
            //this.Close();
        }

    }
}
