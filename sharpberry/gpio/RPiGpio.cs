using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Raspberry.IO.GeneralPurpose;
using sharpberry.configuration;

namespace sharpberry.gpio
{
    public class RPiGpio : IGpioProxy
    {
        public RPiGpio(Config config)
        {
            this.vAccPin = config.Gpio.vAcc.Input().PullDown().Name("vAcc");
            this.vBattPin = config.Gpio.vBatt.Input().PullDown().Name("vBatt");

            this.connection = new GpioConnection(
                new GpioConnectionSettings
                {
                    Driver = new GpioConnectionDriver()
                },
                this.vAccPin,
                this.vBattPin,
                this.powerLedPin,
                this.activityLedPin);
            this.connection.PinStatusChanged += this.PinStatusChanged;

            this.holdTimer = new Timer(100);
            this.holdTimer.Elapsed += this.HoldTimerElapsed;
            this.holdTimer.AutoReset = false;

            this.connection.Open();
        }

        internal readonly InputPinConfiguration vAccPin;
        internal readonly InputPinConfiguration vBattPin;
        internal readonly OutputPinConfiguration powerLedPin;
        internal readonly OutputPinConfiguration activityLedPin;
        internal bool vAccValue;
        internal bool vBattValue;
        internal bool lastvAccValue;
        internal bool lastvBattValue;
        internal readonly Timer holdTimer;
        internal readonly GpioConnection connection;

        protected void PinStatusChanged(object sender, PinStatusEventArgs args)
        {
            if (args.Configuration.Pin == this.vAccPin.Pin)
                this.vAccValue = args.Enabled;
            else if (args.Configuration.Pin == this.vBattPin.Pin)
                this.vBattValue = args.Enabled;
            else
                return;

            this.holdTimer.Stop();
            this.holdTimer.Start();
        }

        protected void HoldTimerElapsed(object sender, ElapsedEventArgs args)
        {
            // raise event!
            this.holdTimer.Stop();

            this.lastvAccValue = this.vAccValue;
            this.lastvBattValue = this.vBattValue;
        }

        public event EventHandler<PinChangeEventArgs> PinChanged;
        public bool PowerLed { get; set; }
        public bool ActivityLed { get; set; }
        public bool vAcc { get; set; }
        public bool vBatt { get; set; }
    }
}
