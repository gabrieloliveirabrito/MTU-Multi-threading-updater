using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Updater.Comparers
{
    using Models;

    public class UpdateComparer : EqualityComparer<Update>
    {
        public override bool Equals(Update x, Update y)
        {
            return x.Filename.Equals(y.Filename);
        }

        public override int GetHashCode(Update obj)
        {
            return obj.GetHashCode();
        }
    }
}