using System;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    [Serializable]
    public abstract class Command
    {
        protected Command(string name, ExpectedResponse expectedResponse)
        {
            this.Name = name;
            this.ExpectedResponse = expectedResponse;
        }

        public string Name { get; private set; }
        public ExpectedResponse ExpectedResponse { get; private set; }

        public abstract string GetCommandString();

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.Name, this.GetCommandString());
        }
    }
}
