using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    public class ErrorEventArgs : BaseEventArgs
    {
        public Exception Error { get; protected set; }

        public ErrorEventArgs(Exception error) : base()
        {
            Error = error;
        }
    }
}
