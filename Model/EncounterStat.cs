using System.Collections.Generic;

namespace Statter
{
    public class EncounterStat
    {
        public Stat Stat { get; set; }
        public List<StatReading> Readings { get; set; }
        public StatReading MinReading { get; set; }
        public StatReading MaxReading { get; set; }
    }
}
