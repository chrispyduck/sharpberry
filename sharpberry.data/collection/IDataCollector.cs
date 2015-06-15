using System;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.collection
{
    /// <summary>
    /// Fetches and manages data in preparation for it to be stored elsewhere
    /// </summary>
    public interface IDataCollector
    {
        /// <summary>
        /// Performs initial configuration of the data collector. May be called more than once to repeat initialization procedures.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Collects one or more samples and adds them to the current trip
        /// </summary>
        void Collect(Trip trip, IDataProvider dataProvider);
    }
}
