using adrilight.ViewModel;
using System.Windows.Controls;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AudioDeviceSelectionWindow
    {
        public AudioDeviceSelectionWindow()
        {
            InitializeComponent();

        }


        private void SourceIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewViewModel;
            if (viewModel.ClickedRegionButtonParameter == null)
                return;
            if (sourceList.SelectedIndex < 0)
                return;
            if (viewModel.AvailableBitmaps == null || viewModel.AvailableBitmaps.Count == 0)
                return;
            viewModel.ClickedAudioButtonParameter.CapturingSourceIndex = sourceList.SelectedIndex;
            //viewModel.CalculateAdjustingRectangle(viewModel.AvailableBitmaps[sourceList.SelectedIndex].Bitmap, viewModel.ClickedRegionButtonParameter.CapturingRegion);


        }


    }
}
