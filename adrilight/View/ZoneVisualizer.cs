using adrilight.Spots;
using MathNet.Numerics;
using NAudio.Gui;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TimeLineTool;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace adrilight.View
{
    public class ZoneVisualizer : System.Windows.Controls.Control
    {
        private const double UPDATE_FRAME_RATE = 25.0;
        private readonly DispatcherTimer _timer;
        private readonly SolidColorBrush _fillBrush;
        private readonly Pen _pen;
        private readonly SolidColorBrush _penBrush;
        private readonly List<LEDVisualizer> _leds;
        private Color[] _previousState = Array.Empty<Color>();
        public Geometry? DisplayGeometry { get; private set; }
        public ZoneVisualizer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(1000.0 / UPDATE_FRAME_RATE) };
            _timer.Start();
            _timer.Tick += TimerOnTick;
            _fillBrush = new SolidColorBrush();
            _penBrush = new SolidColorBrush();
            _pen = new Pen(_penBrush, 1.0) { LineJoin = PenLineJoin.Round };
            _leds = new List<LEDVisualizer>();
            this.Loaded += ZoneVisualizer_Loaded;
            this.Unloaded += ZoneVisualizer_Unloaded;
            //CreateLedGeometry();
            //PointerReleased += OnPointerReleased;
            //PropertyChanged += OnPropertyChanged;
        }

        private void ZoneVisualizer_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= TimerOnTick;
        }
        private void ZoneVisualizer_Loaded(object sender, RoutedEventArgs e)

        {
            _timer.Start();
            _timer.Tick += TimerOnTick;
          
        }
        protected override void OnRender(DrawingContext drawingContext)
        {

            try
            {
                foreach(var led in _leds )
                {
                    led.RenderGeometry(drawingContext);
                }

            }
            finally
            {
                //boundsPush?.Dispose();
            }
        }

       private void SetupZone()
        {
            lock(_leds)
            {
                _leds.Clear();
            }
            lock (_leds)
            {
                foreach (DeviceSpot spot in Zone.Spots)
                    _leds.Add(new LEDVisualizer(spot));
            }
        }



        private bool IsDirty()
        {
            if (Zone == null)
                return false;

            Color[] state = new Color[Zone.Spots.Count()];
            bool difference = _previousState.Length != state.Length;

            // Check all LEDs for differences and copy the colors to a new state
            int index = 0;
            foreach (DeviceSpot spot in Zone.Spots)
            {
                if (!difference && !spot.OnDemandColor.Equals(_previousState[index]))
                    difference = true;

                state[index] = spot.OnDemandColor;
                index++;
            }

            // Store the new state for next time
            _previousState = state;

            return difference;
        }

        private void Update()
        {
            InvalidateVisual();
        }
        private void TimerOnTick(object? sender, EventArgs e)
        {
            if(IsDirty())
            Update();
        }
        #region Properties


        public LEDSetup Zone {
            get => (LEDSetup)GetValue(ZoneProperty);
            set => SetValue(ZoneProperty, value);
        }
        public static readonly DependencyProperty ZoneProperty =
          DependencyProperty.Register(nameof(Zone), typeof(LEDSetup),
            typeof(ZoneVisualizer), new UIPropertyMetadata(null, new PropertyChangedCallback(SpotDataChanged)));
        public static void SpotDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoneVisualizer led = d as ZoneVisualizer;
            led.SetupZone();
        }
        #endregion
    }
}
