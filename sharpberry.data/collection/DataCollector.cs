using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.collection
{
    /// <summary>
    /// Provides a base implementation of a <see cref="IDataCollector" /> that offers basic management of temporary data storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataCollector<T> : IDataCollector
    {
        /// <inheritdoc/>
        public abstract void Initialize();

        /// <inheritdoc/>
        public void Collect(Trip trip, IDataProvider dataProvider)
        {
            foreach (var sample in GetSamples())
                AddSampleToTrip(trip, sample, dataProvider);
        }

        /// <summary>
        /// Saves a collected sample to the current trip
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void AddSampleToTrip(Trip trip, T sample, IDataProvider dataProvider);

        /// <summary>
        /// Collects and returns raw data samples for addition to temporary storage
        /// </summary>
        internal abstract IEnumerable<T> GetSamples();
    }
}
