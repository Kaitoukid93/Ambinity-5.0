using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PasswordDialog
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }
        public string UnsafePassword { get; set; }

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            UnsafePassword = pw.Password;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
