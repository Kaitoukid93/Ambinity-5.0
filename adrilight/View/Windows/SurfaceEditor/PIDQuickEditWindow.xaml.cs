using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PIDQuickEditWindow
    {
        public PIDQuickEditWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);


            NonClientAreaContent = new PIDQuickEditNonClientAreaContent();

        }

    }
}
