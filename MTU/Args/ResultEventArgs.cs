using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    public class ResultEventArgs : BaseEventArgs
    {
        public bool Result { get; private set; }

        public ResultEventArgs(bool result) : base()
        {
            Result = result;
        }
    }
}
