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
        public WebClient Client { get; private set; }

        public Worker(string name, WorkerType type, WebClient client)
        {
            Name = name;
            Type = type;
            Client = client;
        }
    }
}