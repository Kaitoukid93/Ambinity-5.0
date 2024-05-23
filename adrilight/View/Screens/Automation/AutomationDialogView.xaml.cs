using System.Windows;
using System.Windows.Controls;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for Delete.xaml
    /// </summary>
    public partial class AutomationDialogView : UserControl
    {
        public AutomationDialogView()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
