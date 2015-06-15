using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    public class DtcStatus
    {
        public DtcStatus(ParsedResponse response)
        {
            this.Header = response.Header;
            if (response.Status != ResponseStatus.Valid)
            {
                this.CodeCount = 0;
                this.IsCheckEngineLightOn = false;
                return;
            }

            this.IsCheckEngineLightOn = (response.DataBytes[0] & 0x80) == 0x80;
            this.CodeCount = response.DataBytes[0] & ~0x80;
        }

        public byte[] Header { get; private set; }
        public int CodeCount { get; private set; }
        public bool IsCheckEngineLightOn { get; private set; }
    }
}
