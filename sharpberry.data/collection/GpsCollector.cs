using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.collection
{
    public sealed class GpsCollector : DataCollector<GpsSample>
    {
        public override void Initialize()
        {
            
        }

        internal override void AddSampleToTrip(Trip trip, GpsSample sample, IDataProvider dataProvider)
        {
            dataProvider.WriteGpsSample(trip, sample);
        }

        internal override IEnumerable<GpsSample> GetSamples()
        {
            yield break;
        }
    }
}
