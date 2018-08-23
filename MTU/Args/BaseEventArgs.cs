using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    public abstract class BaseEventArgs : EventArgs
    {
        public DateTime Time { get; private set; }
        public BaseEventArgs()
        {
            Time = DateTime.Now;
        }
    }
}
