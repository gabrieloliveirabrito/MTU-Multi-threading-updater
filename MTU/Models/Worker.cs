using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MTU.Models
{
    using Enums;

    public class Worker
    {
        public string Name { get; private set; }
        public WorkerType Type { get; private set; }
        public object RealWorker { get; private set; }
        public object State { get; set; }

        public Worker(string name, WorkerType type, object rWorker)
        {
            Name = name;
            Type = type;
            RealWorker = rWorker;
        }
    }
}