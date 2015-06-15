namespace sharpberry.configuration
{
    public class Display
    {
        public Display()
        {
            this.StandbyDelay = 1000;
            this.InitializingDelay = 500;
            this.BlinkDelay = 100;
        }
        public int StandbyDelay { get; set; }
        public int InitializingDelay { get; set; }
        public int BlinkDelay { get; set; }
    }
}
