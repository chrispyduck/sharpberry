using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using sharpberry.configuration;
using sharpberry.gpio;
using Timer = System.Timers.Timer;

namespace sharpberry.controllers
{
    public class LedController : Controller
    {
        public LedController(IGpioProxy gpio, Config config)
            : base(StartupPriority.UserInterface)
        {
            this.config = config;
            this.gpio = gpio;

            this.ledTimer = new Timer(1000);
            this.ledTimer.Elapsed += this.LedTimerElapsed;
            this.ledTimer.AutoReset = false;
            this.Behavior = LedBehavior.Initializing;
            this.ledTimer.Start();
        }

        protected override void OnShutdown()
        {
            if (this.ledTimer != null)
                this.ledTimer.Dispose();
        }

        protected override void OnEnterPowerSave()
        {
            this.Behavior = LedBehavior.Standby;
        }

        protected override void OnExitPowerSave()
        {
            this.Behavior = LedBehavior.On;
        }

        internal readonly IGpioProxy gpio;
        internal readonly Timer ledTimer;
        internal readonly Config config;
        private LedBehavior behavior;
        public LedBehavior Behavior
        {
            get { return behavior; }
            set
            {
                this.behavior = value;
                BehaviorChanged();
            }
        }

        protected void BehaviorChanged()
        {
            switch (this.Behavior)
            {
                case LedBehavior.Standby:
                    this.ledTimer.Interval = config.Display.StandbyDelay;
                    gpio.PowerLed = false;
                    gpio.ActivityLed = false;
                    break;

                case LedBehavior.Initializing:
                    this.ledTimer.Interval = config.Display.InitializingDelay;
                    gpio.PowerLed = true;
                    gpio.ActivityLed = false;
                    break;

                case LedBehavior.On:
                    this.ledTimer.Interval = 15000;
                    gpio.PowerLed = true;
                    gpio.ActivityLed = false;
                    break;
            }

            this.ledTimer.Stop();
            this.ledTimer.Start();
        }

        protected void LedTimerElapsed(object sender, ElapsedEventArgs args)
        {
            try
            {
                switch (this.Behavior)
                {
                    case LedBehavior.Standby:
                        if (gpio.PowerLed && gpio.ActivityLed)
                        {
                            gpio.PowerLed = false;
                            gpio.ActivityLed = false;
                            this.ledTimer.Interval = config.Display.StandbyDelay;
                        }
                        else
                        {
                            gpio.PowerLed = true;
                            gpio.ActivityLed = true;
                            this.ledTimer.Interval = config.Display.BlinkDelay;
                        }
                        break;

                    case LedBehavior.Initializing:
                        var a = gpio.PowerLed;
                        var b = gpio.ActivityLed;
                        if (a == b)
                            gpio.PowerLed = !a;
                        else
                        {
                            gpio.PowerLed = !a;
                            gpio.ActivityLed = !b;
                        }
                        break;

                    case LedBehavior.On:
                        gpio.PowerLed = true;
                        break;
                }
            }
            finally
            {
                this.ledTimer.Start();
            }
        }
    }
}
