using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class PositionEditWindow
    {
        public PositionEditWindow()
        {
            InitializeComponent();
        }
        public MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (adjustingRect.Height + Canvas.GetTop(adjustingRect) > ViewModel.CanvasHeight || adjustingRect.Width + Canvas.GetLeft(adjustingRect) > ViewModel.CanvasWidth || Canvas.GetLeft(adjustingRect) < 0 || Canvas.GetTop(adjustingRect) < 0)
            {
                HandyControl.Controls.MessageBox.Show("Position and Size is out of range, Please chose another position or small down the size", "Invalid Position", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
               // ViewModel.ApplyCurrentOuputCapturingPosition();
                this.Close();
            }

        }
        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (adjustingRect.Height + Canvas.GetTop(adjustingRect) > ViewModel.CanvasHeight || adjustingRect.Width + Canvas.GetLeft(adjustingRect) > ViewModel.CanvasWidth || Canvas.GetLeft(adjustingRect) < 0 || Canvas.GetTop(adjustingRect) < 0)
            {
                HandyControl.Controls.MessageBox.Show("Position and Size is out of range, Please chose another position or small down the size", "Invalid Position", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //ViewModel.ApplyCurrentOuputCapturingPosition();
                
            }
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


            if (designerItem != null)
            {
                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange,designerItem.ActualHeight - designerItem.MinHeight);
                      
                        designerItem.Height -= deltaVertical;
                        






                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                        Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + deltaVertical);
                        designerItem.Height -= deltaVertical;
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                        Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + deltaHorizontal);
                        designerItem.Width -= deltaHorizontal;
                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange,designerItem.ActualHeight - designerItem.MinHeight);
                     
                        designerItem.Width -= deltaHorizontal;
                    



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

            if (designerItem != null)
            {
                double left = Canvas.GetLeft(designerItem);
                double top = Canvas.GetTop(designerItem);

                Canvas.SetLeft(designerItem, left + e.HorizontalChange);
                Canvas.SetTop(designerItem, top + e.VerticalChange);
            }
        }
    }

    //public class SizeAdorner : Adorner
    //{
    //    private Control chrome;
    //    private VisualCollection visuals;
    //    private ContentControl designerItem;

    //    protected override int VisualChildrenCount {
    //        get
    //        {
    //            return this.visuals.Count;
    //        }
    //    }

    //    public SizeAdorner(ContentControl designerItem)
    //        : base(designerItem)
    //    {
    //        this.SnapsToDevicePixels = true;
    //        this.designerItem = designerItem;
    //        this.chrome = new Control();
    //        this.chrome.DataContext = designerItem;
    //        this.visuals = new VisualCollection(this);
    //        this.visuals.Add(this.chrome);
    //    }

    //    protected override Visual GetVisualChild(int index)
    //    {
    //        return this.visuals[index];
    //    }

    //    protected override Size ArrangeOverride(Size arrangeBounds)
    //    {
    //        this.chrome.Arrange(new Rect(new Point(0.0, 0.0), arrangeBounds));
    //        return arrangeBounds;
    //    }
    //}
}
