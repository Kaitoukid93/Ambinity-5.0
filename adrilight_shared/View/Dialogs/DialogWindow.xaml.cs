using System.Windows;

namespace adrilight_shared.View.Dialogs
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
        }
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            //this.Close();
        }
    }
}
