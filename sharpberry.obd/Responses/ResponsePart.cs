using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.obd.Responses
{
    public class ResponsePart
    {
        public ResponsePart(string text, bool isComplete)
        {
            this.Text = text.Trim();
            this.IsComplete = isComplete;
        }

        public string Text { get; private set; }
        public bool IsComplete { get; private set; }
    }
}
