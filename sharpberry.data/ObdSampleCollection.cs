using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.data
{
    public class ObdSampleCollection : List<ObdSample>
    {
        public ObdSampleCollection()
            : base()
        {
            this.Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; set; }
    }
}
