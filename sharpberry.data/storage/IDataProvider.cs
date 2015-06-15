using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using sharpberry.data.types;

namespace sharpberry.data.storage
{
    public interface IDataProvider
    {
        Trip StartTrip();
        void EndTrip(Trip trip);

        void WriteObdSample(Trip trip, ObdSample sample);
        void WriteGpsSample(Trip trip, GpsSample sample);
    }
}
