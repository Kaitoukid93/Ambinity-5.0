

using adrilight.ViewModel;
using adrilight_shared.Models.Drawable;
using System.Windows.Controls;
using Border = System.Windows.Controls.Border;


namespace adrilight.View.DeviceCanvas
{
    /// <summary>
    /// Interaction logic for RichCavnas.xaml
    /// </summary>
    public partial class LayerView : UserControl
    {
        public LayerView()
        {
            InitializeComponent();
        }
        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var element = (Border)sender;
            var item = element.DataContext as IDrawable;
            if (item != null)
            {
                item.IsMouseOver = true;
            }
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var element = (Border)sender;
            var item = element.DataContext as IDrawable;
            if (item != null)
            {
                item.IsMouseOver = false;
            }
        }

        private void Border_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var element = (Border)sender;
            element.BringIntoView();
            //bring into view
        }
    }
}
