using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;

namespace TimeLineTool
{
    //public class TimeLineItemControl:ContentPresenter
    public class TimeLineItemControl : Button
    {
        private Boolean _ready = true;
        internal Boolean ReadyToDraw
        {
            get { return _ready; }
            set
            {
                _ready = value;
            }
        }



        public Boolean IsExpanded
        {
            get { return (Boolean)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(Boolean), typeof(TimeLineItemControl), new UIPropertyMetadata(false));

        public Boolean IsSelected
        {
            get { return (Boolean)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(Boolean), typeof(TimeLineItemControl), new UIPropertyMetadata(false));

        #region unitsize
        public Double UnitSize
        {
            get { return (Double)GetValue(UnitSizeProperty); }
            set { SetValue(UnitSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register("UnitSize", typeof(Double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(5.0,
                    new PropertyChangedCallback(OnUnitSizeChanged)));

        private static void OnUnitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            TimeLineItemControl ctrl = d as TimeLineItemControl;
            if (ctrl != null)
            {
                ctrl.PlaceOnCanvas();
            }

        }
        #endregion

        #region ViewLevel
        public TimeLineViewLevel ViewLevel
        {
            get { return (TimeLineViewLevel)GetValue(ViewLevelProperty); }
            set { SetValue(ViewLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewLevelProperty =
            DependencyProperty.Register("ViewLevel", typeof(TimeLineViewLevel), typeof(TimeLineItemControl),
            new UIPropertyMetadata(TimeLineViewLevel.Frame,
                new PropertyChangedCallback(OnViewLevelChanged)));
        private static void OnViewLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineItemControl ctrl = d as TimeLineItemControl;
            if (ctrl != null)
            {
                ctrl.PlaceOnCanvas();
            }


        }
        #endregion

        #region timeline start time
        public double TimeLineStartFrame
        {
            get { return (double)GetValue(TimeLineStartFrameProperty); }
            set { SetValue(TimeLineStartFrameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeLineStartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeLineStartFrameProperty =
            DependencyProperty.Register("TimeLineStartFrame", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(0.0,
                new PropertyChangedCallback(OnTimeValueChanged)));
        #endregion

        #region start time

        public double StartFrame
        {
            get { return (double)GetValue(StartFrameProperty); }
            set { SetValue(StartFrameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartFrameProperty =
            DependencyProperty.Register("StartFrame", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(0.0,
                new PropertyChangedCallback(OnTimeValueChanged)));


        #endregion

        #region end time
        public double EndFrame
        {
            get { return (double)GetValue(EndFrameProperty); }
            set { SetValue(EndFrameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndFrameProperty =
            DependencyProperty.Register("EndFrame", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(30.0,
                                    new PropertyChangedCallback(OnTimeValueChanged)));



        #endregion

        #region trim start
        public double TrimStart
        {
            get { return (double)GetValue(TrimStartProperty); }
            set { SetValue(TrimStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrimStartProperty =
            DependencyProperty.Register("TrimStart", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(0.0,
                                    new PropertyChangedCallback(OnTimeValueChanged)));



        #endregion
        #region trim end
        public double TrimEnd
        {
            get { return (double)GetValue(TrimEndProperty); }
            set { SetValue(TrimEndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrimEndProperty =
            DependencyProperty.Register("TrimEnd", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(0.0,
                                    new PropertyChangedCallback(OnTimeValueChanged)));



        #endregion
        #region original  duration
        public double OriginalDuration
        {
            get { return (double)GetValue(OriginalDurationProperty); }
            set { SetValue(OriginalDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginalDurationProperty =
            DependencyProperty.Register("OriginalDuration", typeof(double), typeof(TimeLineItemControl),
            new UIPropertyMetadata(0.0,
                                    new PropertyChangedCallback(OnTimeValueChanged)));



        #endregion


      
        public Double EditBorderThreshold
        {
            get { return (Double)GetValue(EditBorderThresholdProperty); }
            set { SetValue(EditBorderThresholdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditBorderThreshold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditBorderThresholdProperty =
            DependencyProperty.Register("EditBorderThreshold", typeof(Double), typeof(TimeLineItemControl), new UIPropertyMetadata(4.0, new PropertyChangedCallback(OnEditThresholdChanged)));

        private static void OnEditThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineItemControl ctrl = d as TimeLineItemControl;



        }


        private static void OnTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLineItemControl ctrl = d as TimeLineItemControl;
            if (ctrl != null)
                ctrl.PlaceOnCanvas();
        }

        internal void PlaceOnCanvas()
        {
            var w = CalculateWidth();
            if (w > 0)
                Width = w;
            var p = CalculateLeftPosition();
            if (p >= 0)
            {
                Canvas.SetLeft(this, p);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }
        private ContentPresenter _LeftIndicator;
        private ContentPresenter _RightIndicator;
        public override void OnApplyTemplate()
        {
            _LeftIndicator = Template.FindName("PART_LeftIndicator", this) as ContentPresenter;
            _RightIndicator = Template.FindName("PART_RightIndicator", this) as ContentPresenter;
            if (_LeftIndicator != null)
                _LeftIndicator.Visibility = System.Windows.Visibility.Collapsed;
            if (_RightIndicator != null)
                _RightIndicator.Visibility = System.Windows.Visibility.Collapsed;
            base.OnApplyTemplate();
        }


        internal Double CalculateWidth()
        {
            try
            {

                double start = (double)GetValue(StartFrameProperty);
                double end = (double)GetValue(EndFrameProperty);
                double duration = end - start;
                return ConvertFrameToDistance((double)duration);
            }
            catch (Exception)
            {

                return 0;
            }

        }

        internal Double CalculateLeftPosition()
        {
            double start = (double)GetValue(StartFrameProperty);
            double timelinestart = (double)GetValue(TimeLineStartFrameProperty);

            double Duration = start - timelinestart;
            return ConvertFrameToDistance((double)Duration);
        }


        #region conversion utilities
        private Double ConvertFrameToDistance(double duration)
        {

            TimeLineViewLevel lvl = (TimeLineViewLevel)GetValue(ViewLevelProperty);
            Double unitSize = (Double)GetValue(UnitSizeProperty);
            Double value = unitSize;
            switch (lvl)
            {
                case TimeLineViewLevel.Frame:
                    value = duration * unitSize;
                    break;
                case TimeLineViewLevel.Frame5:
                    value = (duration / 5.0) * unitSize;
                    break;
                case TimeLineViewLevel.Frame10:
                    value = (duration / 10.0) * unitSize;
                    break;
                case TimeLineViewLevel.Frame20:
                    value = (duration / 20.0) * unitSize;
                    break;
                case TimeLineViewLevel.Frame50:
                    value = (duration / 50.0) * unitSize;
                    break;
                case TimeLineViewLevel.Frame100:
                    value = (duration / 100.0) * unitSize;
                    break;
                default:
                    break;
            }
            return value;


        }

        private double ConvertDistanceToFrame(Double distance)
        {

            TimeLineViewLevel lvl = (TimeLineViewLevel)GetValue(ViewLevelProperty);
            Double unitSize = (Double)GetValue(UnitSizeProperty);
            double frame, frame5, frame10, frame20, frame50, frame100, totalframe = 0;

            switch (lvl)
            {
                case TimeLineViewLevel.Frame:
                    //value = span.TotalMinutes * unitSize;
                    frame = (distance / unitSize);
                    //convert to milliseconds
                    totalframe = frame;
                    break;
                case TimeLineViewLevel.Frame5:
                    frame5 = (distance / unitSize);
                    //convert to milliseconds
                    totalframe = frame5;
                    break;
                case TimeLineViewLevel.Frame10:
                    frame10 = (distance / unitSize);
                    //convert to milliseconds
                    totalframe = frame10;
                    break;
                case TimeLineViewLevel.Frame20:
                    //value = (span.TotalDays / 7.0) * unitSize;
                    frame20 = (distance / unitSize);
                    //convert to milliseconds
                    totalframe = frame20;
                    break;
                case TimeLineViewLevel.Frame50:
                    frame50 = (distance / unitSize); ;
                    //convert to milliseconds
                    totalframe = frame50;
                    break;
                case TimeLineViewLevel.Frame100:
                    frame100 = (distance / unitSize);
                    //convert to milliseconds
                    totalframe = frame100;
                    break;
                default:
                    break;
            }
            return totalframe;

            //return new TimeSpan(0, 0, 0, 0, (int)milliseconds);


        }

        #endregion
        private void SetIndicators(System.Windows.Visibility left, System.Windows.Visibility right)
        {
            if (_LeftIndicator != null)
            {
                _LeftIndicator.Visibility = left;
            }
            if (_RightIndicator != null)
            {
                _RightIndicator.Visibility = right;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            switch (GetClickAction())
            {

                case TimeLineAction.StretchStart:
                    SetIndicators(System.Windows.Visibility.Visible, System.Windows.Visibility.Collapsed);
                    break;
                case TimeLineAction.StretchEnd:
                    SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Visible);
                    //this.Cursor = Cursors.SizeWE;//Cursors.Hand;//Cursors.ScrollWE;
                    break;
                default:
                    SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed);
                    break;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            SetIndicators(System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed);
            if (IsExpanded && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
            {
                return;
            }
            IsExpanded = false;
            base.OnMouseLeave(e);
        }

        #region manipulation tools
        internal TimeLineAction GetClickAction()
        {
            var X = Mouse.GetPosition(this).X;
            Double borderThreshold = (Double)GetValue(EditBorderThresholdProperty);// 4;
            Double unitsize = (Double)GetValue(UnitSizeProperty);

            if (X < borderThreshold)
                return TimeLineAction.StretchStart;
            if (X > Width - borderThreshold)
                return TimeLineAction.StretchEnd;
            return TimeLineAction.Move;

        }

        internal bool CanDelta(int StartOrEnd, Double deltaX)
        {

            Double unitS = (Double)GetValue(UnitSizeProperty);
            Double threshold = unitS / 3.0;
            Double newW = unitS;
            if (StartOrEnd == 0)//we are moving the start
            {
                if (deltaX < 0)
                    return true;
                //otherwises get what our new width would be
                newW = Width - deltaX;//delta is + but we are actually going to shrink our width by moving start +
                return newW > threshold;
            }
            else
            {
                if (deltaX > 0)
                    return true;
                newW = Width + deltaX;
                return newW > threshold;
            }
        }

        internal double GetDeltaFrame(double deltaX)
        {
            return ConvertFrameToDistance(deltaX);
        }

        internal void GetPlacementInfo(ref Double left, ref Double width, ref Double end)
        {
            left = Canvas.GetLeft(this);
            width = Width;
            end = left + Width;
            //Somewhere on the process of removing a timeline control from the visual tree
            //it resets our start time to min value.  In that case it then results in ridiculous placement numbers
            //that this feeds to the control and crashes the whole app in a strange way.
            //if (TimeLineStartFrame == 0)//minvalue
            //{
            //    left = 0;
            //    width = 1;
            //    end = 1;
            //}
        }

        internal void MoveMe(Double deltaX)
        {

            var left = Canvas.GetLeft(this);
            left += deltaX;
            if (left < 0)
                left = 0;
            Canvas.SetLeft(this, left);

            double startTs = ConvertDistanceToFrame(left);
            double tlStart = TimeLineStartFrame;
            double s = StartFrame;
            double e = EndFrame;
            double duration = e - s;
            if (duration > OriginalDuration)
                duration = OriginalDuration;
            double val = tlStart + startTs;
            val = val * 100;
            val = Math.Round(val);
            val = val / 100;
            StartFrame = val;
            double val2 = StartFrame + duration;
            val2 = val2 * 100;
            val2 = Math.Round(val2);
            val2 = val2 / 100;
            EndFrame = val2;
           

        }
        #endregion

        internal void MoveEndTime(double delta)
        {
            if (delta < 0)
            {
                if (Width > 1 * UnitSize)
                {
                    Width += delta;

                    //calculate our new end time
                    double s = (double)GetValue(StartFrameProperty);
                    double ts = ConvertDistanceToFrame(Width);
                    double oldEndFrame = (Double)GetValue(EndFrameProperty);
                    double val = s + ts;
                    val = val * 100;
                    val = Math.Round(val);
                    val = val / 100;
                    EndFrame = val;
                    TrimEnd += oldEndFrame - EndFrame;
                    if (TrimEnd > OriginalDuration)
                        TrimEnd = OriginalDuration;
                   // TrimEnd = Math.Round(TrimEnd);
                }
            }
            else
            {


                if (Width < OriginalDuration * UnitSize)
                {
                    Width += delta;

                    //calculate our new end time
                    double s = (double)GetValue(StartFrameProperty);
                    double ts = ConvertDistanceToFrame(Width);
                    double oldEndFrame = (Double)GetValue(EndFrameProperty);
                    double val = s+ts;
                    val = val * 100;
                    val = Math.Round(val);
                    val = val / 100;
                    if (val - s > (double)GetValue(OriginalDurationProperty))
                        val = s + (double)GetValue(OriginalDurationProperty);
                    EndFrame = val;
                    TrimEnd -= EndFrame - oldEndFrame;
                    if (TrimEnd < 0)
                        TrimEnd = 0;
                    //TrimEnd = Math.Round(TrimEnd);
                }

            }


        }

        internal void MoveStartTime(double delta)
        {
            if (delta > 0)
            {
                if (Width > 1 * UnitSize)
                {
                    Double curLeft = Canvas.GetLeft(this);
                    if (curLeft == 0 && delta < 0)
                        return;
                    curLeft += delta;
                    Width = Width - delta;
                    if (curLeft < 0)
                    {
                        //we need to 
                        Width -= curLeft;//We are moving back to 0 and have to fix our width to not bump a bit.
                        curLeft = 0;
                    }
                    Canvas.SetLeft(this, curLeft);
                    //recalculate start time;
                    double ts = ConvertDistanceToFrame(curLeft);
                    double oldStartFrame = (Double)GetValue(StartFrameProperty);
                    double val = TimeLineStartFrame + ts;
                    val = val * 100;
                    val = Math.Round(val);
                    val = val / 100;
                    if ((double)GetValue(EndFrameProperty)-val > (double)GetValue(OriginalDurationProperty))
                        val = (double)GetValue(EndFrameProperty) - (double)GetValue(OriginalDurationProperty);
                    StartFrame = val;
                    TrimStart += StartFrame - oldStartFrame;
                    if (TrimStart > OriginalDuration)
                        TrimStart = OriginalDuration;
                    // TrimStart = Math.Round(TrimStart);
                }
            }
            else
            {
                if (Width < OriginalDuration * UnitSize)
                {
                    Double curLeft = Canvas.GetLeft(this);
                    if (curLeft == 0 && delta < 0)
                        return;
                    curLeft += delta;
                    Width = Width - delta;
                    if (curLeft < 0)
                    {
                        //we need to 
                        Width -= curLeft;//We are moving back to 0 and have to fix our width to not bump a bit.
                        curLeft = 0;
                    }
                    Canvas.SetLeft(this, curLeft);
                    //recalculate start time;
                    double ts = ConvertDistanceToFrame(curLeft);
                    double oldStartFrame = (Double)GetValue(StartFrameProperty);
                    double val = TimeLineStartFrame + ts;
                    val = val * 100;
                    val = Math.Round(val);
                    val = val / 100;
                    StartFrame = val;
                    TrimStart -= oldStartFrame - StartFrame;
                    if (TrimStart < 0)
                        TrimStart = 0;
                    // TrimStart = Math.Round(TrimStart);
                }
            }


        }

        internal void MoveToNewStartTime(double start)
        {
            double s = (double)GetValue(StartFrameProperty);
            double e = (double)GetValue(EndFrameProperty);
            double duration = e - s;
            StartFrame = start;
            EndFrame = start + duration;
            PlaceOnCanvas();

        }
        /// <summary>
        /// Sets up with a default of 55 of our current units in size.
        /// </summary>
        internal void InitializeDefaultLength()
        {
            double duration = ConvertDistanceToFrame(OriginalDuration * (Double)GetValue(UnitSizeProperty));
            EndFrame = StartFrame + duration;
            Width = CalculateWidth();
        }
    }
}