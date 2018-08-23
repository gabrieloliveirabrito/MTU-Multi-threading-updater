using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    using Enums;

    public class WorkerAppendEventArgs : BaseEventArgs
    {
        public string Name { get; private set; }
        public WorkerType Type { get; private set; }

        public WorkerAppendEventArgs(string name, WorkerType type) : base()
        {
            Name = name;
            Type = type;
        }
    }
}
