using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Rectangle = System.Drawing.Rectangle;

namespace adrilight.Spots
{
    [DebuggerDisplay("Spot: Rectangle={Rectangle}, Color={Red},{Green},{Blue}")]
    public class DeviceSpot : ViewModelBase, IDeviceSpot, IDrawable
    {

        public DeviceSpot(double top, double left, double width, double height, double scaleTop, double scaleLeft, double scaleWidth, double scaleHeight, int index, int positionIndex, int virtualIndex, int musicIndex, int columnIndex, bool isActivated, Geometry geometry)
        {
            Top = top;
            Left = left;
            ActualWidth = width;
            ActualHeight = height;

            Geometry = geometry;


            Index = index;
            VID = virtualIndex;
            MID = musicIndex;
            CID = columnIndex;

            PID = positionIndex;
            IsActivated = isActivated;
            VisualProperties = new VisualProperties();
            Scale = new System.Windows.Point(1, 1);


        }
        public DeviceSpot()
        {
            VisualProperties = new VisualProperties();
            Scale = new System.Windows.Point(1, 1);
        }
        [JsonIgnore]
        public Type DataType => typeof(DeviceSpot);
        public int Index { get; set; } // Physical index

        private bool _isFirst;
        public bool IsFirst {
            get => _isFirst;
            set { Set(() => IsFirst, ref _isFirst, value); }
        }
        public int MID { get; set; }
        private Geometry _geometry;
        public Geometry Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public Color OnDemandColor => Color.FromRgb(Red, Green, Blue);
        public Color SentryColor => Color.FromRgb(SentryRed, SentryGreen, SentryBlue);



        public int VID { get; set; }
        public bool HasVID { get; set; }
        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public int PID { get; set; }
        public int CID { get; set; }
        public int BackupID { get; set; }
        public bool IsActivated { get; set; }
        public byte SentryRed { get; private set; }
        public byte SentryGreen { get; private set; }
        public byte SentryBlue { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
        public Rectangle GetRect => new Rectangle((int)(Left), (int)(Top), (int)Width, (int)Height);
        public bool GetVIDIfNeeded(int vid, Rectangle rect, int mode)
        {
            if (mode == 0)
            {
                if (!HasVID)
                {
                    var intersectRect = Rectangle.Intersect(GetRect, rect);
                    double intersectArea = intersectRect.Width * intersectRect.Height;
                    double spotArea = GetRect.Width* GetRect.Height;
                    if ((intersectArea / spotArea) > 0.1)
                    {
                        SetVID(vid);
                        HasVID = true;
                        return true;
                    }

                }
            }
            else if (mode == 1)
            {
                if (HasVID)
                {
                    var intersectRect = Rectangle.Intersect(GetRect, rect);
                    if (intersectRect.Width * intersectRect.Height / (GetRect.Width * GetRect.Height) > 0.1)
                    {
                        SetVID(0);
                        HasVID = false;
                        return true;
                    }

                }
            }

            return false;

        }
        public void SetColor(byte red, byte green, byte blue, bool raiseEvents)
        {
            Red = red;
            Green = green;
            Blue = blue;
            _lastMissingValueIndication = null;

            if (raiseEvents)
            {
                RaisePropertyChanged(nameof(OnDemandColor));
               
            }
        }
        public void SetSentryColor(byte red, byte green, byte blue)
        {
            SentryRed = red;
            SentryGreen = green;
            SentryBlue = blue;

        }
        public void SetVID(int vid)
        {
            VID = vid;
            RaisePropertyChanged(nameof(VID));

        }
        public void SetMID(int mid)
        {
            MID = mid;
            RaisePropertyChanged(nameof(MID));
        }
        public void SetID(int index)
        {
            Index = index;

            RaisePropertyChanged(nameof(Index));
        }
        public void SetCID(int ID)
        {
            CID = ID;

            RaisePropertyChanged(nameof(CID));


        }

        private DateTime? _lastMissingValueIndication;
        private readonly double _dimToBlackIntervalInMs = TimeSpan.FromMilliseconds(900).TotalMilliseconds;

        private float _dimR, _dimG, _dimB;

        public void DimLED(float dimFactor)
        {

            SetColor((byte)(dimFactor * Red), (byte)(dimFactor * Green), (byte)(dimFactor * Blue), true);
        }

        private double _top = 0;
        private double _left = 0;
        private bool _isSelected;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _width = 100;
        private double _height = 100;
        private VisualProperties _visualProperties;
        private bool _shouldBringIntoView;
        private System.Windows.Point _directionPoint;
        private RelayCommand<double> leftChangedCommand;
        private RelayCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;
        private string _name;
        private double _actualWidth;
        private double _actualHeight;
        private bool _isResizeable;
        private bool _isDeleteable;
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }

        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }
        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }
        public double ActualWidth { get => _actualWidth; set { Set(() => ActualWidth, ref _actualWidth, value); } }

        public double ActualHeight { get => _actualHeight; set { Set(() => ActualHeight, ref _actualHeight, value); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public System.Windows.Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {


            var width = ActualWidth * scaleX;
            var height = ActualHeight * scaleY;
            if (width < 1 || height < 1)
            {
                return false;
            }
            else
            {
                ActualWidth *= scaleX;
                ActualHeight *= scaleY;
                if (!keepOrigin)
                {
                    Left *= scaleX;
                    Top *= scaleY;
                }
            }
            return true;


        }




        public void OnLeftChanged(double delta) { }

        public void OnTopChanged(double delta) { }

        public void OnWidthUpdated() { }

        public void OnHeightUpdated() { }

        public void OnRotationChanged() { }

        public void OnIsSelectedChanged(bool value)
        {

            if (value)
            {
                SetColor(255, 0, 0, true);
            }
            else
            {
                SetColor(0, 0, 0, true);
            }

        }

        public void OnDrawingEnded(Action<object> callback = default) { }
    }
}

