using System;

namespace sharpberry.obd.Responses
{
    [Serializable]
    public class CustomExpectedResponse : ExpectedResponse
    {
        public CustomExpectedResponse(Func<string, ResponseStatus> function)
        {
            this.Function = function;
        }
        
        public Func<string, ResponseStatus> Function { get; private set; }

        public override ResponseStatus CheckInput(string input)
        {
            return this.Function(input);
        }
    }
}
