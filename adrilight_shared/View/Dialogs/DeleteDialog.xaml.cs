using System.Windows;
using System.Windows.Controls;

namespace adrilight_shared.View.Dialogs
{
    /// <summary>
    /// Interaction logic for Delete.xaml
    /// </summary>
    public partial class DeleteDialog : UserControl
    {
        public DeleteDialog()
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
