using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public class Tick
    {
        public Tick() { IsRunning = true; }
        public double CurrentTick { get; set; }
        public TickEnum TickType { get; set; }
        public int MaxTick { get; set; }
        public double TickSpeed { get; set; }
        public bool IsRunning { get; set; }
        public string TickUID { get;set; }
    }
}
