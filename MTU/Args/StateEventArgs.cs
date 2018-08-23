using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    using Enums;
 
    public class StateEventArgs : BaseEventArgs
    {
        public UpdaterState State { get; private set; }

        public StateEventArgs(UpdaterState state) : base()
        {
            State = state;
        }
    }
}
