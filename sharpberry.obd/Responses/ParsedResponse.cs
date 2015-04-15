using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.obd.Responses
{
    public class ParsedResponse
    {
        public byte[] Header { get; set; }
        public byte[] Command { get; set; }
        public string Data { get; set; }
        public byte[] DataBytes { get; set; }
        public byte[] Checksum { get; set; }

        public string Error { get; set; }
        public Exception Exception { get; set; }
        public bool IsError { get; set; }
        public ResponseStatus Status { get; set; }

        public override string ToString()
        {
            if (IsError)
                return Error;

            var parts = new List<string>();
            if (Header != null)
                parts.Add(Header.ToHexString());
            if (Command != null)
                parts.Add(Command.ToHexString());
            if (Data != null)
                parts.Add(Data);
            if (Checksum != null)
                parts.Add(Checksum.ToHexString());
            return string.Join("-", parts);
        }
    }
}
