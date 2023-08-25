using adrilight.Helpers;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
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
        IOutputSettings SelectedOutput { get; set; }
        //private void NewModeSelected(object sender, SelectionChangedEventArgs e) => PopupMode.IsOpen = false;
        //private void ButtonMode_OnClick(object sender, RoutedEventArgs e) => PopupMode.IsOpen = true;
        private void Done_Button_Click(object sender, RoutedEventArgs e)
        {
            //SelectedOutput = (IOutputSettings)outputList.SelectedItem;
            DialogResult = true;

            // close this dialog
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
