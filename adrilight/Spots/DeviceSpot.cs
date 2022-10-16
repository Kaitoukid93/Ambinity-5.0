using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Windows.Media.Color;

namespace adrilight.Spots
{
    [DebuggerDisplay("Spot: Rectangle={Rectangle}, Color={Red},{Green},{Blue}")]
    sealed class DeviceSpot : ViewModelBase, IDisposable, IDeviceSpot
    {

        public DeviceSpot(int x, int y,int top, int left, int width, int height,int index,int positionIndex, int virtualIndex , int musicIndex, int columnIndex, bool isActivated, bool isIDVissible)
        {
            Rectangle = new Rectangle(top, left, width, height);
       

            RadiusX = 0;
            RadiusY = 0;
            ID = index.ToString();
            id = index;
            VID = virtualIndex;
            MID = musicIndex;
            CID = columnIndex;
            XIndex = x;
            YIndex = y;
            PID = positionIndex;
            IsActivated = isActivated;
            BorderThickness = 0;
            IsIDVissible = isIDVissible;



        }

        public Rectangle Rectangle { get;  set; }
        public int id { get; set; }

        private bool _isFirst;
        public bool IsFirst {
            get => _isFirst;
            set { Set(() => IsFirst, ref _isFirst, value); }
        }
        public int MID { get; set; }
        public bool IsIDVissible { get; set; }

        public Color OnDemandColor => Color.FromRgb(Red, Green, Blue);
        public Color SentryColor => Color.FromRgb(SentryRed, SentryGreen, SentryBlue);
        public Color OnDemandColorTransparent => Color.FromArgb(255, Red, Green, Blue);
        public int RadiusX { get; set; }
        public int RadiusY { get;  set; }
        public string ID { get; set; }
        public int VID { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int PID { get; set; }
        public int CID { get; set; }
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public bool IsActivated { get; set; }
        public double BorderThickness { get; set; }
        public byte SentryRed { get; private set; }
        public byte SentryGreen { get; private set; }
        public byte SentryBlue { get; private set; }

        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

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
        public void SetStroke(double strokeThickness)
        {
            BorderThickness=strokeThickness;
            RaisePropertyChanged(nameof(BorderThickness));
        }
        public void SetRectangle(Rectangle rectangle)
        {
            Rectangle = rectangle;
            RaisePropertyChanged(nameof(Rectangle));
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
        public void SetIDVissible(bool iDVissible)
        {
            IsIDVissible = iDVissible;



            RaisePropertyChanged(nameof(IsIDVissible));


        }
        public void SetID(int ID)
        {
            id = ID;

            RaisePropertyChanged(nameof(id));


        }
        public void SetCID(int ID)
        {
            CID = ID;

            RaisePropertyChanged(nameof(CID));


        }
        //public void SetNextVID(int currentGroupVIDCounter)
        //{
        //    VID = currentGroupVIDCounter + 1;
        //}

        public void Dispose()
        {
        }

        private DateTime? _lastMissingValueIndication;
        private readonly double _dimToBlackIntervalInMs = TimeSpan.FromMilliseconds(1000).TotalMilliseconds;

        private float _dimR, _dimG, _dimB;

        public void DimLED(float dimFactor)
        {
          
            SetColor((byte)(dimFactor * Red), (byte)(dimFactor * Green), (byte)(dimFactor * Blue), true);
        }
    }
}

