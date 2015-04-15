using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.data
{
    public abstract class DataCollector<T>
    {
        public abstract T Collect();
    }
}
