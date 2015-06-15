using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using sharpberry.configuration;
using sharpberry.data.collection;

namespace sharpberry.controllers
{
    public class DataController : Controller 
    {
        public DataController(DataCollectionManager dataCollectionManager, Config config)
            : base(StartupPriority.DataCollection)
        {
            this.dataCollectionManager = dataCollectionManager;
            this.config = config;

            this.dataTimer = new Timer(config.DataCollection.CollectionInterval.TotalMilliseconds);
            this.dataTimer.AutoReset = true;
            this.dataTimer.Enabled = false;
            this.dataTimer.Elapsed += TimerElapsed;
        }

        private readonly Config config;
        private readonly DataCollectionManager dataCollectionManager;
        private readonly Timer dataTimer;

        protected override void OnEnterPowerSave()
        {
            dataCollectionManager.EndTrip();
            this.dataTimer.Stop();
        }

        protected override void OnExitPowerSave()
        {
            dataCollectionManager.StartTrip();
            this.dataTimer.Start();
        }

        protected override void OnShutdown()
        {
            if (this.dataTimer.Enabled)
                this.dataTimer.Stop();
            if (dataCollectionManager.CurrentTrip != null)
                dataCollectionManager.EndTrip();
        }

        protected void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            RaiseEvent(ControllerEventType.ReceiveData);
            dataCollectionManager.Collect();
        }
    }
}
