using System;
using System.Linq;
using System.Timers;
using sharpberry.configuration;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.collection
{
    public class DataCollectionManager
    {
        public DataCollectionManager(
            IDataCollector[] collectors,
            IDataProvider provider,
            Config config)
        {
            this.collectors = collectors;
            this.provider = provider;

            this.collectionTimer = new Timer();
            this.collectionTimer.Interval = config.DataCollection.CollectionInterval.TotalMilliseconds;
            this.collectionTimer.Elapsed += OnCollectionTimerCall;
            this.collectionTimer.Enabled = false;
        }

        private Trip currentTrip;
        private readonly Config config;
        private readonly IDataCollector[] collectors;
        private readonly IDataProvider provider;
        private readonly Timer collectionTimer;

        public Trip CurrentTrip { get { return this.currentTrip; } }
        
        void OnCollectionTimerCall(object sender, EventArgs e)
        {
            this.Collect();
        }

        public void Collect()
        {
            collectors.AsParallel().ForAll(c => c.Collect(this.currentTrip, this.provider));
        }

        public void StartTrip()
        {
            if (this.currentTrip != null)
                throw new InvalidOperationException("A trip is already in progress");
            this.currentTrip = this.provider.StartTrip();
            this.collectionTimer.Enabled = true;
        }

        public void EndTrip()
        {
            if (this.currentTrip == null)
                throw new InvalidOperationException("There is no current trip. Cannot end trip.");
            this.provider.EndTrip(this.currentTrip);
            this.currentTrip = null;
            this.collectionTimer.Enabled = false;
        }
    }
}
