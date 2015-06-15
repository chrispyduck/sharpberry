using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.data.types
{
    public struct GpsSample
    {
        public GpsSample(double lat, double lon)
        {
            this.Ticks = DateTime.Now.Ticks;
            this.Latitude = lat;
            this.Longtitude = lon;
        }

        public double Ticks;
        public double Latitude;
        public double Longtitude;
    }
}
