
using adrilight.View.DeviceCanvas.Adorners;
using System.Windows;

namespace adrilight.View
{

    public partial class OOTBWindow
    {
        public OOTBWindow()
        {
            InitializeComponent();
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        private void Done_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();

        }
        private void OnScrolling(object sender, RoutedEventArgs e)
        {
            AttachedAdorner.OnScrolling();
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //Calculate half of the offset to move the form

            if (sizeInfo.HeightChanged)
                this.Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

            if (sizeInfo.WidthChanged)
                this.Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
        }
    }
}
