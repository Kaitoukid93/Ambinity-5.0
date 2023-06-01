using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace adrilight.Spots
{
    [DebuggerDisplay("Spot: Rectangle={Rectangle}, Color={Red},{Green},{Blue}")]
    public class DeviceSpot : ViewModelBase, IDeviceSpot, IDrawable
    {

        public DeviceSpot(double top, double left, double width, double height, double scaleTop, double scaleLeft, double scaleWidth, double scaleHeight, int index, int positionIndex, int virtualIndex, int musicIndex, int columnIndex, bool isActivated, Geometry geometry)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;

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
        public Rect GetRect => new Rect(Left, Top, Width, Height);
        public bool GetVIDIfNeeded(int vid, Rect rect, int mode)
        {
            if (mode == 0)
            {
                if (!HasVID)
                {
                    var intersectRect = Rect.Intersect(GetRect, rect);
                    if (intersectRect.IsEmpty)
                        return false;
                    double intersectArea = intersectRect.Width * intersectRect.Height;
                    double spotArea = GetRect.Width * GetRect.Height;
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
                    var intersectRect = Rect.Intersect(GetRect, rect);
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
        public void UpdateView()
        {
            RaisePropertyChanged(nameof(OnDemandColor));
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

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public System.Windows.Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        private DrawableHelpers DrawableHlprs => new DrawableHelpers();
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public bool SetScale(double scaleX, double scaleY, bool keepOrigin)
        {


            var width = Width * scaleX;
            var height = Height * scaleY;
            if (width < 1 || height < 1)
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

        public void RotateSpot(double angleInDegrees, Point centerPoint, double offsetX, double offsetY)
        {
            //clone the geometry
            var inputGeometryClone = Geometry.Clone();
            //scale the geometry from native size to spot size
            var boundsLeft = inputGeometryClone.Bounds.Left;
            var boundsTop = inputGeometryClone.Bounds.Top;
            var scaleX = Width / inputGeometryClone.Bounds.Width;
            var scaleY = Height / inputGeometryClone.Bounds.Height;
            inputGeometryClone.Transform = new TransformGroup {
                Children = new TransformCollection
                {
                    new ScaleTransform(scaleX, scaleY),
                    new TranslateTransform(offsetX+Left-boundsLeft*scaleX, offsetY+Top -boundsTop*scaleY),
                   // new RotateTransform(angleInDegrees)
                }
            };
            inputGeometryClone.Transform = new RotateTransform(angleInDegrees, centerPoint.X - (offsetX + Left), centerPoint.Y - (offsetY + Top));
            var deltaX = inputGeometryClone.Bounds.Left;
            var deltaY = inputGeometryClone.Bounds.Top;
            Top += deltaY + offsetY;
            Left += deltaX + offsetX;
            Width = inputGeometryClone.Bounds.Width;
            Height = inputGeometryClone.Bounds.Height;
            Angle += angleInDegrees;
            if (Angle > 360)
            {
                Angle -= 360;
            }

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

