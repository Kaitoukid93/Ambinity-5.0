using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PIDEditWindow
    {
        private bool isLeftMouseButtonDownOnWindow = false;
        private bool isDraggingSelectionRect = false;
        private Point origMouseDownPoint;
        private double _scale = 1;
        private static readonly double DragThreshold = 1;
        public PIDEditWindow()
        {
            InitializeComponent();
            MatrixWidth.VerifyFunc = str => double.TryParse(str, out var v)
               ? v > 80
                   ? OperationResult.Failed("This LED Number is not Supported")
                   : OperationResult.Success()
               : OperationResult.Failed("This LED Number is not Supported");

            MatrixHeight.VerifyFunc = str => double.TryParse(str, out var v)
               ? v > 80
                   ? OperationResult.Failed("This LED Number is not Supported")
                   : OperationResult.Success()
               : OperationResult.Failed("This LED Number is not Supported");

        }



        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;

        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta < 0 && _scale > 0.7)
            {
                _scale -= 0.1;
                MotherGrid.LayoutTransform = new ScaleTransform(_scale, _scale);
            }
            else if (e.Delta > 0 && _scale < 1.5)
            {
                _scale += 0.1;
                MotherGrid.LayoutTransform = new ScaleTransform(_scale, _scale);
            }

        }
        private void Confirmed(object sender, EventArgs e)
        {

            this.Close();

        }
        private void PIDEditWindowClosed(object sender, CancelEventArgs e)
        {
           // ViewModel.CurrentOutput.OutputLEDSetup.Spots = ViewModel.BackupSpots;
            ViewModel.RaisePropertyChanged(nameof(ViewModel.CurrentOutput));
            //ViewModel.WriteDeviceInfoJson();
            //ViewModel.CurrentOutput.IsInSpotEditWizard = false;
            ViewModel.CurrentLEDEditWizardState = 0;

            ViewModel.Count = 0;


        }

        private void Canceled(object sender, EventArgs e)
        {


            this.Close();
        }

        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isLeftMouseButtonDownOnWindow = true;
                origMouseDownPoint = e.GetPosition(PreviewGird);

                PreviewGird.CaptureMouse();

                e.Handled = true;
            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (isDraggingSelectionRect)
                {
                    //
                    // Drag selection has ended, apply the 'selection rectangle'.
                    //

                    isDraggingSelectionRect = false;
                    ApplyDragSelectionRect();

                    e.Handled = true;
                }

                if (isLeftMouseButtonDownOnWindow)
                {
                    isLeftMouseButtonDownOnWindow = false;
                    PreviewGird.ReleaseMouseCapture();

                    e.Handled = true;
                }
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSelectionRect)
            {
                //
                // Drag selection is in progress.
                //

                Point curMouseDownPoint = e.GetPosition(PreviewGird);
                UpdateDragSelectionRect(origMouseDownPoint, curMouseDownPoint);

                e.Handled = true;
            }
            else if (isLeftMouseButtonDownOnWindow)
            {
                //
                // The user is left-dragging the mouse,
                // but don't initiate drag selection until
                // they have dragged past the threshold value.
                //
                Point curMouseDownPoint = e.GetPosition(PreviewGird);
                var dragDelta = curMouseDownPoint - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value commence drag selection.
                    //
                    isDraggingSelectionRect = true;

                    //
                    //  Clear selection immediately when starting drag selection.
                    //
                    //listBox.SelectedItems.Clear();


                    InitDragSelectionRect(origMouseDownPoint, curMouseDownPoint);
                }

                e.Handled = true;
            }
        }
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {


            UpdateDragSelectionRect(pt1, pt2);

            dragSelectionCanvas.Visibility = Visibility.Visible;
        }
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Determine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //

            ContainerVisual child = VisualTreeHelper.GetChild(PreviewBox, 0) as ContainerVisual;
            ScaleTransform scale = child.Transform as ScaleTransform;
            var offsetX = (MotherGrid.ActualWidth - PreviewBox.ActualWidth) / 2;
            var offsetY = (MotherGrid.ActualHeight - PreviewBox.ActualHeight) / 2;
            Canvas.SetLeft(dragSelectionBorder, offsetX + x * scale.ScaleX); ;
            Canvas.SetTop(dragSelectionBorder, offsetY + y * scale.ScaleY);
            dragSelectionBorder.Width = width * scale.ScaleX;
            dragSelectionBorder.Height = height * scale.ScaleY;
        }
        private void ApplyDragSelectionRect()
        {
            dragSelectionCanvas.Visibility = Visibility.Collapsed;
            ContainerVisual child = VisualTreeHelper.GetChild(PreviewBox, 0) as ContainerVisual;
            ScaleTransform scale = child.Transform as ScaleTransform;
            var offsetX = (MotherGrid.ActualWidth - PreviewBox.ActualWidth) / 2;
            var offsetY = (MotherGrid.ActualHeight - PreviewBox.ActualHeight) / 2;
            double x = (Canvas.GetLeft(dragSelectionBorder) - offsetX) / scale.ScaleX;
            double y = (Canvas.GetTop(dragSelectionBorder) - offsetY) / scale.ScaleY;
            double width = dragSelectionBorder.Width / scale.ScaleX;
            double height = dragSelectionBorder.Height / scale.ScaleY;

            Rect dragRect = new Rect(x, y, width, height);

            //
            // Inflate the drag selection-rectangle by 1/10 of its size to 
            // make sure the intended item is selected.
            //
            //dragRect.Inflate(width / 10, height / 10);

            //
            // Clear the current selection.
            ////
            //foreach (var spot in ViewModel.CurrentOutput.OutputLEDSetup.Spots)
            //{
            //    spot.SetStroke(0);
            //};
            //foreach (var spot in ViewModel.CurrentOutput.OutputLEDSetup.Spots)
            //{
            //    Rect itemRect = new Rect(spot.Rectangle.X, spot.Rectangle.Y, spot.Rectangle.Width, spot.Rectangle.Height);
            //    if (dragRect.IntersectsWith(itemRect))
            //        spot.SetStroke(0.5);

            //}

            ////
            //// Find and select all the list box items.
            ////
            //foreach (RectangleViewModel rectangleViewModel in this.ViewModel.Rectangles)
            //{
            //    Rect itemRect = new Rect(rectangleViewModel.X, rectangleViewModel.Y, rectangleViewModel.Width, rectangleViewModel.Height);
            //    if (dragRect.Contains(itemRect))
            //    {
            //        listBox.SelectedItems.Add(rectangleViewModel);
            //    }
            //}
        }


    }
}
