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

            // don't immediately set the values; instead, wait a bit and report after the values have settled down
            this.holdTimer.Stop();
            this.holdTimer.Start();
        }

        protected void HoldTimerElapsed(object sender, ElapsedEventArgs args)
        {
            this.holdTimer.Stop();

            if (this.lastvAccValue != this.vAccValue)
            {
                this.PinChanged?.Invoke(this, new PinChangeEventArgs(nameof(vAcc), this.vAccValue));
                this.lastvAccValue = this.vAccValue;
            }
            
            if (this.lastvBattValue != this.vBattValue)
            {
                this.PinChanged?.Invoke(this, new PinChangeEventArgs(nameof(vBatt), this.vAccValue));
                this.lastvBattValue = this.vBattValue;
            }            
        }

        public event EventHandler<PinChangeEventArgs> PinChanged;

        public bool PowerLed
        {
            get { return this.connection[this.powerLedPin]; }
            set { this.connection[this.powerLedPin] = value; }
        }

        public bool ActivityLed
        {
            get { return this.connection[this.activityLedPin]; }
            set { this.connection[this.activityLedPin] = value; }
        }

        public bool vAcc
        {
            get { return this.lastvAccValue; }
            set
            {
                this.lastvAccValue = value;
                this.connection[this.vAccPin] = value;
                this.PinChanged?.Invoke(this, new PinChangeEventArgs(nameof(vAcc), value));
            }
        }

        public bool vBatt
        {
            get { return this.lastvBattValue; }
            set
            {
                this.lastvBattValue = value;
                this.connection[this.vBattPin] = value;
                this.PinChanged?.Invoke(this, new PinChangeEventArgs(nameof(vBatt), value));
            }
        }
    }
}
