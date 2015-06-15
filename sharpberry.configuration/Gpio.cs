using Raspberry.IO.GeneralPurpose;

namespace sharpberry.configuration
{
    public class Gpio
    {
        public ConnectorPin vAcc { get; set; }
        public ConnectorPin vBatt { get; set; }
        public ConnectorPin PowerLed { get; set; }
        public ConnectorPin ActivityLed { get; set; }
    }
}
