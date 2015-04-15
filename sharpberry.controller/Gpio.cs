using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Raspberry.IO.GeneralPurpose;
using sharpberry.controller.configuration;
using Timer = System.Timers.Timer;

namespace sharpberry.controller
{
    public class Gpio : IDisposable 
    {
        public Gpio(IGpioConnectionDriver driver)
        {
            this.vAcc = Config.Gpio.vAcc.Input().PullDown().Name("vAcc");
            this.vBatt = Config.Gpio.vBatt.Input().PullDown().Name("vBatt");
            this.activityLed = Config.Gpio.ActivityLed.Output().Name("Activity LED").Enable();
            this.powerLed = Config.Gpio.PowerLed.Output().Name("Power LED").Enable();

            this.connection = new GpioConnection(
                new GpioConnectionSettings
                    {
                        Driver = driver
                    },
                this.vAcc,
                this.vBatt,
                this.powerLed,
                this.activityLed);
            this.connection.PinStatusChanged += this.PinStatusChanged;

            this.holdTimer = new Timer(100);
            this.holdTimer.Elapsed += this.HoldTimerElapsed;
            this.holdTimer.AutoReset = false;

            this.ledTimer = new Timer(1000);
            this.ledTimer.Elapsed += this.LedTimerElapsed;
            this.ledTimer.AutoReset = true;
            this.ledTimer.Start();
        }

        public void Dispose()
        {
            if (this.connection.IsOpened)
                this.connection.Close();
            this.holdTimer.Dispose();
            this.ledTimer.Dispose();
        }

        internal readonly InputPinConfiguration vAcc;
        internal readonly InputPinConfiguration vBatt;
        internal readonly OutputPinConfiguration powerLed;
        internal readonly OutputPinConfiguration activityLed;
        internal bool vAccValue;
        internal bool vBattValue;
        internal readonly Timer holdTimer;
        internal readonly Timer ledTimer;
        internal readonly GpioConnection connection;
        internal const int StandbyDelay = 1000;
        internal const int InitializingDelay = 500;
        internal const int BlinkDelay = 100;

        public event EventHandler<PowerEventArgs> PowerEvent;

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

        protected void PinStatusChanged(object sender, PinStatusEventArgs args)
        {
            if (args.Configuration.Pin == this.vAcc.Pin)
                this.vAccValue = args.Enabled;
            else if (args.Configuration.Pin == this.vBatt.Pin)
                this.vBattValue = args.Enabled;
            else
                return;

            this.holdTimer.Stop();
            this.holdTimer.Start();
        }

        protected void HoldTimerElapsed(object sender, ElapsedEventArgs args)
        {
            var x = this.PowerEvent;
            if (x != null)
                x.Invoke(this, new PowerEventArgs(this.vAccValue, this.vBattValue));
        }

        protected void BehaviorChanged()
        {
            switch (this.Behavior)
            {
                case LedBehavior.Standby:
                    this.ledTimer.Interval = StandbyDelay;
                    this.connection[this.powerLed] = false;
                    this.connection[this.activityLed] = false;
                    break;

                case LedBehavior.Initializing:
                    this.ledTimer.Interval = InitializingDelay;
                    this.connection[this.activityLed] = true;
                    this.connection[this.activityLed] = false;
                    break;

                case LedBehavior.On:
                    this.ledTimer.Interval = 15000;
                    this.connection[this.activityLed] = true;
                    this.connection[this.activityLed] = false;
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
                        this.connection[this.powerLed] = true;
                        this.connection[this.activityLed] = true;
                        Thread.Sleep(BlinkDelay);
                        this.connection[this.powerLed] = false;
                        this.connection[this.activityLed] = false;
                        break;

                    case LedBehavior.Initializing:
                        var a = this.connection[this.powerLed];
                        var b = this.connection[this.activityLed];
                        if (a == b)
                            this.connection[this.powerLed] = !a;
                        else
                        {
                            this.connection[this.powerLed] = !a;
                            this.connection[this.activityLed] = !b;
                        }
                        break;

                    case LedBehavior.On:
                        this.connection[this.powerLed] = true;
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
