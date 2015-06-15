using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.gpio
{
    public class PinChangeEventArgs : PropertyChangedEventArgs 
    {
        public PinChangeEventArgs(string property, bool value)
            : base(property)
        {
            this.Value = value;
        }

        public bool Value { get; private set; }
    }
}
