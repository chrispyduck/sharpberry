using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using Raspberry.IO.GeneralPurpose;
using sharpberry.gpio;
using Timer = System.Timers.Timer;

namespace sharpberry.controllers
{
    public class PowerController : Controller
    {
        public PowerController(IGpioProxy gpio)
            : base(StartupPriority.DevicePower)
        {
            gpio.PinChanged += GpioPinChanged;
        }

        protected void GpioPinChanged(object sender, PinChangeEventArgs e)
        {
            if (e.PropertyName == "vAcc")
                base.RaiseEvent(e.Value
                    ? ControllerEventType.RequestExitPowerSave
                    : ControllerEventType.RequestEnterPowerSave);
        }

        protected override void OnEnterPowerSave()
        {
            
        }

        protected override void OnExitPowerSave()
        {
            
        }
    }
}
