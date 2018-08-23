using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Args
{
    using Enums;

    public class ProgressEventArgs : BaseEventArgs
    {
        public int Progress { get; private set; }
        public string Name { get; private set; }
        public WorkerType Type { get; private set; }

        public ProgressEventArgs(int progress, string name, WorkerType type) : base()
        {
            Progress = progress;
            Name = name;
            Type = type;
        }
    }
}