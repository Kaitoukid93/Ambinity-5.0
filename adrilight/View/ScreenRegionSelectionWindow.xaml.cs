using adrilight.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ScreenRegionSelectionWindow
    {
        public ScreenRegionSelectionWindow()
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
            if (viewModel.AvailableBitmaps == null || viewModel.AvailableBitmaps.Count == 0)
                return;
            viewModel.ClickedRegionButtonParameter.CapturingSourceIndex = sourceList.SelectedIndex;
            viewModel.CalculateAdjustingRectangle(viewModel.AvailableBitmaps[sourceList.SelectedIndex].Bitmap, viewModel.ClickedRegionButtonParameter.CapturingRegion);


        }
    }

    public class ResizeThumb : Thumb
    {

        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control designerItem = this.DataContext as Control;
            double left = Canvas.GetLeft(designerItem);
            double top = Canvas.GetTop(designerItem);
            double bottom = top + designerItem.Height;
            double right = left + designerItem.Width;
            var canvas = designerItem.Parent as Canvas;
            if (designerItem != null)
            {
                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange, designerItem.Height - designerItem.MinHeight);
                        if (bottom - deltaVertical < canvas.Height)
                        {
                            designerItem.Height -= deltaVertical;
                        }

                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, designerItem.Height - designerItem.MinHeight);
                        if (top + deltaVertical > 0)
                        {
                            designerItem.Height -= deltaVertical;
                            Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + deltaVertical);
                        }
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange, designerItem.Width - designerItem.MinWidth);
                        if (left + deltaHorizontal > 0)
                        {
                            designerItem.Width -= deltaHorizontal;
                            Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + deltaHorizontal);
                        }

                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange, designerItem.Width - designerItem.MinHeight);
                        if (right - deltaHorizontal < canvas.Width)
                        {
                            designerItem.Width -= deltaHorizontal;
                        }
                        break;
                    default:
                        break;
                }
            }

            e.Handled = true;
        }
    }


    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control designerItem = this.DataContext as Control;
            var canvas = designerItem.Parent as Canvas;
            if (designerItem != null)
            {
                double left = Canvas.GetLeft(designerItem);
                double top = Canvas.GetTop(designerItem);
                if (left + e.HorizontalChange > 0 && left + e.HorizontalChange + designerItem.Width < canvas.Width)
                {
                    Canvas.SetLeft(designerItem, left + e.HorizontalChange);
                }
                if (top + e.VerticalChange > 0 && top + e.VerticalChange + designerItem.Height < canvas.Height)
                {
                    Canvas.SetTop(designerItem, top + e.VerticalChange);
                }

            }
        }
    }




}

