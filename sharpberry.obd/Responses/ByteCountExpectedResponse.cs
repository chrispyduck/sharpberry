using System;

namespace sharpberry.obd.Responses
{
    [Serializable]
    public class ByteCountExpectedResponse : ExpectedResponse
    {
        public ByteCountExpectedResponse(int bytes)
        {
            this.NumberOfBytes = bytes;
        }

        public int NumberOfBytes { get; private set; }

        public override ResponseStatus CheckInput(string input)
        {
            if (input == "NO DATA")
                return ResponseStatus.NoData;

            var bytes = input.GetBytesFromHexString();

            if (bytes == null || bytes.Length > this.NumberOfBytes)
                return ResponseStatus.Invalid;

            if (bytes.Length < this.NumberOfBytes)
                return ResponseStatus.Incomplete;

            return ResponseStatus.Valid;
        }

        public override string ToString()
        {
            return this.NumberOfBytes + " bytes";
        }
    }
}
