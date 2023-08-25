using adrilight.Settings;
using adrilight.ViewModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;


namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class GifRegionSelectionWindow
    {
        public GifRegionSelectionWindow()
        {
            InitializeComponent();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (adjustingRect.Height + Canvas.GetTop(adjustingRect) > ViewModel.CanvasHeight || adjustingRect.Width + Canvas.GetLeft(adjustingRect) > ViewModel.CanvasWidth || Canvas.GetLeft(adjustingRect) < 0 || Canvas.GetTop(adjustingRect) < 0)
            //{
            //    HandyControl.Controls.MessageBox.Show("Position and Size is out of range, Please chose another position or small down the size", "Invalid Position", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //else
            //{
            //    ViewModel.ApplyCurrentOuputCapturingPosition();
            //    this.Close();
            // }

        }
        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //if (adjustingRect.Height + Canvas.GetTop(adjustingRect) > ViewModel.CanvasHeight || adjustingRect.Width + Canvas.GetLeft(adjustingRect) > ViewModel.CanvasWidth || Canvas.GetLeft(adjustingRect) < 0 || Canvas.GetTop(adjustingRect) < 0)
            //{
            //    HandyControl.Controls.MessageBox.Show("Position and Size is out of range, Please chose another position or small down the size", "Invalid Position", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //else
            //{
            //    ViewModel.ApplyCurrentOuputCapturingPosition();

            //}
        }

        private void SourceIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewViewModel;
            if (viewModel.ClickedRegionButtonParameter == null)
                return;
            if (sourceList.SelectedIndex < 0)
                return;
            viewModel.ClickedRegionButtonParameter.CapturingSourceIndex = sourceList.SelectedIndex;
            var selectedBitmap = new Bitmap((sourceList.SelectedItem as GifCard).Path);
            viewModel.CalculateAdjustingRectangle(selectedBitmap, viewModel.ClickedRegionButtonParameter.CapturingRegion);
            selectedBitmap.Dispose();

        }
    }









}

