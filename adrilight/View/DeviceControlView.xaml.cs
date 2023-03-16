using adrilight.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for DeviceLigtingControl.xaml
    /// </summary>
    public  partial class DeviceControlView : UserControl
    {
        private bool isLeftMouseButtonDownOnWindow = false;
        private bool isDraggingSelectionRect = false;
        private Point origMouseDownPoint;
        private static readonly double DragThreshold = 1;

        public  DeviceControlView()
        {
            InitializeComponent();
           

        }

        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        //private void ButtonMode_OnClick(object sender, RoutedEventArgs e) => PopupMode.IsOpen = true;
        private void ButtonProfile_OnClick(object sender, RoutedEventArgs e) => profile_selection.IsOpen = true;
        //private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        isLeftMouseButtonDownOnWindow = true;
        //        origMouseDownPoint = e.GetPosition(PreviewGird);

        //        PreviewGird.CaptureMouse();

        //        e.Handled = true;
        //    }
        //}
        //private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        if (isDraggingSelectionRect)
        //        {
        //            //
        //            // Drag selection has ended, apply the 'selection rectangle'.
        //            //

        //            isDraggingSelectionRect = false;
        //            ApplyDragSelectionRect();

        //            e.Handled = true;
        //        }

        //        if (isLeftMouseButtonDownOnWindow)
        //        {
        //            isLeftMouseButtonDownOnWindow = false;
        //            PreviewGird.ReleaseMouseCapture();

        //            e.Handled = true;
        //        }
        //    }
        //}
        //private void Window_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (isDraggingSelectionRect)
        //    {
        //        //
        //        // Drag selection is in progress.
        //        //
                
        //        Point curMouseDownPoint = e.GetPosition(PreviewGird);
        //        UpdateDragSelectionRect(origMouseDownPoint, curMouseDownPoint);

        //        e.Handled = true;
        //    }
        //    else if (isLeftMouseButtonDownOnWindow)
        //    {
        //        //
        //        // The user is left-dragging the mouse,
        //        // but don't initiate drag selection until
        //        // they have dragged past the threshold value.
        //        //
        //        Point curMouseDownPoint = e.GetPosition(PreviewGird);
        //        var dragDelta = curMouseDownPoint - origMouseDownPoint;
        //        double dragDistance = Math.Abs(dragDelta.Length);
        //        if (dragDistance > DragThreshold)
        //        {
        //            //
        //            // When the mouse has been dragged more than the threshold value commence drag selection.
        //            //
        //            isDraggingSelectionRect = true;

        //            //
        //            //  Clear selection immediately when starting drag selection.
        //            //
        //            //listBox.SelectedItems.Clear();
                    
                    
        //            InitDragSelectionRect(origMouseDownPoint, curMouseDownPoint);
        //        }

        //        e.Handled = true;
        //    }
        //}
        //private void InitDragSelectionRect(Point pt1, Point pt2)
        //{
            

        //    UpdateDragSelectionRect(pt1, pt2);

        //    dragSelectionCanvas.Visibility = Visibility.Visible;
        //}
        //private void UpdateDragSelectionRect(Point pt1, Point pt2)
        //{
        //    double x, y, width, height;

        //    //
        //    // Determine x,y,width and height of the rect inverting the points if necessary.
        //    // 

        //    if (pt2.X < pt1.X)
        //    {
        //        x = pt2.X;
        //        width = pt1.X - pt2.X;
        //    }
        //    else
        //    {
        //        x = pt1.X;
        //        width = pt2.X - pt1.X;
        //    }

        //    if (pt2.Y < pt1.Y)
        //    {
        //        y = pt2.Y;
        //        height = pt1.Y - pt2.Y;
        //    }
        //    else
        //    {
        //        y = pt1.Y;
        //        height = pt2.Y - pt1.Y;
        //    }

        //    //
        //    // Update the coordinates of the rectangle used for drag selection.
        //    //

        //    ContainerVisual child = VisualTreeHelper.GetChild(PreviewBox, 0) as ContainerVisual;
        //    ScaleTransform scale = child.Transform as ScaleTransform;
        //    var offsetX = (MotherGrid.ActualWidth - PreviewBox.ActualWidth) / 2;
        //    var offsetY = (MotherGrid.ActualHeight - PreviewBox.ActualHeight) / 2;
        //    Canvas.SetLeft(dragSelectionBorder,offsetX+ x * scale.ScaleX); ;
        //    Canvas.SetTop(dragSelectionBorder, offsetY+ y * scale.ScaleY);
        //    dragSelectionBorder.Width = width*scale.ScaleX;
        //    dragSelectionBorder.Height = height*scale.ScaleY;
        //}
        //private void ApplyDragSelectionRect()
        //{
        //    dragSelectionCanvas.Visibility = Visibility.Collapsed;
        //    ContainerVisual child = VisualTreeHelper.GetChild(PreviewBox, 0) as ContainerVisual;
        //    ScaleTransform scale = child.Transform as ScaleTransform;
        //    var offsetX = (MotherGrid.ActualWidth - PreviewBox.ActualWidth) / 2;
        //    var offsetY = (MotherGrid.ActualHeight - PreviewBox.ActualHeight) / 2;
        //    double x = (Canvas.GetLeft(dragSelectionBorder)-offsetX)/scale.ScaleX;
        //    double y = (Canvas.GetTop(dragSelectionBorder)-offsetY)/scale.ScaleY;
        //    double width = dragSelectionBorder.Width/scale.ScaleX;
        //    double height = dragSelectionBorder.Height/scale.ScaleY;
            
        //    Rect dragRect = new Rect(x, y, width, height);

        //    //
        //    // Inflate the drag selection-rectangle by 1/10 of its size to 
        //    // make sure the intended item is selected.
        //    //
        //    //dragRect.Inflate(width / 10, height / 10);

        //    //
        //    // Clear the current selection.
        //    ////
        //    foreach(var spot in ViewModel.CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        spot.SetStroke(0);
        //    };
        //    foreach (var spot in ViewModel.CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        Rect itemRect = new Rect(spot.Rectangle.X, spot.Rectangle.Y, spot.Rectangle.Width, spot.Rectangle.Height);
        //        if (dragRect.IntersectsWith(itemRect))
        //            spot.SetStroke(0.5);

        //    }

        //    ////
        //    //// Find and select all the list box items.
        //    ////
        //    //foreach (RectangleViewModel rectangleViewModel in this.ViewModel.Rectangles)
        //    //{
        //    //    Rect itemRect = new Rect(rectangleViewModel.X, rectangleViewModel.Y, rectangleViewModel.Width, rectangleViewModel.Height);
        //    //    if (dragRect.Contains(itemRect))
        //    //    {
        //    //        listBox.SelectedItems.Add(rectangleViewModel);
        //    //    }
        //    //}
        //}



        

       

        //private void NewModeSelected(object sender, SelectionChangedEventArgs e) => PopupMode.IsOpen = false;

        private void ChangeOutputMode(object sender, RoutedEventArgs e)
        {
           // ViewModel.CurrentDevice.IsUnionMode = !ViewModel.CurrentDevice.IsUnionMode;
            
        }

        private void RenameButtonClick(object sender, RoutedEventArgs e)
        {
            RenamePopup.IsOpen = true;
        }

        private void ChangeCurrentDeviceName(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentDevice.DeviceName = RenameTextBox.Text;
            RenamePopup.IsOpen = false;
        }

        private void BrightnessButton_OnClick(object sender, RoutedEventArgs e)
        {
           
        }

        private void outputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ViewModel.CurrentDevice.AvailableOutputs[ViewModel.CurrentDevice.SelectedOutput] != null)
            //{
            //    outputList.UpdateLayout();
            //    outputList.ScrollIntoView(ViewModel.CurrentDevice.AvailableOutputs[ViewModel.CurrentDevice.SelectedOutput]);
            //}
        }
    }
}
