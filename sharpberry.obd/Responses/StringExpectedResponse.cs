using System;

namespace sharpberry.obd.Responses
{
    [Serializable]
    public class StringExpectedResponse : ExpectedResponse
    {
        public StringExpectedResponse(string expectedValue)
            : this(expectedValue, false)
        { }
        public StringExpectedResponse(string expectedValue, bool allowExtraData)
        {
            this.ExpectedValue = expectedValue;
            this.AllowExtraData = allowExtraData;
        }

        public string ExpectedValue { get; private set; }
        public bool AllowExtraData { get; private set; }

        public override ResponseStatus CheckInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ResponseStatus.Incomplete;

            if (input == "NO DATA")
                return ResponseStatus.NoData;

            if (input.Length < this.ExpectedValue.Length)
                return this.ExpectedValue.StartsWith(input, StringComparison.InvariantCultureIgnoreCase)
                           ? ResponseStatus.Incomplete
                           : ResponseStatus.Invalid;

            var testPassed = this.AllowExtraData
                ? input.StartsWith(this.ExpectedValue, StringComparison.InvariantCultureIgnoreCase)
                : string.Equals(input, this.ExpectedValue, StringComparison.InvariantCultureIgnoreCase);

            return testPassed
                ? ResponseStatus.Valid
                : ResponseStatus.Invalid;
        }

        public override string ToString()
        {
            return string.Format("string: '{0}{1}'", this.ExpectedValue, this.AllowExtraData ? "*" : "");
        }
    }
}
