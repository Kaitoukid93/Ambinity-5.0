
using System.Windows;


namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class HotKeySelectionDialog
    {
        public HotKeySelectionDialog()
        {
            InitializeComponent();

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var window = this.Parent as Window;
            window.DialogResult = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            var window = this.Parent as Window;
            window.DialogResult = false;
        }
    }
}
