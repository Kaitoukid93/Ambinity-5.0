using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace adrilight.View
{
    public class ZoneVisualizer : System.Windows.Controls.Control
    {
        private const double UPDATE_FRAME_RATE = 25.0;
        private readonly DispatcherTimer _timer;
        private readonly List<LEDVisualizer> _leds;
        private Color[] _previousState = Array.Empty<Color>();
        public Geometry? DisplayGeometry { get; private set; }
        public ZoneVisualizer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(1000.0 / UPDATE_FRAME_RATE) };
            _timer.Start();
            _timer.Tick += TimerOnTick;
            _leds = new List<LEDVisualizer>();
            this.Loaded += ZoneVisualizer_Loaded;
            this.Unloaded += ZoneVisualizer_Unloaded;
        }
        DrawingGroup backingStore = new DrawingGroup();
        private void Render()
        {
            var drawingContext = backingStore.Open();
            try
            {
                lock (_leds)
                {
                    foreach (var led in _leds)
                    {
                        led.RenderGeometry(drawingContext);
                    }
                }
            }
            finally
            {
                //boundsPush?.Dispose();
            }
            drawingContext.Close();
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
            base.OnRender(drawingContext);

            Render(); // put content into our backingStore
            drawingContext.DrawDrawing(backingStore);

        }

        private void SetupZone()
        {
            lock (_leds)
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
            Render();
            //InvalidateVisual();
        }
        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (IsDirty())
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
