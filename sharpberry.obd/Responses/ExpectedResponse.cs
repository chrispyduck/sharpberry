using System;

namespace sharpberry.obd.Responses
{
    [Serializable]
    public abstract class ExpectedResponse
    {
        public abstract ResponseStatus CheckInput(string input);

        public static ExpectedResponse Ok = new StringExpectedResponse("OK");
        public static ExpectedResponse ByteCount(int count)
        {
            return new ByteCountExpectedResponse(count);
        }
        public static ExpectedResponse Any = new CustomExpectedResponse(x => ResponseStatus.Valid);
        public static ExpectedResponse String(string value, bool allowExtraData = false)
        {
            return new StringExpectedResponse(value, allowExtraData);
        }
    }
}
