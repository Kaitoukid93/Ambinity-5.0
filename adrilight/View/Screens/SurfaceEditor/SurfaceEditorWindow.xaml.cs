
using adrilight.View.Adorners;
using System;
using System.Windows;

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
