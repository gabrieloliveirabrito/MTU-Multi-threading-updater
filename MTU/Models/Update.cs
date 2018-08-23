using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTU.Models
{
    public class Update
    {
        public string Filename { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
    }
}