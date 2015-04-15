using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.data
{
    public class ObdSample
    {
        public ObdSample(short mode, short pid, long value)
        {
            this.Mode = mode;
            this.Pid = pid;
            this.Value = value;
        }

        public short Mode { get; set; }
        public short Pid { get; set; }
        public long Value { get; set; }
    }
}
