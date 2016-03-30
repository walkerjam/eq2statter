using System;

namespace Statter
{
    public class StatReading
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }

        public StatReading()
        {
            Time = DateTime.Now;
        }
    }
}
