using System;
using System.Collections.Generic;

namespace sharpberry.data.types
{
    public class Trip
    {
        public Trip()
        {
            this.Id = Guid.NewGuid();
            this.Start = DateTime.Now;
            this.Obd = new List<ObdSample>();
            this.Gps = new List<GpsSample>();
        }

        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public List<ObdSample> Obd { get; private set; }
        public List<GpsSample> Gps { get; private set; } 
    }
}
