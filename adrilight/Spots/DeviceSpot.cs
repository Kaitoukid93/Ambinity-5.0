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
using Color = System.Windows.Media.Color;

namespace adrilight.Spots
{
    [DebuggerDisplay("Spot: Rectangle={Rectangle}, Color={Red},{Green},{Blue}")]
    sealed class DeviceSpot : ViewModelBase, IDeviceSpot, IDrawable
    {

        public DeviceSpot(double top, double left, double width, double height, double scaleTop, double scaleLeft, double scaleWidth, double scaleHeight, int index, int positionIndex, int virtualIndex, int musicIndex, int columnIndex, bool isActivated, string shape)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
    
            Shape = shape;


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
        public string Shape { get; set; }
        public Color OnDemandColor => Color.FromRgb(Red, Green, Blue);
        public Color SentryColor => Color.FromRgb(SentryRed, SentryGreen, SentryBlue);
        public Color OnDemandColorTransparent => Color.FromArgb(255, Red, Green, Blue);



        public int VID { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int PID { get; set; }
        public int CID { get; set; }
        public bool IsActivated { get; set; }
        public byte SentryRed { get; private set; }
        public byte SentryGreen { get; private set; }
        public byte SentryBlue { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
        public Rectangle GetRect => new Rectangle((int)(Left), (int)(Top), (int)Width, (int)Height);
        public void SetColor(byte red, byte green, byte blue, bool raiseEvents)
        {
            Red = red;
            Green = green;
            Blue = blue;
            _lastMissingValueIndication = null;

            if (raiseEvents)
            {
                RaisePropertyChanged(nameof(OnDemandColor));
                RaisePropertyChanged(nameof(OnDemandColorTransparent));
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

        private bool _isResizeable;
        private double _scaleWidth;
        private double _scaleHeight;
        private double _scaleTop;
        private double _scaleLeft;
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


            var width = Width * scaleX;
            var height = Height * scaleY;
            if (width < 8 || height < 8)
            {
                return false;
            }
            else
            {
                Width *= scaleX;
                Height *= scaleY;
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

